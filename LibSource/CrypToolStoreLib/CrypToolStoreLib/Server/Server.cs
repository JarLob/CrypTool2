/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypToolStoreLib.Database;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Server
{    
    public class CrypToolStoreServer
    {
        public const int DEFAULT_PORT = 15151;
        private Logger logger = Logger.GetLogger();

        public X509Certificate2 ServerKey
        {
            get;
            set;
        }

        /// <summary>
        /// Responsible for incoming TCP connections
        /// </summary>
        private TcpListener TCPListener
        {
            get;
            set;
        }

        /// <summary>
        /// Port to connect to
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to set state of server to running/not running
        /// </summary>
        internal bool Running
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CrypToolStoreServer()
        {
            Port = DEFAULT_PORT;
        }        

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            if (Running)
            {
                return;
            }
            logger.LogText("Starting listen thread", this, Logtype.Info);
            Running = true;
            Thread listenThread = new Thread(ListenThread);
            listenThread.IsBackground = true;
            listenThread.Start();
            Thread.Sleep(1000); //just to let the thread start
            logger.LogText("Listen thread started", this, Logtype.Info);
        }
        
        /// <summary>
        /// Listens for new incoming connections
        /// </summary>
        private void ListenThread()
        {
            try
            {
                TCPListener = new TcpListener(IPAddress.Any, Port);
                TCPListener.Start();

                while (Running)
                {
                    try
                    {
                        TcpClient client = TCPListener.AcceptTcpClient();
                        logger.LogText(String.Format("New client connected from IP/Port={0}", client.Client.RemoteEndPoint), this, Logtype.Info);
                        Thread handlerthread = new Thread(() =>
                        {
                            try
                            {
                                ClientHandler handler = new ClientHandler();
                                handler.CrypToolStoreServer = this;
                                handler.HandleClient(client);
                            }
                            catch (Exception ex)
                            {
                                logger.LogText(String.Format("Exception during handling of client from IP/Port={0} : {1}", client.Client.RemoteEndPoint, ex.Message), this, Logtype.Error);
                            }
                        });
                        handlerthread.IsBackground = true;
                        handlerthread.Start();
                    }
                    catch (Exception ex)
                    {
                        if (Running)
                        {
                            logger.LogText(String.Format("Exception in ListenThread: {0}", ex.Message), this, Logtype.Error);
                        }
                    }
                }
                logger.LogText("ListenThread terminated", this, Logtype.Info);
            }
            catch (Exception ex)
            {
                logger.LogText(String.Format("Exception in ListenThread: {0}", ex.Message), this, Logtype.Error);
            }
        }      

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {            
            if (!Running)
            {
                return;
            }
            try
            {
                logger.LogText("Stopping server", this, Logtype.Info);
                Running = false;
                TCPListener.Stop();
                logger.LogText("Server stopped", this, Logtype.Info);
            }
            catch (Exception ex)
            {
                logger.LogText(String.Format("Exception during stopping of TCPListener: {0}", ex.Message), this, Logtype.Error);
            }
        }
    }

    /// <summary>
    /// A single client handler is responsible for the communication with one client
    /// </summary>
    public class ClientHandler
    {
        private Logger Logger = Logger.GetLogger();
        private CrypToolStoreDatabase Database = CrypToolStoreDatabase.GetDatabase();        
        private bool ClientIsAuthenticated { get; set; }
        private bool ClientIsAdmin { get; set; }
        private string Username { get; set; }
        private IPAddress IPAddress { get; set; }        

        /// <summary>
        /// This hashset memorizes the tries of a password from a dedicated IP
        /// </summary>
        private static ConcurrentDictionary<IPAddress, PasswordTry> PasswordTries = new ConcurrentDictionary<IPAddress, PasswordTry>();

        private int PASSWORD_RETRY_INTERVAL = 5; //minutes
        private int ALLOWED_PASSWORD_RETRIES = 3;

        /// <summary>
        /// Reference to the server object
        /// </summary>
        public CrypToolStoreServer CrypToolStoreServer
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientHandler()
        {
            Username = "anonymous"; //default username is anonymous
            ClientIsAuthenticated = false;
            ClientIsAdmin = false;
        }

        /// <summary>
        /// This method receives messages from the client and handles it
        /// </summary>
        /// <param name="client"></param>
        public void HandleClient(TcpClient client)
        {
            IPAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            using (SslStream sslstream = new SslStream(client.GetStream()))
            {                
                //Step 0: Authenticate SSLStream as server
                sslstream.AuthenticateAsServer(CrypToolStoreServer.ServerKey, false, false);
                try
                {
                    while (CrypToolStoreServer.Running && client.Connected && sslstream.CanRead && sslstream.CanWrite)
                    {
                        //Step 1: Read message header                    
                        byte[] headerbytes = new byte[21]; //a message header is 21 bytes
                        int bytesread = 0;

                        while (bytesread < 21)
                        {
                            int readbytes = sslstream.Read(headerbytes, bytesread, 21 - bytesread);
                            if (readbytes == 0)
                            {
                                //stream was closed
                                return;
                            }
                            bytesread += readbytes;
                        }

                        //Step 2: Deserialize message header and get payloadsize
                        MessageHeader header = new MessageHeader();
                        header.Deserialize(headerbytes);
                        int payloadsize = header.PayloadSize;
                        if (payloadsize > Message.MAX_PAYLOAD_SIZE)
                        {
                            //if we receive a message larger than MAX_PAYLOAD_SIZE we throw an exception which terminates the session
                            throw new Exception(String.Format("Receiving a message with a payload which is larger (={0} bytes) than the Message.MAX_PAYLOAD_SIZE={1} bytes", payloadsize, Message.MAX_PAYLOAD_SIZE));
                        }

                        //Step 3: Read complete message
                        byte[] messagebytes = new byte[payloadsize + 21];
                        Array.Copy(headerbytes, 0, messagebytes, 0, 21);

                        while (bytesread < payloadsize + 21)
                        {
                             int readbytes = sslstream.Read(messagebytes, bytesread, payloadsize + 21 - bytesread);
                             if (readbytes == 0)
                             {
                                 //stream was closed
                                 return;
                             }
                             bytesread += readbytes;
                        }

                        //Step 4: Deserialize Message
                        Message message = Message.DeserializeMessage(messagebytes);
                        Logger.LogText(String.Format("Received a message of type {0}", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

                        //Step 5: Handle received message
                        HandleMessage(message, sslstream);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during HandleClient: {0}", ex.Message), this, Logtype.Error);
                    return;
                }
                finally
                {
                    if (sslstream != null)
                    {
                        sslstream.Close();
                    }
                    if (client != null)
                    {
                        client.Close();
                    }                   
                    Logger.LogText("Client disconnected", this, Logtype.Info);
                }
            }
        }

        /// <summary>
        /// Responsible for handling received messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslstream"></param>
        private void HandleMessage(Message message, SslStream sslStream)
        {
            switch (message.MessageHeader.MessageType)
            {
                case MessageType.Login:
                    HandleLoginMessage((LoginMessage)message, sslStream);
                    break;
                case MessageType.Logout:
                    HandleLogoutMessage((LogoutMessage)message, sslStream);
                    break;
                case MessageType.CreateNewDeveloper:
                    HandleCreateNewDeveloperMessage((CreateNewDeveloperMessage)message, sslStream);
                    break;
                case MessageType.UpdateDeveloper:
                    HandleUpdateDeveloperMessage((UpdateDeveloperMessage)message, sslStream);
                    break;
                case MessageType.DeleteDeveloper:
                    HandleDeleteDeveloperMessage((DeleteDeveloperMessage)message, sslStream);
                    break;
                case MessageType.RequestDeveloper:
                    HandleRequestDeveloperMessage((RequestDeveloperMessage)message, sslStream);
                    break;
                case MessageType.RequestDeveloperList:
                    HandleRequestDeveloperListMessage((RequestDeveloperListMessage)message, sslStream);
                    break;
                case MessageType.CreateNewPlugin:
                    HandleCreateNewPluginMessage((CreateNewPluginMessage)message, sslStream);
                    break;
                case MessageType.UpdatePlugin:
                    HandleUpdatePluginMessage((UpdatePluginMessage)message, sslStream);
                    break;
                case MessageType.RequestPlugin:
                    HandleRequestPluginMessage((RequestPluginMessage)message, sslStream);
                    break;
                case MessageType.RequestPluginList:
                    HandleRequestPluginListMessage((RequestPluginListMessage)message, sslStream);
                    break;
                case MessageType.CreateNewSource:
                    HandleCreateNewSourceMessage((CreateNewSourceMessage)message, sslStream);
                    break;

                default:
                    HandleUnknownMessage(message, sslStream);
                    break;
            }
        }

        /// <summary>
        /// Handles messages of unknown message type
        /// Sends that we do not know the type of message
        /// Also writes a log entry
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslStream"></param>
        private void HandleUnknownMessage(Message message, SslStream sslStream)
        {
            Logger.LogText(String.Format("Received message of unknown type {0} from user {1} from IP={2}", message.MessageHeader.MessageType, Username, IPAddress), this, Logtype.Warning);
            ServerErrorMessage error = new ServerErrorMessage();
            error.Message = String.Format("Unknown type of message: {0}", message.MessageHeader.MessageType);            
            SendMessage(error, sslStream);
        }
        
        /// <summary>
        /// Handles login attempts
        /// Each IP is only allowed to try ALLOWED_PASSWORD_RETRIES passwords
        /// After ALLOWED_PASSWORD_RETRIES wrong passwords, the authentication is always refused within the next PASSWORD_RETRY_INTERVAL minutes
        /// Login attempts refresh the interval
        /// </summary>
        /// <param name="loginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleLoginMessage(LoginMessage loginMessage, SslStream sslStream)
        {
            //Initially, we set everything to false
            ClientIsAuthenticated = false;
            ClientIsAdmin = false;

            string username = loginMessage.Username;
            string password = loginMessage.Password;

            if (PasswordTries.ContainsKey(IPAddress))
            {                            
                if (DateTime.Now > PasswordTries[IPAddress].LastTryDateTime.AddMinutes(PASSWORD_RETRY_INTERVAL))
                {
                    PasswordTry passwordTry;
                    PasswordTries.TryRemove(IPAddress, out passwordTry);
                }
                else if (PasswordTries[IPAddress].Number >= ALLOWED_PASSWORD_RETRIES)
                {                    
                    // after 3 tries, we just close the connection and refresh the timer
                    PasswordTries[IPAddress].LastTryDateTime = DateTime.Now;
                    PasswordTries[IPAddress].Number++;
                    Logger.LogText(String.Format("{0}. try of a username/password (username={1}) combination from IP {2} - kill the sslStream now", PasswordTries[IPAddress].Number, username, IPAddress), this, Logtype.Warning);
                    sslStream.Close();
                    return;
                }    
            }            

            if (Database.CheckDeveloperPassword(username, password) == true)
            {                
                ClientIsAuthenticated = true;
                Username = username;
                ResponseLoginMessage response = new ResponseLoginMessage();
                response.LoginOk = true;
                response.Message = "Login credentials correct!";
                Logger.LogText(String.Format("User {0} successfully authenticated from IP={1}", username, IPAddress), this, Logtype.Info);
                Developer developer = Database.GetDeveloper(username);
                if (developer.IsAdmin)
                {
                    response.IsAdmin = true;
                    ClientIsAdmin = true;
                    Logger.LogText(String.Format("User {0} is admin", username), this, Logtype.Info);                                        
                }
                SendMessage(response, sslStream);
            }
            else
            {
                if (!PasswordTries.ContainsKey(IPAddress))
                {
                    PasswordTries.TryAdd(IPAddress, new PasswordTry() { Number = 1, LastTryDateTime = DateTime.Now });
                }
                else
                {
                    PasswordTries[IPAddress].Number++;
                    PasswordTries[IPAddress].LastTryDateTime = DateTime.Now;
                }               
                ResponseLoginMessage response = new ResponseLoginMessage();
                response.LoginOk = false;
                response.Message = "Login credentials incorrect!";
                Logger.LogText(String.Format("{0}. try of a username/password (username={1}) combination from IP={2}", PasswordTries[IPAddress].Number, username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles logouts; set ClientIsAuthenticated to false
        /// </summary>
        /// <param name="logoutMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleLogoutMessage(LogoutMessage logoutMessage, SslStream sslStream)
        {
            ClientIsAuthenticated = false;
            Logger.LogText(String.Format("User {0} from IP={1} logged out", Username, IPAddress), this, Logtype.Info);
            sslStream.Close();
        }

        /// <summary>
        /// Handles CreateNewDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to create a new developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewDeveloperMessage(CreateNewDeveloperMessage createNewDeveloperMessage, SslStream sslStream)
        {
            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                response.Message = "Unauthorized to create new developers! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to create new developer={1} from IP={2}", Username, createNewDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, creation of new developer in database is started
            try
            {
                Developer developer = createNewDeveloperMessage.Developer;
                Database.CreateDeveloper(developer.Username, developer.Firstname, developer.Lastname, developer.Email, developer.Password, developer.IsAdmin);
                Logger.LogText(String.Format("User {0} created new developer in database: {1}", Username, developer), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = true;
                response.Message = String.Format("Created new developer in database: {0}", developer.ToString());                
                SendMessage(response, sslStream);                                
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                Logger.LogText(String.Format("User {0} tried to create a new developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during creation of new developer";
                SendMessage(response, sslStream);       
            }
        }

        /// <summary>
        /// Handles UpdateDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to update an existing developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updateDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateDeveloperMessage(UpdateDeveloperMessage updateDeveloperMessage, SslStream sslStream)
        {
            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                response.Message = "Unauthorized to create new developers! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to update developer={1} from IP={2}", Username, updateDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, update of existing in database is started
            try
            {
                Developer developer = updateDeveloperMessage.Developer;
                Database.UpdateDeveloper(developer.Username, developer.Firstname, developer.Lastname, developer.Email, developer.IsAdmin);
                Logger.LogText(String.Format("User {0} updated existing developer in database: {1}", Username, developer), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = true;
                response.Message = String.Format("Updated developer in database: {0}", developer.ToString());
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                Logger.LogText(String.Format("User {0} tried to update an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during update of existing developer";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeleteDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to delete an existing developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deleteDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDeleteDeveloperMessage(DeleteDeveloperMessage deleteDeveloperMessage, SslStream sslStream)
        {
            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                response.Message = "Unauthorized to delete developers! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to delete developer={1} from IP={2}", Username, deleteDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, deletion of existing user in database is started
            try
            {
                Developer developer = deleteDeveloperMessage.Developer;
                Database.DeleteDeveloper(developer.Username);
                Logger.LogText(String.Format("User {0} deleted existing developer in database: {1}", Username, developer), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = true;
                response.Message = String.Format("Deleted developer in database: {0}", developer.ToString());
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                Logger.LogText(String.Format("User {0} tried to delete an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during deletion of existing developer";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to get an existing developer from the database        
        /// Then, it sends a response message which contains it and if it succeeded or failed
        /// </summary>
        /// <param name="requestDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDeveloperMessage(RequestDeveloperMessage requestDeveloperMessage, SslStream sslStream)
        {
            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage();
                response.ModifiedDeveloper = false;
                response.Message = "Unauthorized to get developers! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to request developer={1} from IP={2}", Username, requestDeveloperMessage.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, requesting existing user from database
            try
            {
                Developer developer = Database.GetDeveloper(requestDeveloperMessage.Username);

                if (developer == null)
                {
                    Logger.LogText(String.Format("User {0} requested a non existing developer from database: {1}", Username, requestDeveloperMessage.Username), this, Logtype.Info);
                    ResponseDeveloperMessage response = new ResponseDeveloperMessage();
                    response.Message = String.Format("Developer does not exist: {0}", requestDeveloperMessage.Username);
                    response.DeveloperExists = false;
                    SendMessage(response, sslStream);
                }
                else
                {
                    Logger.LogText(String.Format("User {0} requested an existing developer from database: {1}", Username, developer), this, Logtype.Info);
                    ResponseDeveloperMessage response = new ResponseDeveloperMessage();
                    response.Message = String.Format("Return developer: {0}", developer.ToString());
                    response.DeveloperExists = true;
                    response.Developer = developer;
                    SendMessage(response, sslStream);
                }
            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperMessage response = new ResponseDeveloperMessage();
                response.DeveloperExists = false;
                Logger.LogText(String.Format("User {0} tried to get an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during request of existing developer:";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestDeveloperListMessages
        /// If the user is authenticated and he is admin, it tries to get a list of developers from the database        
        /// Then, it sends a response message which contains it and if it succeeded or failed
        /// </summary>
        /// <param name="requestDeveloperListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDeveloperListMessage(RequestDeveloperListMessage requestDeveloperListMessage, SslStream sslStream)
        {
            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperListMessage response = new ResponseDeveloperListMessage();
                response.AllowedToViewList = false;
                response.Message = "Unauthorized to get developer list! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to request developer list from IP={1}", Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, requesting existing user from database
            try
            {
                List<Developer> developerList = Database.GetDevelopers();

                Logger.LogText(String.Format("User {0} requested a developer list", Username), this, Logtype.Info);
                    ResponseDeveloperListMessage response = new ResponseDeveloperListMessage();
                    response.Message = String.Format("Return developer list");
                    response.AllowedToViewList = true;
                    response.DeveloperList = developerList;
                    SendMessage(response, sslStream);

            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperListMessage response = new ResponseDeveloperListMessage();
                response.AllowedToViewList = false;
                Logger.LogText(String.Format("User {0} tried to get a developer list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during request of developer list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewPluginMessages
        /// If the user is authenticated, it tries to create a new plugin in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewPluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewPluginMessage(CreateNewPluginMessage createNewPluginMessage, SslStream sslStream)
        {
            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                response.Message = "Unauthorized to create new plugins! Please authenticate!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to create new plugin={1} from IP={2}", Username, createNewPluginMessage.Plugin.Name, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated; thus, creation of new plugin in database is started
            try
            {
                Plugin plugin = createNewPluginMessage.Plugin;
                Database.CreatePlugin(Username, plugin.Name, plugin.ShortDescription, plugin.LongDescription, plugin.Authornames, plugin.Authoremails, plugin.Authorinstitutes, plugin.Icon);
                Logger.LogText(String.Format("User {0} created new plugin in database: {1}", Username, plugin), this, Logtype.Info);
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = true;
                response.Message = String.Format("Created new plugin in database: {0}", plugin.ToString());
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                Logger.LogText(String.Format("User {0} tried to create a new plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during creation of new plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdatePluginMessages
        /// If the user is authenticated, it tries to update an existing plugin in the database
        /// Users can only update their plugins; admins can update all plugins
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updatePluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdatePluginMessage(UpdatePluginMessage updatePluginMessage, SslStream sslStream)
        {
            //Only authenticated users are allowed to update plugins
            if (!ClientIsAuthenticated)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                response.Message = "Unauthorized to create new developers! Please authenticate as admin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to update plugin={1} from IP={2}", Username, updatePluginMessage.Plugin, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }            
            //check, if plugin to update exist
            Plugin plugin = Database.GetPlugin(updatePluginMessage.Plugin.Id);
            if (plugin == null)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                response.Message = "Unauthorized to update that plugin!"; // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                Logger.LogText(String.Format("User {0} tried to update non-existing plugin={1} from IP={2}", Username, updatePluginMessage.Plugin, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own plugins
            if (ClientIsAdmin == false && plugin.Username != Username)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                response.Message = "Unauthorized to update that plugin!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to update plugin={1} from IP={2}", Username, updatePluginMessage.Plugin, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing plugin in database is started
            try
            {
                plugin = updatePluginMessage.Plugin;
                Database.UpdatePlugin(plugin.Id, Username, plugin.Name, plugin.ShortDescription, plugin.LongDescription, plugin.Authornames, plugin.Authoremails, plugin.Authorinstitutes, plugin.Icon);
                Logger.LogText(String.Format("User {0} updated existing plugin in database: {1}", Username, plugin.ToString()), this, Logtype.Info);
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = true;
                response.Message = String.Format("Updated plugin in database: {0}", plugin.ToString());
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage();
                response.ModifiedPlugin = false;
                Logger.LogText(String.Format("User {0} tried to update an existing plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during update of existing plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPluginMessage
        /// Returns the plugin if it exists in the database
        /// Everyone is able to get plugins
        /// </summary>
        /// <param name="requestPluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPluginMessage(RequestPluginMessage requestPluginMessage, SslStream sslStream)
        {
            try
            {
                Plugin plugin = Database.GetPlugin(requestPluginMessage.Id);
                if (plugin == null)
                {
                    ResponsePluginMessage response = new ResponsePluginMessage();
                    response.PluginExists = false;
                    Logger.LogText(String.Format("User {0} tried to get a non-existing plugin", Username), this, Logtype.Warning);
                    response.Message = String.Format("Plugin {0} does not exist", requestPluginMessage.Id);
                    SendMessage(response, sslStream);
                }
                else
                {
                    ResponsePluginMessage response = new ResponsePluginMessage();
                    response.Plugin = plugin;
                    response.PluginExists = true;
                    string message = String.Format("Responding with plugin: {0}", plugin.ToString());
                    Logger.LogText(message, this, Logtype.Info);
                    response.Message = message;
                    SendMessage(response, sslStream);
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePluginMessage response = new ResponsePluginMessage();
                Logger.LogText(String.Format("User {0} tried to get an existing plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during request of existing plugin.";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPluginListMessages
        /// responses with lists of plugins
        /// </summary>
        /// <param name="requestPluginListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPluginListMessage(RequestPluginListMessage requestPluginListMessage, SslStream sslStream)
        {
            Logger.LogText(String.Format("User {0} requested a list of plugins", Username), this, Logtype.Info);
            try
            {
                List<Plugin> plugins = Database.GetPlugins(requestPluginListMessage.Username.Equals("*") ? null : requestPluginListMessage.Username);
                ResponsePluginListMessage response = new ResponsePluginListMessage();
                response.Plugins = plugins;
                string message = String.Format("Responding with plugin list containing {0} elements", plugins.Count);
                Logger.LogText(message, this, Logtype.Info);
                response.Message = message;
                SendMessage(response, sslStream);                
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePluginMessage response = new ResponsePluginMessage();
                Logger.LogText(String.Format("User {0} tried to get a plugin list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during request of existing plugin.";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewSourceMessages
        /// checks, if corresponding plugin exist and is owned by the user. If yes, it creates the source
        /// </summary>
        /// <param name="createNewSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewSourceMessage(CreateNewSourceMessage createNewSourceMessage, SslStream sslStream)
        {
            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage();
                response.ModifiedSource = false;
                response.Message = "Unauthorized to create new plugins! Please authenticate!";
                Logger.LogText(String.Format("Unauthorized user {0} tried to create new source={1} from IP={2}", Username, createNewSourceMessage.Source, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //check, if plugin exists and is owned by the user
            Source source = createNewSourceMessage.Source;
            Plugin plugin = Database.GetPlugin(source.PluginId);

            //Plugin does not exist
            if (plugin == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage();
                response.ModifiedSource = false;
                response.Message = String.Format("Plugin with id={0} does not exist!", source.PluginId);
                Logger.LogText(String.Format("User {0} tried to create new source={1} from IP={2} for a non-existing plugin id={3}", Username, createNewSourceMessage.Source, IPAddress, source.PluginId), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Plugin is not owned by the user
            if (plugin.Username != Username)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage();
                response.ModifiedSource = false;
                response.Message = "Not authorized!";
                Logger.LogText(String.Format("User {0} tried to create new source={1} from IP={2} for a plugin (id={3}) that he does not own ", Username, createNewSourceMessage.Source, IPAddress, source.PluginId), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, everything is fine; thus, we try to create the source
            try
            {
                Database.CreateSource(source.PluginId, source.PluginVersion, source.ZipFile, DateTime.Now, BUILD_STATES.UPLOADED);
                Logger.LogText(String.Format("User {0} created new source for plugin={0} in database: {2}", Username, plugin, source), this, Logtype.Info);
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage();
                response.ModifiedSource = true;
                response.Message = String.Format("Created new source in database: {0}", source.ToString());
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage();
                response.ModifiedSource = false;
                Logger.LogText(String.Format("User {0} tried to create a new source. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                response.Message = "Exception during creation of new source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslStream"></param>
        private void SendMessage(Message message, SslStream sslStream)
        {
            lock (this)
            {
                byte[] messagebytes = message.Serialize();
                sslStream.Write(messagebytes);
                sslStream.Flush();
                Logger.LogText(String.Format("Sent a \"{0}\" to the client", message.ToString()), this, Logtype.Debug);
            }
        }
    }
}
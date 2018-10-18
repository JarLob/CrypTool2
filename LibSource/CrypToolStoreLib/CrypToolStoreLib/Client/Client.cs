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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using System.Net.Sockets;
using System.Net.Security;
using System.Net;
using System.IO;

namespace CrypToolStoreLib.Client
{
    public class CrypToolStoreClient
    {
        public const int DEFAULT_PORT = 15151;
        public const string DEFAULT_ADDRESS = "localhost";
        private const int READ_TIMEOUT = 5000;
        private const int WRITE_TIMEOUT = 5000;
        private const int FILE_BUFFER_SIZE = 1048576; // 1MB

        private Logger logger = Logger.GetLogger();        

        /// <summary>
        /// Encrypted stream between server and client
        /// </summary>
        private SslStream sslStream
        {
            get;
            set;
        }

        /// <summary>
        /// Responsible for the TCP connection
        /// </summary>
        private TcpClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// Port to connect to
        /// </summary>
        public int ServerPort
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true, if the client is connected to the server
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (sslStream != null && sslStream.CanRead && sslStream.CanWrite)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true, if the client is authenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true, if the user is administrator
        /// </summary>
        public bool IsAdmin
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets the server address
        /// </summary>
        public string ServerAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CrypToolStoreClient()
        {
            ServerPort = DEFAULT_PORT;
            ServerAddress = DEFAULT_ADDRESS;            
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect()
        {
            if (IsConnected)
            {
                return;
            }
            logger.LogText("Trying to connect to server", this, Logtype.Info);
            Client = new TcpClient(ServerAddress, ServerPort);
            sslStream = new SslStream(Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCert));
            sslStream.ReadTimeout = READ_TIMEOUT;
            sslStream.WriteTimeout = WRITE_TIMEOUT;
            sslStream.AuthenticateAsClient(ServerAddress);            
            logger.LogText("Connected to server", this, Logtype.Info);
        }

        private bool ValidateServerCert(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //todo: create server certificate validation
            return true;
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }
            else
            {
                LogoutMessage logout = new LogoutMessage();
                SendMessage(logout);
                sslStream.Close();
                Client.Close();
                logger.LogText("Disconnected from the server", this, Logtype.Info);
            }
        }

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslStream"></param>
        private void SendMessage(Message message)
        {
            byte[] messagebytes = message.Serialize();
            sslStream.Write(messagebytes);
            sslStream.Flush();
            logger.LogText(String.Format("Sent a \"{0}\" message to the server", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);            
        }


        /// <summary>
        /// Receive a message from the ssl stream
        /// if stream is closed, returns null
        /// </summary>
        /// <returns></returns>
        private Message ReceiveMessage()
        {
            if (!IsConnected)
            {
                return null;
            }
            //Step 1: Read message header                    
            byte[] headerbytes = new byte[21]; //a message header is 21 bytes
            int bytesread = 0;
            while (bytesread < 21)
            {
                int readbytes = sslStream.Read(headerbytes, bytesread, 21 - bytesread);
                if (readbytes == 0)
                {
                    //stream was closed
                    return null;
                }
                bytesread += readbytes;
            }

            //Step 2: Deserialize message header and get payloadsize
            MessageHeader header = new MessageHeader();
            header.Deserialize(headerbytes);
            int payloadsize = header.PayloadSize;

            //Step 3: Read complete message
            byte[] messagebytes = new byte[payloadsize + 21];
            Array.Copy(headerbytes, 0, messagebytes, 0, 21);

            while (bytesread < payloadsize + 21)
            {
                int readbytes = sslStream.Read(messagebytes, bytesread, payloadsize + 21 - bytesread);
                if (readbytes == 0)
                {
                    //stream was closed
                    return null;
                }
                bytesread += readbytes;
            }

            //Step 4: Deserialize Message
            Message message = Message.DeserializeMessage(messagebytes);
            logger.LogText(String.Format("Received a \"{0}\" message from the server", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

            return message;
        }

        #region methods to login/logout

        /// <summary>
        /// Try to log into CrypToolStore using given username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            if (!IsConnected)
            {
                return false;
            }
            lock (this)
            {
                logger.LogText(String.Format("Trying to login as {0}", username), this, Logtype.Info);

                //0. Step: Initially set everything to false
                IsAuthenticated = false;
                IsAdmin = false;

                //1. Step: Send LoginMessage to server
                LoginMessage message = new LoginMessage();
                message.Username = username;
                message.Password = password;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return false;
                }
                //Received ResponseLogin
                if (response_message.MessageHeader.MessageType == MessageType.ResponseLogin)
                {
                    ResponseLoginMessage responseLoginMessage = (ResponseLoginMessage)response_message;
                    if (responseLoginMessage.LoginOk == true)
                    {
                        logger.LogText(String.Format("Successfully logged in as {0}. Server response message was: {1}", username, responseLoginMessage.Message), this, Logtype.Info);
                        IsAuthenticated = true;
                        if (responseLoginMessage.IsAdmin)
                        {
                            logger.LogText(String.Format("User {0} is admin", username), this, Logtype.Info);
                            IsAdmin = true;
                        }
                        return true;
                    }
                    logger.LogText(String.Format("Could not log in as {0}. Server response message was: {1}", username, responseLoginMessage.Message), this, Logtype.Info);
                    IsAuthenticated = false;
                    return false;
                }

                //Received another (wrong) message
                logger.LogText(String.Format("Response message to login attempt was not a ResponseLoginMessage. It was {0}", response_message.MessageHeader.MessageType.ToString()), this, Logtype.Info);
                return false;
            }
        }

        #endregion

        #region Methods for working with developers

        /// <summary>
        /// Creates a new developer account in the database
        /// Only possible, when the user is authenticated as an admin
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult CreateDeveloper(Developer developer)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only admins are allowed, thus, we do not even send any creation messages
                //if we are not authenticated as admin
                if (!IsAdmin)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated as admin",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to create a new developer: {0}", developer.ToString()), this, Logtype.Info);

                //1. Step: Send CreateNewDeveloper to server
                CreateNewDeveloperMessage message = new CreateNewDeveloperMessage();
                message.Developer = developer;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseDeveloperModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseDeveloperModification)
                {
                    //received a response, forward it to user
                    ResponseDeveloperModificationMessage responseDeveloperModificationMessage = (ResponseDeveloperModificationMessage)response_message;
                    logger.LogText(String.Format("{0} a new developer. Return message was: {1}", responseDeveloperModificationMessage.ModifiedDeveloper == true ? "Successfully created" : "Did not create", responseDeveloperModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseDeveloperModificationMessage.Message,
                        Success = responseDeveloperModificationMessage.ModifiedDeveloper
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to create new developer was not a ResponseDeveloperModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Updates an existing developer account in the database
        /// Only possible, when the user is authenticated as an admin
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult UpdateDeveloper(Developer developer)
        {
            lock (this)
            {
                //we can only update users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }               

                logger.LogText(String.Format("Trying to update an existing developer: {0}", developer.ToString()), this, Logtype.Info);

                //1. Step: Send UpdateDeveloperMessage to server
                UpdateDeveloperMessage message = new UpdateDeveloperMessage();
                message.Developer = developer;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseDeveloperModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseDeveloperModification)
                {
                    //received a response, forward it to user
                    ResponseDeveloperModificationMessage responseDeveloperModificationMessage = (ResponseDeveloperModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing developer. Return message was: {1}", responseDeveloperModificationMessage.ModifiedDeveloper == true ? "Successfully updated" : "Did not update", responseDeveloperModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseDeveloperModificationMessage.Message,
                        Success = responseDeveloperModificationMessage.ModifiedDeveloper
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to update an existing developer was not a ResponseDeveloperModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Deletes an existing developer account in the database
        /// Only possible, when the user is authenticated as an admin
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DeleteDeveloper(string username)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only admins are allowed, thus, we do not even send any delete messages
                //if we are not authenticated as admin
                if (!IsAdmin)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated as admin",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to delete an existing developer: {0}", username), this, Logtype.Info);

                //1. Step: Send DeleteDeveloperMessage to server
                DeleteDeveloperMessage message = new DeleteDeveloperMessage();
                message.Developer = new Developer() { Username = username };
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseDeveloperModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseDeveloperModification)
                {
                    //received a response, forward it to user
                    ResponseDeveloperModificationMessage responseDeveloperModificationMessage = (ResponseDeveloperModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing developer. Return message was: {1}", responseDeveloperModificationMessage.ModifiedDeveloper == true ? "Successfully deleted" : "Did not delete", responseDeveloperModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseDeveloperModificationMessage.Message,
                        Success = responseDeveloperModificationMessage.ModifiedDeveloper
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to delete an existing developer was not a ResponseDeveloperModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests an existing developer from the database
        /// Only possible, when the user is authenticated as an admin
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetDeveloper(string username)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }                

                logger.LogText(String.Format("Trying to get an existing developer: {0}", username), this, Logtype.Info);

                //1. Step: Send UpdateDeveloper to server
                RequestDeveloperMessage message = new RequestDeveloperMessage();
                message.Username = username;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseDeveloperMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseDeveloper)
                {
                    //received a response, forward it to user
                    ResponseDeveloperMessage responseDeveloperModificationMessage = (ResponseDeveloperMessage)response_message;
                    logger.LogText(String.Format("{0} an existing developer. Return message was: {1}", responseDeveloperModificationMessage.DeveloperExists == true ? "Successfully received" : "Did not receive", responseDeveloperModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseDeveloperModificationMessage.Message,
                        Success = responseDeveloperModificationMessage.DeveloperExists,
                        DataObject = responseDeveloperModificationMessage.Developer
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request an existing developer was not a ResponseDeveloperMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests the list of developers
        /// Only possible, when the user is authenticated as an admin
        /// </summary>
        /// <returns></returns>
        public DataModificationOrRequestResult GetDeveloperList()
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }
                //only admins are allowed, thus, we do not even send any request messages
                //if we are not authenticated as admin
                if (!IsAdmin)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated as admin",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }

                logger.LogText("Trying to get the list of developers", this, Logtype.Info);

                //1. Step: Send UpdateDeveloper to server
                RequestDeveloperListMessage message = new RequestDeveloperListMessage();
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }
                //Received ResponseDeveloperListMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseDeveloperList)
                {
                    //received a response, forward it to user
                    ResponseDeveloperListMessage responseDeveloperListMessage = (ResponseDeveloperListMessage)response_message;
                    logger.LogText(String.Format("Received developer list. Message was: {0}", responseDeveloperListMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseDeveloperListMessage.Message,
                        Success = responseDeveloperListMessage.AllowedToViewList,
                        DataObject = responseDeveloperListMessage.DeveloperList
                    };
                }
                //Received another (wrong) message
                string msg = String.Format("Response message to request a developer list was not a ResponseDeveloperList. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false,
                    DataObject = new List<Developer>()
                };
            }
        }

        #endregion

        #region Methods for working with plugins

        /// <summary>
        /// Creates a new plugin in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult CreatePlugin(Plugin plugin)
        {
            lock (this)
            {
                //we can only create plugins, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed to create plugins
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }

                logger.LogText(String.Format("Trying to create a new plugin: {0}", plugin.ToString()), this, Logtype.Info);

                //1. Step: Send CreateNewPlugin to server
                CreateNewPluginMessage message = new CreateNewPluginMessage();
                message.Plugin = plugin;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponsePluginModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponsePluginModification)
                {
                    //received a response, forward it to user
                    ResponsePluginModificationMessage responsePluginModificationMessage = (ResponsePluginModificationMessage)response_message;
                    logger.LogText(String.Format("{0} a new plugin. Return message was: {1}", responsePluginModificationMessage.ModifiedPlugin == true ? "Successfully created" : "Did not create", responsePluginModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginModificationMessage.Message,
                        Success = responsePluginModificationMessage.ModifiedPlugin
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to create new plugin was not a ResponsePluginModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Updates an existing plugin in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult UpdatePlugin(Plugin plugin)
        {
            lock (this)
            {
                //we can update plugins, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any update messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to update an existing plugin: {0}", plugin.ToString()), this, Logtype.Info);

                //1. Step: Send UpdatePluginMessage to server
                UpdatePluginMessage message = new UpdatePluginMessage();
                message.Plugin = plugin;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponsePluginModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponsePluginModification)
                {
                    //received a response, forward it to user
                    ResponsePluginModificationMessage responsePluginModificationMessage = (ResponsePluginModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing plugin. Return message was: {1}", responsePluginModificationMessage.ModifiedPlugin == true ? "Successfully updated" : "Did not update", responsePluginModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginModificationMessage.Message,
                        Success = responsePluginModificationMessage.ModifiedPlugin
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to update an existing plugin was not a ResponsePluginModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Deletes an existing plugin in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DeletePlugin(int pluginId)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any delete messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to delete an existing plugin: {0}", pluginId), this, Logtype.Info);

                //1. Step: Send DeletePluginMessage to server
                DeletePluginMessage message = new DeletePluginMessage();
                message.Plugin = new Plugin() { Id = pluginId };
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponsePluginModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponsePluginModification)
                {
                    //received a response, forward it to user
                    ResponsePluginModificationMessage responsePluginModificationMessage = (ResponsePluginModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing plugin. Return message was: {1}", responsePluginModificationMessage.ModifiedPlugin == true ? "Successfully deleted" : "Did not delete", responsePluginModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginModificationMessage.Message,
                        Success = responsePluginModificationMessage.ModifiedPlugin
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to delete an existing plugin was not a ResponsePluginModification. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests an existing plugin from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetPlugin(int pluginId)
        {
            lock (this)
            {
                //we can only receive plugins when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get an existing plugin: {0}", pluginId), this, Logtype.Info);

                //1. Step: Send RequestPluginMessage to server
                RequestPluginMessage message = new RequestPluginMessage();
                message.Id = pluginId;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponsePlugin
                if (response_message.MessageHeader.MessageType == MessageType.ResponsePlugin)
                {
                    //received a response, forward it to user
                    ResponsePluginMessage responsePluginModificationMessage = (ResponsePluginMessage)response_message;
                    logger.LogText(String.Format("{0} an existing plugin. Return message was: {1}", responsePluginModificationMessage.PluginExists == true ? "Successfully received" : "Did not receive", responsePluginModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginModificationMessage.Message,
                        Success = responsePluginModificationMessage.PluginExists,
                        DataObject = responsePluginModificationMessage.Plugin
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request an existing plugin was not a ResponsePluginMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests a list of plugins from the server
        /// if username is *, it requests a list of all plugins; otherwise it returns all plugins of the given user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetPluginList(string username = "*")
        {
            lock (this)
            {
                //we can only receive plugin lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get plugins of user: {0}", username), this, Logtype.Info);

                //1. Step: Send RequestPluginListMessage to server
                RequestPluginListMessage message = new RequestPluginListMessage();
                message.Username = username;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponsePluginListMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponsePluginList)
                {
                    //received a response, forward it to user
                    ResponsePluginListMessage responsePluginListMessage = (ResponsePluginListMessage)response_message;
                    logger.LogText(String.Format("Received a plugin list. Message was {0}", responsePluginListMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginListMessage.Message,
                        DataObject = responsePluginListMessage.Plugins
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request a plugin list was not a ResponsePluginListMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        #endregion

        #region Methods for working with sources

        /// <summary>
        /// Creates a new source in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult CreateSource(Source source)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed to create plugins
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }

                logger.LogText(String.Format("Trying to create a new source: {0}", source.ToString()), this, Logtype.Info);

                //1. Step: Send CreateNewSourceMessage to server
                CreateNewSourceMessage message = new CreateNewSourceMessage();
                message.Source = source;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseSourceModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseSourceModification)
                {
                    //received a response, forward it to user
                    ResponseSourceModificationMessage responseSourceModificationMessage = (ResponseSourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} a new resource. Return message was: {1}", responseSourceModificationMessage.ModifiedSource == true ? "Successfully created" : "Did not create", responseSourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseSourceModificationMessage.Message,
                        Success = responseSourceModificationMessage.ModifiedSource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to create new plugin was not a ResponseSourceModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Updates an existing source account in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult UpdateSource(Source source)
        {
            lock (this)
            {
                //we can update plugins, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any update messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to update an existing source: {0}", source.ToString()), this, Logtype.Info);

                //1. Step: Send UpdatePluginMessage to server
                UpdateSourceMessage message = new UpdateSourceMessage();
                message.Source = source;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseSourceModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseSourceModification)
                {
                    //received a response, forward it to user
                    ResponseSourceModificationMessage responseSourceModificationMessage = (ResponseSourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing source. Return message was: {1}", responseSourceModificationMessage.ModifiedSource == true ? "Successfully updated" : "Did not update", responseSourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseSourceModificationMessage.Message,
                        Success = responseSourceModificationMessage.ModifiedSource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to update an existing source was not a ResponseSourceModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Deletes an existing source in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="pluginversion"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DeleteSource(int pluginid, int pluginversion)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any delete messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to delete an existing source: pluginid={0}, pluginversion={1}", pluginid, pluginversion), this, Logtype.Info);

                //1. Step: Send DeletePluginMessage to server
                DeleteSourceMessage message = new DeleteSourceMessage();
                message.Source = new Source() { PluginId = pluginid, PluginVersion = pluginversion };
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseSourceModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseSourceModification)
                {
                    //received a response, forward it to user
                    ResponseSourceModificationMessage responseSourceModificationMessage = (ResponseSourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing source. Return message was: {1}", responseSourceModificationMessage.ModifiedSource == true ? "Successfully deleted" : "Did not delete", responseSourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseSourceModificationMessage.Message,
                        Success = responseSourceModificationMessage.ModifiedSource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to delete an existing source was not a ResponseSourceModification. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests an existing source from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetSource(int pluginId, int pluginversion)
        {
            lock (this)
            {
                //we can only receive plugins when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get an existing source: {0} {1}", pluginId, pluginversion), this, Logtype.Info);

                //1. Step: Send RequestSourceMessage to server
                RequestSourceMessage message = new RequestSourceMessage();
                message.PluginId = pluginId;
                message.PluginVersion = pluginversion;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseSourceMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseSource)
                {
                    //received a response, forward it to user
                    ResponseSourceMessage responseSourceMessage = (ResponseSourceMessage)response_message;
                    logger.LogText(String.Format("{0} an existing source. Return message was: {1}", responseSourceMessage.SourceExists == true ? "Successfully received" : "Did not receive", responseSourceMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseSourceMessage.Message,
                        Success = responseSourceMessage.SourceExists,
                        DataObject = responseSourceMessage.Source
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request an existing plugin was not a ResponseSourceMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests a list of sources from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetSourceList(int pluginid)
        {
            lock (this)
            {
                //we can only receive source lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get sources of plugin: {0}", pluginid), this, Logtype.Info);

                //1. Step: Send RequestPluginListMessage to server
                RequestSourceListMessage message = new RequestSourceListMessage();
                message.PluginId = pluginid;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseSourceListMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseSourceList)
                {
                    //received a response, forward it to user
                    ResponseSourceListMessage responseSourceListMessage = (ResponseSourceListMessage)response_message;
                    logger.LogText(String.Format("Received a source list. Message was {0}", responseSourceListMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseSourceListMessage.Message,
                        DataObject = responseSourceListMessage.SourceList,
                        Success = responseSourceListMessage.AllowedToViewList
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request a plugin list was not a ResponsePluginList. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Uploads a zip file for the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        /// <param name="stop"></param>
        public DataModificationOrRequestResult UploadZipFile(Source source, string filename, ref bool stop)
        {
            lock (this)
            {
                //we can only receive source lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                //Step 1: Send startUploadZipfileMessage to start the uploading process
                FileInfo fileInfo = new FileInfo(filename);
                long filesize = fileInfo.Length;

                StartUploadZipfileMessage startUploadZipfileMessage = new StartUploadZipfileMessage();
                startUploadZipfileMessage.Source = source;
                startUploadZipfileMessage.FileSize = filesize;
                SendMessage(startUploadZipfileMessage);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }

                //Received ResponseUploadDownloadDataMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseUploadDownloadData)
                {
                    //received a response, forward it to user
                    ResponseUploadDownloadDataMessage responseUploadDownloadDataMessage = (ResponseUploadDownloadDataMessage)response_message;

                    if (responseUploadDownloadDataMessage.Success == false)
                    {
                        string failmsg = String.Format("Upload failed, reason: {0}", responseUploadDownloadDataMessage.Message);
                        logger.LogText(failmsg, this, Logtype.Info);
                        return new DataModificationOrRequestResult()
                        {
                            Message = failmsg,
                            Success = false
                        };
                    }

                    //Step 3: send file
                    long totalbytesread = 0;
                    long lasttotalbytesread = 0;
                    DateTime LastEventFireTime = DateTime.Now;

                    using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[FILE_BUFFER_SIZE];                        
                        while (totalbytesread < filesize)
                        {
                            if (stop)
                            {
                                //user wants to stop the upload, thus, we notify the server
                                StopUploadDownloadMessage stopUploadDownloadMessage = new StopUploadDownloadMessage();
                                SendMessage(stopUploadDownloadMessage);
                                //return USERSTOP, thus, the ui wont update itself
                                return new DataModificationOrRequestResult()
                                {
                                    Success = false,
                                    DataObject = null,
                                    Message = "USERSTOP"
                                };
                            }

                            //read a block of data
                            int bytesread = 0;
                            int current_bytesread = 0;                                                        
                            
                            while ((current_bytesread = fileStream.Read(buffer, bytesread, FILE_BUFFER_SIZE - bytesread)) > 0 && bytesread < FILE_BUFFER_SIZE)
                            {
                                bytesread += current_bytesread;
                                totalbytesread += current_bytesread;
                            }

                            byte[] data;
                            if (bytesread < FILE_BUFFER_SIZE)
                            {
                                data = new byte[bytesread];
                                Array.Copy(buffer, 0, data, 0, bytesread);
                            }
                            else
                            {
                                data = buffer;
                            }

                            //send the block of data

                            UploadDownloadDataMessage uploadDownloadDataMessage = new UploadDownloadDataMessage();
                            uploadDownloadDataMessage.Data = data;
                            uploadDownloadDataMessage.Offset = totalbytesread;
                            uploadDownloadDataMessage.FileSize = filesize;

                            SendMessage(uploadDownloadDataMessage);

                            //check, if block of data was received without error
                            response_message = ReceiveMessage();

                            //Received null = connection closed
                            if (response_message == null)
                            {
                                logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                                sslStream.Close();
                                Client.Close();
                                return new DataModificationOrRequestResult()
                                {
                                    Message = "Connection to server lost",
                                    Success = false
                                };
                            }                            

                            //Received ResponseUploadDownloadDataMessage
                            if (response_message.MessageHeader.MessageType == MessageType.ResponseUploadDownloadData)
                            {
                                responseUploadDownloadDataMessage = (ResponseUploadDownloadDataMessage)response_message;
                                if (responseUploadDownloadDataMessage.Success == false)
                                {
                                    string failmsg = String.Format("Upload failed, reason: {0}", responseUploadDownloadDataMessage.Message);
                                    logger.LogText(failmsg, this, Logtype.Info);
                                    return new DataModificationOrRequestResult()
                                    {
                                        Message = failmsg,
                                        Success = false
                                    };
                                }

                                //every second fire event for upload progress
                                if (UploadDownloadProgressChanged != null && DateTime.Now > LastEventFireTime.AddMilliseconds(1000))
                                {
                                    UploadDownloadProgressEventArgs args = new UploadDownloadProgressEventArgs();
                                    args.FileName = filename;
                                    args.FileSize = filesize;
                                    args.DownloadedUploaded = totalbytesread;
                                    TimeSpan duration = DateTime.Now - LastEventFireTime;
                                    args.BytePerSecond = (long)((((double)totalbytesread - (double)lasttotalbytesread) / duration.TotalMilliseconds) * 1000.0);
                                    lasttotalbytesread = totalbytesread;
                                    UploadDownloadProgressChanged.Invoke(this, args);
                                    LastEventFireTime = DateTime.Now;
                                }
                            }
                            else
                            {
                                //Received another (wrong) message
                                string msg = String.Format("Response message to upload a zipfile was not a ResponseUploadDownloadDataMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                                logger.LogText(msg, this, Logtype.Info);
                                return new DataModificationOrRequestResult()
                                {
                                    Message = msg,
                                    Success = false
                                };
                            }
                        }
                    }

                    //fire last event when file is completely uploaded
                    if (UploadDownloadProgressChanged != null)
                    {
                        UploadDownloadProgressEventArgs args = new UploadDownloadProgressEventArgs();
                        args.FileName = filename;
                        args.FileSize = filesize;
                        args.DownloadedUploaded = totalbytesread;
                        args.BytePerSecond = 0;
                        UploadDownloadProgressChanged.Invoke(this, args);
                    }

                    //Received another (wrong) message                    
                    logger.LogText("Upload completed", this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Upload completed",
                        Success = true
                    };
                }
                else
                {

                    //Received another (wrong) message
                    string msg = String.Format("Response message to upload a zipfile was not a ResponseUploadDownloadDataMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                    logger.LogText(msg, this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = msg,
                        Success = false
                    };
                }
            }           
        }

        /// <summary>
        /// Dowloads a zip file of the specified source and stores it in the specified file
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filename"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DownloadZipFile(Source source, string filename, ref bool stop)
        {
            lock (this)
            {
                //we can only receive source lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                //Step 1: Send requestDownloadZipfileMessage to start the uploading process      
                RequestDownloadZipfileMessage requestDownloadZipfileMessage = new RequestDownloadZipfileMessage();
                requestDownloadZipfileMessage.Source = source;
                SendMessage(requestDownloadZipfileMessage);
             
                long writtenData = 0;
                long lastWrittenData = 0;
                DateTime LastEventFireTime = DateTime.Now;

                //Step 2: Download file
                using (FileStream fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
                {
                    while (true)
                    {
                        //Receive message from stream
                        Message responseMessage = ReceiveMessage();

                        if (stop)
                        {
                            //user wants to stop the upload, thus, we notify the server
                            StopUploadDownloadMessage stopUploadDownloadMessage = new StopUploadDownloadMessage();
                            SendMessage(stopUploadDownloadMessage);
                            //return USERSTOP, thus, the ui wont update itself
                            return new DataModificationOrRequestResult()
                            {
                                Success = false,
                                DataObject = null,
                                Message = "USERSTOP"
                            };
                        }

                        //receive data message
                        if (responseMessage.MessageHeader.MessageType == MessageType.UploadDownloadData)
                        {
                            UploadDownloadDataMessage uploadDownloadDataMessage = (UploadDownloadDataMessage)responseMessage;
                            fileStream.Write(uploadDownloadDataMessage.Data, 0, uploadDownloadDataMessage.Data.Length);
                            writtenData += uploadDownloadDataMessage.Data.Length;

                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            response.Success = true;
                            response.Message = "OK";
                            SendMessage(response);
                            if (writtenData == uploadDownloadDataMessage.FileSize)
                            {
                                // download completed
                                if (UploadDownloadProgressChanged != null)
                                {
                                    UploadDownloadProgressEventArgs args = new UploadDownloadProgressEventArgs();
                                    args.FileName = filename;
                                    args.FileSize = uploadDownloadDataMessage.FileSize;
                                    args.DownloadedUploaded = writtenData;
                                    args.BytePerSecond = 0;
                                    UploadDownloadProgressChanged.Invoke(this, args);                                }
                                
                                return new DataModificationOrRequestResult()
                                {
                                    Message = "Download completed",
                                    Success = true
                                };
                            }

                            //every second fire event for upload progress
                            if (UploadDownloadProgressChanged != null && DateTime.Now > LastEventFireTime.AddMilliseconds(1000))
                            {
                                UploadDownloadProgressEventArgs args = new UploadDownloadProgressEventArgs();
                                args.FileName = filename;
                                args.FileSize = uploadDownloadDataMessage.FileSize;
                                args.DownloadedUploaded = writtenData;
                                TimeSpan duration = DateTime.Now - LastEventFireTime;
                                args.BytePerSecond = (long)((((double)writtenData - (double)lastWrittenData) / duration.TotalMilliseconds) * 1000.0);
                                lastWrittenData = writtenData;
                                UploadDownloadProgressChanged.Invoke(this, args);
                                LastEventFireTime = DateTime.Now;
                            }

                        }                       
                        //we receive something wrong...
                        else
                        {
                            string msg = String.Format("Response message to download a zipfile was not a UploadDownloadData. It was {0}", responseMessage.MessageHeader.MessageType.ToString());
                            logger.LogText(msg, this, Logtype.Info);
                            return new DataModificationOrRequestResult()
                            {
                                Message = msg,
                                Success = false
                            };
                        }
                    }
                }
            }
        }

        public event EventHandler<UploadDownloadProgressEventArgs> UploadDownloadProgressChanged;

        #endregion

        #region Methods for working with Resources

        /// <summary>
        /// Creates a new resource in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult CreateResource(Resource resource)
        {
            lock (this)
            {
                //we can only create resources, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed to create resources
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }

                logger.LogText(String.Format("Trying to create a new plugin: {0}", resource.ToString()), this, Logtype.Info);

                //1. Step: Send CreateNewResourceMessage to server
                CreateNewResourceMessage message = new CreateNewResourceMessage();
                message.Resource = resource;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceModification)
                {
                    //received a response, forward it to user
                    ResponseResourceModificationMessage responseResourceModificationMessage = (ResponseResourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} a new resource. Return message was: {1}", responseResourceModificationMessage.ModifiedResource == true ? "Successfully created" : "Did not create", responseResourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceModificationMessage.Message,
                        Success = responseResourceModificationMessage.ModifiedResource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to create new plugin was not a ResponseResourceModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Updates an existing resource in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult UpdateResource(Resource resource)
        {
            lock (this)
            {
                //we can update resources, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any update messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to update an existing resource: {0}", resource.ToString()), this, Logtype.Info);

                //1. Step: Send UpdatePluginMessage to server
                UpdateResourceMessage message = new UpdateResourceMessage();
                message.Resource = resource;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceModification)
                {
                    //received a response, forward it to user
                    ResponseResourceModificationMessage responsePluginModificationMessage = (ResponseResourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource. Return message was: {1}", responsePluginModificationMessage.ModifiedResource == true ? "Successfully updated" : "Did not update", responsePluginModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responsePluginModificationMessage.Message,
                        Success = responsePluginModificationMessage.ModifiedResource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to update an existing plugin was not a ResponseResourceModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Deletes an existing resource in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DeleteResource(int resourceId)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any delete messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to delete an existing resource: {0}", resourceId), this, Logtype.Info);

                //1. Step: Send DeleteResourceMessage to server
                DeleteResourceMessage message = new DeleteResourceMessage();
                message.Resource = new Resource() { Id = resourceId };
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceModification)
                {
                    //received a response, forward it to user
                    ResponseResourceModificationMessage responseResourceModificationMessage = (ResponseResourceModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource. Return message was: {1}", responseResourceModificationMessage.ModifiedResource == true ? "Successfully deleted" : "Did not delete", responseResourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceModificationMessage.Message,
                        Success = responseResourceModificationMessage.ModifiedResource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to delete an existing resource was not a ResponseResourceModification. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests an existing resource from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetResource(int resourceId)
        {
            lock (this)
            {
                //we can only receive resources when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get an existing resource: {0}", resourceId), this, Logtype.Info);

                //1. Step: Send RequestResourceMessage to server
                RequestResourceMessage message = new RequestResourceMessage();
                message.Id = resourceId;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResource
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResource)
                {
                    //received a response, forward it to user
                    ResponseResourceMessage responseResourceModificationMessage = (ResponseResourceMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource. Return message was: {1}", responseResourceModificationMessage.ResourceExists == true ? "Successfully received" : "Did not receive", responseResourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceModificationMessage.Message,
                        Success = responseResourceModificationMessage.ResourceExists,
                        DataObject = responseResourceModificationMessage.Resource
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request an existing resource was not a ResponseResourceMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests a list of resources from the server
        /// if username is *, it requests a list of all resources; otherwise it returns all resources of the given user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetResourceList(string username = "*")
        {
            lock (this)
            {
                //we can only receive resource lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get resources of user: {0}", username), this, Logtype.Info);

                //1. Step: Send RequestResourceListMessage to server
                RequestResourceListMessage message = new RequestResourceListMessage();
                message.Username = username;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceListMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceList)
                {
                    //received a response, forward it to user
                    ResponseResourceListMessage responseResourceListMessage = (ResponseResourceListMessage)response_message;
                    logger.LogText(String.Format("Received a resource list. Message was {0}", responseResourceListMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceListMessage.Message,
                        DataObject = responseResourceListMessage.Resources,
                        Success = true
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request a resource list was not a ResponseResourceListMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        #endregion

        #region Methods for working with ResourceDatas

        /// <summary>
        /// Creates a new resource adata in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult CreateResourceData(ResourceData resourceData)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed to create plugins
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false,
                        DataObject = new List<Developer>()
                    };
                }

                logger.LogText(String.Format("Trying to create a new resource data: {0}", resourceData.ToString()), this, Logtype.Info);

                //1. Step: Send CreateNewResourceDataMessage to server
                CreateNewResourceDataMessage message = new CreateNewResourceDataMessage();
                message.ResourceData = resourceData;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceDataModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceDataModification)
                {
                    //received a response, forward it to user
                    ResponseResourceDataModificationMessage responseResourceDataModificationMessage = (ResponseResourceDataModificationMessage)response_message;
                    logger.LogText(String.Format("{0} a new reresource data. Return message was: {1}", responseResourceDataModificationMessage.ModifiedResourceData == true ? "Successfully created" : "Did not create", responseResourceDataModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceDataModificationMessage.Message,
                        Success = responseResourceDataModificationMessage.ModifiedResourceData
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to create new plugin was not a ResponseResourceDataModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Updates an existing resource data in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="developer"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult UpdateResourceData(ResourceData resourceData)
        {
            lock (this)
            {
                //we can update plugins, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any update messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to update an existing resource data: {0}", resourceData.ToString()), this, Logtype.Info);

                //1. Step: Send UpdatePluginMessage to server
                UpdateResourceDataMessage message = new UpdateResourceDataMessage();
                message.ResourceData = resourceData;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceDataModification
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceDataModification)
                {
                    //received a response, forward it to user
                    ResponseResourceDataModificationMessage responseResourceModificationMessage = (ResponseResourceDataModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource data. Return message was: {1}", responseResourceModificationMessage.ModifiedResourceData == true ? "Successfully updated" : "Did not update", responseResourceModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceModificationMessage.Message,
                        Success = responseResourceModificationMessage.ModifiedResourceData
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to update an existing resource data was not a ResponseResourceDataModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Deletes an existing resource data in the database
        /// Only possible, when the user is authenticated
        /// </summary>
        /// <param name="resourceid"></param>
        /// <param name="resourceversion"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult DeleteResourceData(int resourceid, int resourceversion)
        {
            lock (this)
            {
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only authenticated users are allowed, thus, we do not even send any delete messages
                if (!IsAuthenticated)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to delete an existing resource data: resourceid={0}, version={1}", resourceid, resourceversion), this, Logtype.Info);

                //1. Step: Send DeleteResourceDataMessage to server
                DeleteResourceDataMessage message = new DeleteResourceDataMessage();
                message.ResourceData = new ResourceData() { ResourceId = resourceid, ResourceVersion = resourceversion };
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceDataModificationMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceDataModification)
                {
                    //received a response, forward it to user
                    ResponseResourceDataModificationMessage responseReresourceDataModificationMessage = (ResponseResourceDataModificationMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource data. Return message was: {1}", responseReresourceDataModificationMessage.ModifiedResourceData == true ? "Successfully deleted" : "Did not delete", responseReresourceDataModificationMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseReresourceDataModificationMessage.Message,
                        Success = responseReresourceDataModificationMessage.ModifiedResourceData
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to delete an existing resource data was not a ResponseResourceDataModificationMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests an existing resource data from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetResourceData(int resourceid, int resourceversion)
        {
            lock (this)
            {
                //we can only receive resources when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get an existing resource data: {0} {1}", resourceid, resourceversion), this, Logtype.Info);

                //1. Step: Send RequestReresourceDataMessage to server
                RequestResourceDataMessage message = new RequestResourceDataMessage();
                message.ResourceId = resourceid;
                message.ResourceVersion = resourceversion;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceDataMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceData)
                {
                    //received a response, forward it to user
                    ResponseResourceDataMessage responseReresourceDataMessage = (ResponseResourceDataMessage)response_message;
                    logger.LogText(String.Format("{0} an existing resource data. Return message was: {1}", responseReresourceDataMessage.ResourceDataExists == true ? "Successfully received" : "Did not receive", responseReresourceDataMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseReresourceDataMessage.Message,
                        Success = responseReresourceDataMessage.ResourceDataExists,
                        DataObject = responseReresourceDataMessage.ResourceData
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request an existing resource data was not a ResponseResourceDataMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        /// <summary>
        /// Requests a list of resource data from the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public DataModificationOrRequestResult GetResourceDataList(int resourceid)
        {
            lock (this)
            {
                //we can only receive resourceData lists when we are connected
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }

                logger.LogText(String.Format("Trying to get a list of resource data of resource: {0}", resourceid), this, Logtype.Info);

                //1. Step: Send RequestResourceDataListMessage to server
                RequestResourceDataListMessage message = new RequestResourceDataListMessage();
                message.ResourceId = resourceid;
                SendMessage(message);

                //2. Step: Receive response message from server
                var response_message = ReceiveMessage();

                //Received null = connection closed
                if (response_message == null)
                {
                    logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                    sslStream.Close();
                    Client.Close();
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Connection to server lost",
                        Success = false
                    };
                }
                //Received ResponseResourceDataListMessage
                if (response_message.MessageHeader.MessageType == MessageType.ResponseResourceDataList)
                {
                    //received a response, forward it to user
                    ResponseResourceDataListMessage responseResourceDataListMessage = (ResponseResourceDataListMessage)response_message;
                    logger.LogText(String.Format("Received a resource data list. Message was {0}", responseResourceDataListMessage.Message), this, Logtype.Info);
                    return new DataModificationOrRequestResult()
                    {
                        Message = responseResourceDataListMessage.Message,
                        DataObject = responseResourceDataListMessage.ResourceDataList
                    };
                }

                //Received another (wrong) message
                string msg = String.Format("Response message to request a resource data list was not a ResponseResourceDataListMessage. It was {0}", response_message.MessageHeader.MessageType.ToString());
                logger.LogText(msg, this, Logtype.Info);
                return new DataModificationOrRequestResult()
                {
                    Message = msg,
                    Success = false
                };
            }
        }

        #endregion
    }

    /// <summary>
    /// Arguments for the UploadDownload event
    /// Used by the UI to display upload/download speed, time, etc
    /// </summary>
    public class UploadDownloadProgressEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public long DownloadedUploaded { get; set; }
        public long FileSize { get; set; }
        public long BytePerSecond { get; set; }
    }
}
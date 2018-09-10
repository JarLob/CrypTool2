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

namespace CrypToolStoreLib.Client
{
    public class CrypToolStoreClient
    {
        public const int DEFAULT_PORT = 15151;
        public const string DEFAULT_ADDRESS = "localhost";

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
            lock (this)
            {
                byte[] messagebytes = message.Serialize();
                sslStream.Write(messagebytes);
                sslStream.Flush();
                logger.LogText(String.Format("Sent a \"{0}\" to the server", message.ToString()), this, Logtype.Debug);
            }
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
            logger.LogText(String.Format("Received a message of type {0}", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

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
                //we can only create users, when we are connected to the server
                if (!IsConnected)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not connected to server",
                        Success = false
                    };
                }
                //only admins are allowed, thus, we do not even send any update messages
                //if we are not authenticated as admin
                if (!IsAdmin)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated as admin",
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
                //only admins are allowed, thus, we do not even send any get messages
                //if we are not authenticated as admin
                if (!IsAdmin)
                {
                    return new DataModificationOrRequestResult()
                    {
                        Message = "Not authenticated as admin",
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

        public string UpdatePlugin()
        {
            return string.Empty;
        }

        public string DeletePlugin()
        {
            return string.Empty;
        }

        public Plugin GetPlugin()
        {
            return null;
        }

        public List<Plugin> GetPluginList()
        {
            return null;
        }

        #endregion

        #region Methods for working with sources

        public string CreateSource()
        {
            return string.Empty;
        }

        public string UpdateSource()
        {
            return string.Empty;
        }

        public string DeleteSource()
        {
            return string.Empty;
        }

        public Source GetSource()
        {
            return null;
        }

        public List<Source> GetSourceList()
        {
            return null;
        }

        #endregion

        #region Methods for working with Resources

        public string CreateResource()
        {
            return string.Empty;
        }

        public string UpdateResource()
        {
            return string.Empty;
        }

        public string DeleteResource()
        {
            return string.Empty;
        }

        public Resource GetResource()
        {
            return null;
        }

        public List<Resource> GetResourceList()
        {
            return null;
        }

        #endregion

        #region Methods for working with ResourceDatas

        public string CreateResourceData()
        {
            return string.Empty;
        }

        public string UpdateResourceData()
        {
            return string.Empty;
        }

        public string DeleteResourceData()
        {
            return string.Empty;
        }

        public ResourceData GetResourceData()
        {
            return null;
        }

        public List<ResourceData> GetResourceDataList()
        {
            return null;
        }

        #endregion
    }
}
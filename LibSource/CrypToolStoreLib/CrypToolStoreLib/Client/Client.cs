﻿/*
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

        private Logger Logger = Logger.GetLogger();

        private SslStream SSLStream;


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

        public bool IsConnected
        {
            get
            {
                if (SSLStream != null && SSLStream.CanRead && SSLStream.CanWrite)
                {
                    return true;
                }
                return false;
            }            
        }

        public string ServerAddress
        {
            get;
            set;
        }

        public CrypToolStoreClient()
        {
            ServerPort = DEFAULT_PORT;
            ServerAddress = DEFAULT_ADDRESS;
        }        

        public void Connect()
        {
            if (IsConnected)
            {
                return;
            }
            Logger.LogText("Trying to connect to server", this, Logtype.Info);
            Client = new TcpClient(ServerAddress, ServerPort);
            SSLStream = new SslStream(Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCert));                                    
            SSLStream.AuthenticateAsClient(ServerAddress);
            Logger.LogText("Connected to server", this, Logtype.Info);
        }

        private bool ValidateServerCert(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //todo: create server certificate validation
            return true;
        }

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
                SSLStream.Close();
                Client.Close();
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
                SSLStream.Write(messagebytes);
                SSLStream.Flush();
                Logger.LogText(String.Format("Sent a \"{0}\" to the server", message.ToString()), this, Logtype.Debug);
            }
        }



        #region methods for login and logout

        /// <summary>
        /// Try to log into CrypToolStore
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            LoginMessage message = new LoginMessage();
            message.Username = username;
            message.Password = password;
            SendMessage(message);
            var response_message = ReceiveMessage();
            if (response_message.MessageHeader.MessageType == MessageType.ResponseLogin)
            {
                ResponseLoginMessage responseLoginMessage = (ResponseLoginMessage)response_message;
                if (responseLoginMessage.LoginOk == true)
                {
                    Logger.LogText(String.Format("Successfully logged in as {0}. Server response message was: {1}", username, responseLoginMessage.Message), this, Logtype.Info);
                    return true;
                }
                Logger.LogText(String.Format("Could not log in as {0}. Server response message was: {1}", username, responseLoginMessage.Message), this, Logtype.Info);
                return false;
            }
            Logger.LogText(String.Format("Response message to login attempt was not a ResponseLoginMessage. It was {0}", response_message.MessageHeader.MessageType.ToString()), this, Logtype.Info);
            return false;
        }

        private Message ReceiveMessage()
        {
            //Step 1: Read message header                    
            byte[] headerbytes = new byte[21]; //a message header is 21 bytes
            int bytesread = 0;
            while (bytesread < 21)
            {
                bytesread += SSLStream.Read(headerbytes, bytesread, 21 - bytesread);
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
                bytesread += SSLStream.Read(messagebytes, bytesread, payloadsize + 21 - bytesread);
            }

            //Step 4: Deserialize Message
            Message message = Message.DeserializeMessage(messagebytes);
            Logger.LogText(String.Format("Received a message of type {0}", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

            return message;
        }



        #endregion



        #region Methods for working with developers

        public string CreateNewDeveloper()
        {

            return string.Empty;
        }

        public string UpdateDeveloper()
        {
            return string.Empty;
        }

        public string DeleteDeveloper()
        {
            return string.Empty;
        }

        public List<Developer> GetDeveloperList()
        {
            return null;
        }

        public Developer GetDeveloper()
        {
            return null;
        }

        #endregion

        #region Methods for working with plugins

        public string CreatePlugin()
        {
            return string.Empty;
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

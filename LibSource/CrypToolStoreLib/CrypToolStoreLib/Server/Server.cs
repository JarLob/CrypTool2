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

using CrypToolStoreLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Server
{
    public class CrypToolStoreServer
    {
        private Logger logger = Logger.GetLogger();

        private TcpListener Server
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        internal bool Running
        {
            get;
            set;
        }

        public CrypToolStoreServer()
        {
            Port = 15151;
        }        

        public void Start()
        {
            if (Running)
            {
                return;
            }
            logger.LogText("Starting listen thread", this, Logtype.Debug);
            Running = true;
            Thread listenThread = new Thread(ListenThread);
            listenThread.IsBackground = true;
            listenThread.Start();
            logger.LogText("Listen thread started", this, Logtype.Debug);
        }
        
        private void ListenThread()
        {
            Server = new TcpListener(IPAddress.Any, Port);
            Server.Start();

            while (Running)
            {
                try
                {
                    TcpClient client = Server.AcceptTcpClient();
                    logger.LogText(String.Format("New client connected: {0}", client.Client.RemoteEndPoint), this, Logtype.Info);
                    Thread handlerthread = new Thread(() =>
                    {
                        try
                        {
                            ServerHandler handler = new ServerHandler();
                            handler.HandleClient(client);
                        }
                        catch (Exception ex)
                        {
                            logger.LogText(String.Format("Exception during handling of client: {0}", ex.Message), this, Logtype.Error);
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
            logger.LogText("ListenThread terminated", this, Logtype.Debug);
        }      

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            logger.LogText("Stopping server", this, Logtype.Info);
            Running = false;
            Server.Stop();
            logger.LogText("Server stopped", this, Logtype.Info);
        }
    }

    public class ServerHandler
    {
        private Logger logger = Logger.GetLogger();

        public CrypToolStoreServer CrypToolStoreServer
        {
            get;
            set;
        }

        public ServerHandler()
        {

        }

        public void HandleClient(TcpClient client)
        {            
            using (SslStream sslstream = new SslStream(client.GetStream()))
            {
                while (CrypToolStoreServer.Running)
                {
                    //Step 1: Read message header

                    //a message header is 21 bytes
                    byte[] headerbytes = new byte[21];
                    int bytesread = 0;
                    while (bytesread < 21)
                    {
                        bytesread += sslstream.Read(headerbytes, bytesread, 21 - bytesread);
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
                        bytesread += sslstream.Read(messagebytes, bytesread, payloadsize + 21 - bytesread);
                    }

                    //Step 4: Deserialize Message
                    Message message = Message.DeserializeMessage(messagebytes);
                    logger.LogText(String.Format("Received a message of type {0}", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

                    //Step 5: Handle received message
                    HandleMessage(message, sslstream);
                }
            }
        }

        private void HandleMessage(Message message, SslStream sslstream)
        {
            
        }
    }


}

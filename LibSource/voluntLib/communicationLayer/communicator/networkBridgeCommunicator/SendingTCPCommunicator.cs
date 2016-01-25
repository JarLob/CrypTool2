// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using System.Security.Cryptography;

#endregion

namespace voluntLib.communicationLayer.communicator.networkBridgeCommunicator
{
    public class SendingTCPCommunicator : TCPCommunicator
    {
        
        #region private member

        private const int MAX_CONNECTION_TRIES = 5;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool hasStopped;
        private int unableToConnectCounter = 0;

        #endregion

        #region properties

        private TcpClient tcpClient;
        public IPAddress RemoteNetworkBridgeIP { get; private set; }
        public int RemoteNetworkBridgePort { get; private set; }
        public IPAddress LocalInterface { get; set; }
        protected ICommunicationLayer CommunicationLayer { get; set; }

        #endregion

        public SendingTCPCommunicator(string endPointIP, int endPointPort)
        {
            RemoteNetworkBridgeIP = IPAddress.Parse(endPointIP);
            RemoteNetworkBridgePort = endPointPort;
        }

        #region start stop

        public override void Start() {}
        public override void Stop() {}

        #endregion

        #region receiving

        protected void OnConnect(Byte[] data, IAsyncResult asyncResult)
        {
            if (!tcpClient.Connected) return; 
            
            using (var netStream = tcpClient.GetStream())
            {
                WriteToStream(data, netStream);
                
                //wait and read messages
                netStream.ReadTimeout = WaitForRemoteAnswerMS;
                InstanceBeginReadFromStream(netStream, RemoteNetworkBridgeIP);
            }
            Logger.Error("ended");
            CloseConnection(tcpClient);
        }

        /// <summary>
        ///   Begins to read messages from the stream.
        ///   This method waits busy for messages occurring on the networkStream.
        ///   It will return if:
        ///   - it cannot read from the stream.
        ///   - the connections is closed ( ether by sending an 0-sized message or forcely drop the connection)
        ///   - it times out.
        /// </summary>
        /// <param name="netStream">The net stream.</param>
        /// <param name="remoteIP">The remote IP.</param>
        protected void InstanceBeginReadFromStream(NetworkStream netStream, IPAddress remoteIP)
        {
            if (!netStream.CanRead)
            {
                Logger.Debug("Read from TCP stream failed, cant read from stream");
                return;
            }

            try
            {
                // read message length
                var emptyBytes = new byte[2];
                var bufferLength = new byte[2];
                netStream.Read(bufferLength, 0, 2);
                var messageLength = BitConverter.ToUInt16(bufferLength, 0);

                while (messageLength != 0 && netStream.CanRead)
                {
                    //repeat until all messages have been fetched
                    var buffer = readFromNetworkStream(netStream, messageLength);
                    try
                    { 
                        //handle data
                        comLayer.HandleIncomingMessages(buffer, remoteIP);
                    }
                    catch (Exception e)
                    {
                        Logger.Debug("Could not handle remotes message: " + e.Message + e.StackTrace);
                    }
                    // read next 2 bytes to see if another message is available
                    emptyBytes.CopyTo(bufferLength, 0);
                    netStream.Read(bufferLength, 0, 2);
                    messageLength = BitConverter.ToUInt16(bufferLength, 0); 
                }
            }
            catch (Exception e)
            {
                Logger.Debug("Read from TCP stream faild due: " + e.Message + e.StackTrace);
            }
        }
        #endregion


        #region send

        public override void ProcessMessage(AMessage data, IPAddress to)
        {
            if (unableToConnectCounter >= MAX_CONNECTION_TRIES) return;

            try
            {               
                tcpClient = new TcpClient();            
                tcpClient.BeginConnect(RemoteNetworkBridgeIP, RemoteNetworkBridgePort, ar => OnConnect(data.Serialize(), ar), tcpClient);
                unableToConnectCounter = 0;
           } 
           catch (Exception e)
           {
               Logger.Warn("VoluntLib could not process message from remote client {0}:{1} {2}",RemoteNetworkBridgeIP.ToString(), RemoteNetworkBridgePort, e.Message);

               unableToConnectCounter++;
               if (unableToConnectCounter >= MAX_CONNECTION_TRIES)
               {
                   Logger.Warn("VoluntLib could not connect to remote {0}:{1} more than {2} times. Igoring remote", RemoteNetworkBridgeIP.ToString(), RemoteNetworkBridgePort, MAX_CONNECTION_TRIES);
               }
           }
       }
       #endregion
    }
}
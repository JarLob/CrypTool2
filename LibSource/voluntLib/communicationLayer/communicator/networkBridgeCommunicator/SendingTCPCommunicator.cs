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

#endregion

namespace voluntLib.communicationLayer.communicator.networkBridgeCommunicator
{
    public class SendingTCPCommunicator : TCPCommunicator
    {
        #region private member

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool hasStopped;

        #endregion

        #region properties

        private readonly TcpClient tcpClient = new TcpClient();
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
            using (var netStream = tcpClient.GetStream())
            {
                WriteToStream(data, netStream);

                //wait and read messages
                netStream.ReadTimeout = WaitForRemoteAnswerMS;
                BeginReadFromStream(netStream, RemoteNetworkBridgeIP);
            }
            CloseConnection(tcpClient);
        }

        #endregion

        #region send

        public override void ProcessMessage(AMessage data, IPAddress to)
        {
            //reuse connection if connected
            if (tcpClient.Connected)
            {
                WriteToStream(data.Serialize(), tcpClient.GetStream());
                return;
            }
            // else start new connection
            tcpClient.BeginConnect(RemoteNetworkBridgeIP, RemoteNetworkBridgePort, ar => OnConnect(data.Serialize(), ar), tcpClient);
        }

        #endregion
    }
}
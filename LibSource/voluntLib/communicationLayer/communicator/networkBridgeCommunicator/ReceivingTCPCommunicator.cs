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

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.common.utils;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.communicator.networkBridgeCommunicator
{
    public class ReceivingTCPCommunicator : TCPCommunicator
    {
        #region private member

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool hasStopped;

        #endregion

        #region properties

        private readonly ConcurrentDictionary<IPAddress, TcpClient> activeClients = new ConcurrentDictionary<IPAddress, TcpClient>();
        private CancellationTokenSource cancelToken;
        private TcpListener listener;
        public int KeepRemoteClientActive { get; set; }
        public IPEndPoint ListeningEndPoint { get; set; }
        protected ICommunicationLayer CommunicationLayer { get; set; }
        public bool HasStarted { get; set; }

        #endregion

        public ReceivingTCPCommunicator(int listeningPort, IPAddress localEndPoint = null)
        {
            ListeningEndPoint = new IPEndPoint((localEndPoint ?? IPAddress.Any), listeningPort);
            KeepRemoteClientActive = 10000;
        }

        #region start stop

        public override void Start()
        {
            if (!HasStarted)
            {
                HasStarted = true;
                listener = new TcpListener(ListeningEndPoint);
                listener.Start(20); //TODO get to config of facade
                cancelToken = new CancellationTokenSource();
                var token = cancelToken.Token;

                // start acceptingTCP loop in new Task
                Task.Factory.StartNew(() =>
                {
                    while (!hasStopped)
                    {
                        token.ThrowIfCancellationRequested();
                        var client = listener.AcceptTcpClient();

                        //handle new client in new Task
                        Task.Factory.StartNew(() => OnReceive(client));
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        public override void Stop()
        {
            if (HasStarted)
            {
                hasStopped = true;
                cancelToken.Cancel();
                listener.Stop();
            }
        }

        #endregion

        #region receiving

        protected void OnReceive(TcpClient client)
        {
            var clientAddress = ((IPEndPoint) client.Client.RemoteEndPoint).Address;

            //test if there is an active connection
            if (!activeClients.TryAdd(clientAddress, client))
            {
                CloseConnection(client);
                return;
            }

            //remove client after a certain amount of time
            TaskHelper.Delay(KeepRemoteClientActive).ContinueWith(_ => CloseAndRemoveConnection(clientAddress));

            using (var netStream = client.GetStream())
            {
                netStream.ReadTimeout = 5000;
                BeginReadFromStream(netStream, clientAddress);
            }

            CloseAndRemoveConnection(clientAddress);
        }

        #endregion

        private void CloseAndRemoveConnection(IPAddress clientAddress)
        {
            TcpClient client;
            if (activeClients.TryRemove(clientAddress, out client))
            {
                CloseConnection(client);
            }
        }

        #region send

        public override void ProcessMessage(AMessage data, IPAddress to)
        {
            TcpClient client;
            if (activeClients.TryGetValue(to, out client))
            {
                WriteToStream(data.Serialize(), client.GetStream());
            }
        }

        #endregion
    }
}
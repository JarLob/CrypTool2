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
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.common.utils;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using System.Collections.Generic;

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
        private Dictionary<IPAddress, List<AMessage>> dataForClients = new Dictionary<IPAddress, List<AMessage>>();
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
                cancelToken = new CancellationTokenSource();
                var token = cancelToken.Token;
                listener = new TcpListener(ListeningEndPoint);
                listener.Start(20); //TODO get to config of facade   
                listener.BeginAcceptTcpClient(OnAcceptTcpClient, null);            
            }
        }

        private void OnAcceptTcpClient(IAsyncResult asyncResult)
        {
            TcpClient client = listener.EndAcceptTcpClient(asyncResult);
            listener.BeginAcceptTcpClient(OnAcceptTcpClient, null);
            OnReceive(client);
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
            try
            {
                activeClients.TryAdd(clientAddress, client);
                using (var netStream = client.GetStream())
                {
                    netStream.ReadTimeout = 5000;
                    BridgeBeginReadFromStream(netStream, clientAddress);
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Could not read from tcp stream {0}.", e.Message);               
            }
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
        protected void BridgeBeginReadFromStream(NetworkStream netStream, IPAddress remoteIP)
        {
            if ( ! netStream.CanRead)
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

                var buffer = readFromNetworkStream(netStream, messageLength);
                try
                {
                    //handle data
                    comLayer.HandleIncomingMessages(buffer, remoteIP);
                }
                catch (Exception e)
                {
                    Logger.Debug("Could not handle message from clientd due: " + e.Message + e.StackTrace);
                }

            }
            catch (Exception e)
            {
                Logger.Debug("Read from TCP stream faild due: " + e.Message + e.StackTrace);
            }
            finally
            {
                Logger.Debug("sending collected messages to" + remoteIP);
                SendMessages(remoteIP);
                CloseAndRemoveConnection(remoteIP);
            }
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
            lock(dataForClients)
            { 
                if ( ! dataForClients.ContainsKey(to))
                {
                    dataForClients.Add(to, new List<AMessage>());
                }
                dataForClients[to].Add(data);
            }            
        }
        
        internal void SendMessages(IPAddress to)
        {
            var dataToSend = new List<AMessage>();
            lock (dataForClients)
            {
                if (dataForClients.ContainsKey(to))
                {
                    dataToSend.AddRange(dataForClients[to]);
                    dataForClients[to].Clear();
                }
            }

            TcpClient client;  
            if (activeClients.TryRemove(to, out client))
            { 
                if(dataToSend.Count == 0) 
                {   
                    Logger.Debug("no data to send");
                    return;
                }

                using(var stream = client.GetStream())
                {
                    if (!stream.CanWrite)
                    {
                        Logger.Debug("Write to TCP stream failed, can't write to stream");
                        return;
                    }

                    Logger.Debug("Write to TCP stream");
                    foreach (var message in dataToSend)
                    {
                        var messageBytes = message.Serialize();
                        WriteToStream(messageBytes, stream);
                    }        

                    Logger.Debug("closing stream");
                    CloseAndRemoveConnection(to);
                }
            }
           
        }
        #endregion
    }
}
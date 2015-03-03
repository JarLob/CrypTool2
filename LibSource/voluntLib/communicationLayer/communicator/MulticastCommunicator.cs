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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.communicator
{
    public class MulticastCommunicator : ICommunicator
    {
        #region private member

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<byte[]> lastOutboundPackets;
        private readonly IPAddress multicastIP;
        private readonly int port;

        private UdpClient client;
        private bool hasStopped;
        private UdpClient sender;

        #endregion

        #region properties

        public IPAddress LocalInterface { get; set; }
        protected ICommunicationLayer CommunicationLayer { get; set; }

        #endregion

        public MulticastCommunicator(int port, string multicastIP = "ff05::1", IPAddress localEndPoint = null)
        {
            lastOutboundPackets = new List<byte[]>(5);
            this.port = port;
            this.multicastIP = IPAddress.Parse(multicastIP);
            LocalInterface = localEndPoint ?? (this.multicastIP.IsIPv6Multicast ? IPAddress.IPv6Any : IPAddress.Any);
        }

        public virtual void RegisterCommunicationLayer(ICommunicationLayer communicationLayer)
        {
            CommunicationLayer = communicationLayer;
        }

        #region start stop

        public virtual void Start()
        {
            hasStopped = false;
            sender = new UdpClient(0, multicastIP.AddressFamily) {MulticastLoopback = true};
            sender.JoinMulticastGroup(multicastIP);

            client = new UdpClient(multicastIP.AddressFamily);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(LocalInterface, port));
            client.JoinMulticastGroup(multicastIP);

            StartReceive();
        }

        public void Stop()
        {
            sender.DropMulticastGroup(multicastIP);
            sender.Close();

            client.DropMulticastGroup(multicastIP);
            client.Close();
            hasStopped = true;
        }

        #endregion

        #region receiving

        private void StartReceive()
        {
            if (!hasStopped)
            {
                client.BeginReceive(OnReceive, new object());
            }
        }

        protected void OnReceive(IAsyncResult asyncResult)
        {
            try
            {
                var ip = new IPEndPoint(LocalInterface, port);
                var receivedBytes = client.EndReceive(asyncResult, ref ip);

                var allreadyHandeld = false;
                lock (lastOutboundPackets)
                {
                    allreadyHandeld = lastOutboundPackets.Any(bytes => bytes.SequenceEqual(receivedBytes)); 
                }

                if ( ! allreadyHandeld)
                {
                    CommunicationLayer.HandleIncomingMessages(receivedBytes, IPAddress.Broadcast);
                }
            } finally
            {
                StartReceive();
            }
        }

        #endregion

        #region send

        public virtual void ProcessMessage(AMessage data, IPAddress to)
        {
            var bytes = data.Serialize();

            lock (lastOutboundPackets)
            {
                lastOutboundPackets.Add(bytes);
            }

            if (lastOutboundPackets.Count >= 5)
            {
                lastOutboundPackets.Remove(lastOutboundPackets.First());
            }

            try
            {
                sender.Send(bytes, bytes.Length, new IPEndPoint(multicastIP, port));
            } catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        #endregion
    }
}
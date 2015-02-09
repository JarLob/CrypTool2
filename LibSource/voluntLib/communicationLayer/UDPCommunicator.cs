using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NLog;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

namespace voluntLib.communicationLayer {
    public class UDPCommunicator {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private UdpClient client;
        private UdpClient sender;
        private readonly IPAddress multicastGrpAddr;
        private readonly int port;
        protected ICommunicatonLayer CommunicationLayer { get; set; }
        private readonly List<byte[]> lastOutboundPackets;

        public UDPCommunicator(int port, string multicastIP = "224.0.0.1") {
            lastOutboundPackets = new List<byte[]>(5);
            this.port = port;
            multicastGrpAddr = IPAddress.Parse(multicastIP);
        }

        public virtual void RegisterCommunicationLayer(ICommunicatonLayer communicationLayer) {
            CommunicationLayer = communicationLayer; 
        }

        public virtual void Start() {
            sender =  new UdpClient(0) {MulticastLoopback = true};
            sender.JoinMulticastGroup(multicastGrpAddr);

            client = new UdpClient();
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            client.JoinMulticastGroup(multicastGrpAddr);

            StartReceiveing();
        }

        public void Stop() {
            sender.DropMulticastGroup(multicastGrpAddr);
            sender.Close();

            client.DropMulticastGroup(multicastGrpAddr);
            client.Close();
        }

        private void StartReceiveing() {
            client.BeginReceive(OnReceive, new object());
        }

        protected void OnReceive(IAsyncResult asyncResult) {
            var ip = new IPEndPoint(IPAddress.Any, port);
            var receivedBytes = client.EndReceive(asyncResult, ref ip);
            if (!lastOutboundPackets.Any(bytes => bytes.SequenceEqual(receivedBytes))) {
              CommunicationLayer.HandleIncommingMessages(receivedBytes);
            }
            StartReceiveing();
        } 

        public virtual void SendData(AMessage data  = null) {
            var bytes = data.Serialize();

            lastOutboundPackets.Add(bytes);
            if (lastOutboundPackets.Count >= 5) {
                lastOutboundPackets.Remove(lastOutboundPackets.First());
            }

            try { 
                sender.Send(bytes,bytes.Length,new IPEndPoint(multicastGrpAddr,port));
            } catch (Exception e){
               logger.Error(e.Message);
            } 
        }

   
    }
}
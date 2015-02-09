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
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.communicator.networkBridgeCommunicator;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.messageHandler
{
    /// <summary>
    /// Abstract MessageHandler.
    /// - Deserilizes the incoming bytestream to a MessageObj of Type T
    /// - checks the signature and iff valid calls the abstract HandlesValidPacket method
    /// 
    /// Provides Methods to create simple messagehandler classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AMessagesHandler<T> : IMessageHandler where T : AMessage, new()
    {
        protected readonly CertificateService certificateService;
        protected readonly CommunicationLayer commLayer;
        protected readonly IManagementLayerCallback managementCallback;

        protected AMessagesHandler(IManagementLayerCallback managementCallback, CommunicationLayer commLayer, CertificateService certificateService)
        {
            if (certificateService == null)
                throw new ArgumentNullException("certificateService");

            this.managementCallback = managementCallback;
            this.commLayer = commLayer;
            this.certificateService = certificateService;
        }

        public virtual void HandleByteArray(byte[] messageBytes, IPAddress from)
        {
            var packet = new T();
            packet.Deserialize(messageBytes);

            if (certificateService.VerifySignature(packet) == CertificateValidationState.Valid)
                HandleValidPacket(packet, from);
        }

        protected void RedirectToNetworkBridges(AMessage message, IPAddress from)
        {
            var connectedSendingTcpComms = commLayer.GetCommunicator().FindAll(com => com is SendingTCPCommunicator).Cast<SendingTCPCommunicator>();
            foreach (var com in connectedSendingTcpComms.Where(com => !from.Equals(com.RemoteNetworkBridgeIP)))
            {
                commLayer.SignAndSendAPacket(message, com.RemoteNetworkBridgeIP);
            }
        }
        
        public abstract void HandleValidPacket(T message, IPAddress from);
    }
}
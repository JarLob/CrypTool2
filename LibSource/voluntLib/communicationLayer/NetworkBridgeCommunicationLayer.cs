﻿// Copyright 2014 Christopher Konze, University of Kassel
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

using System.Net;
using System.Security.Cryptography.X509Certificates;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer
{
    public class NetworkBridgeCommunicationLayer : CommunicationLayer
    {
        private readonly ICommunicator communicator;

        public NetworkBridgeCommunicationLayer(IManagementLayerCallback managementCallback, CertificateService certService, ICommunicator communicator) :
            base(managementCallback, certService, communicator)
        {
            this.communicator = communicator;
        }

        public override void SignAndSendAPacket(AMessage message, IPAddress to)
        {
            certificateHandler.SignAndAddInformation(message);
            Logger.Info("Sending " + (MessageType) message.Header.MessageType + " over TCP-Channel");
            communicator.ProcessMessage(message, to);
        }

        public override void Stop()
        {
            communicator.Stop();
        }

        internal void SendMessages(IPAddress from)
        {
            ((voluntLib.communicationLayer.communicator.networkBridgeCommunicator.ReceivingTCPCommunicator)communicator).SendMessages(from);
        }
    }
}
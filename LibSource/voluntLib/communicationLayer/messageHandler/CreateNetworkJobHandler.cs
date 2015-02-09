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
using voluntLib.common;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.communicator;
using voluntLib.communicationLayer.communicator.networkBridgeCommunicator;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.messageHandler
{
    internal class CreateNetworkJobHandler : AMessagesHandler<CreateNetworkJobMessage>
    {
        public CreateNetworkJobHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer, CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        public override void HandleValidPacket(CreateNetworkJobMessage message, IPAddress from)
        {
            var newNetworkJob = new NetworkJob(message.Header.JobID);
            newNetworkJob.Fill(message);
            managementCallback.OnJobCreation(newNetworkJob);

            RedirectToNetworkBridges(message, from);
        }
    }
}
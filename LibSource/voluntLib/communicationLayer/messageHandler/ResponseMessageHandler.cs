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

using System.Collections.Generic;
using System.Net;
using voluntLib.common;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.messageHandler
{
    internal class ResponseJobListHandler : AMessagesHandler<ResponseJobListMessage>
    {
        public ResponseJobListHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer, CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        public override void HandleValidPacket(ResponseJobListMessage message, IPAddress from)
        {
            var jobList = new List<NetworkJob>();

            message.JobMetaDataList.ForEach(job =>
            {
                var networkJob = new NetworkJob(job.JobID)
                {
                    World = message.Header.WorldName
                };
                networkJob.Fill(job);
                jobList.Add(networkJob);
            });

            managementCallback.OnJobListReceived(message.Header.WorldName, jobList, from);
        }
    }

    internal class ResponseJobDetailsHandler : AMessagesHandler<ResponseJobDetailsMessage>
    {
        public ResponseJobDetailsHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer,
            CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        public override void HandleValidPacket(ResponseJobDetailsMessage message, IPAddress from)
        {
            managementCallback.OnJobDetailsReceived(message.Header.WorldName,
                message.Header.JobID,
                message.JobDetails.JobPayload, from);
        }
    }


    internal class ResponseWorldListHandler : AMessagesHandler<ResponseWorldListMessage>
    {
        public ResponseWorldListHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer,
            CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        public override void HandleValidPacket(ResponseWorldListMessage message, IPAddress from)
        {
            managementCallback.OnWorldListReceived(message.Worlds, from);
        }
    }
}
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

using System.Net;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.messageHandler
{
    internal class RequestJobListHandler : AMessagesHandler<RequestJobListMessage>
    {
        public RequestJobListHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer, CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        /// <summary>
        ///   Handles the valid packet.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="from">From.</param>
        public override void HandleValidPacket(RequestJobListMessage message, IPAddress from)
        {
            managementCallback.OnJobListRequest(message.Header.WorldName, from);
        }
    }

    internal class RequestJobDetailHandler : AMessagesHandler<RequestJobDetailsMessage>
    {
        public RequestJobDetailHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer, CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        /// <summary>
        ///   Handles the valid packet.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="from">From.</param>
        public override void HandleValidPacket(RequestJobDetailsMessage message, IPAddress from)
        {
            managementCallback.OnJobDetailRequest(message.Header.WorldName, message.Header.JobID, from);
        }
    }

    /// <summary>
    ///   Handles an incoming RequestWorldList
    /// </summary>
    internal class RequestWorldListHandler : AMessagesHandler<RequestWorldListMessage>
    {
        public RequestWorldListHandler(IManagementLayerCallback managementCallback, CommunicationLayer comLayer, CertificateService certificateService)
            : base(managementCallback, comLayer, certificateService) {}

        /// <summary>
        ///   Handles the valid packet.
        ///   Redirects the call to the OnWorldListRequest-method of the ManagementCallback
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="from">From.</param>
        public override void HandleValidPacket(RequestWorldListMessage message, IPAddress from)
        {
            managementCallback.OnWorldListRequest(from);
        }
    }
}
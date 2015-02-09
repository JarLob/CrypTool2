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
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.communicationLayer.messageHandler
{
    internal class JoinNetworkJobHandler : AMessagesHandler<JoinNetworkJobMessage>
    {
        private readonly PropagateStateHandler propagateStateHandle;

        public JoinNetworkJobHandler(IManagementLayerCallback managementCallback, CommunicationLayer commLayer, CertificateService certificateService)
            : base(managementCallback, commLayer, certificateService)
        {
            propagateStateHandle = new PropagateStateHandler(managementCallback, commLayer, certificateService);
        }

        public override void HandleValidPacket(JoinNetworkJobMessage message, IPAddress from)
        {
            var nullState = new NullState();
            var propagateState = new PropagateStateMessage {Header = message.Header, StateData = nullState.Serialize()};
            propagateStateHandle.HandleValidPacket(propagateState, from);
        }
    }
}
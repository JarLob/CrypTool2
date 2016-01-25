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
using System.Numerics;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.common.interfaces
{
    public interface IManagementLayerCallback
    {
        void OnIncomingState(PropagateStateMessage stateRaw, IPAddress from);
        void OnJobCreation(NetworkJob newJob, IPAddress from);

        void OnWorldListRequest(IPAddress from);
        void OnWorldListReceived(List<string> worldList, IPAddress from);

        void OnJobListRequest(string world, IPAddress from);
        void OnJobListReceived(string world, List<NetworkJob> receivedJobs, IPAddress from);

        void OnJobDetailRequest(string world, BigInteger jobID, IPAddress from);
        void OnJobDetailsReceived(string world, BigInteger jobID, byte[] detailPayload, IPAddress from);

        void OnJobDeletion(DeleteNetworkJobMessage message, IPAddress fromIP);
        List<string> GetWorlds();
    }
}
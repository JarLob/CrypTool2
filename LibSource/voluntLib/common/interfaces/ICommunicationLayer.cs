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
using System.Net;
using System.Numerics;
using voluntLib.communicationLayer.messages.commonStructs;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.common.interfaces
{
    public interface ICommunicationLayer
    {
        void PropagateState(BigInteger jobID, string worldName, ALocalState state, IPAddress to);

        void JoinNetworkJob(BigInteger jobID, string worldName, IPAddress to);

        void CreateNetworkJob(NetworkJob job, IPAddress to);

        void RequestJobList(string worldName, IPAddress to);

        void SendJobList(string worldName, List<NetworkJobMetaData> descriptions, IPAddress to);

        void RequestJobDetails(BigInteger jobID, string worldName, IPAddress to);

        void SendJobDetails(string worldName, BigInteger jobID, NetworkJobPayload toNetworkJobPayload, IPAddress to);

        void RequestWorldList(IPAddress to);

        void SendWorldList(List<String> worlds, IPAddress to);

        void HandleIncomingMessages(byte[] endReceive, IPAddress from);

        void DeleteNetworkJob(DeleteNetworkJobMessage message, IPAddress any);
        DeleteNetworkJobMessage DeleteNetworkJob(NetworkJob job, IPAddress any);
    }
}
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
using System.Numerics;
using voluntLib.communicationLayer.messages.commonStructs;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.localStateManagement.states.config;

#endregion

namespace voluntLib.common
{
    /// <summary>
    ///   Represents a networkJob from above the NetworkCommunicationLayer
    /// </summary>
    public class NetworkJob : IEquatable<NetworkJob>
    {
        public NetworkJob(BigInteger jobID)
        {
            JobID = jobID;
            World = "";
            JobName = "";
            Creator = "";
            JobType = "";
            StateConfig = new EpochStateConfig {NumberOfBlocks = 5, BitMaskWidth = 8};
            JobPayload = new byte[1] {255};
            JobDescription = new byte[0];
            DeletionMessage = new DeleteNetworkJobMessage(0, "");
        }

        public BigInteger JobID { get; set; }
        public string World { get; set; }
        public string JobName { get; set; }
        public string Creator { get; set; }
        public string JobType { get; set; }
        public byte[] JobDescription { get; set; }
        public byte[] JobPayload { get; set; }
        public EpochStateConfig StateConfig { get; set; }
        public bool IsDeleted { get; set; }
        public DeleteNetworkJobMessage DeletionMessage { get; set; }

        public bool Equals(NetworkJob other)
        {
            return JobID.Equals(other.JobID);
        }

        public NetworkJobMetaData ToNetworkJobMetaData()
        {
            return new NetworkJobMetaData(Creator, JobName, JobType, JobID, JobDescription, StateConfig);
        }

        public NetworkJobPayload ToNetworkJobPayload()
        {
            return new NetworkJobPayload(JobPayload);
        }

        public bool HasPayload()
        {
            if (JobPayload.Length != 1)
            {
                return true;
            }
            return JobPayload[0] != 255;
        }

        public void Fill(CreateNetworkJobMessage message)
        {
            JobID = message.JobMetaData.JobID;
            World = message.Header.WorldName;
            JobPayload = message.JobPayload.JobPayload;
            Fill(message.JobMetaData);
        }

        public void Fill(NetworkJobMetaData jobMetaData)
        {
            Creator = jobMetaData.Creator;
            JobName = jobMetaData.Name;
            JobType = jobMetaData.JobType;
            JobDescription = jobMetaData.JobDescription;
            StateConfig = jobMetaData.AlgorithmInformation;
        }

        public override int GetHashCode()
        {
            return JobID.GetHashCode();
        }
    }
}
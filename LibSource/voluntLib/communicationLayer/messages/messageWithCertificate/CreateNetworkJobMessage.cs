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

using System.Linq;
using System.Numerics;
using voluntLib.communicationLayer.messages.commonStructs;

#endregion

namespace voluntLib.communicationLayer.messages.messageWithCertificate
{
    /*
struct {
	Header{ message _ID: 3};
    Signature;
	Certificate;
	NetworkJobMetaData;
    NetworkJobPayload;
} createNetworkJob
    */

    public class CreateNetworkJobMessage : AMessage
    {
        public CreateNetworkJobMessage() : this(BigInteger.Zero, "", new NetworkJobMetaData(), new NetworkJobPayload(new byte[0])) {}

        public CreateNetworkJobMessage(BigInteger jobID, string worldName, NetworkJobMetaData jobMetaData, NetworkJobPayload jobPayload)
            : base(new HeaderStruct(MessageType.CreateNetworkJob, jobID, worldName))
        {
            JobMetaData = jobMetaData;
            JobPayload = jobPayload;
        }

        public NetworkJobMetaData JobMetaData { get; private set; }
        public NetworkJobPayload JobPayload { get; private set; }

        public override byte[] Serialize()
        {
            var result = base.Serialize();
            return result
                .Concat(JobMetaData.Serialize())
                .Concat(JobPayload.Serialize()).ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            var originalStartIndex = startIndex;
            startIndex += base.Deserialize(data, startIndex);
            startIndex += JobMetaData.Deserialize(data, startIndex);
            startIndex += JobPayload.Deserialize(data, startIndex);
            return startIndex - originalStartIndex;
        }
    }
}
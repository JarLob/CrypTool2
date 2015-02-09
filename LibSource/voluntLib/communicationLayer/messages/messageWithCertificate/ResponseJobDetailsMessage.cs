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
	Header header{ message _ID: 7};
Signature;
	Certificate;
NetworkJobDetails
} JobDetails

    */

    public class ResponseJobDetailsMessage : AMessage
    {
        public ResponseJobDetailsMessage() : this(0, "*", new NetworkJobPayload(new byte[0])) {}

        public ResponseJobDetailsMessage(BigInteger jobID, string worldName, NetworkJobPayload jobDetails)
            : base(new HeaderStruct(MessageType.ResponseJobDetails, jobID, worldName))
        {
            JobDetails = jobDetails;
        }

        public NetworkJobPayload JobDetails { get; private set; }

        public override byte[] Serialize()
        {
            var result = base.Serialize();
            return result.Concat(JobDetails.Serialize()).ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            startIndex += base.Deserialize(data, startIndex);
            startIndex += JobDetails.Deserialize(data, startIndex);
            return startIndex;
        }
    }
}
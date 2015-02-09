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
using System.Linq;
using voluntLib.communicationLayer.messages.commonStructs;

#endregion

namespace voluntLib.communicationLayer.messages.messageWithCertificate
{
    /*
struct {	 			
	Header header{ message _ID: 5};	
    Signature;
	Certificate;

    ushort amount_of_Jobs
	NetworkJobMetaData [] jobs
} JobList

    */

    public class ResponseJobListMessage : AMessage
    {
        public ResponseJobListMessage() : this("*", new List<NetworkJobMetaData>()) {}

        public ResponseJobListMessage(string worldName, List<NetworkJobMetaData> jobMetaDataList)
            : base(new HeaderStruct(MessageType.ResponseJobList, 0, worldName))
        {
            JobMetaDataList = jobMetaDataList;
        }

        public List<NetworkJobMetaData> JobMetaDataList { get; private set; }

        public override byte[] Serialize()
        {
            var result = base.Serialize();
            var jobResult = result.Concat(BitConverter.GetBytes((ushort) JobMetaDataList.Count));
            jobResult = JobMetaDataList.Aggregate(jobResult, (current, job) => current.Concat(job.Serialize()));

            return jobResult.ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            startIndex += base.Deserialize(data, startIndex);

            var amountOfJobs = BitConverter.ToUInt16(data, startIndex);
            startIndex += 2;

            for (var i = 0; i < amountOfJobs; i++)
            {
                var job = new NetworkJobMetaData();
                startIndex += job.Deserialize(data, startIndex);
                JobMetaDataList.Add(job);
            }
            return startIndex;
        }
    }
}
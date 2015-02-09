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
using System.Linq;
using voluntLib.common.interfaces;

#endregion

/*
struct {
    ushort JobPayloadLength	
    byte[] JobPayload 		
} NetworkJobPayload;
*/

namespace voluntLib.communicationLayer.messages.commonStructs
{
    public class NetworkJobPayload : IMessageData
    {
        public NetworkJobPayload(byte[] jobPayload)
        {
            JobPayload = jobPayload;
        }

        #region IMessageData

        public byte[] Serialize()
        {
            var jobPayloadLength = BitConverter.GetBytes((ushort) JobPayload.Length);
            return jobPayloadLength.Concat(JobPayload).ToArray();
        }

        public int Deserialize(byte[] data, int startIndex = 0)
        {
            var originalStartIndex = startIndex;
            var jobPayloadLength = BitConverter.ToUInt16(data, startIndex);
            if (jobPayloadLength + 2 > data.Length)
            {
                throw new IndexOutOfRangeException();
            }

            startIndex += 2;
            JobPayload = data.Skip(startIndex).Take(jobPayloadLength).ToArray();

            return startIndex - originalStartIndex;
        }

        #endregion

        public byte[] JobPayload { get; private set; }
    }
}
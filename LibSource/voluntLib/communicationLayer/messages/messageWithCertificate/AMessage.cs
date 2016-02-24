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

using voluntLib.common.interfaces;
using voluntLib.communicationLayer.messages.commonStructs;

#endregion

namespace voluntLib.communicationLayer.messages.messageWithCertificate
{
   
    public class AMessage : IMessageData
    {
        protected AMessage(HeaderStruct header)
        {
            Header = header;
        }

        public HeaderStruct Header { get; set; }

        public virtual byte[] Serialize()
        {
            return Header.Serialize();
        }

        public virtual int Deserialize(byte[] data, int startIndex = 0)
        {
            return Header.Deserialize(data);
        }

        public void ClearSignature()
        {
            Header.SignatureData = new byte[0];
        }
    }

    public enum MessageType
    {
        RequestJobList = 4,
        ResponseJobList = 5,

        RequestJobDetails = 6,
        ResponseJobDetails = 7,

        RequestWorldList = 8,
        ResponseWorldList = 9,

        CreateNetworkJob = 3,
        JoinNetworkJob = 10,
        PropagateState = 11,
        DeleteNetworkJob = 12,
        All = 99
    }

}
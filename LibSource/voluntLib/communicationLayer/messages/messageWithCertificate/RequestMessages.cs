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

using System.Numerics;
using voluntLib.communicationLayer.messages.commonStructs;

#endregion

namespace voluntLib.communicationLayer.messages.messageWithCertificate
{
    public class RequestWorldListMessage : AMessage
    {
        public RequestWorldListMessage() : base(new HeaderStruct(MessageType.RequestWorldList)) {}
    }

    public class RequestJobListMessage : AMessage
    {
        public RequestJobListMessage() : this("*") {}
        public RequestJobListMessage(string worldName) : base(new HeaderStruct(MessageType.RequestJobList, 0, worldName)) {}
    }

    public class RequestJobDetailsMessage : AMessage
    {
        public RequestJobDetailsMessage() : this(0, "") {}
        public RequestJobDetailsMessage(BigInteger jobID, string worldName) : base(new HeaderStruct(MessageType.RequestJobDetails, jobID, worldName)) {}
    }
}
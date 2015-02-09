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
using System.Numerics;
using voluntLib.communicationLayer.messages.commonStructs;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.communicationLayer.messages.messageWithCertificate
{
    /*
struct {
	Header header{ message _ID: 11}; 
    Signature;
	byte[] stateData	
} PropagateState

    */

    public class PropagateStateMessage : AMessage
    {
        public PropagateStateMessage() : this(0, "", new NullState().Serialize()) {}

        public PropagateStateMessage(BigInteger jobID, string worldName, byte[] stateData)
            : base(new HeaderStruct(MessageType.PropagateState, jobID, worldName))
        {
            StateData = stateData;
        }

        public byte[] StateData { get; set; }

        public override byte[] Serialize()
        {
            var result = base.Serialize();
            var length = BitConverter.GetBytes((ushort) StateData.Length);
            return result.Concat(length).Concat(StateData).ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            startIndex += base.Deserialize(data, startIndex);
            var length = BitConverter.ToInt16(data, startIndex);
            startIndex += 2;
            StateData = data.Skip(startIndex).Take(length).ToArray();
            startIndex += length;
            return startIndex;
        }
    }
}
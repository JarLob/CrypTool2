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
using voluntLib.common.utils;
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

    public class ResponseWorldListMessage : AMessage
    {
        public ResponseWorldListMessage() : this(new List<string>()) {}

        public ResponseWorldListMessage(List<string> worlds) : base(new HeaderStruct(MessageType.ResponseWorldList))
        {
            Worlds = worlds;
        }

        public List<String> Worlds { get; private set; }

        public override byte[] Serialize()
        {
            var result = base.Serialize();
            var worldResult = result.Concat(BitConverter.GetBytes((ushort) Worlds.Count));
            worldResult = Worlds.Aggregate(worldResult,
                (current, item) => current.Concat(SerializationHelper.SerializeString(item)));

            return worldResult.ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            startIndex += base.Deserialize(data, startIndex);

            var amountOfWorlds = BitConverter.ToUInt16(data, startIndex);
            startIndex += 2;

            for (var i = 0; i < amountOfWorlds; i++)
            {
                int length;
                var item = SerializationHelper.DeserializeString(data, startIndex, out length);
                Worlds.Add(item);
                startIndex += length;
            }
            return startIndex;
        }
    }
}
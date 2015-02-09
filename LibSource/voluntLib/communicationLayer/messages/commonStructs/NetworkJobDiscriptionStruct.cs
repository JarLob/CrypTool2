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
using voluntLib.common.interfaces;
using voluntLib.common.utils;
using voluntLib.managementLayer.localStateManagement.states.config;

#endregion

/*
struct {
    byte[] JobID		//128-bit
    string JobName		//null-terminated string
    string Creator		//null-terminated string
    string JobType		//null-terminated string	
    ushort StateInformationLength	//16-bit    
    byte[] StateInformation	
    ushort JobDescriptionLength	//16-bit    
    byte[] JobDescriptionData	
} NetworkJobMetaData;
*/

namespace voluntLib.communicationLayer.messages.commonStructs
{
    public class NetworkJobMetaData : IMessageData
    {
        public NetworkJobMetaData() : this("", "", "", BigInteger.Zero, new byte[0], new EpochStateConfig {NumberOfBlocks = 5, BitMaskWidth = 8}) {}

        public NetworkJobMetaData(string creator, string jobName, string jobType, BigInteger jobID, byte[] jobDescription,
            EpochStateConfig algorithmInformation)
        {
            JobType = jobType;
            Name = jobName;
            JobDescription = jobDescription;
            Creator = creator;
            JobID = jobID;
            AlgorithmInformation = algorithmInformation;
        }

        #region IMessageData

        public byte[] Serialize()
        {
            var jobIDAsBytes = SerializationHelper.SerializeBigInt(JobID);
            var authorBytes = SerializationHelper.SerializeString(Creator);
            var typeBytes = SerializationHelper.SerializeString(JobType);
            var name = SerializationHelper.SerializeString(Name);
            var length = BitConverter.GetBytes((ushort) JobDescription.Length);
            var stateConfigBytes = AlgorithmInformation.Serialize();
            var stateConfigLength = BitConverter.GetBytes((ushort) stateConfigBytes.Length);
            return jobIDAsBytes.Concat(name)
                .Concat(authorBytes)
                .Concat(typeBytes)
                .Concat(stateConfigLength)
                .Concat(stateConfigBytes)
                .Concat(length)
                .Concat(JobDescription).ToArray();
        }

        public int Deserialize(byte[] data, int startIndex = 0)
        {
            var originalStartIndex = startIndex;

            int byteLength;
            JobID = SerializationHelper.DeserializeBigInt(data, startIndex, out byteLength);
            startIndex += byteLength;

            Name = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += byteLength;

            Creator = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += Creator.Length + 1;

            JobType = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += byteLength;

            var length = BitConverter.ToUInt16(data, startIndex);
            startIndex += 2;

            AlgorithmInformation = new EpochStateConfig(data, startIndex);
            startIndex += length;

            length = BitConverter.ToUInt16(data, startIndex);
            startIndex += 2;

            JobDescription = data.Skip(startIndex).Take(length).ToArray();
            startIndex += length;

            return startIndex - originalStartIndex;
        }

        #endregion

        public string Creator { get; private set; }
        public string JobType { get; private set; }
        public BigInteger JobID { get; private set; }
        public byte[] JobDescription { get; private set; }
        public string Name { get; set; }
        public EpochStateConfig AlgorithmInformation { get; set; }
    }
}
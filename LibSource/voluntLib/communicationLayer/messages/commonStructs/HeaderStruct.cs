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
using System.Numerics;
using voluntLib.common.interfaces;
using voluntLib.common.utils;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.communicationLayer.messages.commonStructs
{
    /*
struct {
    byte   ProtocolVersion	
    byte   MessageType
    byte[] JobID           # 128 bit
    string WorldName
    string SenderName 
    string HostName 
} Header;
*/

    public class HeaderStruct : IMessageData
    {
        public byte protocolVersion = 1;
        public HeaderStruct(MessageType messageType) : this(messageType, BigInteger.Zero) {}

        public HeaderStruct(MessageType messageType, BigInteger jobID, string worldName = "*", string senderName = "")
        {
            JobID = jobID;
            WorldName = worldName;
            SenderName = senderName;
            MessageType = (byte) messageType;
            HostName = Environment.MachineName;
            SignatureData = new byte[0];
            CertificateData = new byte[0];
            Extensions = new Dictionary<string, byte[]>();
        }

        public byte MessageType { get; private set; }
        public BigInteger JobID { get; private set; }
        public string WorldName { get; private set; }
        public string SenderName { get; set; }
        public string HostName { get; set; }
        public byte[] CertificateData { get; set; }
        public byte[] SignatureData { get; set; }
        public Dictionary<string, byte[]> Extensions { get; set; }

        public byte[] Serialize()
        {
            var result = new List<byte> {protocolVersion, MessageType};
            var jobIDAsBytes = SerializationHelper.SerializeJobID(JobID);
            var worldNameAsBytes = SerializationHelper.SerializeString(WorldName);
            var senderNameAsBytes = SerializationHelper.SerializeString(SenderName);
            var hostNameAsBytes = SerializationHelper.SerializeString(HostName);
            var signatureLength = BitConverter.GetBytes((ushort) SignatureData.Length);
            var certificateLength = BitConverter.GetBytes((ushort) CertificateData.Length);

            // add extension  
            var extensionsBytes = new List<byte>(BitConverter.GetBytes((ushort) Extensions.Count));
            foreach (var extension in Extensions)
            {
                extensionsBytes.AddRange(SerializationHelper.SerializeString(extension.Key));
                extensionsBytes.AddRange(BitConverter.GetBytes((ushort) extension.Value.Length));
                extensionsBytes.AddRange(extension.Value);
            }

            return result.Concat(jobIDAsBytes)
                .Concat(worldNameAsBytes)
                .Concat(senderNameAsBytes)
                .Concat(hostNameAsBytes)
                .Concat(certificateLength)
                .Concat(CertificateData)
                .Concat(signatureLength)
                .Concat(SignatureData)
                .Concat(extensionsBytes).ToArray();
        }

        public int Deserialize(byte[] data, int startIndex = 0)
        {
            var originalStartIndex = startIndex;
            protocolVersion = data[startIndex];
            MessageType = data[startIndex + 1];
            startIndex += 2;
            int byteLength;
            JobID = SerializationHelper.DeserializeJobID(data, startIndex, out byteLength);
            startIndex += byteLength;

            WorldName = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += byteLength;

            SenderName = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += byteLength;

            HostName = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
            startIndex += byteLength;

            //certificate
            var certificateLength = BitConverter.ToUInt16(data, startIndex);
            if (certificateLength > data.Length)
            {
                throw new IndexOutOfRangeException();
            }
            startIndex += 2;
            CertificateData = new byte[certificateLength];
            Array.Copy(data, startIndex, CertificateData, 0, certificateLength);
            startIndex += certificateLength;

            //signature
            var signatureLength = BitConverter.ToUInt16(data, startIndex);
            if (signatureLength > data.Length)
            {
                throw new IndexOutOfRangeException();
            }
            startIndex += 2;
            SignatureData = new byte[signatureLength];
            Array.Copy(data, startIndex, SignatureData, 0, signatureLength);
            startIndex += signatureLength;

            //extensions
            var numberOfExtensions = BitConverter.ToUInt16(data, startIndex);
            startIndex += 2;

            for (var i = 0; i < numberOfExtensions; i++)
            {
                var key = SerializationHelper.DeserializeString(data, startIndex, out byteLength);
                startIndex += byteLength;

                var valueLength = BitConverter.ToUInt16(data, startIndex);
                startIndex += 2;
                var value = new byte[valueLength];
                Array.Copy(data, startIndex, value, 0, valueLength);
                Extensions.Add(key, value);

                startIndex += valueLength;
            }

            return startIndex - originalStartIndex;
        }
    }
}
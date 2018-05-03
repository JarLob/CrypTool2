/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer.Messages
{
    /// <summary>
    /// Each message has a unique type number defined by this enum
    /// </summary>
    public enum MessageType
    {
        Undefined = 0,
        CreateNetworkJobMessage = 10,
        DeleteNetworkJobMessage = 20,

        RequestJobListMessage = 30,
        ResponseJobListMessage = 31,

        RequestJobDetailsMessage = 40,
        ResponseJobDetailsMessage = 41,

        PropagateJobStateMessage = 50
    }

    /// <summary>
    /// Helper class with a static method for Deserialization
    /// </summary>
    internal class MessageHelper
    {
        public static Message Deserialize(byte[] data)
        {
            //Deserialize to general message object; if it fails we did not get a valid message
            var message = new Message();
            message.Deserialize(data);

            switch (message.MessageHeader.MessageType)
            {
                case MessageType.Undefined:
                    throw new VoluntLib2MessageDeserializationException(string.Format("Received a message of MessageType {0} - can not do anything with that!", message.MessageHeader.MessageType));
                case MessageType.CreateNetworkJobMessage:
                    message = new CreateNetworkJobMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.DeleteNetworkJobMessage:
                    message = new DeleteNetworkJobMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.RequestJobListMessage:
                    message = new RequestJobListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseJobListMessage:
                    message = new ResponseJobListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.RequestJobDetailsMessage:
                    message = new RequestJobDetailsMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseJobDetailsMessage:
                    message = new ResponseJobDetailsMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.PropagateJobStateMessage:
                    message = new PropagateJobStateMessage();
                    message.Deserialize(data);
                    return message;
                //add new message types here

                default:
                    throw new VoluntLib2MessageDeserializationException(string.Format("Received a message of an unknown MessageType: {0}", message.MessageHeader.MessageType));
            }
        }
    }

    /// <summary>
    /// The header of all messages of VoluntLib2 JobManagementLayer
    /// </summary>
    public class MessageHeader
    {        
        private const int STRING_MAX_LENGTH = 255;

        public byte[] MessageId = new byte[16];        // 16 bytes
        public MessageType MessageType;                // 1 byte
        public ushort PayloadLength;                   // 2 bytes
        //public ushort WorldNameLength;               // 2 bytes
        public string WorldName;                       // WorldNameLength bytes
        //public ushort SenderNameLength;              // 2 bytes
        public string SenderName;                      // SenderNameLength bytes
        //public ushort CertificateLength;             // 2 bytes
        public byte[] CertificateData = new byte[0];   // CertificateLength bytes
        //public ushort SignatureLength;               // 2 bytes
        public byte[] SignatureData = new byte[0];     // SignatureLength bytes
        
        public byte[] Serialize()
        {
            //World Name
            if (WorldName.Length > STRING_MAX_LENGTH)
            {
                WorldName = WorldName.Substring(0, STRING_MAX_LENGTH);
            }
            //convert World Name to byte array and get its length
            byte[] worldNameBytes = UTF8Encoding.UTF8.GetBytes(WorldName);
            int worldNameLength = worldNameBytes.Length;

            //Sender Name
            if (SenderName.Length > STRING_MAX_LENGTH)
            {
                SenderName = SenderName.Substring(0, STRING_MAX_LENGTH);
            }
            //convert Sender Name to byte array and get its length
            byte[] senderNameBytes = UTF8Encoding.UTF8.GetBytes(SenderName);
            int senderNameLength = senderNameBytes.Length;         

            byte[] data = new byte[16 + 1 + 2 + 2 + worldNameBytes.Length + 2 + senderNameBytes.Length + 2 + CertificateData.Length + 2 + SignatureData.Length];
            
            Array.Copy(MessageId, 0, data, 0, 16);
            
            data[16] = (byte)MessageType;
            
            byte[] payloadLengthBytes = BitConverter.GetBytes(PayloadLength);            
            data[17] = payloadLengthBytes[0];
            data[18] = payloadLengthBytes[1];                        
           
            byte[] worldNameLengthBytes = BitConverter.GetBytes(worldNameLength);
            data[19] = worldNameLengthBytes[0];
            data[20] = worldNameLengthBytes[1];
            Array.Copy(worldNameBytes, 0, data, 21, worldNameBytes.Length);           
           
            byte[] senderNameLengthBytes = BitConverter.GetBytes(senderNameLength);
            data[21 + worldNameBytes.Length] = senderNameLengthBytes[0];
            data[21 + worldNameBytes.Length + 1] = senderNameLengthBytes[1];
            Array.Copy(senderNameBytes, 0, data, 21 + worldNameBytes.Length + 2, senderNameBytes.Length);

            //Certificate Data
            int certificateDataLength = CertificateData.Length;
            byte[] certificateDataLengthBytes = BitConverter.GetBytes(certificateDataLength);
            data[23 + worldNameBytes.Length + senderNameBytes.Length] = certificateDataLengthBytes[0];
            data[23 + worldNameBytes.Length + senderNameBytes.Length + 1] = certificateDataLengthBytes[1];
            Array.Copy(CertificateData, 0, data, 23 + worldNameBytes.Length + senderNameBytes.Length + 2, CertificateData.Length);

            //Signature Data
            int signatureDataLength = SignatureData.Length;
            byte[] signatureDataLengthBytes = BitConverter.GetBytes(signatureDataLength);
            data[25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length] = signatureDataLengthBytes[0];
            data[25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length + 1] = signatureDataLengthBytes[1];
            Array.Copy(SignatureData, 0, data, 25 + worldNameBytes.Length + senderNameBytes.Length + CertificateData.Length + 2, SignatureData.Length);

            return data;
        }
        public void Deserialize(byte[] data)
        {
            MessageId = new byte[16];
            Array.Copy(data, 0, MessageId, 0, 16);

            MessageType = (MessageType)data[16];

            PayloadLength = BitConverter.ToUInt16(data, 17);

            int worldNameLength = BitConverter.ToUInt16(data, 19);
            WorldName = UTF8Encoding.UTF8.GetString(data, 21, worldNameLength);

            int senderNameLength = BitConverter.ToUInt16(data, 21 + worldNameLength);
            SenderName = UTF8Encoding.UTF8.GetString(data, 23 + worldNameLength, senderNameLength);

            int certificateDataLength = BitConverter.ToUInt16(data, 23 + worldNameLength + senderNameLength);
            CertificateData = new byte[certificateDataLength];
            Array.Copy(data, 25 + worldNameLength + senderNameLength, CertificateData, 0, certificateDataLength);

            int signatureDataLength = BitConverter.ToUInt16(data, 25 + worldNameLength + senderNameLength + certificateDataLength);
            SignatureData = new byte[signatureDataLength];
            Array.Copy(data, 27 + worldNameLength + senderNameLength + certificateDataLength, SignatureData, 0, signatureDataLength);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("MessageHeader");
            builder.AppendLine("{");
            builder.Append("  MessageID: ");
            builder.AppendLine(BitConverter.ToString(MessageId) + ",");
            builder.Append("  MessageType: ");
            builder.AppendLine(MessageType.ToString() + ",");
            builder.Append("  PayloadLength: ");
            builder.AppendLine("" + PayloadLength + ",");
            builder.Append("  WorldName: ");
            builder.AppendLine("" + WorldName + ",");
            builder.Append("  SenderName: ");
            builder.AppendLine("" + SenderName + ",");
            builder.Append("  CertificateData: ");
            builder.AppendLine("" + BitConverter.ToString(CertificateData) + ",");
            builder.Append("  SignatureData: ");
            builder.AppendLine("" + BitConverter.ToString(SignatureData));
            builder.AppendLine("}");

            return builder.ToString();
        }
    }

    /// <summary>
    /// Super class of all messages
    /// containing a MessageHeader and the Payload
    /// </summary>
    public class Message
    {
        public const string VLIB2MNGMT = "VLib2Mngmt";  //Magic Number to identify VoluntLib2 management protocol
        public const byte VOLUNTLIB2_VERSION = 0x01;    //Protocol version number

        public MessageHeader MessageHeader;
        public byte[] Payload;                              //length defined by header.PayloadLength
        public byte VoluntLibVersion = VOLUNTLIB2_VERSION;

        public Message()
        {
            MessageHeader = new MessageHeader();
            MessageHeader.MessageType = MessageType.Undefined;
            MessageHeader.MessageId = Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// Serializes the message to a byte array.
        /// if signMessage == false, the MessageHeader.SignatureData is byte[0] after calling this method.
        /// if signMessage == true, the MessageHeader.SignatureData is the signature of the method after calling this method.
        /// Uses the cert given to the CertificateService
        /// </summary>
        /// <param name="signMessage"></param>
        /// <returns></returns>
        public virtual byte[] Serialize(bool signMessage = true)
        {
            if (Payload != null && Payload.Length != 0)
            {
                MessageHeader.PayloadLength = (ushort)Payload.Length;
            }
            else
            {
                MessageHeader.PayloadLength = 0;
            }
            
            MessageHeader.SignatureData = new byte[0];

            byte[] magicNumber = Encoding.ASCII.GetBytes(VLIB2MNGMT);       //10 bytes
            // 1 byte protocol version
            byte[] headerbytes = MessageHeader.Serialize();             

            ushort payloadLengthBytes = (ushort)(Payload != null ? Payload.Length : 0);
            byte[] messagebytes = new byte[10 + 1 + headerbytes.Length + payloadLengthBytes];

            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 11 + headerbytes.Length, Payload.Length);
            }

            //If we don't sign the message, we are finished here
            if (!signMessage) 
            {
                return messagebytes;
            }

            byte[] signature = CertificateService.GetCertificateService().SignData(messagebytes);
            MessageHeader.SignatureData = signature;

            headerbytes = MessageHeader.Serialize();
            messagebytes = new byte[10 + 1 + headerbytes.Length + payloadLengthBytes];
            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, headerbytes.Length);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 11 + headerbytes.Length, Payload.Length);
            }

            return messagebytes;
        }
        public virtual void Deserialize(byte[] data)
        {
            if (data.Length < 27)
            {
                throw new VoluntLib2MessageDeserializationException(String.Format("Invalid message received. Expected minimum 27 bytes. Got {0} bytes!", data.Length));
            }
            string magicnumber = Encoding.ASCII.GetString(data, 0, 10);
            if (!magicnumber.Equals(VLIB2MNGMT))
            {
                throw new VoluntLib2MessageDeserializationException(String.Format("Invalid magic number. Expected {0}. Received {1}", VLIB2MNGMT, magicnumber));
            }
            if (data[10] > VOLUNTLIB2_VERSION)
            {
                throw new VoluntLib2MessageDeserializationException(String.Format("Expected a VoluntLib2 version <= {0}. Received a version {1}. Please update!", VLIB2MNGMT, magicnumber));
            }

            MessageHeader = new MessageHeader();
            byte[] messageheaderbytes = new byte[data.Length - 11];
            VoluntLibVersion = data[10];
            Array.Copy(data, 11, messageheaderbytes, 0, messageheaderbytes.Length);
            MessageHeader.Deserialize(messageheaderbytes);
            Payload = new byte[MessageHeader.PayloadLength];
            Array.Copy(data, data.Length - Payload.Length, Payload, 0, Payload.Length);
        }
    }

    internal class CreateNetworkJobMessage : Message
    {
        public CreateNetworkJobMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.CreateNetworkJobMessage;
        }
    }
    internal class DeleteNetworkJobMessage : Message
    {
        public DeleteNetworkJobMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.DeleteNetworkJobMessage;
        }
    }

    internal class RequestJobListMessage : Message
    {
        public RequestJobListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestJobListMessage;
        }
    }
    internal class ResponseJobListMessage : Message
    {
        public ResponseJobListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.ResponseJobListMessage;
        }
    }
    internal class RequestJobDetailsMessage : Message
    {
        public RequestJobDetailsMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestJobDetailsMessage;
        }
    }
    internal class ResponseJobDetailsMessage : Message
    {
        public ResponseJobDetailsMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.ResponseJobDetailsMessage;
        }
    }

    internal class PropagateJobStateMessage : Message
    {
        public PropagateJobStateMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.PropagateJobStateMessage;
        }
    }
}

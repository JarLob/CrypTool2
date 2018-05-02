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
    internal enum MessageType
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
    internal class MessageHeader
    {        
        public byte[] MessageId = new byte[16];        // 16 bytes
        public MessageType MessageType;                // 1 byte
        public ushort PayloadLength;                   // 2 bytes
        //public ushort WorldNameLength;               // 2 bytes
        public string WorldName;                       // WorldNameLength bytes
        //public ushort SenderNameLength;              // 2 bytes
        public string SenderName;                      // SenderNameLength bytes
        //public ushort HostNameLength;                // 2 bytes
        public string HostName;                        // HostNameLength bytes
        //public ushort CertificateLength;             // 2 bytes
        public byte[] CertificateData;                 // CertificateLength bytes
        //public ushort SignatureLength;               // 2 bytes
        public byte[] SignatureData;                   // SignatureLength bytes

        public byte[] Serialize()
        {
            byte[] data = new byte[16 + 1 + 2 + 2 + WorldName.Length + 2 + SenderName.Length + 2 + HostName.Length + 2 + CertificateData.Length + 2 + SignatureData.Length];
            
            return data;
        }
        public void Deserialize(byte[] data)
        {
           
        }

        public override string ToString()
        {
            return "";
        }
    }

    /// <summary>
    /// Super class of all messages
    /// containing a MessageHeader and the Payload
    /// </summary>
    internal class Message
    {
        public const string VLIB2MNGMT = "VLib2Mngmt";  //Magic Number to identify protocol
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

        public virtual byte[] Serialize()
        {
            if (Payload != null && Payload.Length != 0)
            {
                MessageHeader.PayloadLength = (ushort)Payload.Length;
            }
            else
            {
                MessageHeader.PayloadLength = 0;
            }

            byte[] magicNumber = Encoding.ASCII.GetBytes(VLIB2MNGMT);       //10 bytes
            // 1 byte protocol versin
            byte[] headerbytes = MessageHeader.Serialize();                 //63 bytes

            ushort payloadLengthBytes = (ushort)(Payload != null ? Payload.Length : 0);
            byte[] messagebytes = new byte[10 + 1 + 63 + payloadLengthBytes];

            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[11] = VOLUNTLIB2_VERSION;
            Array.Copy(headerbytes, 0, messagebytes, 11, 62);
            if (Payload != null && Payload.Length > 0)
            {
                Array.Copy(Payload, 0, messagebytes, 73, Payload.Length);
            }

            return messagebytes;
        }
        public virtual void Deserialize(byte[] data)
        {
            if (data.Length < 74)
            {
                throw new VoluntLib2MessageDeserializationException(String.Format("Invalid message received. Expected minimum 74 byte. Got {0} bytes!", data.Length));
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
            byte[] messageheaderbytes = new byte[63];
            VoluntLibVersion = data[0];
            Array.Copy(data, 11, messageheaderbytes, 0, 63);
            MessageHeader.Deserialize(messageheaderbytes);
            Payload = new byte[MessageHeader.PayloadLength];
            Array.Copy(data, 73, Payload, 0, Payload.Length);
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

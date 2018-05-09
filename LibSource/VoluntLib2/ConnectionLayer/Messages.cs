using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VoluntLib2.Tools;

namespace VoluntLib2.ConnectionLayer.Messages
{    
    /*
    Hello Protocol:
    A       --> HelloMessage -->             B
    A       <-- HelloResponseMessage <--     B
    The hello protocol is used every 30 seconds to "refresh" connections between A and B
     
    Neighbor lists exchange protocol:
    A       --> RequestNeighborListMessage -->  B
    A       <-- ResponseNeighborListMessage <-- B
     
    Connection Protocol:
    A       --> HelpMeConnectMessage    -->  S
    B       <-- WantsConnectionMessage  <--  S
    now A and B perform HelloMessages n times, until both received a HelloResponse
    if A received HelloResponse from B, connection from A->B works for A
    if B received HelloResponse from A, connection from B->A works for B
    
    Data Protocol:
    A         --> DataMessage -->  B    
    
    Offline Protocol
    A         --> GoingOfflineMessage --> B
     
    */
    
    /// <summary>
    /// Each message has a unique type number defined by this enum
    /// </summary>
    internal enum MessageType
    {
        Undefined = 0,
        HelloMessage = 10,
        HelloResponseMessage = 11,
        RequestNeighborListMessage = 20,
        ResponseNeighborListMessage = 21,
        HelpMeConnectMessage = 30,
        WantsConnectionMessage = 31,
        DataMessage = 40,
        GoingOfflineMessage = 50
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
                    throw new VoluntLibSerializationException(string.Format("Received a message of MessageType {0} - can not do anything with that!", message.MessageHeader.MessageType));
                case MessageType.HelloMessage:
                    message = new HelloMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.HelloResponseMessage:
                    message = new HelloResponseMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.RequestNeighborListMessage:
                    message = new RequestNeighborListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.ResponseNeighborListMessage:
                    message = new ResponseNeighborListMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.HelpMeConnectMessage:
                    message = new HelpMeConnectMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.WantsConnectionMessage:
                    message = new WantsConnectionMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.DataMessage:
                    message = new DataMessage();
                    message.Deserialize(data);
                    return message;
                case MessageType.GoingOfflineMessage:
                    message = new GoingOfflineMessage();
                    message.Deserialize(data);
                    return message;
                //add new message types here

                default:
                    throw new VoluntLibSerializationException(string.Format("Received a message of an unknown MessageType: {0}", message.MessageHeader.MessageType));
            }            
        }
    }
    
    /// <summary>
    /// The header of all messages of VoluntLib2 Connection Layer
    /// </summary>
    internal class MessageHeader
    {        
        //we have a header size of total 63 bytes
        public byte[] MessageId = new byte[16];        // 16 bytes
        public MessageType MessageType;                // 1 byte
        public ushort PayloadLength;                   // 2 bytes

        public byte[] SenderPeerId = new byte[16];     // 16 bytes
        public byte[] ReceiverPeerId = new byte[16];   // 16 bytes

        public byte[] SenderIPAddress = new byte[4];   // 4 bytes
        public byte[] ReceiverIPAddress = new byte[4]; // 4 bytes
        
        public ushort SenderExternalPort = 0;           // 2 bytes
        public ushort ReceiverExternalPort = 0;         // 2 bytes

        public byte[] Serialize()
        {            
            byte[] data = new byte[16 + 1 +2 + 16 + 16 + 4 + 4 + 2 + 2];
            Array.Copy(MessageId, 0, data, 0, 16);
            Array.Copy(BitConverter.GetBytes((byte)MessageType), 0, data, 16, 1);
            Array.Copy(BitConverter.GetBytes(PayloadLength), 0, data, 17, 2);
            Array.Copy(SenderPeerId, 0, data, 19, 16);
            Array.Copy(ReceiverPeerId, 0, data, 35, 16);
            Array.Copy(SenderIPAddress, 0, data, 51, 4);
            Array.Copy(ReceiverIPAddress, 0, data, 55, 4);
            Array.Copy(BitConverter.GetBytes(SenderExternalPort), 0, data, 59, 2);
            Array.Copy(BitConverter.GetBytes(ReceiverExternalPort), 0, data, 61, 2);
            return data;
        }
        public void Deserialize(byte[] data)
        {
            Array.Copy(data, 0, MessageId, 0, 16);
            MessageType = (MessageType)data[16];
            PayloadLength = BitConverter.ToUInt16(data, 17);
            Array.Copy(data, 19, SenderPeerId, 0, 16);
            Array.Copy(data, 35, ReceiverPeerId, 0, 16);
            Array.Copy(data, 51, SenderIPAddress, 0, 4);
            Array.Copy(data, 55, ReceiverIPAddress, 0, 4);
            SenderExternalPort = BitConverter.ToUInt16(data, 59);
            ReceiverExternalPort = BitConverter.ToUInt16(data, 61);
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
            builder.Append("  SenderPeerId: ");
            builder.AppendLine("" + BitConverter.ToString(SenderPeerId) + ",");
            builder.Append("  ReceiverPeerId: ");
            builder.AppendLine("" + BitConverter.ToString(ReceiverPeerId) + ",");
            builder.Append("  SenderAddress: ");
            builder.AppendLine("" + new IPAddress(SenderIPAddress) + ",");
            builder.Append("  ReceiverAddress: ");
            builder.AppendLine("" + new IPAddress(ReceiverIPAddress) + ",");
            builder.Append("  SenderPort: ");
            builder.AppendLine("" + SenderExternalPort + ",");
            builder.Append("  ReceiverPort: ");
            builder.AppendLine("" + ReceiverExternalPort + ",");
            builder.AppendLine("}");

            return builder.ToString();
        }
    }

    /// <summary>
    /// Super class of all messages
    /// containing a MessageHeader and the Payload
    /// </summary>
    internal class Message
    {
        public const string VOLUNTLIB2 = "VoluntLib2";  //Magic Number to identify protocol
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

            byte[] magicNumber = Encoding.ASCII.GetBytes(VOLUNTLIB2);       //10 bytes
                                                                            // 1 byte protocol versin
            byte[] headerbytes = MessageHeader.Serialize();                 //63 bytes

            ushort payloadLengthBytes = (ushort)(Payload != null ? Payload.Length : 0);
            byte[] messagebytes = new byte[10 + 1 + 63 + payloadLengthBytes];

            Array.Copy(magicNumber, 0, messagebytes, 0, 10);
            messagebytes[10] = VOLUNTLIB2_VERSION;
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
                throw new VoluntLibSerializationException(String.Format("Invalid message received. Expected minimum 74 bytes. Got {0} bytes!", data.Length));
            }            
            string magicnumber = Encoding.ASCII.GetString(data, 0, 10);
            if (!magicnumber.Equals(VOLUNTLIB2))
            {
                throw new VoluntLibSerializationException(String.Format("Invalid magic number. Expected '{0}'. Received '{1}'", VOLUNTLIB2, magicnumber));
            }
            if (data[10] > VOLUNTLIB2_VERSION)
            {
                throw new VoluntLibSerializationException(String.Format("Expected a VoluntLib2 version <= {0}. Received a version {1}. Please update!", VOLUNTLIB2, magicnumber));
            }

            MessageHeader = new MessageHeader();
            byte[] messageheaderbytes = new byte[63];
            VoluntLibVersion = data[10];
            Array.Copy(data, 11, messageheaderbytes, 0, 63);
            MessageHeader.Deserialize(messageheaderbytes);
            Payload = new byte[MessageHeader.PayloadLength];
            Array.Copy(data, 73, Payload, 0, Payload.Length);
        }
    }

    /// <summary>
    /// A HelloMessage which is answered by a HelloResponseMessage
    /// The message contains a nonce for identifcation of the HelloResponse
    /// </summary>
    internal class HelloMessage : Message
    {
        public byte[] HelloNonce = Guid.NewGuid().ToByteArray();

        public HelloMessage() : base (){
            MessageHeader.MessageType = MessageType.HelloMessage;
        }

        public override byte[] Serialize(){
            Payload = HelloNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            HelloNonce = Payload;
        }
    }

    /// <summary>
    /// A HelloResponseMessage is the answer to a HelloMessage
    /// Must contain the nonce of the HelloMessage which is answered
    /// </summary>
    internal class HelloResponseMessage : Message
    {
        public byte[] HelloResponseNonce = new byte[16];

        public HelloResponseMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.HelloResponseMessage;
        }

        public override byte[] Serialize()
        {
            Payload = HelloResponseNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            HelloResponseNonce = Payload;
        }
    }

    /// <summary>
    /// A RequestNeighborListMessage asks a neighbor to send all of his neighbors
    /// </summary>
    internal class RequestNeighborListMessage : Message
    {
        public byte[] RequestNeighborListNonce = Guid.NewGuid().ToByteArray();
        public RequestNeighborListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.RequestNeighborListMessage;
        }

        public override byte[] Serialize()
        {
            Payload = RequestNeighborListNonce;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            RequestNeighborListNonce = Payload;
        }
    }

    /// <summary>
    /// A ResponseNeighborListMessage contains a list of neighbors send from one peer to another
    /// </summary>
    internal class ResponseNeighborListMessage : Message
    {
        public byte[] ResponseNeighborListNonce = new byte[16];
        public List<Contact> Neighbors = new List<Contact>();

        public ResponseNeighborListMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.ResponseNeighborListMessage;
        }

        public override byte[] Serialize()
        {
            byte[] data = new byte[16 + 2 + Neighbors.Count * 22];            
            Array.Copy(ResponseNeighborListNonce, data, 16);
            Array.Copy(BitConverter.GetBytes((ushort)Neighbors.Count), 0, data, 16, 2);
            uint offset = 18;
            foreach (Contact contact in Neighbors)
            {
                Array.Copy(contact.Serialize(), 0, data, offset, 22);
                offset += 22;
            }
            Payload = data;
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {            
            base.Deserialize(data);
            ResponseNeighborListNonce = new byte[16];
            Array.Copy(Payload, ResponseNeighborListNonce, 16);
            ushort count = BitConverter.ToUInt16(Payload, 16);

            for (uint offset = 18; offset < 18 + count * 22; offset += 22)
            {
                Contact contact = new Contact();
                byte[] array = new byte[22];
                Array.Copy(Payload, offset, array, 0, 22);
                contact.Deserialize(array);
                Neighbors.Add(contact);
            }
            Payload = data;
        }
    }

    /// <summary>
    /// Send from a peer to another to request help for connection to another peer
    /// </summary>
    internal class HelpMeConnectMessage : Message
    {
        public ushort Port = 0;
        public IPAddress IPAddress;

        /// <summary>
        /// Creates a HelpMeConnectMessage
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public HelpMeConnectMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.HelpMeConnectMessage;
        }
        
        public override byte[] Serialize()
        {
            Payload = new byte[6];
            byte[] ipbytes = IPAddress.GetAddressBytes();
            byte[] portbytes = BitConverter.GetBytes(Port);
            Payload[0] = ipbytes[0];
            Payload[1] = ipbytes[1];
            Payload[2] = ipbytes[2];
            Payload[3] = ipbytes[3];
            Payload[4] = portbytes[0];
            Payload[5] = portbytes[1];            
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            IPAddress = new IPAddress(new byte[] { Payload[0], Payload[1], Payload[2], Payload[3]});
            Port = BitConverter.ToUInt16(Payload, 4);
        }
    }

    /// <summary>
    /// Send from a peer to another to tell him, which peers wants to connect to him
    /// </summary>
    internal class WantsConnectionMessage : Message
    {
        public ushort Port = 0;
        public IPAddress IPAddress;

        /// <summary>
        /// Creates a WantsConnectionMessage
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public WantsConnectionMessage()
            : base()
        {
            MessageHeader.MessageType = MessageType.WantsConnectionMessage;
        }

        public override byte[] Serialize()
        {
            Payload = new byte[6];
            byte[] ipbytes = IPAddress.GetAddressBytes();
            byte[] portbytes = BitConverter.GetBytes(Port);
            Payload[0] = ipbytes[0];
            Payload[1] = ipbytes[1];
            Payload[2] = ipbytes[2];
            Payload[3] = ipbytes[3];
            Payload[4] = portbytes[0];
            Payload[5] = portbytes[1];
            return base.Serialize();
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            IPAddress = new IPAddress(new byte[] { Payload[0], Payload[1], Payload[2], Payload[3]});
            Port = BitConverter.ToUInt16(Payload, 4);
        }
    }

    /// <summary>
    /// A message for sending data to a peer
    /// </summary>
    internal class DataMessage : Message
    {
        public DataMessage()
        {
            MessageHeader.MessageType = MessageType.DataMessage;
        }
    }

    /// <summary>
    /// A message for informing every other peer that this peer goes offline now
    /// </summary>
    internal class GoingOfflineMessage : Message
    {
        public GoingOfflineMessage()
        {
            MessageHeader.MessageType = MessageType.GoingOfflineMessage;
        }
    }   
}

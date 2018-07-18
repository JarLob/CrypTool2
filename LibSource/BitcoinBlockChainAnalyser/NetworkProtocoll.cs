using System;
using System.Net.Sockets;

namespace BitcoinBlockChainAnalyser
{
    /// <summary>
    /// An message header for the communication between BCA Clients and the BitCoinApi Server from the department AIS
    /// </summary>
    public class Header
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MessageType MessageType;
        public Int32 Size;

        /// <summary>
        /// Create an empty header
        /// </summary>
        public Header()
        {

        }

        /// <summary>
        /// Create a header with messageType and size set
        /// </summary>
        public Header(MessageType type, Int32 size)
        {
            MessageType = type;
            Size = size;
        }

        /// <summary>
        /// Serialize the header into a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            var bytes = new byte[11];
            bytes[0] = (byte)'B';
            bytes[1] = (byte)'C';
            bytes[2] = (byte)'A';
            byte[] messageTypeBytes = BitConverter.GetBytes((Int32)MessageType);
            byte[] sizebytes = BitConverter.GetBytes(Size);
            bytes[3] = messageTypeBytes[0];
            bytes[4] = messageTypeBytes[1];
            bytes[5] = messageTypeBytes[2];
            bytes[6] = messageTypeBytes[3];
            bytes[7] = sizebytes[0];
            bytes[8] = sizebytes[1];
            bytes[9] = sizebytes[2];
            bytes[10] = sizebytes[3];
            return bytes;
        }

        /// <summary>
        /// Deserialize the header from a byte array
        /// </summary>
        public void Deserialize(byte[] bytes)
        {
            MessageType = (MessageType)BitConverter.ToInt32(bytes, 3);
            Size = BitConverter.ToInt32(bytes, 7);
        }
    }

    /// <summary>
    /// Types of messages between DownloadListener and DownloadClients
    /// </summary>
    public enum MessageType
    {
        GetblockhashRequestMessage,
        GetblockhashResponseMessage,
        GetblockRequestMessage,
        GetblockResponseMessage,
        GetblockcountRequestMessage,
        GetblockcountResponseMessage,
        GettransactionRequestMessage,
        GettransactionResponseMessage,
        GettxoutRequestMessage,
        GettxoutResponsetMessage,
    }

    /// <summary>
    /// Messages between BCA Client and BCA Server
    /// </summary>
    public class Message
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Header Header;
        public byte[] Payload;

        /// <summary>
        /// Create a new message
        /// </summary>
        public Message()
        {
            Header = new Header();
        }

        /// <summary>
        /// Receive a message from the network stream
        /// </summary>
        public static Message ReceiveMessage(NetworkStream stream)
        {
            var headerbytes = new byte[11];
            stream.Read(headerbytes, 0, 11);
            var header = new Header();
            header.Deserialize(headerbytes);
            var message = new Message();
            message.Header = header;
            var buffer = new byte[header.Size];
            //Console.WriteLine("Want to read " + header.Size + " bytes");
            var totalbytes = 0;
            do
            {
                var readbytes = stream.Read(buffer, totalbytes, buffer.Length - totalbytes);
                totalbytes += readbytes;
            } while (totalbytes < buffer.Length);
            //Console.WriteLine("Read " + totalbytes + " bytes");
            message.Payload = buffer;
            return message;
        }

        /// <summary>
        /// Send a message to the network stream
        /// </summary>
        public static void SendMessage(NetworkStream stream, Message message)
        {
            message.Header.Size = message.Payload.Length;
            var header = message.Header.Serialize();
            stream.Write(header, 0, header.Length);
            stream.Write(message.Payload, 0, message.Payload.Length);
            stream.Flush();
        }
    }
}


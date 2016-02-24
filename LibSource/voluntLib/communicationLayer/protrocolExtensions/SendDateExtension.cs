using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using voluntLib.communicationLayer.messages.messageWithCertificate;

namespace voluntLib.communicationLayer.protrocolExtensions
{
    public class SendDateExtension : AExtension
    {

        public SendDateExtension() : base("asd", new List<MessageType> { MessageType.All })
        {}

        public override byte[] GetData()
        {
            return BitConverter.GetBytes(DateTime.Now.Millisecond);
        }

        public override void OnReceive(byte[] data)
        {
            var ms = BitConverter.ToInt32(data, 0);
            var dateOfMessage = new DateTime(ms);
        }
    }
}

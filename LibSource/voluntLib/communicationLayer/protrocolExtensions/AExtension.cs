using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using voluntLib.communicationLayer.messages.messageWithCertificate;

namespace voluntLib.communicationLayer.protrocolExtensions
{
    public abstract class AExtension
    {
        /// <summary>
        /// unique extrention key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Spezify messageTypes the extrention should be added to.
        /// mind MessageType.All
        /// </summary>
        public List<MessageType> MessageTypes { get; private set; }

        protected AExtension(string key, List<MessageType> messageTypes)
        {
            this.Key = key;
            this.MessageTypes = messageTypes;
        }
        

        /// <summary>
        /// This method will be called whenever a message of the spezified typ is about to send.        /// 
        /// </summary>
        /// <returns></returns>
        abstract public byte[] GetData();

        /// <summary>
        /// This method suppose to handle any incomming message containing this extention
        /// </summary>
        /// <param name="data"></param>
        abstract public void OnReceive(byte[] data);

    }
}

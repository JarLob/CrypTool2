/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BitcoinBlockChainAnalyser
{

    public class BlockChainDownloader
    {

        public static String HighestBlockDownloader(NetworkStream networkStream)
        {
            //Message create with GetblockcountRequestMessage
            //no further information needed
            var message = new Message();
            byte[] byteArray = Encoding.ASCII.GetBytes("HighestNumber");

            message.Header.MessageType = MessageType.GetblockcountRequestMessage;
            message.Payload = byteArray;

            Message.SendMessage(networkStream, message);

            Message response = Message.ReceiveMessage(networkStream);
            String s = System.Text.Encoding.UTF8.GetString(response.Payload, 0, response.Header.Size);

            return s;
        }

        public static String BlockDownloader(NetworkStream networkStream, String payload)
        {
            //Message create with Blocknumber and GetblockRequestMessage
            //add blocknumber as payload to the message
            var message = new Message();
            byte[] byteArray = Encoding.ASCII.GetBytes(payload);

            message.Header.MessageType = MessageType.GetblockRequestMessage;
            message.Payload = byteArray;

            Message.SendMessage(networkStream, message);

            Message response = Message.ReceiveMessage(networkStream);
            String s = System.Text.Encoding.UTF8.GetString(response.Payload, 0, response.Header.Size);

            return s;
        }
        //Message create with transactionhash and GettransactionRequestMessage
        //add transactionhash as payload to the message
        public static String TransactionDownloader(NetworkStream networkStream, String payload)
        {

            var message = new Message();
            byte[] byteArray = Encoding.ASCII.GetBytes(payload);

            message.Header.MessageType = MessageType.GettransactionRequestMessage;
            message.Payload = byteArray;

            Message.SendMessage(networkStream, message);

            Message response = Message.ReceiveMessage(networkStream);
            String s = System.Text.Encoding.UTF8.GetString(response.Payload, 0, response.Header.Size);

            return s;
        }

        //Message create with transactionhash, vout id and GettransactionRequestMessage
        //add transactionhash as payload to the message
        //only return the vout information with the vout id
        public static String TxoutDownloader(NetworkStream networkStream, String hash, int vout)
        {
            var message = new Message();
            byte[] byteArray = Encoding.ASCII.GetBytes(hash);

            message.Header.MessageType = MessageType.GettransactionRequestMessage;
            message.Payload = byteArray;

            Message.SendMessage(networkStream, message);

            Message response = Message.ReceiveMessage(networkStream);
            String s = System.Text.Encoding.UTF8.GetString(response.Payload, 0, response.Header.Size);

            if (!s.Equals(""))
            {
                JObject joeResult = JObject.Parse(s);
                JObject joeTransaction = JObject.Parse(joeResult.GetValue("result").ToString());
                JArray joeVout = JArray.Parse(joeTransaction.GetValue("vout").ToString());
                s = joeVout[vout].ToString();
            }

            return s;
        }

    }
}

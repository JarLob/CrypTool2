using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using BitcoinBlockChainAnalyser;

namespace BitcoinDownloadServer
{
    class Server
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string genesisTransaction = "{\"result\":{\"txid\":\"4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b\",\"hash\":\"4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b\",\"version\":1,\"size\":285,\"vsize\":285,\"locktime\":0,\"vin\":[{\"coinbase\":\"04ffff001d0104\",\"sequence\":4294967295}],\"vout\":[{\"value\":50.00000000,\"n\":0,\"scriptPubKey\":{\"asm\":\"0496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeOP_CHECKSIG\",\"hex\":\"410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac\",\"reqSigs\":1,\"type\":\"pubkey\",\"addresses\":[\"1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa\"]}}],\"hex\":\"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000\",\"blockhash\":\"000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f\",\"confirmations\":531370,\"time\":1231006505,\"blocktime\":1231006505},\"error\":null,\"id\":null}";

        /*
         * This method creates a listener so that the requests from CT2 can be accepted and processed
         */
        static void Main(string[] args)
        {

            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
                listener.Start();
                log.Info("Service was started");

                while (true)
                {
                    log.Info("Waiting for incoming client connections...");
                    TcpClient client = listener.AcceptTcpClient();
                    log.Info("Accepted new Client connection...");
                    Thread thread = new Thread(ClientRequests);
                    thread.IsBackground = true;
                    thread.Start(client);
                }

            }
            catch (Exception e)
            {
                log.Error("Error while starting the Service: "+ e.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }

        }

         /*
         * This method controls the client requests and uses the message type to decide 
         * which additional method is called to retrieve data from the server.
         */
        private static void ClientRequests(object argument)
        {
            TcpClient client = (TcpClient)argument;
            try
            {
                var networkStream = client.GetStream();
                var message = new Message();
                do
                {
                    message = Message.ReceiveMessage(networkStream);
                    String s = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Header.Size);

                    Message response = null;

                    if (message.Header.MessageType == MessageType.GetblockcountRequestMessage)
                    {
                        response = Getblockcount();
                    }
                    else if (message.Header.MessageType == MessageType.GetblockRequestMessage)
                    {
                        if(int.TryParse(s, out int number))
                        {
                            string hash = Getblockhash(number);
                            response = Getblock(hash);
                        }
                        else
                        {
                            response = Getblock(s);
                        }
                        
                    }
                    else if (message.Header.MessageType == MessageType.GettransactionRequestMessage)
                    {
                        response = GetTransaction(s);
                    }else if (message.Header.MessageType == MessageType.GettxoutRequestMessage)
                    {
                        response = Gettxout(s);
                    }
                    
                    Message.SendMessage(networkStream, response);


                } while (client.Connected);
                //close the complete client connection
                client.Close();
                log.Info("Closing client Connection!");
            }
            catch (Exception e)
            {
                client.Close();
                log.Info("Exiting thread: "+e.Message);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        /*
         * This method establishes the connection to the Blockchain server
         * return webRequest
         */
        private static HttpWebRequest ConnectToServer()
        {
            // The properties for server connection
            HttpWebRequest webRequest = null;
            try
            {
                string ServerIp = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiServerUrl;
                string UserName = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiUsername;
                string Password = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiPassword;

                webRequest = (HttpWebRequest)WebRequest.Create(ServerIp);
                webRequest.Credentials = new NetworkCredential(UserName, Password);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";

                return webRequest;
            }catch(Exception e)
            {
                log.Error("ConnectToServer(): "+e.Message);
                return webRequest;
            }
        }

        /*
         * This method defines the beginning of the beginning of the message
         * return Json Object
         */
        private static JObject BitcoinApiCall(string methodName)
        {
            JObject joe = new JObject();
            joe.Add(new JProperty("jsonrpc", "1.0"));
            joe.Add(new JProperty("id", "1"));
            joe.Add(new JProperty("method", methodName));

            return joe;
        }


        /*
         * The AddParameter methods can be used to attach a wide variety of parameters.
         * This is necessary because the parameters differ in the variable type from request to request.
         * return Json Object
         */
        private static JObject AddParameter(string parameter, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(int parameter, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(string parameter, bool value , JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            props.Add(value);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(string parameter,int vout, bool value, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            props.Add(vout);
            props.Add(value);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        /*
         * This method serializes and sends the data to the blockchain server and receives the response. 
         */
        private static string SendRequestAndGetResponse(JObject joe, HttpWebRequest webRequest)
        {

            try
            {
                // serialize JSON for request
                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse webResponse = null;
                // deserialze the response
                StreamReader sReader = null;

                webResponse = webRequest.GetResponse();

                //WebResponse webResponse = webRequest.GetResponse();
                sReader = new StreamReader(webResponse.GetResponseStream(), true);
                string responseValue = sReader.ReadToEnd();
                return responseValue;
            }
            catch (Exception e)
            {
                log.Error("SendRequestAndGetResponse(): " + e.Message);
                return "";
            }
        }

        /*
         * The following methods generate the various Api calls and use the methods available above.
         * 
         */

        //Returns the number of the most recent block
        private static Message Getblockcount()
        {
            Message message = new Message();
            try
            {
                //send a request and get a response from api
                HttpWebRequest webRequest = ConnectToServer();
                JObject joe = BitcoinApiCall("getblockcount");
                string apiResponse = SendRequestAndGetResponse(joe, webRequest);
                JObject getblockcount = JObject.Parse(apiResponse);
                string clientResponse = (string)getblockcount.GetValue("result");
                //create a message

                byte[] byteArray = Encoding.ASCII.GetBytes(clientResponse);
                message.Header.MessageType = MessageType.GetblockcountResponseMessage;
                message.Payload = byteArray;
                
            }
            catch (Exception e)
            {
                log.Error("Error while executing Getblockcount method: " + e.Message);
            }
            return message;
        }

        //gets the block hash matching the block number
        private static string Getblockhash(int number)
        {

            try
            {
                //Step 1: we need the blockhash from the blocknumber
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeHash = BitcoinApiCall("getblockhash");
                joeHash = AddParameter(number, joeHash);
                string hash = SendRequestAndGetResponse(joeHash, webRequest);
                JObject getblockhash = JObject.Parse(hash);

                //bockhash are written in the result parameter of the json string
                return getblockhash.GetValue("result").ToString();
            }
            catch (Exception e)
            {
                log.Error("Error while executing Getblockhash method: "+e.Message);
                return "Error";
            }

        }

        //gets the block data by specifying the block hash
        private static Message Getblock(string hash)
        {
            Message message = new Message();
            try
            {
                //we can push the block informations with the hash result
                JObject joeBlock = BitcoinApiCall("getblock");
                joeBlock = AddParameter(hash, joeBlock);
                HttpWebRequest webRequest2 = ConnectToServer();
                string getblock = SendRequestAndGetResponse(joeBlock, webRequest2);

                //create a message
                byte[] byteArray = Encoding.ASCII.GetBytes(getblock);
                message.Header.MessageType = MessageType.GetblockResponseMessage;
                message.Payload = byteArray;
            }
            catch (Exception e)
            {
                log.Error("Error while executing Getblock method: "+e.Message);
            }
            return message;
        }

        //gets the transaction data by specifying the transaction hash
        private static Message GetTransaction(string hash)
        {
            Message message = new Message();
            try
            {
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeTransaction = BitcoinApiCall("getrawtransaction");
                string getTransaction;
                if (hash.Equals("4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b"))
                {
                    getTransaction = genesisTransaction;
                }
                else
                {
                    joeTransaction = AddParameter(hash, true, joeTransaction);
                    getTransaction = SendRequestAndGetResponse(joeTransaction, webRequest);
                }

                //create a message
                byte[] byteArray = Encoding.ASCII.GetBytes(getTransaction);
                message.Header.MessageType = MessageType.GettransactionResponseMessage;
                message.Payload = byteArray;
            }
            catch (Exception e)
            {
                log.Error("Error while executing GetTransaction method: " + e.Message);
            }

            return message;
        }

        //Get only the information from an output of a transaction
        private static Message Gettxout(string payload)
        {
            Message message = new Message();
            try
            {
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeTxOut = BitcoinApiCall("gettxout");
                JObject joePayload = JObject.Parse(payload);

                joeTxOut = AddParameter(joePayload.GetValue("txid").ToString(), (int)joePayload.GetValue("vout"), true, joeTxOut);
                string getTxOut = SendRequestAndGetResponse(joeTxOut, webRequest);

                //create a message
                byte[] byteArray = Encoding.ASCII.GetBytes(getTxOut);
                message.Header.MessageType = MessageType.GettxoutResponsetMessage;
                message.Payload = byteArray;
            }
            catch (Exception e)
            {
                log.Error("Error while executing Gettxout method: " + e.Message);
            }

            return message;
        }

    }
}

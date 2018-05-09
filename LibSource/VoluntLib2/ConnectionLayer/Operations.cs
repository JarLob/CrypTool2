﻿/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VoluntLib2.ConnectionLayer.Messages;
using VoluntLib2.Tools;

namespace VoluntLib2.ConnectionLayer.Operations
{
    /// <summary>
    /// Abstract super class of all operations:
    /// Operations are state machines that send messages and work on received message using the ConnectionManager
    /// If an operation's IsFinished equals true, it can be deleted by ConnectionManager
    /// </summary>
    internal abstract class Operation
    {
        /// <summary>
        /// Needed by each operation for message sending, updates of contacts, etc
        /// </summary>
        public ConnectionManager ConnectionManager { get; set; }

        /// <summary>
        /// Tells the worker thread if this operation is finished. If it is, it can be deleted
        /// </summary>
        public abstract bool IsFinished { get; }

        /// <summary>
        /// Called by the worker thread (cooperative multitasking)
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Each message is handed by the ConnectionManager to this operation calling this method
        /// </summary>
        /// <param name="message"></param>
        public abstract void HandleMessage(Message message);
    }

    /// <summary>
    /// This operation continuesly sends HelloMessages to a given IP/Port.
    /// It does this every 5 seconds until 30 seconds are reached
    /// If it receives a corresponding HelloResponseMessage it updates the contact at the ConnectionManager
    /// Then it can be deleted ( IsFinished = true)
    /// If this operations fails for 30 seconds, it can also be deleted (IsFinished = true)
    /// </summary>
    internal class HelloOperation : Operation
    {
        private const long TIMEOUT = 30000;
        private const long RETRY_TIMESPAN = 5000;

        private Logger Logger = Logger.GetLogger();

        private enum State
        {
            Started,
            WaitHelloResponse,
            Finished
        }

        private State MyState = State.Started;
        private ushort Port;
        private IPAddress IP;
        private byte[] HelloNonce = Guid.NewGuid().ToByteArray();
        private DateTime LastHelloSendTime;
        private DateTime CreationTime = DateTime.Now;

        /// <summary>
        /// Creates a HelloOperation
        /// It needs an IPAddress and a port which are the target of the operation
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public HelloOperation(IPAddress ip, ushort port)
        {
            this.IP = ip;
            this.Port = port;
        }

        /// <summary>
        /// A HelloOperation is finished if
        /// (A) it was successfull; i.e. it received a HelloResponse  OR
        /// (B) if it received for 30 seconds NO HelloResponse
        /// </summary>
        public override bool IsFinished
        {
            get { return MyState == State.Finished || DateTime.Now > CreationTime.AddMilliseconds(TIMEOUT); }
        }

        /// <summary>
        /// Called by the ConnectionManager to execute the operation
        /// </summary>
        public override void Execute()
        {
            switch (MyState)
            {
                case State.Started:
                    HandleStarted();
                    break;
                case State.WaitHelloResponse:
                    HandleWaitHelloResponse();
                    break;
                case State.Finished:
                    //we do nothing when finished
                    break;
            }
        }

        /// <summary>
        /// Only called once at the start
        /// State then goes to WaitHelloResponse
        /// and it sends the first HelloMessage
        /// </summary>
        private void HandleStarted()
        {
            //Initially, send a hello message
            ConnectionManager.SendHello(IP, Port, HelloNonce);
            LastHelloSendTime = DateTime.Now;
            //Then wait for the response
            MyState = State.WaitHelloResponse;
        }

        /// <summary>
        /// In this state, it resends HelloMessages for 30 seconds
        /// </summary>
        private void HandleWaitHelloResponse()
        {
            //If we are here, we did not receive a response
            //thus, we send the hello again; but only every RETRY_TIMESPAN ms

            if (DateTime.Now < LastHelloSendTime.AddMilliseconds(RETRY_TIMESPAN))
            {
                return;
            }
            ConnectionManager.SendHello(IP, Port, HelloNonce);
            LastHelloSendTime = DateTime.Now;
            //Then wait for the response
            MyState = State.WaitHelloResponse;
        }

        /// <summary>
        /// Checks each HelloResponseMessage if it belongs to this operation
        /// If it does, the operation was successfull
        /// Then, it updates the contact
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is HelloResponseMessage)
            {
                HelloResponseMessage helloResponseMessage = (HelloResponseMessage)message;
                //check if nonces are equal
                for (int i = 0; i < 16; i++)
                {
                    if (this.HelloNonce[i] != helloResponseMessage.HelloResponseNonce[i])
                    {
                        return;
                    }
                }
                //here, we know, that we received a HelloResponse for this operation
                Logger.LogText("Got a HelloResponseMessage for my HelloMessage", this, Logtype.Debug);
                MyState = State.Finished;
            }
        }       

        /// <summary>
        /// Returns the IP endpoint this operation is referring to
        /// </summary>
        /// <returns></returns>
        public IPEndPoint GetReferringIpEndpoint()
        {
            return new IPEndPoint(IP, Port);
        }

        /// <summary>
        /// Sets this HelloOperation to finished
        /// </summary>
        public void SetFinished()
        {
            MyState = State.Finished;
        }
    }

    /// <summary>
    /// Never expiring operation that answers a HelloResponseMessage to each received HelloMessage
    /// </summary>
    internal class HelloResponseOperation : Operation
    {
        private ConcurrentQueue<HelloMessage> HelloMessages = new ConcurrentQueue<HelloMessage>();
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HelloResponseOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Answer to each HelloMessage a HelloResponseMessage with the corresponding nonce
        /// </summary>
        public override void Execute()
        {
            HelloMessage message;
            while (HelloMessages.TryDequeue(out message) == true)
            {                
                try
                {
                    //then, we answer with a HelloResponseMessage
                    ConnectionManager.SendHelloResponse(new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort, message.HelloNonce);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception occured during sending of HelloResponse: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// If it gets a HelloMessage it puts it into its internal message queue
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is HelloMessage)
            {
                HelloMessages.Enqueue((HelloMessage)message);
            }
        }
    }

    /// <summary>
    /// Checks, if a contact did not send a message within 30 seconds; then creates a HelloMessage
    /// if nothing is received in 5 minutes, the contact is set to offline
    /// if nothing is received within 24h, the contact is removed
    /// </summary>
    internal class CheckContactsOperation : Operation
    {
        private const int SAY_HELLO_INTERVAL = 30000; //30 seconds
        private const int SET_CONTACT_OFFLINE = 300000; //5 minutes
        private const int REMOVE_OFFLINE_CONTACT = 86400000; //24 hours

        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The CheckContactsOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Every 30 seconds this operation creates a HelloOperation for each contact.
        /// If a contact did not respond in 5 minutes, we assume it is offline.
        /// If a contact did not say anything for 24h, we delete the contact.
        /// </summary>
        public override void Execute()
        {
            //Check all of our direct neighbors;
            IPEndPoint removeKey = null;
            foreach (var keyvaluepair in ConnectionManager.Contacts)
            {
                if (keyvaluepair.Value.IsOffline == false && DateTime.Now > keyvaluepair.Value.LastSeen.AddMilliseconds(SAY_HELLO_INTERVAL) && DateTime.Now > keyvaluepair.Value.LastHelloSent.AddMilliseconds(SAY_HELLO_INTERVAL))
                {
                    HelloOperation operation = new HelloOperation(keyvaluepair.Value.IPAddress, keyvaluepair.Value.Port) { ConnectionManager = ConnectionManager };
                    ConnectionManager.Operations.Enqueue(operation);
                    keyvaluepair.Value.LastHelloSent = DateTime.Now;
                    Logger.LogText(String.Format("Created HelloOperation for contact {0}:{1} because did not see him in a while...", keyvaluepair.Value.IPAddress, keyvaluepair.Value.Port), this, Logtype.Debug);
                }
                if (keyvaluepair.Value.IsOffline == false && DateTime.Now > keyvaluepair.Value.LastSeen.AddMilliseconds(SET_CONTACT_OFFLINE))
                {
                    keyvaluepair.Value.IsOffline = true;
                    Logger.LogText(String.Format("Set contact {0}:{1} to offline because did not see him in a while...", keyvaluepair.Value.IPAddress, keyvaluepair.Value.Port), this, Logtype.Debug);
                }
                if (keyvaluepair.Value.IsOffline == true && DateTime.Now > keyvaluepair.Value.LastSeen.AddMilliseconds(REMOVE_OFFLINE_CONTACT))
                {
                    removeKey = keyvaluepair.Key;
                }
            }
            if (removeKey != null)
            {
                Contact contact;
                if (ConnectionManager.Contacts.TryRemove(removeKey, out contact))
                {
                    Logger.LogText(String.Format("Removed contact {0}:{1} because did not see him in a long while...", contact.IPAddress, contact.Port), this, Logtype.Debug);
                }
                else
                {
                    Logger.LogText(String.Format("Could not remove contact {0}:{1} from contacts. TryRemove returned false.", contact.IPAddress, contact.Port), this, Logtype.Debug);
                }
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //we do nothing with messages...
        }
    }

    /// <summary>
    /// Operation for asking a dedicated neighbor for a neighborlist
    /// </summary>
    internal class RequestNeighborListOperation : Operation
    {

        private const long TIMEOUT = 30000;
        private const long RETRY_TIMESPAN = 5000;

        private Logger Logger = Logger.GetLogger();

        private enum State
        {
            Started,
            WaitResponseNeighborList,
            Finished
        }

        private State MyState = State.Started;
        private ushort Port;
        private IPAddress IP;
        private byte[] RequestNeighborListNonce = Guid.NewGuid().ToByteArray();
        private DateTime LastRequestSendTime;
        private DateTime CreationTime = DateTime.Now;

        /// <summary>
        /// Creates a RequestNeighborListOperation
        /// It needs an IPAddress and a port which are the target of the operation
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public RequestNeighborListOperation(IPAddress ip, ushort port)
        {
            this.IP = ip;
            this.Port = port;
        }

        /// <summary>
        /// A RequestNeighborListOperation is finished if
        /// (A) it was successfull; i.e. it received a ResponseNeighborListMessage  OR
        /// (B) if it received for 30 seconds NO ResponseNeighborListMessage
        /// </summary>
        public override bool IsFinished
        {
            get { return MyState == State.Finished || DateTime.Now > CreationTime.AddMilliseconds(TIMEOUT); }
        }

        /// <summary>
        /// Called by the ConnectionManager to execute the operation
        /// </summary>
        public override void Execute()
        {
            switch (MyState)
            {
                case State.Started:
                    HandleStarted();
                    break;
                case State.WaitResponseNeighborList:
                    HandleWaitResponseNeighborList();
                    break;
                case State.Finished:
                    //we do nothing when finished
                    break;
            }
        }

        /// <summary>
        /// Only called once at the start
        /// State then goes to WaitHelloResponse
        /// it sends the first HelloMessage
        /// </summary>
        private void HandleStarted()
        {
            //Initially, send a request neighborlist message
            ConnectionManager.SendRequestNeighborListMessage(IP, Port, RequestNeighborListNonce);
            LastRequestSendTime = DateTime.Now;
            //Then wait for the response
            MyState = State.WaitResponseNeighborList;
        }

        /// <summary>
        /// In this state, it resends HelloMessages for 30 seconds
        /// </summary>
        private void HandleWaitResponseNeighborList()
        {
            //If we are here, we did not receive a response
            //thus, we send the request again; but only every RETRY_TIMESPAN ms
            if (DateTime.Now < LastRequestSendTime.AddMilliseconds(RETRY_TIMESPAN))
            {
                return;
            }
            ConnectionManager.SendRequestNeighborListMessage(IP, Port, RequestNeighborListNonce);
            LastRequestSendTime = DateTime.Now;
            //Then wait for the response
            MyState = State.WaitResponseNeighborList;
        }

        /// <summary>
        /// Checks each ResponseNeighborListMessage if it belongs to this operation
        /// If it does, the operation was successfull
        /// Then, it updates the contact
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is ResponseNeighborListMessage)
            {
                ResponseNeighborListMessage responseNeighborListMessage = (ResponseNeighborListMessage)message;
                //check if nonces are equal
                for (int i = 0; i < 16; i++)
                {
                    if (this.RequestNeighborListNonce[i] != responseNeighborListMessage.ResponseNeighborListNonce[i])
                    {
                        return;
                    }
                }
                //here, we know, that we received a ResponseNeighborListMessage for this operation
                Logger.LogText("Got a ResponseNeighborListMessage for my RequestNeighborListMessage", this, Logtype.Debug);
                MyState = State.Finished;

                IPEndPoint receivedFromKey = new IPEndPoint(new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort);
                Contact receivedFromContact = ConnectionManager.Contacts[receivedFromKey];
                foreach (Contact contact in responseNeighborListMessage.Neighbors)
                {
                    if (IpTools.IsPrivateIP(contact.IPAddress))
                    {
                        //Someone sent us a private IP; we can not do anything with that address; so dont save it
                        continue;
                    }
                    IPEndPoint contactEndpoint = new IPEndPoint(contact.IPAddress, contact.Port);
                    if (ConnectionManager.Contacts.ContainsKey(contactEndpoint) && ConnectionManager.Contacts[contactEndpoint].IsOffline == false)
                    {
                        //if we are already connected to it, we dont need to store it
                        //if we have it in our contacts and it is offline, we additionally add it to the received contacts
                        continue;
                    }
                    //if we dont know the contact add it                    
                    if (!ConnectionManager.ReceivedContacts.ContainsKey(contactEndpoint))
                    {
                        ConnectionManager.ReceivedContacts.TryAdd(contactEndpoint, contact);
                    }
                    //add the sender to the KnownByList of this contact
                    if (!ConnectionManager.ReceivedContacts[contactEndpoint].KnownBy.ContainsKey(receivedFromKey))
                    {
                        ConnectionManager.ReceivedContacts[contactEndpoint].KnownBy.TryAdd(receivedFromKey, receivedFromContact);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the IPEndpoint this operation refers to
        /// </summary>
        /// <returns></returns>
        public IPEndPoint GetReferringIpEndpoint()
        {
            return new IPEndPoint(IP, Port);
        }

        /// <summary>
        /// Sets this operation to finished
        /// </summary>
        public void SetFinished()
        {
            MyState = State.Finished;
        }
    }
    /// <summary>
    /// Never expiring operation that answers a ResponseNeighborListMessage to each received HelloMessage
    /// </summary>
    internal class ResponseNeighborListOperation : Operation
    {
        private ConcurrentQueue<RequestNeighborListMessage> RequestNeighborListMessages = new ConcurrentQueue<RequestNeighborListMessage>();
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The ResponseNeighborListOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Answer to each RequestNeighborLista ResponseNeighborListMessage with the corresponding nonce
        /// </summary>
        public override void Execute()
        {
            RequestNeighborListMessage message;
            while (RequestNeighborListMessages.TryDequeue(out message) == true)
            {               
                try
                {
                    List<Contact> contacts = new List<Contact>();
                    foreach(Contact contact in ConnectionManager.Contacts.Values)
                    {
                        if (IpTools.IsPrivateIP(contact.IPAddress))
                        {
                            //we dont send private ips
                            continue;
                        }
                        if (contact.IsOffline == true)
                        {
                            //we also dont send offline contacts
                            continue;
                        }
                        if (contact.IPAddress.Equals(new IPAddress(message.MessageHeader.SenderIPAddress)) && contact.Port == message.MessageHeader.SenderExternalPort)
                        {
                            //we also dont send the contact of the request itself
                            continue;
                        }
                        contacts.Add(contact);
                    }
                    //then, we answer with a ResponseNeighborListMessage
                    //we do this EVEN if we have none; thus, the requester will stop his RequestNeighborListOperation
                    ConnectionManager.SendResponseNeighborList(new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort, message.RequestNeighborListNonce, contacts);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception occured during sending of ResponseNeighborList: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// If it gets a RequestNeighborListit puts it into its internal message queue
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is RequestNeighborListMessage)
            {
                RequestNeighborListMessages.Enqueue((RequestNeighborListMessage)message);
            }
        }
    }

    /// <summary>
    /// Loggs the number of connections every 5 seconds ONLY if the number changed
    /// </summary>
    internal class MyStatusOperation : Operation
    {
        private const uint STATUS_SHOW_INTERVAL = 5000; // 5sec
        private DateTime LastStatusShownTime = DateTime.Now;
        private Logger Logger = Logger.GetLogger();
        private ushort LastConnectionCount = ushort.MaxValue;

        /// <summary>
        /// The MyStatusOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Loggs the number of connections every 5 seconds ONLY if the number changed
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastStatusShownTime.AddMilliseconds(STATUS_SHOW_INTERVAL))
            {
                LastStatusShownTime = DateTime.Now;
                ushort connectionCount = ConnectionManager.GetConnectionCount();
                if (connectionCount != LastConnectionCount)
                {
                    LastConnectionCount = connectionCount;
                    Logger.LogText(String.Format("Number of connections changed! I am currently connected to {0} peer(s)!", connectionCount), this, Logtype.Info);
                    foreach (var keyvalue in ConnectionManager.Contacts)
                    {
                        if (keyvalue.Value.IsOffline == false)
                        {
                            Logger.LogText(String.Format("Connected to {0}:{1}", keyvalue.Value.IPAddress, keyvalue.Value.Port), this, Logtype.Info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }

    /// <summary>
    /// This operation creats a set of RequestNeighborListOperation every 5 minutes for all of our contacts
    /// </summary>
    internal class AskForNeighborListsOperation : Operation
    {
        private const int ASK_FOR_NEIGHBORLIST_INTERVAL = 300000; // 5 minutes
        private DateTime LastTimeAsked = DateTime.Now;
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The AskForNeighborListsOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            if (DateTime.Now > LastTimeAsked.AddMilliseconds(ASK_FOR_NEIGHBORLIST_INTERVAL))
            {
                Logger.LogText("Requesting neighbor lists from my contacts now", this, Logtype.Debug);
                foreach (var entry in ConnectionManager.Contacts)
                {
                    //we only asks contacts that are online
                    if (entry.Value.IsOffline == false)
                    {
                        Logger.LogText(String.Format("Creating RequestNeighborListOperation for {0}:{1}", entry.Value.IPAddress, entry.Value.Port), this, Logtype.Debug);
                        RequestNeighborListOperation requestNeighborListOperation = new RequestNeighborListOperation(entry.Value.IPAddress, entry.Value.Port) { ConnectionManager = ConnectionManager };
                        ConnectionManager.Operations.Enqueue(requestNeighborListOperation);
                    }
                }
                LastTimeAsked = DateTime.Now;
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }
    
    /// <summary>
    /// This operation is for asking someone to help to connect to another one
    /// </summary>
    internal class TryCreateNewConnectionOperation : Operation
    {
        private bool isFinished = false;
        private Random Random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The TryCreateNewConnectionOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return isFinished; }
        }

        public override void Execute()
        {
            if (ConnectionManager.ReceivedContacts.Count == 0)
            {
                //we have non to connect to...
                isFinished = true;
                return;
            }
            //randomly select a contact of the received contacts
            int count = ConnectionManager.ReceivedContacts.Count;
            int index = Random.Next(0, count - 1);            
            IPEndPoint contactEndpoint = null;
            int i = 0;
            foreach (var entry in ConnectionManager.ReceivedContacts)
            {
                contactEndpoint = entry.Key;                
                if (i == index)
                {
                    break;
                }
                i++;
            }
            
            Contact contact;
            if (ConnectionManager.ReceivedContacts.TryRemove(contactEndpoint, out contact) == true)                
            {
                Logger.LogText(String.Format("Trying to connect to {0}:{1}", contact.IPAddress, contact.Port), this, Logtype.Debug);
                //Send to everyone who is online and knows the contact a HelpMeConnectMessage
                bool sendedAtLeastOnce = false;
                foreach (var entry in contact.KnownBy)
                {
                    if (entry.Value.IsOffline == false)
                    {
                        ConnectionManager.SendHelpMeConnectMessage(entry.Value.IPAddress, entry.Value.Port, contact.IPAddress, contact.Port);
                        Logger.LogText(String.Format("Asked {0}:{1} for help", entry.Value.IPAddress, entry.Value.Port), this, Logtype.Debug);
                        sendedAtLeastOnce = true;
                    }
                }
                //Then start sending HelloMessages to the new contact using a HelloOperation  
                //We only do this, if we send it at least once to a neighbor      
                if (sendedAtLeastOnce)
                {
                    HelloOperation helloOperation = new HelloOperation(contact.IPAddress, contact.Port) { ConnectionManager = ConnectionManager };
                    ConnectionManager.Operations.Enqueue(helloOperation);
                }
                //finally we are finished            
            }
            else
            {
                Logger.LogText(String.Format("Could not remove {0}:{1} from ReceivedContactsList", contactEndpoint.Address, contactEndpoint.Port), this, Logtype.Debug);
            }
            isFinished = true;
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }

    /// <summary>
    /// This operation tries to help peers to connec to other peers by sending WantsConnectionMessages
    /// </summary>
    internal class HelpWithConnectionOperation : Operation
    {
        private ConcurrentQueue<HelpMeConnectMessage> HelpMeConnectMessages = new ConcurrentQueue<HelpMeConnectMessage>();
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HelpWithConnectionOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Send WantsConnectionMessages
        /// </summary>
        public override void Execute()
        {
            HelpMeConnectMessage message;
            while (HelpMeConnectMessages.TryDequeue(out message) == true)
            {
                Logger.LogText(String.Format("Trying to help {0}:{1} to connect to {2}:{3}", new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort, message.IPAddress, message.Port), this, Logtype.Debug);
                ConnectionManager.SendWantsConnectionMessage(message.IPAddress, message.Port, new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort);
            }
        }

        /// <summary>
        /// Handles only HelpMeConnectMessages
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            // we only work on HelpMeConnectMessages
            if (message is HelpMeConnectMessage)
            {
                HelpMeConnectMessage helpMeConnectMessage = (HelpMeConnectMessage)message;
                HelpMeConnectMessages.Enqueue(helpMeConnectMessage);
            }
        }
    }

    /// <summary>
    /// Handles WantsConnectionMessage by creating HelloOperations 
    /// </summary>
    internal class WantsConnectionOperation : Operation
    {
        private ConcurrentQueue<WantsConnectionMessage> WantsConnectionMessages = new ConcurrentQueue<WantsConnectionMessage>();
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HelpWithConnectionOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Send WantsConnectionMessages
        /// </summary>
        public override void Execute()
        {
            WantsConnectionMessage message;
            while (WantsConnectionMessages.TryDequeue(out message) == true)
            {
                Logger.LogText(String.Format("The peer {0}:{1} wants to connect to me. Created HelloOperation for him.", message.IPAddress, message.Port), this, Logtype.Debug);
                HelloOperation helloOperation = new HelloOperation(message.IPAddress, message.Port) { ConnectionManager = ConnectionManager };
                ConnectionManager.Operations.Enqueue(helloOperation);
            }
        }

        /// <summary>
        /// Handles only WantsConnectionMessage
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            // we only work on WantsConnectionMessage
            if (message is WantsConnectionMessage)
            {
                WantsConnectionMessage wantsConnectionMessage = (WantsConnectionMessage)message;
                WantsConnectionMessages.Enqueue(wantsConnectionMessage);
            }
        }
    }

    /// <summary>
    /// Checks the current number of connections
    /// If it is below 10, it creates a TryCreateNewConnectionOperation
    /// If it is above 20, it removes a randomly chosen peer from our neighbors
    /// </summary>
    internal class CheckMyConnectionsNumberOperation : Operation
    {
        private const int CHECK_CONNECTIONS_INTERVAL = 30000; //30 sec 
        private const int MIN_CONNECTIONS_NUMBER = 10;
        private const int MAX_CONNECTIONS_NUMBER = 20;

        private Logger Logger = Logger.GetLogger();
        private DateTime LastCheckedTime = DateTime.Now;
        private Random Random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));

        /// <summary>
        /// The CheckMyConnectionsNumber never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            if (DateTime.Now > LastCheckedTime.AddMilliseconds(CHECK_CONNECTIONS_INTERVAL))
            {
                ushort connectionCount = ConnectionManager.GetConnectionCount();
                if (connectionCount == 0)
                {
                    //does not make sense to try to get a new connection using someone if we are not connected to anyone
                    LastCheckedTime = DateTime.Now;
                    return;
                }
                if (connectionCount < MIN_CONNECTIONS_NUMBER)
                {
                    //if we have too few connections, we start a tryCreateNewConnectionOperation to get an additional connection
                    Logger.LogText(String.Format("Not enough connections. We have {0} but want {1}. Created a TryCreateNewConnectionOperation.", connectionCount, MIN_CONNECTIONS_NUMBER), this, Logtype.Debug);
                    TryCreateNewConnectionOperation tryCreateNewConnectionOperation = new TryCreateNewConnectionOperation() { ConnectionManager = ConnectionManager };
                    ConnectionManager.Operations.Enqueue(tryCreateNewConnectionOperation);                    
                }
                if (connectionCount > MAX_CONNECTIONS_NUMBER)
                {
                    //if we have too much connections, we send one randomly chosen peer that we go offline. 
                    //Additionally, we set him to offline and remove all HelloOperations and all RequestNeighborListOperations

                    //todo:
                    //0. Randomly select a neighbor                    
                    int count = ConnectionManager.Contacts.Count;
                    int index = Random.Next(0, count - 1);
                    IPEndPoint contactEndpoint = null;
                    int i = 0;
                    foreach (var entry in ConnectionManager.Contacts)
                    {
                        contactEndpoint = entry.Key;
                        if (i == index)
                        {
                            break;
                        }
                        i++;
                    }
                    Contact contact =  ConnectionManager.Contacts[contactEndpoint];
                    Logger.LogText(String.Format("Too many connections. We have {0} but want a maximum of {1}. Remove {2}:{3} now", connectionCount, MAX_CONNECTIONS_NUMBER, contact.IPAddress, contact.Port), this, Logtype.Debug);
                    
                    //1. Send GoingOfflineMessage
                    ConnectionManager.SendGoingOfflineMessage(contact.IPAddress, contact.Port);
                    
                    //2. Set that peer to offline
                    contact.IsOffline = true;

                    //3. remove all referring messages (HelloRequestMessages and RequestNeighborListMessages)
                    //remove all HelloOperations
                    //and remove all RequestNeighborListOperations
                    foreach (Operation operation in ConnectionManager.Operations)
                    {
                        if (operation is HelloOperation)
                        {
                            HelloOperation helloOperation = (HelloOperation)operation;
                            if (helloOperation.GetReferringIpEndpoint().Equals(contactEndpoint))
                            {
                                //Stop a referring hello operation to not send hellos any more
                                helloOperation.SetFinished();
                            }
                        }
                        if (operation is RequestNeighborListOperation)
                        {
                            RequestNeighborListOperation requestNeighborListOperation = (RequestNeighborListOperation)operation;
                            if (requestNeighborListOperation.GetReferringIpEndpoint().Equals(contactEndpoint))
                            {
                                //Stop a referring hello operation to not send request neighborlists any more
                                requestNeighborListOperation.SetFinished();
                            }
                        }
                    }
                }
                LastCheckedTime = DateTime.Now;
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }

    /// <summary>
    /// The SendDataOperation sends data that is in the DataMessagesOutgoing queue
    /// </summary>
    internal class SendDataOperation : Operation
    {

        /// <summary>
        /// The SendDataOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            DataMessage dataMessage;
            while (ConnectionManager.DataMessagesOutgoing.TryDequeue(out dataMessage))
            {
                //check, if ReceiverPeerId == null || ReceiverPeerId == 0; then sendToAll is true
                bool sendToAll = true;
                if (dataMessage.MessageHeader.ReceiverPeerId != null)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (dataMessage.MessageHeader.ReceiverPeerId[i] != 0)
                        {
                            sendToAll = false;
                            break;
                        }
                    }
                }

                foreach (var keyvalue in ConnectionManager.Contacts)
                {
                    if (keyvalue.Value.IsOffline == false)
                    {
                        if (!sendToAll)
                        {
                            //if we have a ReceiverPeerId != 0, we check if the contact has this PeerId
                            for (int i = 0; i < 16; i++)
                            {
                                if (dataMessage.MessageHeader.ReceiverPeerId[i] != keyvalue.Value.PeerId[i])
                                {
                                    continue;
                                }
                            }
                        }
                        ConnectionManager.SendDataMessage(keyvalue.Value.IPAddress, keyvalue.Value.Port, dataMessage);
                    }
                }
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }

    /// <summary>
    /// The ReceiveDataOperation puts received DataMessages into the DataMessagesIngoing queue
    /// </summary>
    internal class ReceiveDataOperation : Operation
    {
        /// <summary>
        /// The SendDataOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// This operation does nothing during execution...
        /// </summary>
        /// <param name="message"></param>
        public override void Execute()
        {
            //we do nothing here
        }

        /// <summary>
        /// If it receives a DataMessage, it puts it into the ConnectionManager queue for receiving data message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is DataMessage)
            {
                DataMessage dataMessage = (DataMessage)message;
                ConnectionManager.DataMessagesIngoing.Enqueue(dataMessage);
            }
        }
    }

    /// <summary>
    /// Responsible for our initial connections
    /// </summary>
    internal class BootstrapOperation : Operation
    {
        private const int CHECK_INTERVAL = 120000; //2 min
        private DateTime LastCheckedTime = DateTime.MinValue;
        private Logger Logger = Logger.GetLogger();
        
        private List<Contact> WellKnownPeers;

        /// <summary>
        /// Create a new BootstrapOperation having a list of well known peers
        /// </summary>
        /// <param name="wellKnownPeers"></param>
        public BootstrapOperation(List<Contact> wellKnownPeers)
        {
            WellKnownPeers = wellKnownPeers;
        }

        /// <summary>
        /// Only terminates if it does not know any well known peer
        /// </summary>
        public override bool IsFinished
        {
            get { return WellKnownPeers.Count == 0; }
        }

        /// <summary>
        /// Checks every 30 seconds if we need to bootstrap, i.e. we have no connections
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastCheckedTime.AddMilliseconds(CHECK_INTERVAL))
            {                
                if(ConnectionManager.GetConnectionCount() > 0)
                {
                    //we only start a new bootstrapping attempt if we have no connections
                    return;
                }
                Logger.LogText("We have no connection. Start bootstrapping by sending HelloMessages to well known peers",this,Logtype.Info);
                foreach (Contact contact in WellKnownPeers)
                {
                    HelloOperation helloOperation = new HelloOperation(contact.IPAddress, contact.Port) { ConnectionManager = ConnectionManager };
                    ConnectionManager.Operations.Enqueue(helloOperation);
                    Logger.LogText(String.Format("Created HelloOperation for {0}:{1}", contact.IPAddress, contact.Port), this, Logtype.Info);
                }
                LastCheckedTime = DateTime.Now;
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }

    /// <summary>
    /// Waits for GoingOfflineMessages and sets peers to offline when it received such a message from a peer
    /// Additionally, removes all HelloOperations and all RequestNeighborListOperations referring to that peer by setting these to finished
    /// </summary>
    internal class GoingOfflineOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Does nothing during execution
        /// </summary>
        public override void Execute()
        {
            
        }

        /// <summary>
        /// If it receives a GoingOfflineMessage it sets the contact to offline and removes all HelloRequestOperations and all RequestNeighborListOperations referring to that peer
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is GoingOfflineMessage)
            {
                Contact contact;
                IPEndPoint ipendpoint = new IPEndPoint(new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort);
                //set contact to offline
                if(ConnectionManager.Contacts.TryGetValue(ipendpoint, out contact))
                {
                    Logger.LogText(String.Format("Received a GoingOfflineMessage from {0}:{1}. Mark him as offline and remove all my operations referring to him.", contact.IPAddress, contact.Port), this, Logtype.Debug);
                    contact.IsOffline = true;
                }

                //remove all HelloOperations
                //and remove all RequestNeighborListOperations
                foreach (Operation operation in ConnectionManager.Operations)
                {
                    if (operation is HelloOperation)
                    {
                        HelloOperation helloOperation = (HelloOperation)operation;
                        if(helloOperation.GetReferringIpEndpoint().Equals(ipendpoint))
                        {
                            //Stop a referring hello operation to not send hellos any more
                            helloOperation.SetFinished();
                        }
                    }
                    if (operation is RequestNeighborListOperation)
                    {
                        RequestNeighborListOperation requestNeighborListOperation = (RequestNeighborListOperation)operation;
                        if (requestNeighborListOperation.GetReferringIpEndpoint().Equals(ipendpoint))
                        {
                            //Stop a referring hello operation to not send request neighborlists any more
                            requestNeighborListOperation.SetFinished();
                        }
                    }
                }                
            }
        }
    }

    /// <summary>
    /// This message removes contacts from ConnectionManager.ReceivedContacts whose all KnownBy are offline
    /// </summary>
    internal class HousekeepReceivedNeighborsOperation : Operation
    {
        private const int CHECK_INTERVAL = 60000; //1 min
        private DateTime LastCheckedTime = DateTime.Now;
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HousekeepReceivedNeighborsOperation never expires...
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            if (DateTime.Now > LastCheckedTime.AddMilliseconds(CHECK_INTERVAL))
            {
                //1. Collect entries of ConnectionManager.ReceivedContacts to remove
                List<IPEndPoint> removeList = new List<IPEndPoint>();
                foreach (IPEndPoint endpoint in ConnectionManager.ReceivedContacts.Keys)
                {
                    bool remove = true;
                    foreach (Contact contact in ConnectionManager.ReceivedContacts[endpoint].KnownBy.Values)
                    {
                        if (contact.IsOffline == false)
                        {
                            //we have one contact who is online and knows him; thus, we do not need to remove him
                            remove = false;
                            break;
                        }
                    }
                    if (remove)
                    {
                        removeList.Add(endpoint);
                    }
                }

                //2. Remove all entries
                foreach (IPEndPoint endpoint in removeList)
                {
                    Contact contact;
                    if(ConnectionManager.ReceivedContacts.TryRemove(endpoint,out contact))
                    {
                        Logger.LogText(String.Format("Removed {0}:{1} from our received contacts list since nobody is online who knows him", contact.IPAddress, contact.Port), this, Logtype.Debug);
                    }
                }
                LastCheckedTime = DateTime.Now;
            }
        }

        /// <summary>
        /// This operation does nothing with messages...
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //do nothing
        }
    }
}
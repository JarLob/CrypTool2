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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NLog;
using voluntLib.common;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer.communicator;
using voluntLib.communicationLayer.communicator.networkBridgeCommunicator;
using voluntLib.communicationLayer.messageHandler;
using voluntLib.communicationLayer.messages.commonStructs;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.communicationLayer
{
    public class CommunicationLayer : ICommunicationLayer
    {
        private const int DefaultJobListExchangeInterval = 10 * 6000; // 10 min

        #region internal members

        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected readonly CertificateService certificateHandler;
        private readonly Dictionary<IPAddress, ICommunicator> communicators = new Dictionary<IPAddress, ICommunicator>();
        private readonly IManagementLayerCallback managementCallback;
        protected readonly Dictionary<byte, IMessageHandler> messagesHandler;
        private Timer jobListExchangeTimer;

        #endregion

        #region properties

        /// <summary>
        ///   Gets or sets the job list exchange interval, used for scheduled JobListExchanges with all TCP clients and each world
        ///   of the managementLayer
        /// </summary>
        /// <value>
        ///   The job list exchange interval.
        /// </value>
        public long JobListExchangeInterval { get; set; }

        #endregion

        public CommunicationLayer(IManagementLayerCallback managementCallback, CertificateService certificateHandler, ICommunicator communicator)
        {
            this.certificateHandler = certificateHandler;
            this.managementCallback = managementCallback;
            communicator.RegisterCommunicationLayer(this);
            communicators.Add(IPAddress.Broadcast, communicator);
            JobListExchangeInterval = DefaultJobListExchangeInterval;

            messagesHandler = new Dictionary<byte, IMessageHandler>
            {
                {(byte) MessageType.CreateNetworkJob, new CreateNetworkJobHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.DeleteNetworkJob, new DeleteNetworkJobHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.RequestJobList, new RequestJobListHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.ResponseJobList, new ResponseJobListHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.RequestJobDetails, new RequestJobDetailHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.ResponseJobDetails, new ResponseJobDetailsHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.RequestWorldList, new RequestWorldListHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.ResponseWorldList, new ResponseWorldListHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.JoinNetworkJob, new JoinNetworkJobHandler(managementCallback, this, certificateHandler)},
                {(byte) MessageType.PropagateState, new PropagateStateHandler(managementCallback, this, certificateHandler)}
            };
        }

        public void HandleIncomingMessages(byte[] message, IPAddress from)
        {
            if (! messagesHandler.ContainsKey(message[1]))
                return; 

            Logger.Debug("Message Handler found");
            messagesHandler[message[1]].HandleByteArray(message, from);
        }

        #region create message and send

        public void PropagateState(BigInteger jobID, string worldName, ALocalState state, IPAddress to)
        {
            var packet = new PropagateStateMessage(jobID, worldName, state.Serialize());
            SignAndSendAPacket(packet, to);
        }

        public void JoinNetworkJob(BigInteger jobID, string worldName, IPAddress to)
        {
            var packet = new JoinNetworkJobMessage(jobID, worldName);
            SignAndSendAPacket(packet, to);
        }

        public void CreateNetworkJob(NetworkJob job, IPAddress to)
        {
            var packet = new CreateNetworkJobMessage(job.JobID, job.World, job.ToNetworkJobMetaData(), job.ToNetworkJobPayload());
            SignAndSendAPacket(packet, to);
        }

        public void DeleteNetworkJob(DeleteNetworkJobMessage message, IPAddress to)
        {
            SendASignedPacket(message, to);
        }

        public DeleteNetworkJobMessage DeleteNetworkJob(NetworkJob job, IPAddress any)
        {
            var msg = certificateHandler.SignAndAddInformation(new DeleteNetworkJobMessage(job.JobID, job.World));
            SendASignedPacket(msg, any);

            var message = (DeleteNetworkJobMessage) msg;
            message.FromAdmin = certificateHandler.IsAdmin(message);
            return message;
        }

        #region request

        public void RequestWorldList(IPAddress to)
        {
            SignAndSendAPacket(new RequestWorldListMessage(), to);
        }

        public void RequestJobDetails(BigInteger jobID, string worldName, IPAddress to)
        {
            SignAndSendAPacket(
                new RequestJobDetailsMessage(jobID, worldName), to);
        }

        public void RequestJobList(string worldName, IPAddress to)
        {
            SignAndSendAPacket(new RequestJobListMessage(worldName), to);
        }

        #endregion

        #region response

        public void SendWorldList(List<string> worlds, IPAddress to)
        {
            SignAndSendAPacket(new ResponseWorldListMessage(worlds), to);
        }

        public void SendJobDetails(string worldName, BigInteger jobID, NetworkJobPayload payload, IPAddress to)
        {
            var packet = new ResponseJobDetailsMessage(jobID, worldName, payload);
            SignAndSendAPacket(packet, to);
        }

        public void SendJobList(string worldName, List<NetworkJobMetaData> descriptions, IPAddress to)
        {
            SignAndSendAPacket(new ResponseJobListMessage(worldName, descriptions), to);
        }

        #endregion

        #endregion

        #region helper

        /// <summary>
        ///   Adds name and Certificate and then it Signs the packages and calls the SendSignedPacket-Method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="to">To.</param>
        public virtual void SignAndSendAPacket(AMessage message, IPAddress to)
        {
            SendASignedPacket(certificateHandler.SignAndAddInformation(message), to);
        }

        /// <summary>
        ///   Sends a signed packet to the communicator associated with the given IPAddress.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="to">To.</param>
        private void SendASignedPacket(AMessage message, IPAddress to)
        {
            if (IPAddress.Any.Equals(to))
            {
                Logger.Info("Sending " + (MessageType) message.Header.MessageType + " to anyone");
                GetCommunicator().ForEach(com => com.ProcessMessage(message, to));
            } else if (communicators.ContainsKey(to))
            {
                Logger.Info("Sending " + (MessageType) message.Header.MessageType + " to " + to);
                communicators[to].ProcessMessage(message, to);
            } else
            {
                Logger.Warn("No Communication for remoteHost: " + to + " found.");
            }
        }

        #region zip

      

        #endregion
        #endregion

        public void AddCommunicator(IPAddress triggeringIP, ICommunicator communicator)
        {
            communicator.RegisterCommunicationLayer(this);
            communicators.Add(triggeringIP, communicator);
        }

        public List<ICommunicator> GetCommunicator()
        {
            return communicators.Values.ToList();
        }

        #region start and stop

        public void Start()
        {
            foreach (var com in communicators.Values)
            {
                com.Start();

                //send certificate if its a tcp communicator
                if (com is SendingTCPCommunicator)
                {
                    var bridgeIP = (com as SendingTCPCommunicator).RemoteNetworkBridgeIP;
                    jobListExchangeTimer = new Timer(obj =>
                    {
                        foreach (var world in  managementCallback.GetWorlds())
                        {
                            Logger.Info("scheduled JobList-Exchange with " + bridgeIP);
                            RequestJobList(world, bridgeIP);
                        }
                    }, null, new TimeSpan(0, 0, 0, 1), TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            }
        }

        public virtual void Stop()
        {
            communicators.Values.ToList().ForEach(com => com.Stop());
 
            //stop timer
            if (jobListExchangeTimer != null)
            {
                jobListExchangeTimer.Change(Timeout.Infinite, Timeout.Infinite);
                jobListExchangeTimer.Dispose();
            }
        }

        # endregion
    }
}
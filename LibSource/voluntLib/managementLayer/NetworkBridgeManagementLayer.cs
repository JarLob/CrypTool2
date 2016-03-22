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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using voluntLib.common;
using System.Numerics;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states; 
using NLog;
using voluntLib.common.interfaces;

#endregion

namespace voluntLib.managementLayer
{
    public class NetworkBridgeManagementLayer : IManagementLayerCallback
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ManagementLayer managementLayer;
        private dataStructs.JobContainer Jobs;
        private List<string> Worlds;
        
        public ICommunicationLayer NetworkCommunicationLayer { get; set; }
        

        public NetworkBridgeManagementLayer(ManagementLayer managementLayer)
        {
            this.managementLayer = managementLayer;

            // link to data
            Jobs = managementLayer.Jobs;
            Worlds = managementLayer.Worlds;
        }


        public void OnWorldListRequest(IPAddress from){    }

        public void OnWorldListReceived(List<string> worldList, IPAddress from) {    }

     
        public List<string> GetWorlds()
        {
            return Worlds;
        }

        /// <summary>
        ///   Called when job list is requested.
        ///   In comparison to the base-method this only sends those jobs where the payload is known.
        ///   Also after the job list has been transmitted, each jobPayload'll be transfered.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="from">From.</param>
        public void OnJobListRequest(string world, IPAddress from)
        {
            // find jobs with payload
            var jobsWithPayload = Jobs.GetJobsOfWorld(world).FindAll(job => job.HasPayload() && !job.IsDeleted);

            //send metaData of found jobs
            NetworkCommunicationLayer.SendJobList(world, jobsWithPayload.Select(job => job.ToNetworkJobMetaData()).ToList(), from);

            //send payload for each found job
            jobsWithPayload.ForEach(job => NetworkCommunicationLayer.SendJobDetails(world, job.JobID, job.ToNetworkJobPayload(), from));

            //send all other states
            var managers = managementLayer.LocalStates.Values.Where(manager => world.Equals(manager.World));
            foreach (var manager in managers)
            {
                NetworkCommunicationLayer.PropagateState(manager.JobID, world, manager.LocalState, from);
            }
        }


        public void OnJobListReceived(string world, List<NetworkJob> receivedJobs, IPAddress from)
        {
            managementLayer.OnJobListReceived(world, receivedJobs, from);
            var ownJobsWithPayload = Jobs.GetJobsOfWorld(world).FindAll(job => job.HasPayload());
            var newJobs = receivedJobs.Except(ownJobsWithPayload).ToList();
            var missingJobs = Jobs.GetJobsOfWorld(world).Except(receivedJobs).ToList();

            if (newJobs.Any())
            {
                //let the main ManLayer inform the connected network.
                managementLayer.OnJobListReceived(world, receivedJobs, IPAddress.Any);

                // request missing jobDetails
                newJobs.ToList().ForEach(job => NetworkCommunicationLayer.RequestJobDetails(job.JobID, world, from));              
            }

            //send new jobList when
            if (missingJobs.Any())
                OnJobListRequest(world, from); 
        }
        public void OnJobDetailsReceived(string world, BigInteger jobID, byte[] payload, IPAddress from)
        {
            managementLayer.OnJobDetailsReceived(world, jobID, payload, from);
        }

        public void OnJobCreation(NetworkJob newJob, IPAddress from)
        {
            managementLayer.OnJobCreation(newJob, from);
        }

        public void OnJobDetailRequest(string world, BigInteger jobID, IPAddress from)
        {
            var job = Jobs.GetJob(jobID);
            if(job == null || !job.HasPayload() || job.IsDeleted){
                return;
            }

            NetworkCommunicationLayer.SendJobDetails(world, jobID, job.ToNetworkJobPayload(), from);
        }


        public void OnJobDeletion(DeleteNetworkJobMessage message, IPAddress fromIP)
        {
            managementLayer.OnJobDeletion(message, fromIP);
        }

        public void OnIncomingState(PropagateStateMessage message, IPAddress from)
        {
            var jobId = message.Header.JobID;
            var worldName = message.Header.WorldName;

            var job = Jobs.GetJob(jobId);
            if (job == null) //ignor unknown jobs
            {
                Logger.Info("Receiving state of unknown job. requesting job details.");
                NetworkCommunicationLayer.RequestJobDetails(jobId, worldName, from); 
                return;
            }

            if (job.IsDeleted)
            {
                NetworkCommunicationLayer.DeleteNetworkJob(job.DeletionMessage, from);
                return;
            }

            var stateManager = managementLayer.GetOrCreateStateManager(job.JobID, job);
            var incomingState = managementLayer.CreateIncomingState(message, stateManager, jobId);

            if (stateManager.IsSuperSetOf(incomingState))
            {
                // Propagate localState if its a Super-set of the incoming state
                NetworkCommunicationLayer.PropagateState(jobId, worldName, stateManager.LocalState, from);
            }
            else
            {
                 managementLayer.ProcessUsefulStates(jobId, incomingState);
                 managementLayer.FileCommunicationLayer.PropagateState(jobId, worldName, incomingState, IPAddress.Any);
            }
            managementLayer.WorkingPeers.AddOrUpdate(message);
           
   
            //send all other states
            foreach (var manager in  managementLayer.LocalStates.Values)
            {
                if (stateManager.JobID != manager.JobID && worldName.Equals(manager.World))
                {
                    NetworkCommunicationLayer.PropagateState(manager.JobID, worldName, manager.LocalState, from);
                }
            }

        }
      
    }
}
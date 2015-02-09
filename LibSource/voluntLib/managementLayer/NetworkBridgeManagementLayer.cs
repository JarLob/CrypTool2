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
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.managementLayer
{
    public class NetworkBridgeManagementLayer : ManagementLayer
    {
        private readonly ManagementLayer managementLayer;

        public NetworkBridgeManagementLayer(ManagementLayer managementLayer)
        {
            this.managementLayer = managementLayer;
            MaximumBackoffTime = 0; //backoff-task wont wait

            // link to data
            Jobs = managementLayer.Jobs;
            Worlds = managementLayer.Worlds;
        }

        /// <summary>
        ///   Called when job list is requested.
        ///   In comparison to the base-method this only sends those jobs where the payload is known.
        ///   Also after the job list has been transmitted, each jobPayload'll be transfered.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="from">From.</param>
        public override void OnJobListRequest(string world, IPAddress from)
        {
            // find jobs with payload
            var jobsWithPayload = Jobs.GetJobsOfWorld(world).FindAll(job => job.HasPayload());

            //send metaData of found jobs
            NetworkCommunicationLayer.SendJobList(world, jobsWithPayload.Select(job => job.ToNetworkJobMetaData()).ToList(), from);

            //send payload for each found job
            jobsWithPayload.ForEach(job => NetworkCommunicationLayer.SendJobDetails(world, job.JobID, job.ToNetworkJobPayload(), @from));

            //send all other states
            foreach (var manager in managementLayer.LocalStates.Values.Where(manager => world.Equals(manager.World)))
            {
                NetworkCommunicationLayer.PropagateState(manager.JobID, world, manager.LocalState, IPAddress.Any);
            }
        }

        public override void OnJobListReceived(string world, List<NetworkJob> receivedJobs, IPAddress @from)
        {
            var ownJobsWithPayload = Jobs.GetJobsOfWorld(world).FindAll(job => job.HasPayload());
            var newJobs = receivedJobs.Except(ownJobsWithPayload).ToList();
            var missingJobs = Jobs.GetJobsOfWorld(world).Except(receivedJobs).ToList();

            if (newJobs.Any())
            {
                //let the main ManLayer inform the connected network.
                managementLayer.OnJobListReceived(world, receivedJobs, IPAddress.Any);

                // request missing jobDetails
                newJobs.ToList().ForEach(job => NetworkCommunicationLayer.RequestJobDetails(job.JobID, world, @from));
            }

            //send new jobList when
            if (missingJobs.Any())
                OnJobListRequest(world, @from);
        }

        public override void OnIncomingState(PropagateStateMessage stateRaw, IPAddress @from)
        {
            managementLayer.OnIncomingState(stateRaw, @from);

            var jobID = stateRaw.Header.JobID;
            var worldName = stateRaw.Header.WorldName;

            // get StateManager
            LocalStateManager<EpochState> stateManager;
            if ( ! managementLayer.LocalStates.TryGetValue(jobID, out stateManager))
                return;

            var incomingState = CreateIncomingState(stateRaw, stateManager, jobID);
            if (stateManager.IsSuperSetOf(incomingState))
                // Propagate localState if its a Super-set of the incoming state
                NetworkCommunicationLayer.PropagateState(jobID, worldName, stateManager.LocalState, IPAddress.Any);
            else
                managementLayer.NetworkCommunicationLayer.PropagateState(jobID, worldName, stateManager.LocalState, IPAddress.Any);
            

            //send all other states
            foreach (var manager in  managementLayer.LocalStates.Values)
            {
                if (stateManager.JobID != manager.JobID && worldName.Equals(manager.World))
                {
                    NetworkCommunicationLayer.PropagateState(manager.JobID, worldName, manager.LocalState, IPAddress.Any);
                }
            }
        }
    }
}
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
using System.Net;
using voluntLib.common;
using voluntLib.communicationLayer.messages.commonStructs;

#endregion

namespace voluntLib.managementLayer.delayedTasks
{
    internal class SendJobListTask : ARandomDelayedTask
    {
        private readonly ManagementLayer managementLayer;
        private List<NetworkJob> jobs;

        public SendJobListTask(ManagementLayer managementLayer, string world) : base("SendJobList")
        {
            this.managementLayer = managementLayer;
            World = world;
        }

        private string World { get; set; }

        public void StartTimer(List<NetworkJob> jobsToSend, int max)
        {
            jobs = jobsToSend;
            StartTimer(max);
        }

        protected override void TimerAction(IPAddress toAddress)
        {
            if (managementLayer.NetworkCommunicationLayer != null)
            {
                var networkDescriptions = new List<NetworkJobMetaData>();
                jobs.ForEach(job => networkDescriptions.Add(job.ToNetworkJobMetaData()));
                managementLayer.NetworkCommunicationLayer.SendJobList(World, networkDescriptions, toAddress);
            }
        }
    }
}
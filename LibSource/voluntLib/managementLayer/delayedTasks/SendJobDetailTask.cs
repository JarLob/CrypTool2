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

using System.Net;
using voluntLib.common;

#endregion

namespace voluntLib.managementLayer.delayedTasks
{
    internal class SendJobDetailTask : ARandomDelayedTask
    {
        private readonly ManagementLayer managementLayer;
        private readonly string world;
        private NetworkJob job;

        public SendJobDetailTask(ManagementLayer managementLayer, string world) : base("SendJobDetail")
        {
            this.managementLayer = managementLayer;
            this.world = world;
        }

        public void StartTimer(NetworkJob jobToSend, int max)
        {
            job = jobToSend;
            StartTimer(max);
        }

        protected override void TimerAction(IPAddress toAddress)
        {
            if (managementLayer.NetworkCommunicationLayer != null)
            {
                managementLayer.NetworkCommunicationLayer.SendJobDetails(world, job.JobID, job.ToNetworkJobPayload(), toAddress);
            }
        }
    }
}
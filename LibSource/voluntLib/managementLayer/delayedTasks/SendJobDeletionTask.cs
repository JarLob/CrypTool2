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
using System.Numerics;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.managementLayer.delayedTasks
{
    internal class SendJobDeletionTask : ARandomDelayedTask
    {
        private readonly BigInteger jobID;
        private readonly ManagementLayer managementLayer;
        private DeleteNetworkJobMessage message;

        public SendJobDeletionTask(ManagementLayer managementLayer, BigInteger jobID) : base("JobDeletion")
        {
            this.managementLayer = managementLayer;
            this.jobID = jobID;
        }

        public void StartTimer(DeleteNetworkJobMessage message, int max)
        {
            this.message = message;
            StartTimer(max);
        }

        protected override void TimerAction(IPAddress to)
        {
            if (managementLayer.NetworkCommunicationLayer != null)
            {
                managementLayer.NetworkCommunicationLayer.DeleteNetworkJob(message, to);
            }
        }
    }
}
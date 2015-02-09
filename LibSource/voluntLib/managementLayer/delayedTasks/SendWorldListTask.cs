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

#endregion

namespace voluntLib.managementLayer.delayedTasks
{
    internal class SendWorldListTask : ARandomDelayedTask
    {
        private readonly ManagementLayer managementLayer;
        private List<string> worlds;

        public SendWorldListTask(ManagementLayer managementLayer) : base("SendWorldList")
        {
            this.managementLayer = managementLayer;
        }

        public void StartTimer(List<string> worldsToSend, int max)
        {
            worlds = worldsToSend;
            StartTimer(max);
        }

        protected override void TimerAction(IPAddress toAddress)
        {
            if (managementLayer.NetworkCommunicationLayer != null)
            {
                managementLayer.NetworkCommunicationLayer.SendWorldList(worlds, toAddress);
            }
        }
    }
}
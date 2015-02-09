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
using System.Numerics;
using voluntLib.managementLayer.delayedTasks;

#endregion

namespace voluntLib.managementLayer.dataStructs
{
    internal class TaskContainer
    {
        private readonly Dictionary<BigInteger, SendJobDeletionTask> jobDeletionTasks = new Dictionary<BigInteger, SendJobDeletionTask>();
        private readonly ManagementLayer managementLayer;
        private readonly Dictionary<BigInteger, PropagateStateTask> propagateStateTasks = new Dictionary<BigInteger, PropagateStateTask>();

        private readonly Dictionary<string, SendJobDetailTask> sendJobDetailTasks = new Dictionary<string, SendJobDetailTask>();
        private readonly Dictionary<string, SendJobListTask> sendJobListTasks = new Dictionary<string, SendJobListTask>();
        private readonly SendWorldListTask sendWorldListTask;

        public TaskContainer(ManagementLayer managementLayer)
        {
            this.managementLayer = managementLayer;
            sendWorldListTask = new SendWorldListTask(managementLayer);
        }

        public SendWorldListTask GetSendWorldListTask()
        {
            return sendWorldListTask;
        }

        public SendJobDetailTask GetSendJobDetailTask(string world)
        {
            if (!sendJobDetailTasks.ContainsKey(world))
            {
                sendJobDetailTasks.Add(world, new SendJobDetailTask(managementLayer, world));
            }
            return sendJobDetailTasks[world];
        }

        public SendJobListTask GetSendJobListTask(string world)
        {
            if (!sendJobListTasks.ContainsKey(world))
            {
                sendJobListTasks.Add(world, new SendJobListTask(managementLayer, world));
            }
            return sendJobListTasks[world];
        }

        public PropagateStateTask GetPropagateStateTask(BigInteger jobID)
        {
            if (!propagateStateTasks.ContainsKey(jobID))
            {
                propagateStateTasks.Add(jobID, new PropagateStateTask(managementLayer, jobID));
            }
            return propagateStateTasks[jobID];
        }

        public SendJobDeletionTask GetJobDeletionTasks(BigInteger jobID)
        {
            if (!jobDeletionTasks.ContainsKey(jobID))
            {
                jobDeletionTasks.Add(jobID, new SendJobDeletionTask(managementLayer, jobID));
            }
            return jobDeletionTasks[jobID];
        }
    }
}
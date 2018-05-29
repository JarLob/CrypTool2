/*
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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VoluntLib2.ComputationLayer
{
    /// <summary>
    /// Event args for task events
    /// </summary>
    public class TaskEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new TaskEventArgs
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="blockID"></param>
        /// <param name="type"></param>
        public TaskEventArgs(BigInteger jobId, BigInteger blockID, TaskEventArgType type)
        {
            Type = type;
            JobId = jobId;
            BlockID = blockID;
        }

        /// <summary>
        /// Progress of the referenced task
        /// </summary>
        public int TaskProgress { get; set; }

        /// <summary>
        /// BlockId of the referenced task
        /// </summary>
        public BigInteger BlockID { get; private set; }

        /// <summary>
        /// Type of the event
        /// </summary>
        public TaskEventArgType Type { get; private set; }

        /// <summary>
        /// JobId of the referenced task
        /// </summary>
        public BigInteger JobId { get; private set; }
    }

    /// <summary>
    /// Type of the TaskEventArgs
    /// </summary>
    public enum TaskEventArgType
    {
        Started,
        Finished,
        Canceled,
        Progress
    }
}

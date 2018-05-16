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
    public class TaskEventArgs : EventArgs
    {
        public TaskEventArgs(BigInteger JobId, BigInteger blockID, TaskEventArgType type) : this(JobId.ToByteArray(), blockID, type) { }

        public TaskEventArgs(byte[] JobId, BigInteger blockID, TaskEventArgType type)
        {
            Type = type;
            JobId = JobId;
            BlockID = blockID;
        }

        public int TaskProgress { get; set; }
        public BigInteger BlockID { get; private set; }
        public TaskEventArgType Type { get; private set; }
        public byte[] JobId { get; private set; }
    }

    public enum TaskEventArgType
    {
        Started,
        Finished,
        Canceled,
        Progress
    }
}

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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoluntLib2.ManagementLayer.Messages;

namespace VoluntLib2.ManagementLayer
{
    /// <summary>
    /// Abstract super class of all operations:
    /// Operations are state machines that send messages and work on received message using the JobManager
    /// If an operation's IsFinished equals true, it can be deleted by JobManager
    /// </summary>
    internal abstract class Operation
    {
        /// <summary>
        /// Needed by each operation for message sending, etc
        /// </summary>
        public JobManager JobManager { get; set; }

        /// <summary>
        /// Tells the worker thread if this operation is finished. If it is, it can be deleted
        /// </summary>
        public abstract bool IsFinished { get; }

        /// <summary>
        /// Called by the worker thread (cooperative multitasking)
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Each message is handed by the JobManager to this operation calling this method
        /// </summary>
        /// <param name="message"></param>
        public abstract void HandleMessage(Message message);
    }

    internal class TestOperation : Operation
    {

        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            
        }

        public override void HandleMessage(Message message)
        {
            
        }
    }
}

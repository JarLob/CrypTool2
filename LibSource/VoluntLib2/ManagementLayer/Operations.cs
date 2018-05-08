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
using VoluntLib2.Tools;

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

    /// <summary>
    /// Operation for sharing JobLists every 5 minutes
    /// </summary>
    internal class ShareJobListOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();
        private const int SHARE_INTERVAL = 300000; //5min
        private DateTime LastExecutionTime = DateTime.Now;

        /// <summary>
        /// The ShareJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastExecutionTime.AddMilliseconds(SHARE_INTERVAL))
            {
                Logger.LogText("Sending ResponseJobListMessages to all neighbors", this, Logtype.Debug);
                //Send a ResponseJobListMessage to every neighbor
                JobManager.SendResponseJobListMessage(null);
                LastExecutionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //this operation does nothing with messages
        }
    }

    /// <summary>
    /// Operation for requesting JobLists every 5 minutes
    /// </summary>
    internal class RequestJobListOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();
        private const int REQUEST_INTERVAL = 300000; //5min
        private DateTime LastExecutionTime = DateTime.Now.Subtract(new TimeSpan(REQUEST_INTERVAL));

        /// <summary>
        /// The RequestJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastExecutionTime.AddMilliseconds(REQUEST_INTERVAL))
            {
                Logger.LogText("Sending RequestJobListMessages to all neighbors", this, Logtype.Debug);
                //Send a ResponseJobListMessage to every neighbor
                JobManager.SendRequestJobListMessage(null);
                LastExecutionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //this operation does nothing with messages
        }

        /// <summary>
        /// Sets the last execution time to min value forcing it to be executed
        /// </summary>
        public void ForceExecution()
        {
            LastExecutionTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Operation for answering RequestJobListMessages
    /// </summary>
    internal class ResponseJobListOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();
        
        /// <summary>
        /// The ResponseJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {            
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is RequestJobListMessage)
            {
                Logger.LogText(String.Format("Received a RequestJobListMessage from peer {0}. Answering now.", BitConverter.ToString(message.PeerId)), this, Logtype.Debug);
                JobManager.SendResponseJobListMessage(message.PeerId);
            }
        }       
    }

    /// <summary>
    /// Operation for handling ResponseJobListMessage
    /// </summary>
    internal class HandleJobListResponseOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HandleJobListResponseOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is ResponseJobListMessage)
            {
                Logger.LogText(String.Format("Received a ResponseJobListMessage from peer {0}. Updating my jobs", BitConverter.ToString(message.PeerId)), this, Logtype.Debug);
                ResponseJobListMessage responseJobListMessage = (ResponseJobListMessage)message;
                bool newJobReceived = false;
                foreach (var job in responseJobListMessage.Jobs)
                {
                    if(!JobManager.Jobs.ContainsKey(job.JobID))
                    {
                        JobManager.Jobs.TryAdd(job.JobID, job);
                        newJobReceived = true;
                    }
                }
                //we received at least one new job. Thus, we inform that the job list changed
                if (newJobReceived)
                {
                    JobManager.OnJobListChanged();
                }
            }
        }
    }
}

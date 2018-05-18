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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoluntLib2.ManagementLayer;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{
    internal class ComputationManager
    {
        private const int MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        private const int WORKER_THREAD_SLEEPTIME = 1; // ms
        
        private Logger Logger = Logger.GetLogger();
        private bool Running = false;
        private Thread WorkerThread;

        internal ConcurrentDictionary<BigInteger, JobAssignment> JobAssignments = new ConcurrentDictionary<BigInteger, JobAssignment>();
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();

        public ComputationManager(VoluntLib voluntLib, JobManager jobManager)
        {

        }

        public void Start()
        {
            Running = true;
            WorkerThread = new Thread(ComputationManagerWork);
            WorkerThread.IsBackground = true;
            WorkerThread.Start();
        }

        private void ComputationManagerWork(object obj)
        {
            Logger.LogText("WorkerThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    Operation operation;
                    if (Operations.TryDequeue(out operation) == true)
                    {
                        // before we execute an operation, we check if it is finished
                        if (operation.IsFinished == false)
                        {
                            //operations that are not finished are enqueued again
                            Operations.Enqueue(operation);
                        }
                        else
                        {
                            Logger.LogText(String.Format("Operation {0}-{1} has finished. Removed it.", operation.GetType().FullName, operation.GetHashCode()), this, Logtype.Debug);
                            //we dont execute this operation since it is finished
                            continue;
                        }
                        try
                        {
                            operation.Execute();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogText(String.Format("Exception during execution of operation {0}-{1}: {2}", operation.GetType().FullName, operation.GetHashCode(), ex.Message), this, Logtype.Error);
                            Logger.LogException(ex, this, Logtype.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during handling of operation: {2}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
                try
                {
                    Thread.Sleep(WORKER_THREAD_SLEEPTIME);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during sleep of thread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            Logger.LogText("WorkerThread terminated", this, Logtype.Info);
        }

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Logger.LogText("Stop method was called...", this, Logtype.Info);
            Running = false;
            DateTime start = DateTime.Now;
            while ((WorkerThread.IsAlive) && DateTime.Now < start.AddMilliseconds(MAX_TERMINATION_WAIT_TIME))
            {
                Thread.Sleep(100);
            }
            if (WorkerThread.IsAlive)
            {
                Logger.LogText("WorkerThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    WorkerThread.Abort();
                    Logger.LogText("Aborted WorkerThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during abortion of WorkerThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        internal bool JoinJob(BigInteger jobId, ACalculationTemplate calculationTemplate, int amountOfWorkers)
        {
            if (!JobAssignments.ContainsKey(jobId))
            {
                try
                {
                    if (!JobAssignments.TryAdd(jobId, new JobAssignment() { JobId = jobId, CalculationTemplate = calculationTemplate, AmountOfWorkers = amountOfWorkers }))
                    {
                        Logger.LogText(String.Format("Could not add job with jobid {0} to internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Could not add job with jobid {0} to internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    Logger.LogException(ex, this, Logtype.Error);
                }                
            }           
            return false;
        }

        internal void StopJob(BigInteger jobId)
        {
            if (JobAssignments.ContainsKey(jobId))
            {
                try
                {
                    JobAssignment jobassignment;
                    if (!JobAssignments.TryRemove(jobId, out jobassignment))
                    {
                        Logger.LogText(String.Format("Could not remove job with jobid {0} from internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Could not remove job with jobid {0} from internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }        
    }

    internal class JobAssignment
    {
        public BigInteger JobId { get; set; }
        public ACalculationTemplate CalculationTemplate { get; set; }
        public int AmountOfWorkers { get; set; }
        public ArrayList Tasks = ArrayList.Synchronized(new ArrayList());
    }
}

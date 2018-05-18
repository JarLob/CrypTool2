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
    /// <summary>
    /// The ComputationManager is responsible for
    /// A) joining and leaving of jobs
    /// B) taking care, that workers are started and stopped
    /// </summary>
    internal class ComputationManager
    {
        private const int MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        private const int WORKER_THREAD_SLEEPTIME = 1; // ms
        
        private Logger Logger = Logger.GetLogger();
        private bool Running = false;
        private Thread WorkerThread;

        internal ConcurrentDictionary<BigInteger, JobAssignment> JobAssignments = new ConcurrentDictionary<BigInteger, JobAssignment>();
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();

        internal VoluntLib VoluntLib { get; set; }
        internal JobManager JobManager { get; set; }

        /// <summary>
        /// Creates a new ComputationManager
        /// </summary>
        /// <param name="voluntLib"></param>
        /// <param name="jobManager"></param>
        public ComputationManager(VoluntLib voluntLib, JobManager jobManager)
        {
            VoluntLib = voluntLib;
            JobManager = jobManager;
        }

        /// <summary>
        /// Start this ComputationManager
        /// </summary>
        public void Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("The ComputationManager is already running!");
            }
            Logger.LogText("Starting the ComputationManager", this, Logtype.Info);

            Running = true;
            WorkerThread = new Thread(ComputationManagerWork);
            WorkerThread.IsBackground = true;
            WorkerThread.Start();
            //This operation deserializes all serialized jobs; then it terminates
            Operations.Enqueue(new CheckRunningWorkersOperation() { ComputationManager = this });

            Logger.LogText("ComputationManager started", this, Logtype.Info);
        }

        /// <summary>
        /// Main method of the thread of the ComputationManager
        /// </summary>
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

        /// <summary>
        /// Stop this ComputationManager
        /// </summary>
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

        /// <summary>
        /// Join the job with the given jobID, calculation template, and amount of workers
        /// If VoluntLib is stopped, it does nothing
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="calculationTemplate"></param>
        /// <param name="amountOfWorkers"></param>
        /// <returns></returns>
        internal bool JoinJob(BigInteger jobId, ACalculationTemplate calculationTemplate, int amountOfWorkers)
        {
            if (!Running)
            {
                return false;
            }

            if (!JobAssignments.ContainsKey(jobId))
            {
                if (!JobManager.Jobs.ContainsKey(jobId))
                {
                    Logger.LogText(String.Format("Can not join a non existing job with jobid {0}", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    return false;
                }
                Job job = JobManager.Jobs[jobId];
                if (!job.HasPayload)
                {
                    Logger.LogText(String.Format("Can not join job with jobid {0} since we have no JobPayload", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    return false;
                }

                try
                {                    
                    if (!JobAssignments.TryAdd(jobId, new JobAssignment() { Job = job, CalculationTemplate = calculationTemplate, AmountOfWorkers = amountOfWorkers }))
                    {
                        Logger.LogText(String.Format("Could not add job with jobid {0} to internal JobAssignments dictionary", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Could not add job with jobid {0} to internal JobAssignments dictionary", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    Logger.LogException(ex, this, Logtype.Error);
                }                
            }           
            return false;
        }

        /// <summary>
        /// Sops the given job if it exists
        /// </summary>
        /// <param name="jobId"></param>
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
                    foreach (Worker worker in jobassignment.Workers)
                    {
                        if (worker.CancellationToken.CanBeCanceled)
                        {
                            worker.CancellationTokenSource.Cancel();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Could not remove job with jobid {0} from internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }        
    }

    /// <summary>
    /// A job assignment contains information of a joined job.
    /// The ComputationManager takes care, that always AmountOfWorkers workers are running
    /// </summary>
    internal class JobAssignment
    {
        public Job Job { get; set; }
        public ACalculationTemplate CalculationTemplate { get; set; }
        public int AmountOfWorkers { get; set; }
        public ArrayList Workers = ArrayList.Synchronized(new ArrayList());
    }

    /// <summary>
    /// A worker knows its worker thread and a CancellationToken to stop the worker 
    /// </summary>
    internal class Worker
    {
        private Logger Logger = Logger.GetLogger();
        private ACalculationTemplate ACalculationTemplate { get; set; }
        private Job Job { get; set; }
        private AWorker AWorker { get; set; }
        private VoluntLib VoluntLib { get; set; }

        public Thread WorkerThread { get; private set; }

        public Worker(Job job, ACalculationTemplate template, VoluntLib voluntLib)
        {
            Job = job;
            ACalculationTemplate = template;
            VoluntLib = voluntLib;
            AWorker = ACalculationTemplate.WorkerLogic;
            AWorker.JobId = Job.JobId.ToByteArray();
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            AWorker.ProgressChanged+=ProgressChanged;
        }

        private void ProgressChanged(object sender, TaskEventArgs taskEventArgs)
        {
            if (!taskEventArgs.Handled)
            {
                VoluntLib.OnTaskProgessChanged(sender, taskEventArgs);
                taskEventArgs.Handled = true;                
            }
        }

        public CancellationToken CancellationToken{ get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }        
        public BigInteger BlockId { get; set; }

        internal void DoWork()
        {            
            try
            {
                BlockId = BigInteger.Zero; //TODO: add code to get actual blockId                
                CalculationResult result = AWorker.DoWork(Job.JobPayload, BlockId, CancellationToken);
                Logger.LogText(String.Format("Worker-{0} who worked on block {1} terminated after complete computation", this.GetHashCode(), BlockId), this, Logtype.Info);
            }
            catch (OperationCanceledException)
            {
                Logger.LogText(String.Format("Worker-{0} who worked on block {1} was stopped by CancellationToken", this.GetHashCode(), BlockId), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                Logger.LogText(String.Format("Exception during execution of Worker-{0} who worked on block {1}: {2}", this.GetHashCode(), BlockId, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            AWorker.ProgressChanged -= ProgressChanged;
        }

        internal void Start()
        {
            WorkerThread = new Thread(DoWork);
            WorkerThread.IsBackground = true;
            WorkerThread.Start();
            Logger.LogText(String.Format("Started Worker-{0} on block {1}", this.GetHashCode(), BlockId), this, Logtype.Info);
        }
    }
}

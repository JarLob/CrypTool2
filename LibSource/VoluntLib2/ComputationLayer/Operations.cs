using System;
using System.Collections;
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
    /// Abstract super class of all operations:
    /// Operations are state machines that send messages and work on received message using the ConnectionManager
    /// If an operation's IsFinished equals true, it can be deleted by ConnectionManager
    /// </summary>
    internal abstract class Operation
    {
        /// <summary>
        /// Needed by each operation
        /// </summary>
        public ComputationManager ComputationManager { get; set; }

        /// <summary>
        /// Tells the worker thread if this operation is finished. If it is, it can be deleted
        /// </summary>
        public abstract bool IsFinished { get; }

        /// <summary>
        /// Called by the worker thread (cooperative multitasking)
        /// </summary>
        public abstract void Execute();
    }

    internal class CheckRunningWorkersOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
            foreach (JobAssignment assignment in ComputationManager.JobAssignments.Values)
            {
                //1) get the amount of needed workers
                int neededWorkers = assignment.AmountOfWorkers;

                //2) check and remove workers that are finished
                int runningWorkers = 0;
                List<Worker> removeList = new List<Worker>();
                for (int i = 0; i < assignment.Workers.Count; i++)
                {
                    Worker worker = (Worker)assignment.Workers[i];
                    if (!worker.WorkerThread.IsAlive)
                    {
                        removeList.Add(worker);
                    }
                    else
                    {
                        runningWorkers++;
                    }
                }
                foreach (Worker worker in removeList)
                {
                    assignment.Workers.Remove(worker);
                }
                //3) start workers until we have as much as we need
                try
                {
                    while (runningWorkers < neededWorkers)
                    {                        
                        BigInteger blockId = GetFreeBlockId(assignment);
                        if (blockId.Equals(BigInteger.MinusOne))
                        {
                            //no free blockId available; thus, we return
                            return;
                        }
                        Worker worker = new Worker(assignment.Job, assignment.CalculationTemplate, ComputationManager.VoluntLib);
                        assignment.Workers.Add(worker);
                        worker.Start(blockId);
                        ComputationManager.VoluntLib.OnTaskStarted(this, new TaskEventArgs(assignment.Job.JobId, blockId, TaskEventArgType.Started));
                        runningWorkers++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Could not start workers: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        private BigInteger GetFreeBlockId(JobAssignment assignment)
        {
            Job job = assignment.Job;
            ArrayList workers = assignment.Workers;
            while (job.FreeBlocksInEpoch() - assignment.Workers.Count > 0)
            {
                BigInteger blockid = job.GetFreeBlockId();
                if (blockid.Equals(BigInteger.MinusOne))
                {
                    return BigInteger.MinusOne;
                }
                bool different = true;
                foreach (Worker worker in workers)
                {
                    if (worker.BlockId.Equals(blockid))
                    {
                        different = false;
                        break;
                    }
                }
                if (different)
                {
                    return blockid;
                }
            }
            return BigInteger.MinusOne;
        }
    }
}

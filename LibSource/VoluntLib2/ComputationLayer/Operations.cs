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

    /// <summary>
    /// This operation is responsible for starting new workers for our jobs
    /// </summary>
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

                //0) update epoch and bitmask
                assignment.Job.CheckAndUpdateEpochAndBitmask();

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
                        //check, if the worker returned a result, if yes add it to our local results
                        if (worker.CalculationResult != null)
                        {
                            var list = worker.CalculationResult.LocalResults;
                            var blockid = worker.CalculationResult.BlockID;
                            worker.Job.JobEpochState.ResultList = worker.ACalculationTemplate.MergeResults(worker.Job.JobEpochState.ResultList, list);                            
                            uint bitid = (uint)(blockid % (worker.Job.JobEpochState.Bitmask.MaskSize * 8));
                            worker.Job.JobEpochState.Bitmask.SetBit(bitid, true);
                            Logger.LogText(String.Format("Set one bit to true in block id {1} of job {0} which is bit {2} in bitmask", blockid, BitConverter.ToString(worker.Job.JobId.ToByteArray()), bitid), this, Logtype.Debug);
                            ComputationManager.VoluntLib.OnJobProgress(this, new JobProgressEventArgs(worker.Job.JobId, worker.Job.JobEpochState.ResultList.ToList(), worker.Job.NumberOfBlocks, worker.Job.NumberOfCalculatedBlocks));
                            //finally, send our result to everyone
                            ComputationManager.VoluntLib.JobManager.SendResponseJobMessage(null, worker.Job);
                        }
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

        /// <summary>
        /// Returns a free block id
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
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

    /// <summary>
    /// This operation merges two EpochStates (bitmask and epoch id)
    /// </summary>
    internal class MergeResultsOperation : Operation
    {
        private bool executed = false;
        private Job LocalJob { get; set; }
        private Job RemoteJob { get; set; }

        private Logger Logger = Logger.GetLogger();

        public MergeResultsOperation(Job localJob, Job remoteJob)
        {
            LocalJob = localJob;
            RemoteJob = remoteJob;
        }


        public override bool IsFinished
        {
            get { return executed; }
        }

        public override void Execute()
        {            
            if (RemoteJob.JobEpochState.EpochNumber < LocalJob.JobEpochState.EpochNumber)
            {
                //remote epoch is smaller than ours; we ignore it
                Logger.LogText(String.Format("Ignored received epoch state of job {0} since it is in epoch {1}; we are already in {2}", BitConverter.ToString(LocalJob.JobId.ToByteArray()), RemoteJob.JobEpochState.EpochNumber, LocalJob.JobEpochState.EpochNumber), this, Logtype.Debug);
            }
            else if (RemoteJob.JobEpochState.EpochNumber > LocalJob.JobEpochState.EpochNumber)
            {
                //remote epoch index is greater than ours; we take it
                LocalJob.JobEpochState = (EpochState)RemoteJob.JobEpochState.Clone();
                Logger.LogText(String.Format("Took epoch state of job {0} since it is in epoch {1}; we were in {2}", BitConverter.ToString(LocalJob.JobId.ToByteArray()), RemoteJob.JobEpochState.EpochNumber, LocalJob.JobEpochState.EpochNumber), this, Logtype.Debug);
            }
            else
            {
                //remote epoch index is equal to ours
                //case A: we have an job assignment; thus, we have access to a merge function
                JobAssignment assignment = ComputationManager.JobAssignments[LocalJob.JobId];
                if (assignment != null)
                {
                    var mergedResultLists = assignment.CalculationTemplate.MergeResults(RemoteJob.JobEpochState.ResultList, LocalJob.JobEpochState.ResultList);
                    LocalJob.JobEpochState.ResultList = mergedResultLists;
                    LocalJob.JobEpochState.Bitmask = LocalJob.JobEpochState.Bitmask | RemoteJob.JobEpochState.Bitmask;
                    ComputationManager.VoluntLib.OnJobProgress(this, new JobProgressEventArgs(LocalJob.JobId, LocalJob.JobEpochState.ResultList.ToList(), LocalJob.NumberOfBlocks, LocalJob.NumberOfCalculatedBlocks));
                    Logger.LogText(String.Format("Merged two EpochStates using MergeResultsMethod of job {0}", BitConverter.ToString(LocalJob.JobId.ToByteArray())), this, Logtype.Debug);                    
                }
                //case B: we have no job assignment; thus, we can not merge and keep the one with more computed jobs
                else
                {
                    uint finishedLocalJobs = LocalJob.JobEpochState.Bitmask.GetSetBitsCount();
                    uint finishedRemoteJobs = RemoteJob.JobEpochState.Bitmask.GetSetBitsCount();
                    if (finishedRemoteJobs > finishedLocalJobs)
                    {
                        LocalJob.JobEpochState = (EpochState)RemoteJob.JobEpochState.Clone();
                        Logger.LogText(String.Format("Kept remote epoch state of job {0}", BitConverter.ToString(LocalJob.JobId.ToByteArray())), this, Logtype.Debug);
                    }
                    else
                    {
                        Logger.LogText(String.Format("Kept local epoch state of job {0}", BitConverter.ToString(LocalJob.JobId.ToByteArray())), this, Logtype.Debug);
                    }
                }
            }
            executed = true;
        }
    }
}

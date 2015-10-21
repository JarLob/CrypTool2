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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.calculationLayer
{
    public class CalculationLayer : ICalculationLayer
    {
        #region private member 

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int amountOfWorker;
        private readonly ACalculationTemplate calculationTemplate;
        private readonly BigInteger jobID;
        private readonly ConcurrentDictionary<BigInteger, CancellationTokenSource> runningWorkers;
        private LocalStateManager<EpochState> localStateManager;

        #endregion

        #region properties

        public byte[] JobPayload { get; set; }

        public bool IsStarted { get; private set; }

        public int NumberOfRunningWorker
        {
            get { return runningWorkers.Count; }
        }

        #endregion

        /// <summary>
        ///   The calculationLayer is responsible for creating and managing Tasks, that are executing the User's given logic.
        ///   In order to operate a LocalStateManager has to be set, before the Start-Method is called.
        /// </summary>
        public CalculationLayer(BigInteger jobID, ACalculationTemplate calculationTemplate, int amountOfWorker)
        {
            runningWorkers = new ConcurrentDictionary<BigInteger, CancellationTokenSource>();

            this.jobID = jobID;
            calculationTemplate.WorkerLogic.JobID = jobID.ToByteArray();
            this.calculationTemplate = calculationTemplate;
            this.amountOfWorker = amountOfWorker;
        }

        /// <summary>
        ///   Registers the local state manager.
        /// </summary>
        /// <param name="stateManager">The state manager.</param>
        public void RegisterLocalStateManager(LocalStateManager<EpochState> stateManager)
        {
            localStateManager = stateManager;
            if (localStateManager.ManagementLayer != null)
                calculationTemplate.WorkerLogic.ProgressChanged += localStateManager.ManagementLayer.OnTaskProgress;
        }

        /// <summary>
        ///   Merges two result-lists by calling the MergeResults-Method of the CalculationTemplate
        /// </summary>
        public List<byte[]> MergeResults(IEnumerable<byte[]> a, IEnumerable<byte[]> b)
        {
            return calculationTemplate.MergeResults(a, b).ToList();
        }

        #region start, update and stop all tasks

        /// <summary>
        ///   Starts the initial Tasks and binds to the StateHasBeenUpdate in order to update the tasks soon as the local state
        ///   updates
        /// </summary>
        public bool Start()
        {
            if (localStateManager == null)
                return false;

            var i = 0;
            while (i++ < amountOfWorker && StartCalculationTask()) {}

            Logger.Info("Started" + (i - 1) + "Worker");
            localStateManager.StateHasBeenUpdated += UpdateCalculationTasks;
            IsStarted = true;
            return true;
        }

        /// <summary>
        ///   Calls every cancellation token and unbinds events
        /// </summary>
        public void Stop()
        {
            foreach (var jobId in runningWorkers.Keys)
            {
                CancelTask(jobId);
            }

            localStateManager.StateHasBeenUpdated -= UpdateCalculationTasks;

            if (localStateManager.ManagementLayer != null)
                calculationTemplate.WorkerLogic.ProgressChanged -= localStateManager.ManagementLayer.OnTaskProgress;

            IsStarted = false;
        }

        /// <summary>
        ///   Stops Worker, that are calculating finished blocks and tries to start new worker until amountOfWorker is reached.
        ///   Note, that this may not be possible, since for example all Tasks have to operate within the same Epoch.
        /// </summary>
        private void UpdateCalculationTasks(object sender, JobProgressEventArgs arg)
        {
            if (localStateManager.LocalState.IsFinished())
                return;

            //stop worker if its block has been calculated
            foreach (var key in runningWorkers.Keys.Where(key => localStateManager.LocalState.IsBlockCalculated(key)))
            {
                Logger.Debug("External Change on block [" + key + "]. Canceling local task.");
                CancelTask(key);
            }

            //start new worker
            int amountOfWorkerBefore, i;
            string workingOnAfter = "", workingOnBefore;
            bool startedLessThenExpected;

            lock (runningWorkers)
            {
                amountOfWorkerBefore = runningWorkers.Count;
                workingOnBefore = runningWorkers.Keys.Aggregate("", (current, key) => current + (key + " "));

                var limit = amountOfWorker - amountOfWorkerBefore;
                for (i = 0; i < limit && StartCalculationTask(); i++)
                {
                    workingOnAfter = runningWorkers.Keys.Aggregate("", (current, key) => current + (key + " "));
                }
                startedLessThenExpected = i - 1 < limit;
            }

            if (startedLessThenExpected)
                Logger.Info("Running: " + amountOfWorkerBefore + "/" + amountOfWorker 
                    + " on "+ workingOnBefore + " Restarted: " 
                    + i + " worker on " + workingOnAfter);
        }

        #endregion

        #region start and process results single task

        /// <summary>
        ///   Reserves a free block, creates a new Task that is executing the DoWork of the WorkerLogic.
        ///   A CancellationToken for the Task is stored in the RunningWorkers Dictionary
        /// </summary>
        /// <returns>whether a task has been created</returns>
        private bool StartCalculationTask()
        {
            var freeBlockID = localStateManager.LocalState.GetFreeBlock(runningWorkers.Keys.ToList());
            if (freeBlockID == -1)
            {
                Logger.Info("Could not start worker. No free block");
                return false;
            }

            var nextBlockID = freeBlockID;
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            runningWorkers.TryAdd(nextBlockID, tokenSource);

            // start the worker task
            Task.Factory.StartNew((_ =>
            {
                OnTaskStarted(nextBlockID);
                try
                {
                    return calculationTemplate.WorkerLogic.DoWork(JobPayload, nextBlockID, token);
                } catch (Exception e)
                {
                    if (e is OperationCanceledException)
                    {
                        Logger.Info("Task has been cancled");
                    }
                    else
                    {
                        Logger.Warn(e.GetType() + "Exception in working Task: " + e.Message +
                                    "(see Debug for stackTrace)");
                        Logger.Debug(e.StackTrace);
                    }
                    tokenSource.Cancel();
                    return null;
                }
            }), null, token).ContinueWith(ProcessTaskResults, token);

            // remove block reservation on cancellation
            tokenSource.Token.Register(blockObj =>
            {
                var blockID = blockObj as BigInteger? ?? 0; //cast to nullable BigInteger and set to 0 if null
                CancellationTokenSource removedToken;
                runningWorkers.TryRemove(blockID, out removedToken);
                OnTaskStopped(blockID, true);
            }, nextBlockID);
            return true;
        }

        /// <summary>
        ///   Meant to be called after a Worker has finished.
        ///   Removes the block reservation and pushes the merged results to the LocalStateManager.
        /// </summary>
        private void ProcessTaskResults(Task<CalculationResult> task)
        {
            try
            {
                var results = task.Result;
                Logger.Debug("Worker on " + results.BlockID + "has finished");

                lock (runningWorkers)
                {
                    CancellationTokenSource removedToken;
                    runningWorkers.TryRemove(results.BlockID, out removedToken);

                    var merged = MergeResults(results.LocalResults, localStateManager.LocalState.ResultList);
                    localStateManager.FinishCalculation(results.BlockID, merged);
                }
                OnTaskStopped(results.BlockID, false);
            } catch (Exception e)
            {
                Logger.Error("Could not process Worker's result" + e.Message + "(see Debug for stackTrace)");
                Logger.Debug(e.StackTrace);
            }
        }

        #endregion

        #region events

        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<TaskEventArgs> TaskStopped;

        #region invoker

        public virtual void OnTaskStarted(BigInteger blockID)
        {
            Console.Out.WriteLine("Worker on " + blockID + " has started");
            var handler = TaskStarted;
            if (handler != null)
            {
                handler(this, new TaskEventArgs(jobID.ToByteArray(), blockID, TaskEventArgType.Started));
            }
        }

        public virtual void OnTaskStopped(BigInteger blockID, bool hasBeenCanceled)
        {
            Console.Out.WriteLine("Worker on " + blockID + "has finished");
            var handler = TaskStopped;
            if (handler != null)
            {
                handler(this, new TaskEventArgs(jobID, blockID, hasBeenCanceled
                    ? TaskEventArgType.Canceled
                    : TaskEventArgType.Finished));
            }
        }

        #endregion

        #endregion events

        #region helper

        private void CancelTask(BigInteger key)
        {
            try
            {
                runningWorkers[key].Cancel();
            } catch (Exception e)
            {
                Logger.Error("Task Cancelation Failed: " + e.Message);
                Logger.Debug("Task Cancelation Failed: " + e.Message + "\n" + e.StackTrace);
            }
        }

        #endregion
    }
}
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
using System.Collections.Generic;
using System.Numerics;
using NLog;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.managementLayer.localStateManagement
{
    public class LocalStateManager<T> where T : ALocalState, new()
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ICalculationLayer calculationLayer;
        
        public T LocalState { get; private set; }
        public BigInteger JobID { get; set; }
        public ManagementLayer ManagementLayer { get; set; }
        public string World { get; set; }
    
        public LocalStateManager(BigInteger jobID)
        {
            JobID = jobID;
        }
     
        public ICalculationLayer CalculationLayer
        {
            get { return calculationLayer; }
            set
            {
                calculationLayer = value;
                if (ManagementLayer != null)
                {
                    calculationLayer.TaskStarted += ManagementLayer.OnTaskStarted;
                    calculationLayer.TaskProgress += ManagementLayer.OnTaskProgress;
                    calculationLayer.TaskStopped += ManagementLayer.OnTaskStopped;
                }
            }
        }

        public T GetNewStateObject()
        {
            return new T();
        }

        /// <summary>
        ///   Determines whether is a super set of the specified candidate.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        public bool IsSuperSetOf(T candidate)
        {
            if (LocalState == null)
            {
                LocalState = candidate;
            }
            var stateRelation = candidate.CompareWith(LocalState);


            return stateRelation == StateRelation.IsProperSubset;
        }

        /// <summary>
        ///   Processes the state.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        public void ProcessState(T candidate)
        {
            if (LocalState == null)
            {
                LocalState = candidate;
            }

            if (LocalState.IsFinished())
            {
                return;
            }

            var stateRelation = candidate.CompareWith(LocalState);

            if (logger.IsDebugEnabled)
            {
                logger.Debug("<ProcessState> LocalState: " + LocalState);
                logger.Debug("<ProcessState> Canidate: " + candidate);
                logger.Debug("<ProcessState> StateRelation: " + stateRelation);
            }

            if (stateRelation == StateRelation.OutOfSync)
            {
                MergeOrKeepBestState(candidate);
            }

            if (stateRelation == StateRelation.IsSuperSet)
            {
                LocalState.Deserialize(candidate.Serialize());
                OnStateHasBeenUpdated();
            }

            if (LocalState.IsFinished())
            {
                OnStateFinished();
            }
        }

        private void MergeOrKeepBestState(T candidate)
        {
            if (CanMergeStates)
            {
                lock (this)
                {
                    LocalState.MergeMetaData(candidate);
                    LocalState.ResultList = CalculationLayer.MergeResults(candidate.ResultList, LocalState.ResultList);
                }

                OnStateHasBeenMerged();
                OnStateHasBeenUpdated();
                
            }
            else
            {
                OnStateHasBeenMerged(); // propagate old state
                if ( ! LocalState.ContainsMoreInformationThan(candidate))
                {
                    LocalState = candidate;
                }
            }
        }

        private bool CanMergeStates
        {
            get { return CalculationLayer != null; }
        }

        public void FinishCalculation(BigInteger blockID, List<byte[]> mergedResults)
        {
            if (LocalState.IsFinished())
            {
                return;
            }

            logger.Debug("<finishBlockCalculation> BlockID: " + blockID);
            LocalState.MarkBlockAsCalculated(blockID);
            LocalState.ResultList = mergedResults;
            OnStateHasBeenMerged();
            OnStateHasBeenUpdated();

            if (LocalState.IsFinished())
            {
                OnStateFinished();
            }
        }

        public event EventHandler<JobProgressEventArgs> StateHasBeenUpdated;
        public event EventHandler<JobProgressEventArgs> StateHasBeenMerged;

        private void OnStateHasBeenUpdated()
        {
            logger.Debug("Fire StateHasBeenUpdated");
            var handler = StateHasBeenUpdated;
            if (handler != null)
            {
                handler(this, new JobProgressEventArgs(JobID, LocalState.ResultList, LocalState.NumberOfBlocks, LocalState.NumberOfCalculatedBlocks));
            }
        }

        private void OnStateHasBeenMerged()
        {
            logger.Debug("Fire StateHasBeenMerged");
            var handler = StateHasBeenMerged;
            if (handler != null)
            {
                handler(this, new JobProgressEventArgs(JobID, LocalState.ResultList, LocalState.NumberOfBlocks, LocalState.NumberOfCalculatedBlocks));
            }
        }

        private void OnStateFinished()
        {
            if (ManagementLayer != null)
            {
                ManagementLayer.OnJobFinished(this,
                    new JobProgressEventArgs(JobID, LocalState.ResultList, LocalState.NumberOfBlocks, LocalState.NumberOfCalculatedBlocks));
            }
        }

        #region Events
    }

    #endregion
}
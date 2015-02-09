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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
using NLog;
using voluntLib.calculationLayer;
using voluntLib.common;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;
using voluntLib.common.utils;
using voluntLib.communicationLayer.messages.messageWithCertificate;
using voluntLib.managementLayer.dataStructs;
using voluntLib.managementLayer.delayedTasks;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;

#endregion

namespace voluntLib.managementLayer
{
    /// <summary>
    ///   Main class for the ManagementLayer
    /// </summary>
    public class ManagementLayer : IManagementLayerCallback
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly TaskContainer taskContainer;

        public WorkingPeerList WorkingPeers { get; set; }
        public ICommunicationLayer NetworkCommunicationLayer { get; set; }
        public ICommunicationLayer FileCommunicationLayer { get; set; }

        public int MaximumBackoffTime { get; set; }
        public List<string> Worlds { get; set; }
        public JobContainer Jobs { get; set; }
        public Dictionary<BigInteger, LocalStateManager<EpochState>> LocalStates { get; set; }
        public string CertificateName { get; set; }

        public ManagementLayer()
        {
            Worlds = new List<string>();
            Jobs = new JobContainer();
            MaximumBackoffTime = 1000;
            taskContainer = new TaskContainer(this);
            LocalStates = new Dictionary<BigInteger, LocalStateManager<EpochState>>();
            WorkingPeers = new WorkingPeerList();
        }

        /// <summary>
        ///   Gets the world list.
        /// </summary>
        /// <returns></returns>
        public List<string> GetWorlds()
        {
            return Worlds;
        }

        /// <summary>
        ///   Called when a remote sends a  calculation state.
        ///   This method will feed the incoming state to the local stateManager and will send its own state - by using a backoff -
        ///   if the incoming state was older.
        ///   If no StateManager exists, it ll create one fromIP the stored job configurations.
        ///   If the job is not known this does nothing.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="from">From.</param>
        public virtual void OnIncomingState(PropagateStateMessage message, IPAddress from)
        {
            var jobID = message.Header.JobID;
            var worldName = message.Header.WorldName;
          
            var job = Jobs.GetJob(jobID);
            if (job == null) //ignor unknown jobs
            {
                return;
            }

            if (job.IsDeleted)
            {
                RespondWithJobDeletionMessage(job, from);
                return;
            }

            CreateOwnManagerIfNoneIsPresent(jobID, worldName);
            var localStateManager = LocalStates[jobID];
            var incomingState = CreateIncomingState(message, localStateManager, jobID);
            if (IncomingStateIsOlder(localStateManager, incomingState))
            {
                RespondWithOwnState(jobID, worldName, localStateManager.LocalState);
            } 
            else
            {
                ProcessUsefulStates(jobID, incomingState);
            }

            //add to working peer list
            WorkingPeers.AddOrUpdate(message);
        }

        private void ProcessUsefulStates(BigInteger jobID, EpochState incomingState)
        {
            var localStateManager = LocalStates[jobID];
            taskContainer.GetPropagateStateTask(jobID).StopTimer();
            localStateManager.ProcessState(incomingState);
        }

        private void CreateOwnManagerIfNoneIsPresent(BigInteger jobID, string worldName)
        {
            if ( ! LocalStates.ContainsKey(jobID))
            {
                CreateConfiguredStateManager(jobID, worldName);
            }
        }

        private static bool IncomingStateIsOlder(LocalStateManager<EpochState> localStateManager, EpochState incomingState)
        {
            return localStateManager.IsSuperSetOf(incomingState);
        }

        private void RespondWithOwnState(BigInteger jobID, string worldName, ALocalState localState)
        {
            taskContainer.GetPropagateStateTask(jobID).StartTimer(worldName, localState, MaximumBackoffTime);
        }

        /// <summary>
        ///   Stops this instance by stoping all connected calculationlayer
        /// </summary>
        public void Stop()
        {
            var calculationsLayers = LocalStates.Values.Select(stateManager => stateManager.CalculationLayer);
            var activeCalcLayers = calculationsLayers.Where(calculationLayer => calculationLayer != null && calculationLayer.IsStarted);

            foreach (var calculationLayer in activeCalcLayers)
            {
                calculationLayer.Stop();
            }
        }

        /// <summary>
        ///   Joins an distributed Job and starts a given number of local worker worker threads, executing a copy of the template.
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="template"></param>
        /// <param name="amountOfWorker"></param>
        public void JoinNetworkJob(BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {
            var job = Jobs.GetJob(jobID);
            if (job.JobPayload == null)
            {
                NetworkCommunicationLayer.RequestJobDetails(jobID, job.World, IPAddress.Any);
            }

            var stateManager = CreateStateManager(jobID, job);
            CreateCalculationLayer(jobID, template, amountOfWorker, stateManager);

            NetworkCommunicationLayer.JoinNetworkJob(jobID, job.World, IPAddress.Any);

            AddOwnWorklog(jobID);
            WaitForNetworkToSyncState(stateManager, job);
        }

        private static void CreateCalculationLayer(BigInteger jobID, ACalculationTemplate template, int amountOfWorker, LocalStateManager<EpochState> stateManager)
        {
            var calculationLayer = new CalculationLayer(jobID, template, amountOfWorker);
            calculationLayer.RegisterLocalStateManager(stateManager);
            stateManager.CalculationLayer = calculationLayer;
        }

        private LocalStateManager<EpochState> CreateStateManager(BigInteger jobID, NetworkJob job)
        {
            //create state
            if ( ! LocalStates.ContainsKey(jobID))
            {
                CreateConfiguredStateManager(jobID, job.World);
                LocalStates[jobID].ProcessState(new EpochState(job.StateConfig));
            }
            return LocalStates[jobID];
            
        }

        private void WaitForNetworkToSyncState(LocalStateManager<EpochState> stateManager, NetworkJob job)
        {
        // wait 2 times RandomDelayMaximum to let the network sync the state. 
            TaskHelper.Delay(MaximumBackoffTime*2).ContinueWith(_ =>
            {
                stateManager.CalculationLayer.JobPayload = job.JobPayload;
                stateManager.CalculationLayer.Start();
            });
        }

        #region world 

        public void OnWorldListRequest(IPAddress from)
        {
            taskContainer.GetSendWorldListTask().StartTimer(Worlds, MaximumBackoffTime);
        }

        public void OnWorldListReceived(List<string> worldList, IPAddress from)
        {
            var newWorlds = worldList.Except(Worlds).ToList();
            if (newWorlds.Any())
            {
                Worlds.AddRange(newWorlds);
                OnWorldsChanged();
                taskContainer.GetSendWorldListTask().StopTimer(); //there is no need to send our old list anymore
            }

            var missingWorlds = Worlds.Except(worldList);
            if (missingWorlds.Any())
                taskContainer.GetSendWorldListTask().StartTimer(Worlds, MaximumBackoffTime);
            else
                taskContainer.GetSendWorldListTask().StopTimer(); //someone has send an "up-to-date"-List  
            
        }

        #endregion world

        #region job

        #region jobList

        public virtual void OnJobListReceived(string world, List<NetworkJob> receivedJobs, IPAddress from)
        {
            var ownJobs = Jobs.GetJobsOfWorld(world);

            TryAddWorld(world);
            HandleUnknownJobs(receivedJobs, ownJobs, world, @from);
            RespondOwnListIfJobsAreMissing(receivedJobs, ownJobs, world);

            // send deletion messsage for each deleted job
            foreach (var job in ownJobs.FindAll(job => job.IsDeleted).Intersect(receivedJobs))
            {
                RespondWithJobDeletionMessage(job, from);
            }
        }

        private void RespondOwnListIfJobsAreMissing(List<NetworkJob> receivedJobs, List<NetworkJob> ownJobs, string world)
        {
            var sendJobListTask = taskContainer.GetSendJobListTask(world);

            var missingJobs = ownJobs.Except(receivedJobs);
            if (missingJobs.Any())
            {
                sendJobListTask.StartTimer(ownJobs, MaximumBackoffTime);
            }
            else
            {
                sendJobListTask.StopTimer(); //someone has send an "up-to-date"-List  
            }
        }

        private void HandleUnknownJobs(List<NetworkJob> receivedJobs, List<NetworkJob> ownJobs, string world, IPAddress @from)
        {
            var unknownJobs = receivedJobs.Except(ownJobs).ToList();
            if (unknownJobs.Any())
            {
                Jobs.AddJobRange(unknownJobs);
                unknownJobs.ForEach(PersistJob);
                OnJobsChanged();

                var sendJobListTask = taskContainer.GetSendJobListTask(world);
                if (@from.Equals(IPAddress.Broadcast))
                {
                    sendJobListTask.StopTimer(); //there is no need to send our old list anymore
                }
                else
                {
                    sendJobListTask.StartTimer(ownJobs, -1); // new jobs received, publish on broadcast
                }
            }
        }
        
        public virtual void OnJobListRequest(string world, IPAddress from)
        {
            taskContainer.GetSendJobListTask(world).StartTimer(Jobs.GetJobsOfWorld(world), MaximumBackoffTime);
        }

        #endregion

        #region jobDetail

        public void OnJobDetailRequest(string world, BigInteger jobID, IPAddress from)
        {
            var job = Jobs.GetJob(jobID);
            if (job == null)
                return;
            
            //job is deleted
            if (job.IsDeleted)
            {
                RespondWithJobDeletionMessage(job, from);
                return;
            }

            taskContainer.GetSendJobDetailTask(world).StartTimer(job, MaximumBackoffTime);
        }

        /// <summary>
        ///   This Method is meant to be called on incoming JobDetails.
        ///   It sets the payload and invokes the JobListChanged-event iff
        ///   the client knows the job (match of world and jobID) and the the known jobs doesn't already have a detail payload
        /// </summary>
        public void OnJobDetailsReceived(string world, BigInteger jobID, byte[] payload)
        {
            taskContainer.GetSendJobDetailTask(world).StopTimer();
            var job = Jobs.GetJob(jobID);

            //unknown job
            if (job == null)
                return;

            //already have payload
            if (job.HasPayload())
                return;

            job.JobPayload = payload;
            PersistJob(job);
            OnJobsChanged();
        }

        #endregion

        #region create

        /// <summary>
        ///   This Method is meant to be called in incoming CreateNetworkJob.
        ///   It adds the new job if no job with an equal jobID within the same world is known and invokes the JobListChanged-Event
        /// </summary>
        public void OnJobCreation(NetworkJob newJob)
        {
            var job = Jobs.GetJob(newJob.JobID);
            if (job != null)
                return;

            Jobs.AddJob(newJob);
            PersistJob(newJob);
            TryAddWorld(newJob.World);
            OnJobsChanged();
        }

        public void CreateNetworkJob(NetworkJob job)
        {
            OnJobCreation(job);
            NetworkCommunicationLayer.CreateNetworkJob(job, IPAddress.Any);
        }

        #endregion

        #region delete

        public void OnJobDeletion(DeleteNetworkJobMessage message, IPAddress fromIP)
        {
            // is job known
            var job = Jobs.GetJob(message.Header.JobID);
            if (job == null)
                return;

            //job is already deleted, get off
            if (job.IsDeleted)
            {
                if (fromIP.Equals(IPAddress.Broadcast))
                    taskContainer.GetJobDeletionTasks(job.JobID).StopTimer();
                return;
            }

            // is operation allowed
            if ( ! (message.FromAdmin || job.Creator.Equals(message.Header.SenderName)))
                return;
            

            // delete job
            job.IsDeleted = true;
            job.DeletionMessage = message;
            if (FileCommunicationLayer != null)
                FileCommunicationLayer.DeleteNetworkJob(message, IPAddress.Any);

            OnJobsChanged();
        }

        public void DeleteNetworkJob(BigInteger jobId)
        {
            // is job known
            var job = Jobs.GetJob(jobId);
            if (job == null)
                return;

            // send message
            var message = NetworkCommunicationLayer.DeleteNetworkJob(job, IPAddress.Any);

            //call self
            OnJobDeletion(message, IPAddress.Any);
        }

        #endregion

        #endregion job

        #region events

        public event EventHandler<PropertyChangedEventArgs> WorldListChanged;
        public event EventHandler<PropertyChangedEventArgs> JobListChanged;
        public event EventHandler<JobProgressEventArgs> JobFinished;
        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<TaskEventArgs> TaskStopped;
        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskProgress;

        #region invoker

        private void OnWorldsChanged()
        {
            Logger.Info("Worlds have changed. Fire Event");
            var handler = WorldListChanged;

            InvokeHandler(new PropertyChangedEventArgs("WorldList"), handler);
        }
     
        private void OnJobsChanged()
        {
            Logger.Info("Jobs have changed. Fire Event");
            var handler = JobListChanged;

            InvokeHandler(new PropertyChangedEventArgs("JobList"), handler); 
        }

        public void OnJobFinished(object sender, JobProgressEventArgs eventArgs)
        {
            Logger.Info("A Job has finished. Fire Event");
            var handler = JobFinished;

            InvokeHandler(eventArgs, handler);
        }

        private void OnJobProgress(object sender, JobProgressEventArgs eventArgs)
        {
            var handler = JobProgress;
            InvokeHandler(eventArgs, handler);
        }

        public virtual void OnTaskStopped(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskStopped;
            InvokeHandler(taskEventArgs, handler);
        }

        public virtual void OnTaskStarted(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskStarted;
            InvokeHandler(taskEventArgs, handler);
        }

        public virtual void OnTaskProgress(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskProgress;
            InvokeHandler(taskEventArgs, handler);
        }

        #region stupid crappy redundante InvokeHandler Methode and overrides ...
        //why the heck am i not allowed to use baseclasses for generic types ... 
        // okay... than do the same stuff all over again....

        #region ... you really want to see that mess?

        private void InvokeHandler(JobProgressEventArgs eventArgs, EventHandler<JobProgressEventArgs> handler)
        {
            if (handler != null)
                handler(this, eventArgs);
        }

        private void InvokeHandler(PropertyChangedEventArgs eventArgs, EventHandler<PropertyChangedEventArgs> handler)
        {
            if (handler != null)
                handler(this, eventArgs);
        }

        private void InvokeHandler(TaskEventArgs eventArgs, EventHandler<TaskEventArgs> handler)
        {
            if (handler != null)
                handler(this, eventArgs);
        }

        #endregion

        #endregion
        
        #endregion

        #endregion events

        #region helper

        private void TryAddWorld(string world)
        {
            if ( ! Worlds.Contains(world))
            {
                Worlds.Add(world);
                OnWorldsChanged();
            }
        }

        private void RespondWithJobDeletionMessage(NetworkJob job, IPAddress fromIP)
        {
            Logger.Debug("Found Deleted Job, sending stored JobDeletion-Message");
            if (fromIP.Equals(IPAddress.Broadcast))
            {
                taskContainer.GetJobDeletionTasks(job.JobID).StartTimer(job.DeletionMessage, MaximumBackoffTime);
            }
            else
            {
                NetworkCommunicationLayer.DeleteNetworkJob(job.DeletionMessage, fromIP);
            }
        }

        protected EpochState CreateIncomingState(PropagateStateMessage message, LocalStateManager<EpochState> localStateManager, BigInteger jobID)
        {
            var incomingState = localStateManager.GetNewStateObject();

            if (NullState.IsNullState(message.StateData))
                incomingState.ApplyConfig(Jobs.GetJob(jobID).StateConfig);
            else
                incomingState.Deserialize(message.StateData);
            
            return incomingState;
        }

        private void CreateConfiguredStateManager(BigInteger jobID, string worldName)
        {
            var stateManager = new LocalStateManager<EpochState>(jobID) {World = worldName, ManagementLayer = this};
            LocalStates.Add(jobID, stateManager);

            // bind events
            stateManager.StateHasBeenMerged += (sender, args) => NewStateToPropagate(jobID, worldName, stateManager.LocalState);
            stateManager.StateHasBeenUpdated += OnJobProgress;
        }

        private void NewStateToPropagate(BigInteger jobID, string worldName, EpochState localState)
        {
            AddOwnWorklog(jobID);
            NetworkCommunicationLayer.PropagateState(jobID, worldName, localState, IPAddress.Any);

            if (FileCommunicationLayer != null)
                FileCommunicationLayer.PropagateState(jobID, worldName, localState, IPAddress.Any);
            
        }

        private void AddOwnWorklog(BigInteger jobID)
        {
            var log = new WorkLog
            {
                Hostname = Environment.MachineName,
                JobID = jobID,
                LastReceivedMessage = DateTime.Now,
                Name = CertificateName
            };
            WorkingPeers.AddOrUpdate(log);
        }

        private void PersistJob(NetworkJob newJob)
        {
            if (FileCommunicationLayer != null)
                FileCommunicationLayer.CreateNetworkJob(newJob, IPAddress.Any);
        }

        #endregion
    }
}
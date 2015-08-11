using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.utils;
using Cryptool.PluginBase;
using voluntLib;
using voluntLib.common;
using voluntLib.common.interfaces;
using voluntLib.logging;
using WorkspaceManager.Model;
using NLog;
using voluntLib.common.eventArgs;

namespace CrypCloud.Core
{
    public class CrypCloudCore
    {
        public string DefaultWorld = "CryptCloud";
      
        #region singleton

        private static CrypCloudCore instance;
       
        public static CrypCloudCore Instance
        {
            get { return instance ?? (instance = new CrypCloudCore()); }
        }

      
        #endregion

        private VoluntLib voluntLib;

        #region properties

        public bool IsRunning
        {
            get { return voluntLib.IsStarted; } 
        }

        public int AmountOfWorker { get; private set; }

        #endregion

        protected CrypCloudCore()
        {
            AmountOfWorker = 2;
            voluntLib = InitVoluntLib();
        }

        private VoluntLib InitVoluntLib()
        {
            var vlib = new VoluntLib
            {
                LogMode = global::voluntLib.logging.LogMode.EventBased,
                EnablePersistence = true,
                LoadDataFromLocalStorage = true
            };
            vlib.JobListChanged += OnJobListChanged;
            vlib.JobProgress += OnJobStateChanged;
            vlib.TaskStarted += OnTaskHasStarted;
            vlib.TaskStopped += OnTaskHasStopped;
            vlib.TaskProgress += OnTaskProgress;
            vlib.JobFinished += OnJobFinished;
            vlib.ApplicationLog += ConvertVoluntLibToCtLogs;
            return vlib;
        }

        public bool Login(X509Certificate2 ownCertificate)
        {
            if (IsRunning)
            {
                return false;
            }
            var rootCertificate = new X509Certificate2(Properties.Resources.rootCA);
            voluntLib.InitAndStart(rootCertificate, ownCertificate);
            OnConnectionStateChanged(true);
            return true;
        }


        public void Logout()
        {
            if ( ! IsRunning)
            {
                return;
            }
            voluntLib.Stop();
            voluntLib = InitVoluntLib();
        }
        
        public bool IsPartizipationOnJob()
        {
            return voluntLib.GetCurrentRunningWorkersPerJob().Count > 0;
        }
 


        //TODO @ckonze add admin names/move function over to voluntlib
        public bool UserCanDeleteJob(NetworkJob job)
        {
           return job.Creator.Equals(voluntLib.CertificateName);
        }
        
        public void RefreshJobList()
        {
            voluntLib.RefreshJobList(DefaultWorld);
        }

        public void StartLocalCalculation(BigInteger jobId, ACalculationTemplate template)
        {
            voluntLib.JoinNetworkJob(jobId, template, AmountOfWorker);
        }

        public void StopLocalCalculation(BigInteger jobId)
        {
            voluntLib.StopCalculation(jobId);
        }

        public List<NetworkJob> GetJobs()
        {
            if ( ! voluntLib.IsStarted)
            {
                return new List<NetworkJob>();
            }

           return voluntLib.GetJobsOfWorld(DefaultWorld);
        }
        
        public BigInteger GetProgressOfJob(BigInteger jobID)
        {
            if ( ! voluntLib.IsStarted)
            {
                return new BigInteger(0);
            }

           return voluntLib.GetCalculatedBlocksOfJob(jobID);
        }

        public bool JobHasWorkspace(BigInteger jobId)
        {
            var job = voluntLib.GetJobByID(jobId);
            return job != null && job.HasPayload();
        }

        public void DownloadWorkspaceOfJob(BigInteger jobId)
        {
            var job = voluntLib.GetJobByID(jobId);
            if (job == null) 
                return;

            if (job.HasPayload())
            {
                OnJobListChanged(this, null);
                return;
            }

            voluntLib.RequestJobDetails(job);
        }

        public WorkspaceModel GetWorkspaceOfJob(BigInteger jobId)
        {
            var job = voluntLib.GetJobByID(jobId);
            if (job == null || !job.HasPayload())
                return null;

            WorkspaceModel workspaceOfJob;
            try
            {
                workspaceOfJob = PayloadSerialization.Deserialize(job.JobPayload);
            }
            catch (Exception e)
            {
                return null;
            }

            var cloudComponents = workspaceOfJob.GetAllPluginModels().Where(pluginModel => pluginModel.Plugin is ACloudCompatible);
            foreach (var cloudComponent in cloudComponents)
            {
                ((ACloudCompatible)cloudComponent.Plugin).JobID = jobId;
            }
            return workspaceOfJob;
        }

        public void DeleteJob(BigInteger jobId)
        {
            if (UserCanDeleteJob(voluntLib.GetJobByID(jobId)))
            {
                voluntLib.DeleteNetworkJob(jobId);
            }
        }

        public bool CreateJob(string jobType, string jobName, string jobDescription, WorkspaceModel workspaceModel)
        {
            var cloudComponent = GetCloudComponent(workspaceModel);
            if (cloudComponent == null) return false;

            var numberOfBlocks = cloudComponent.NumberOfBlocks;
            var serialize = PayloadSerialization.Serialize(workspaceModel);

            var jobID = voluntLib.CreateNetworkJob(DefaultWorld, jobType, jobName, jobDescription, serialize, numberOfBlocks);
            return jobID != -1;
        }

        private static ACloudCompatible GetCloudComponent(WorkspaceModel workspaceModel)
        {
            try
            {
                var cloudModel = workspaceModel.GetAllPluginModels().First(pluginModel => pluginModel.Plugin is ACloudCompatible);
                return cloudModel.Plugin as ACloudCompatible;
            }
            catch
            {
                return null;
            }
        }

        #region events  

        public event EventHandler<GuiLogEventArgs> ApplicationLog;
        public event Action<bool> ConnectionStateChanged;
        public event EventHandler JobListChanged;
        public event EventHandler<JobProgressEventArgs> JobStateChanged;

        public event EventHandler<TaskEventArgs> TaskHasStarted;
        public event EventHandler<TaskEventArgs> TaskHasStopped;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<JobProgressEventArgs> JobFinished;

        protected virtual void OnTaskHasStarted(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskHasStarted;
            if (handler != null) handler(this, taskEventArgs);
        }

        protected virtual void OnTaskHasStopped(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskHasStopped;
            if (handler != null) handler(this, taskEventArgs);
        }

        protected virtual void OnTaskProgress(object sender, TaskEventArgs taskEventArgs)
        {
            var handler = TaskProgress;
            if (handler != null) handler(this, taskEventArgs);
        }

        protected virtual void OnJobFinished(object sender, JobProgressEventArgs jobProgressEventArgs)
        {
            var handler = JobFinished;
            if (handler != null) handler(this, jobProgressEventArgs);
        }

        protected virtual void OnConnectionStateChanged(bool connected)
        {
            Action<bool> handler = ConnectionStateChanged;
            if (handler != null) handler(connected);
        }
      
        protected virtual void OnJobListChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var handler = JobListChanged;
            if (handler != null) handler(sender, propertyChangedEventArgs);
        }

        protected virtual void OnJobStateChanged(object sender, JobProgressEventArgs jobProgressEventArgs)
        {
            var handler = JobStateChanged;
            if (handler != null) handler(sender, jobProgressEventArgs);
        }

        protected virtual void OnApplicationLog(object sender, GuiLogEventArgs arg)
        {
            var handler = ApplicationLog;
            if (handler != null) handler(this, arg);
        }


        private void ConvertVoluntLibToCtLogs(object sender, LogEventInfoArg logEvent)
        {
            var notificationLevel = GetNotificationLevel(logEvent);
            var message = "(" + logEvent.Location + "): " + logEvent.Message;
            OnApplicationLog(sender, new GuiLogEventArgs(message, null, notificationLevel));
        }

    
        private static NotificationLevel GetNotificationLevel(LogEventInfoArg logEvent)
        {
            var notificationLevel = NotificationLevel.Info;
            if (logEvent.Level >= LogLevel.Error)
                notificationLevel = NotificationLevel.Error;
            if (logEvent.Level == LogLevel.Warn)
                notificationLevel = NotificationLevel.Warning;
            if (logEvent.Level < LogLevel.Info)
                notificationLevel = NotificationLevel.Debug;
            return notificationLevel;
        }

        #endregion

    }
}

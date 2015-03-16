using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.utils;
using voluntLib;
using voluntLib.common;
using voluntLib.common.interfaces;
using WorkspaceManager.Model;

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

        private readonly VoluntLib voluntLib;

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
            voluntLib = new VoluntLib();
            voluntLib.JobListChanged += OnJobListChanged;
        }

        public bool Login(X509Certificate2 ownCertificate)
        {
            if (IsRunning)
            {
                return false;
            }
            var rootCertificate = new X509Certificate2(Properties.Resources.rootCA);
            voluntLib.InitAndStart(rootCertificate, ownCertificate);
            return true;
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

            var workspaceOfJob = PayloadSerialization.Deserialize(job.JobPayload);
            var cloudComponents = workspaceOfJob.GetAllPluginModels().Where(pluginModel => pluginModel.Plugin is ACloudComponent);
            foreach (var cloudComponent in cloudComponents)
            {
                ((ACloudComponent) cloudComponent.Plugin).JobID = jobId;
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

        public bool CreateJob(string jobType, string jobName, string jobDescription, WorkspaceModel workspaceModel, BigInteger numberOfBlocks)
        {
            if (!HasACloudComponent(workspaceModel)) 
                return false;

            var jobID = voluntLib.CreateNetworkJob(DefaultWorld, jobType, jobName, jobDescription, PayloadSerialization.Serialize(workspaceModel), numberOfBlocks);
            return jobID != -1;
        }

        private static bool HasACloudComponent(WorkspaceModel workspaceModel)
        {
            try
            {
                return workspaceModel.GetAllPluginModels().Any(pluginModel => pluginModel.Plugin is ACloudComponent);
            }
            catch
            {
                return false;
            }
        }

   


        #region events 

        public event Action JobListChanged;

        protected virtual void OnJobListChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var handler = JobListChanged;
            if (handler != null) handler();
        }

        #endregion

    }
}

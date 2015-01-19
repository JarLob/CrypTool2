using System; 
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.utils;
using voluntLib; 
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

        public bool Login(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            if (IsRunning)
            {
                return false;
            }
            voluntLib.InitAndStart(caCertificate, ownCertificate);
            return true;
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

        public void GetJobs()
        {
            voluntLib.GetJobsOfWorld(DefaultWorld);
        }

        public WorkspaceModel GetWorkspaceOfJob(BigInteger jobId)
        {
            var job = voluntLib.GetJobByID(jobId);
            if (job == null)
            {
                return null;
            }
            var workspaceOfJob = PayloadSerialization.Deserialize(job.JobPayload);
            var cloudComonents = workspaceOfJob.GetAllPluginModels().Where(pluginModel => pluginModel.Plugin is ACloudComponent);
            foreach (var cloudComonent in cloudComonents)
            {
                ((ACloudComponent)cloudComonent.Plugin).JobID = jobId;
            }
            return workspaceOfJob;
        }

        public bool CreateJob(string jobType, string jobName, string jobDescription, WorkspaceModel workspaceModel, BigInteger numberOfBlocks)
        {
            if (HasACloudComponent(workspaceModel))
            {
                var jobID = voluntLib.CreateNetworkJob(DefaultWorld, jobType, jobName, jobDescription, PayloadSerialization.Serialize(workspaceModel), numberOfBlocks);
                return jobID != -1;
            }
            return false;
        }

        private static bool HasACloudComponent(WorkspaceModel workspaceModel)
        {
            return workspaceModel.GetAllPluginModels().Any(pluginModel => pluginModel.Plugin is ACloudComponent);
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

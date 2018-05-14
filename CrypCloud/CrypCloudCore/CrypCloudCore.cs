using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.Properties;
using CrypCloud.Core.utils;
using Cryptool.PluginBase;
using NLog;
using VoluntLib2;
using WorkspaceManager.Model;
using VoluntLib2.ManagementLayer;
using VoluntLib2.ComputationLayer;
using VoluntLib2.Tools;

namespace CrypCloud.Core
{ 
    public class CrypCloudCore
    {
        public string DefaultWorld = "CrypCloud";

        #region singleton

        private static CrypCloudCore instance;

        public static CrypCloudCore Instance
        {
            get { return instance ?? (instance = new CrypCloudCore()); }
        }

        #endregion

        private VoluntLib voluntLib;
        private readonly Dictionary<BigInteger, JobPayload> jobPayloadCache = new Dictionary<BigInteger, JobPayload>();
      
        #region properties

        public bool IsRunning
        {
            get { return voluntLib.IsStarted; }
        }

        public int AmountOfWorker { get; set; }
        public bool EnableOpenCL { get; set; }
        public int OpenCLDevice { get; set; }

        public bool WritePerformanceLog { get; set; }

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
                LocalStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrypCloud" + Path.DirectorySeparatorChar + "Jobs"),                
            };

            try
            {
                vlib.JobFinished -= OnJobFinished;
                vlib.TaskProgress -= OnTaskProgress;
                vlib.TaskStopped -= OnTaskHasStopped;
                vlib.TaskStarted -= OnTaskHasStarted;
                vlib.JobProgress -= OnJobStateChanged;
                vlib.JobListChanged -= OnJobListChanged;
            }
            finally
            {
                vlib.JobListChanged += OnJobListChanged;
                vlib.JobProgress += OnJobStateChanged;
                vlib.TaskStarted += OnTaskHasStarted;
                vlib.TaskStopped += OnTaskHasStopped;
                vlib.TaskProgress += OnTaskProgress;
                vlib.JobFinished += OnJobFinished;
            }         

            return vlib;
        }

        #region login/Logout/start/stop

        public bool Login(X509Certificate2 ownCertificate)
        {
            if (IsRunning)
            {
                return false;
            }
            try
            {
                var rootCertificate = new X509Certificate2(Resources.rootCA);
                voluntLib.Start(rootCertificate, ownCertificate);

                //When VoluntLib is started, the admin and banned lists are cleared
                //Thus, we here add the admin and banned certificates
                var adminCertificates = Resources.adminCertificates.Replace("\r", "");
                var adminList = adminCertificates.Split('\n').ToList();
                var bannedCertificates = Resources.bannedCertificates.Replace("\r", "");
                var bannedList = bannedCertificates.Split('\n').ToList();
                CertificateService.GetCertificateService().AdminCertificateList.AddRange(adminList);
                CertificateService.GetCertificateService().BannedCertificateList.AddRange(bannedList);

                OnConnectionStateChanged(true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Logout()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                voluntLib.JobListChanged -= OnJobListChanged;
                voluntLib.JobProgress -= OnJobStateChanged;
                voluntLib.TaskStarted -= OnTaskHasStarted;
                voluntLib.TaskStopped -= OnTaskHasStopped;
                voluntLib.TaskProgress -= OnTaskProgress;
                voluntLib.JobFinished -= OnJobFinished;
            }
            catch (Exception e) { }

            try
            {
                voluntLib.Stop();
                OnConnectionStateChanged(false);
            }
            finally
            {
                voluntLib = InitVoluntLib();  
            }

        }
        public void StartLocalCalculation(BigInteger jobId, ACalculationTemplate template)
        {
            voluntLib.JoinJob(jobId, template, AmountOfWorker);
        }

        public void StopLocalCalculation(BigInteger jobId)
        {
            voluntLib.StopCalculation(jobId);
        }
        #endregion
        

        #region job information

        public List<Job> GetJobs()
        {
            if (!voluntLib.IsStarted)
            {
                return new List<Job>();
            }

            return voluntLib.GetJobsOfWorld(DefaultWorld);
        }

        public Job GetJobsById(BigInteger jobid)
        {
            if (!voluntLib.IsStarted)
            {
                return new Job(jobid);
            }

            return voluntLib.GetJobByID(jobid);
        }

        public NetworkJobData GetJobDataById(BigInteger jobId)
        {
            if (!voluntLib.IsStarted) return null;

            return new NetworkJobData
            {
                Job = voluntLib.GetJobByID(jobId),
                HasWorkspace = () => JobHasWorkspace(jobId),
                CalculatedBlocks = () => GetCalculatedBlocksOfJob(jobId),
                Workspace = () => GetWorkspaceOfJob(jobId),
                CreationDate = () => GetCreationDateOfJob(jobId),
                Payload = () => GetPayloadOfJob(jobId),
                GetCurrentTopList = () => GetToplistOfJob(jobId)
            };
        }

        private List<byte[]> GetToplistOfJob(BigInteger jobId)
        {
            if (!voluntLib.IsStarted) return new List<byte[]>();

            var job = voluntLib.GetJobByID(jobId);
            if (job == null) return new List<byte[]>();

            var stateOfJob = voluntLib.GetStateOfJob(jobId);
            if (stateOfJob == null) return  new List<byte[]>();

            return new List<byte[]>(stateOfJob.ResultList);
        }

        public BigInteger GetCalculatedBlocksOfJob(BigInteger jobID)
        {
            if (!voluntLib.IsStarted)
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

        public WorkspaceModel GetWorkspaceOfJob(BigInteger jobId)
        {
            var payloadOfJob = GetPayloadOfJob(jobId);
            if (payloadOfJob == null)
            {
                return null;
            }

            var workspaceOfJob = payloadOfJob.WorkspaceModel;

            var cloudComponents = workspaceOfJob.GetAllPluginModels()
                .Where(pluginModel => pluginModel.Plugin is ACloudCompatible);
            foreach (var cloudComponent in cloudComponents)
            {
                ((ACloudCompatible)cloudComponent.Plugin).JobID = jobId;
                ((ACloudCompatible)cloudComponent.Plugin).ComputeWorkspaceHash = workspaceOfJob.ComputeWorkspaceHash;
                ((ACloudCompatible)cloudComponent.Plugin).ValidWorkspaceHash = workspaceOfJob.ComputeWorkspaceHash();
            }

            return workspaceOfJob;
        }

        public DateTime GetCreationDateOfJob(BigInteger jobId)
        {
            var payloadOfJob = GetPayloadOfJob(jobId);
            if (payloadOfJob == null)
            {
                return new DateTime(0);
            }

            return payloadOfJob.CreationTime;
        }


        public JobPayload GetPayloadOfJob(BigInteger jobId)
        {
            var job = voluntLib.GetJobByID(jobId);
            if (job == null || !job.HasPayload() || job.IsDeleted)
            {
                return null;
            }

            if (!jobPayloadCache.ContainsKey(jobId))
            {
                try
                {
                    var jobPayload = new JobPayload().Deserialize(job.JobPayload);
                    jobPayloadCache.Add(jobId, jobPayload);
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            return jobPayloadCache[jobId];
        }
        public Bitmap GetJobStateVisualization(BigInteger jobId)
        {
            return voluntLib.GetVisualizationOfJobState(jobId);
        }

        public bool UserCanDeleteJob(Job job)
        {
            return voluntLib.CanUserDeleteJob(job);
        }

        #endregion

        public bool IsPartizipationOnJob()
        {
            return voluntLib.GetCurrentRunningWorkersPerJob().Count > 0;
        }
  
        public bool IsBannedCertificate(X509Certificate2 certificate)
        {
            return CertificateService.GetCertificateService().IsBannedCertificate(certificate);
        }

        public void RefreshJobList()
        {
            voluntLib.RefreshJobList();
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

        public void DeleteJob(BigInteger jobId)
        {
            if (UserCanDeleteJob(voluntLib.GetJobByID(jobId)))
            {
                voluntLib.DeleteJob(jobId);
            }
        }

        public bool CreateJob(string jobType, string jobName, string jobDescription, WorkspaceModel workspaceModel)
        {
            var cloudComponent = GetCloudComponent(workspaceModel);
            if (cloudComponent == null) return false;

            var numberOfBlocks = cloudComponent.NumberOfBlocks;
            var jobPayload = new JobPayload
            {
                WorkspaceModel = workspaceModel,
                CreationTime = DateTime.Now
            };

            var serialize = jobPayload.Serialize();

            var jobID = voluntLib.CreateJob(DefaultWorld, jobType, jobName, jobDescription, serialize,
                numberOfBlocks);
            return jobID != -1;
        }

        private static ACloudCompatible GetCloudComponent(WorkspaceModel workspaceModel)
        {
            try
            {
                var cloudModel =
                    workspaceModel.GetAllPluginModels().First(pluginModel => pluginModel.Plugin is ACloudCompatible);
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
            var handler = ConnectionStateChanged;
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

        #endregion

        public BigInteger GetEpochOfJob(Job job)
        {
            var stateOfJob = voluntLib.GetStateOfJob(job.JobID);
            return (stateOfJob != null) ? stateOfJob.EpochNumber : 0;
        }


        public string  GetUsername()
        {
            return voluntLib.CertificateName;
        }
    }

    public class NetworkJobData
    {
        public Job Job { get; set; }
        public Func<BigInteger> CalculatedBlocks { get; set; }
        public Func<bool> HasWorkspace { get; set; }
        public Func<WorkspaceModel> Workspace { get; set; }
        public Func<DateTime> CreationDate { get; set; }
        public Func<JobPayload> Payload { get; set; }
        public Func<List<byte[]>> GetCurrentTopList { get; set; }
    }
}

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VoluntLib2;
using VoluntLib2.Tools;
using VoluntLib2.ConnectionLayer;
using VoluntLib2.ComputationLayer;
using VoluntLib2.ManagementLayer;
using System.Collections.ObjectModel;

namespace VoluntLib2
{
    public class VoluntLib
    {
        private CertificateService CertificateService = CertificateService.GetCertificateService();
        private ConnectionManager ConnectionManager;
        private JobManager JobManager;
        private Logger Logger = Logger.GetLogger();

        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<JobProgressEventArgs> JobFinished;
        public event PropertyChangedEventHandler JobListChanged;        
        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<TaskEventArgs> TaskStopped;
        public event EventHandler<ConnectionsNumberChangedEventArgs> ConnectionsNumberChanged;

        public VoluntLib()
        {            
        }    
        public string LocalStoragePath { get; set; }      
        public string CertificateName { get; internal set; }
        public bool IsStarted { get; internal set; }

        public List<string> WellKnownPeers = new List<string>();

        public void Stop()
        {
            //we have to first stop the connection manager; otherwise the JobManager remains 
            //blocked in its receiving thread and can not be stopped
            ConnectionManager.Stop();
            JobManager.Stop();                        
            IsStarted = false;
        }

        public void Start(X509Certificate2 caCertificate, X509Certificate2 ownCertificate, ushort listenport = 10000)
        {
            //1) Init Certificate ervice
            CertificateService.Init(caCertificate, ownCertificate);
            CertificateName = CertificateService.OwnName;

            //2) Create, initialize, and start ConnectionManager
            ConnectionManager = new ConnectionManager(this, listenport);
            foreach (string wellknownpeer in WellKnownPeers)
            {
                try
                {
                    string[] ipport = wellknownpeer.Split(new char[] { ':' });
                    if (ipport.Length != 2)
                    {
                        Logger.LogText(String.Format("List of WellKnownPeers contained invalid entry (wrong parameter count): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        continue;
                    }
                    IPAddress ip;
                    ushort port;
                    try
                    {
                        ip = IPAddress.Parse(ipport[0]);
                    }
                    catch (Exception)
                    {
                        Logger.LogText(String.Format("List of WellKnownPeers contained invalid entry (invalid ip): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        continue;
                    }
                    try
                    {
                        port = ushort.Parse(ipport[1]);
                    }
                    catch (Exception)
                    {
                        Logger.LogText(String.Format("List of WellKnownPeers contained invalid entry (invalid port): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        continue;
                    }
                    ConnectionManager.AddWellknownPeer(ip, port);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("List of WellKnownPeers contained invalid entry (Exception: {0}): {0}. Ignore it", ex.Message, wellknownpeer), this, Logtype.Warning);
                    continue;
                }
            }
            ConnectionManager.ConnectionsNumberChanged += ConnectionsNumberChanged;
            ConnectionManager.Start();

            //3) Create, initialize, and start JobManager
            JobManager = new JobManager(this, ConnectionManager, LocalStoragePath);
            JobManager.JobListChanged += JobListChanged;
            JobManager.Start();

            IsStarted = true;
        }

        public void StopCalculation(BigInteger jobID)
        {

        }

        public void RefreshJobList()
        {
            JobManager.RefreshJobList();
        }

        public void RequestJobDetails(Job job)
        {
            JobManager.RequestJob(job.JobID);
        }

        public bool JoinJob(BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {
            return false;
        }

        public void DeleteJob(BigInteger jobID)
        {
            JobManager.DeleteJob(jobID);
        }

        public BigInteger CreateJob(string worldName, string jobType, string jobName, string jobDescription, byte[] payload, BigInteger numberOfBlocks)
        {
            return JobManager.CreateJob(worldName, jobType, jobName, jobDescription, payload, numberOfBlocks);
        }

        public Job GetJobByID(BigInteger jobID)
        {
            return JobManager.GetJobById(jobID);
        }

        public List<string> GetWorldNames()
        {
            return new List<string>();
        }

        public List<Job> GetJobsOfWorld(string world)
        {
            return JobManager.GetJobsOfWorld(world);
        }

        public BigInteger GetCalculatedBlocksOfJob(BigInteger jobID)
        {
            return BigInteger.Zero;
        }

        public Dictionary<BigInteger, int> GetCurrentRunningWorkersPerJob()
        {
            return new Dictionary<BigInteger,int>();
        }

        public EpochState GetStateOfJob(BigInteger jobId)
        {
            return new EpochState();
        }

        public Bitmap GetVisualizationOfJobState(BigInteger jobId)
        {
            return new Bitmap(255, 255);
        }

        public bool CanUserDeleteJob(Job job)
        {
            return (job.CreatorName.Equals(CertificateName)) || 
                CertificateService.GetCertificateService().IsAdminCertificate(CertificateService.GetCertificateService().OwnCertificate);
        }

        public ObservableCollection<Job> GetJoblist()
        {
            return JobManager.GetJoblist();
        }
    }
}
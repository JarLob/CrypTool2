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

namespace VoluntLib2
{
    public class VoluntLib
    {
        private CertificateService CertificateService = CertificateService.GetCertificateService();
        private ConnectionManager ConnectionManager;
        private Logger Logger = Logger.GetLogger();

        private ushort ListenPort = 0;        

        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<JobProgressEventArgs> JobFinished;
        public event PropertyChangedEventHandler JobListChanged;
        public event PropertyChangedEventHandler WorldsChanged;
        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<TaskEventArgs> TaskStopped;

        public VoluntLib()
        {
            Logger.SetLogLevel(Logtype.Info);
        }
    
        public string LocalStoragePath { get; set; }
        public List<string> AdminCertificateList { get; set; }
        public List<string> BannedCertificateList { get; set; }
        public string CertificateName { get; set; }
        public bool IsStarted { get; set; }

        public void Stop()
        {
            ConnectionManager.Stop();
            IsStarted = false;
        }

        public void Start(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            CertificateService.Init(caCertificate, ownCertificate);
            CertificateName = CertificateService.OwnName;

            ConnectionManager = new ConnectionManager(ListenPort);
            //Well known peer for testing - my amazon server; will be replaced by official CT2 servers
            ConnectionManager.AddWellknownPeer(IPAddress.Parse("34.218.183.126"), 10000);
            ConnectionManager.Start();
            
            IsStarted = true;            
        }               

        public void StopCalculation(BigInteger jobID)
        {

        }

        public void RefreshJobList(string world)
        {
            JobListChanged.Invoke(this, new PropertyChangedEventArgs("JobList"));
        }

        public void RequestJobDetails(Job job)
        {

        }

        public bool JoinNetworkJob(BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {
            return false;
        }

        public void DeleteNetworkJob(BigInteger jobID)
        {

        }

        public BigInteger CreateNetworkJob(string worldName, string jobType, string jobName, string jobDescription, byte[] payload, BigInteger numberOfBlocks)
        {
            return BigInteger.Zero;
        }

        public Job GetJobByID(BigInteger jobID)
        {
            return new Job(jobID);
        }

        public List<string> GetWorldNames()
        {
            return new List<string>();
        }

        public List<Job> GetJobsOfWorld(string world)
        {
            var list = new List<Job>();
            for (int i = 0; i < 10; i++)
            {
                Job job = new Job(i);
                job.JobName = "Fubar" + i;
                job.JobType = "Bla";
                job.JobDescription = "Blubb";
                //job.StateConfig = new EpochStateConfig();
                //job.StateConfig.BitMaskWidth = 1000;
                //job.StateConfig.NumberOfBlocks = 10000;
                list.Add(job);
            }
            return list;
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
            return false;
        }
    }
}
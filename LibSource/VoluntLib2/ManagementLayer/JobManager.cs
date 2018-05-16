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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VoluntLib2.ConnectionLayer;
using VoluntLib2.ManagementLayer.Messages;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer
{
    internal class JobManager
    {
        private Logger Logger = Logger.GetLogger();

        private const int MAX_TERMINATION_WAIT_TIME = 5000; //5 s
        private const int WORKER_THREAD_SLEEPTIME = 1; // ms
        private const int MAX_JOB_PAYLOAD_SIZE = 50 * 1024; // 50kb

        private bool Running = false;
        private Thread ReceivingThread;
        private Thread WorkerThread;
        
        //Path, where jobs will be serialized to and loaded from
        internal string LocalStoragePath;

        //ConnectionManager is responsible for the core network communication and p2p overlay
        //The JobManager uses it to send and receive messages
        private ConnectionManager ConnectionManager;

        //a queue containing all operations
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();        

        //a dictionary containing all the jobs
        internal ConcurrentDictionary<BigInteger, Job> Jobs = new ConcurrentDictionary<BigInteger, Job>();

        //a list that can be observerd from the outside, i.e. the UI
        internal ObservableCollection<Job> JobList;        

        internal VoluntLib VoluntLib { get; set; }

        public JobManager(VoluntLib voluntLib, ConnectionManager connectionManager, string localStoragePath)
        {
            VoluntLib = voluntLib;
            ConnectionManager = connectionManager;
            LocalStoragePath = localStoragePath;
            connectionManager.ConnectionsNumberChanged += connectionManager_ConnectionsNumberChanged;

            //if we are in a WPF application, we create the ObservableCollection in UI thread
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    JobList = new ObservableItemsCollection<Job>();
                }));
            }
            else
            {
                JobList = new ObservableItemsCollection<Job>();
            }
        }

        /// <summary>
        /// When the connections number changes, we ask everyone for a joblist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectionManager_ConnectionsNumberChanged(object sender, ConnectionsNumberChangedEventArgs e)
        {
            foreach (Operation operation in Operations)
            {
                if (operation is RequestJobListOperation)
                {
                    RequestJobListOperation requestJobListOperation = (RequestJobListOperation)operation;
                    requestJobListOperation.ForceExecution();
                }
            }
        }

        public void Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("The JobManager is already running!");
            }
            Logger.LogText("Starting the JobManager", this, Logtype.Info);
            //Set Running to true; thus, threads know we are alive
            Running = true;
            //Create a thread for receving data
            ReceivingThread = new Thread(HandleIncomingMessages);
            ReceivingThread.IsBackground = true;
            ReceivingThread.Start();
            //Create a thread for the operations
            WorkerThread = new Thread(JobManagerWork);
            WorkerThread.IsBackground = true;
            WorkerThread.Start();
            //This operation deserializes all serialized jobs; then it terminates
            Operations.Enqueue(new JobsDeserializationOperation() { JobManager = this });
            //This operation sends every 5 minutes a ResponseJobListMessage to every neighbor
            Operations.Enqueue(new ShareJobListOperation() { JobManager = this });
            //This operation sends every 5 minutes a RequestJobListMessage to every neighbor
            Operations.Enqueue(new RequestJobListOperation() { JobManager = this });
            //This operation answers to RequestJobListMessages
            Operations.Enqueue(new ResponseJobListOperation() { JobManager = this });
            //This operation handles to JobListResponseMessages
            Operations.Enqueue(new HandleResponseJobListMessageOperation() { JobManager = this });
            //This operation checks JobPayloads of jobs; it requests these from the neighbors
            Operations.Enqueue(new CheckJobsPayloadOperation() { JobManager = this });
            //This operation answers RequestJobMessage by sending an answer containing the requestet job. Only when we HAVE it and it HAS PAYLOAD
            Operations.Enqueue(new HandleRequestJobMessage() { JobManager = this });
            //This operation handles ResponseJobMessages
            Operations.Enqueue(new HandleResponseJobMessageOperation() { JobManager = this });
            //This operation serializes the jobs every 5 minutes to file
            Operations.Enqueue(new JobsSerializationOperation() { JobManager = this });
            

            Logger.LogText("JobManager started", this, Logtype.Info);
        }

        private void HandleIncomingMessages()
        {
            Logger.LogText("ReceivingThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    //ReceiveData blocks until a DataMessage was received by the connection manager
                    //data could also be null; happens, when the ConnectionManager stops
                    Data data = ConnectionManager.ReceiveData();                    
                    if (data == null)
                    {
                        continue;
                    }                    
                    Logger.LogText(String.Format("Data from {0} : {1} bytes", BitConverter.ToString(data.PeerId), data.Payload.Length), this, Logtype.Debug);

                    Message message = null;
                    try
                    {
                        message = MessageHelper.Deserialize(data.Payload);
                        message.PeerId = data.PeerId; //memorize peer id for later usage
                        Logger.LogText(String.Format("Received a {0} from {1}.", message.MessageHeader.MessageType.ToString(), BitConverter.ToString(data.PeerId)), this, Logtype.Debug);
                       
                    }
                    catch (VoluntLibSerializationException vl2mdex)
                    {
                        Logger.LogText(String.Format("Message could not be deserialized: {0}", vl2mdex.Message), this, Logtype.Warning);
                        Logger.LogException(vl2mdex, this, Logtype.Warning);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(String.Format("Exception during deserialization: {0}", ex.Message), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                        continue;
                    }

                    //check signature
                    try
                    {
                        var certificateValidationState = CertificateService.GetCertificateService().VerifySignature(message);
                        if (!certificateValidationState.Equals(CertificateValidationState.Valid))
                        {
                            //we dont accept invalid signatures; thus, we do not handle the message and discard it here
                            Logger.LogText(String.Format("Received a message from {0} and the signature check was: {1}", message.PeerId, certificateValidationState), this, Logtype.Warning);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(String.Format("Exception during check of signature: {0}", ex.Message), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                        continue;
                    }

                    try
                    {
                        Thread handleMessageThread = new Thread(() =>
                        {
                            try
                            {
                                HandleMessage(message);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogText(String.Format("Exception during message handling: {0}", ex.Message), this, Logtype.Error);
                                Logger.LogException(ex, this, Logtype.Error);
                            }
                        }
                        );
                        handleMessageThread.IsBackground = true;
                        handleMessageThread.Start();

                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(String.Format("Exception creating a message handling thread: {0}", ex.Message), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                        continue;
                    }

                }              
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Uncaught exception in HandleIncomingMessages(). Terminate now! {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                    Running = false;
                }
            }
            Logger.LogText("ReceivingThread terminated", this, Logtype.Info);
        }

        private void HandleMessage(Message message)
        {
            foreach (Operation operation in Operations)
            {
                try
                {
                    operation.HandleMessage(message);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during execution of HandleMessage of operation: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        private void JobManagerWork()
        {
            Logger.LogText("WorkerThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    Operation operation;
                    if (Operations.TryDequeue(out operation) == true)
                    {
                        // before we execute an operation, we check if it is finished
                        if (operation.IsFinished == false)
                        {
                            //operations that are not finished are enqueued again
                            Operations.Enqueue(operation);
                        }
                        else
                        {
                            Logger.LogText(String.Format("Operation {0}-{1} has finished. Removed it.", operation.GetType().FullName, operation.GetHashCode()), this, Logtype.Debug);
                            //we dont execute this operation since it is finished
                            continue;
                        }

                        try
                        {
                            operation.Execute();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogText(String.Format("Exception during execution of operation {0}-{1}: {2}", operation.GetType().FullName, operation.GetHashCode(), ex.Message), this, Logtype.Error);
                            Logger.LogException(ex, this, Logtype.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during handling of operation: {2}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
                try
                {
                    Thread.Sleep(WORKER_THREAD_SLEEPTIME);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during sleep of thread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            Logger.LogText("WorkerThread terminated", this, Logtype.Info);
        }

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Logger.LogText("Stop method was called...", this, Logtype.Info);
            Running = false;
            DateTime start = DateTime.Now;
            while ((ReceivingThread.IsAlive || WorkerThread.IsAlive) && DateTime.Now < start.AddMilliseconds(MAX_TERMINATION_WAIT_TIME))
            {
                Thread.Sleep(100);
            }
            if (ReceivingThread.IsAlive)
            {
                Logger.LogText("ReceivingThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    ReceivingThread.Abort();
                    Logger.LogText("Aborted ReceivingThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during abortion of ReceivingThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            if (WorkerThread.IsAlive)
            {
                Logger.LogText("WorkerThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    WorkerThread.Abort();
                    Logger.LogText("Aborted WorkerThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Exception during abortion of WorkerThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }            
            Logger.LogText("Terminated", this, Logtype.Info);
        }

        internal BigInteger CreateJob(string worldName, string jobType, string jobName, string jobDescription, byte[] payload, BigInteger numberOfBlocks)
        {
            if (payload.Length > MAX_JOB_PAYLOAD_SIZE)
            {
                throw new JobPayloadTooBigException(String.Format("Job size too big. Maximum size is {0}, given size was {1}.", MAX_JOB_PAYLOAD_SIZE, payload.Length));
            }
            //copy the payload
            byte[] payloadCopy = new byte[payload.Length];
            if (payload.Length > 0)
            {
                Array.Copy(payload, 0, payloadCopy, 0, payloadCopy.Length);
            }
            //generate positive job id
            BigInteger jobId = new BigInteger(Guid.NewGuid().ToByteArray());
            if (jobId < 0)
            {
                jobId = jobId * -1;
            }
            //create job object
            Job job = new Job(jobId);
            job.WorldName = worldName;
            job.JobType = jobType;
            job.JobName = jobName;
            job.JobDescription = jobDescription;            
            job.NumberOfBlocks = numberOfBlocks;
            job.CreatorName = CertificateService.GetCertificateService().OwnName;
            job.CreationDate = DateTime.UtcNow;
            job.CreatorCertificateData = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
            //create job payload hash
            job.JobPayloadHash = CertificateService.GetCertificateService().ComputeHash(payloadCopy);
            //create job signature (without payload)
            job.JobCreatorSignatureData = GenerateCreatorSignatureData(job);
            //finally, add payload
            job.JobPayload = payloadCopy;

            if (Jobs.TryAdd(jobId, job))
            {
                foreach (Operation operation in Operations)
                {
                    if (operation is JobsSerializationOperation)
                    {
                        //we created a new job, thus, we force to serialize that immediately
                        ((JobsSerializationOperation)operation).ForceSerialization();
                    }
                }
                //Send the job to everyone else                
                SendResponseJobMessage(null, job);
                OnJobListChanged();
                return jobId;
            }
            else
            {
                return BigInteger.MinusOne;
            }            
        }

        /// <summary>
        /// Creates a creator signature of the given job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private byte[] GenerateCreatorSignatureData(Job job)
        {
            byte[] data = job.Serialize();
            return CertificateService.GetCertificateService().SignData(data);
        }    

        /// <summary>
        /// Calls UpdateObservableCollection either from ui or current thread
        /// </summary>
        internal void OnJobListChanged()
        {          
            //If we are in a WPF application, we update the observable collection in UI thread
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateObservableCollection();
                }));
            }
            else
            {
                UpdateObservableCollection();
            }
        }

        /// <summary>
        /// Update observable collection
        /// </summary>
        private void UpdateObservableCollection()
        {
            foreach (Job job in Jobs.Values)
            {
                if (!JobList.Contains(job) && !job.IsDeleted)
                {
                    bool added = false;
                    for (int i = 0; i < JobList.Count; i++)
                    {
                        if (JobList[i].CreationDate <= job.CreationDate)
                        {
                            JobList.Insert(i, job);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        JobList.Insert(JobList.Count, job);
                    }
                }
            }
            List<Job> removeList = new List<Job>();
            foreach (Job job in JobList)
            {
                if (job.IsDeleted)
                {
                    removeList.Add(job);
                }
            }
            foreach (Job job in removeList)
            {
                JobList.Remove(job);
            }
        }
        
        public ObservableCollection<Job> GetJoblist()
        {
            return JobList;
        }

        /// <summary>
        /// Returns the job with the given jobID or null if it does not exist
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        internal Job GetJobById(BigInteger jobID)
        {
            if (Jobs.ContainsKey(jobID))
            {
                return Jobs[jobID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a RequstJobListMessage to peer with peerID
        /// if peerID == null it sends the message to every neighbor
        /// </summary>
        /// <param name="peerID"></param>
        internal void SendRequestJobListMessage(byte[] peerID)
        {
            RequestJobListMessage requestJobList = new RequestJobListMessage();
            requestJobList.MessageHeader.CertificateData = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
            requestJobList.MessageHeader.SenderName = CertificateService.GetCertificateService().OwnName;
            requestJobList.MessageHeader.WorldName = String.Empty;
            byte[] data = requestJobList.Serialize(true);
            ConnectionManager.SendData(data, peerID);
        }

        /// <summary>
        /// Sends several ResponseJobListMessages to peer with peerID, each message containing 5 jobs
        /// if peerID == null it sends the message to every neighbor
        /// </summary>
        /// <param name="peerID"></param>
        internal void SendResponseJobListMessages(byte[] peerID)
        {            
            //we clone the JobList without Payloads to minimize the size of the message
            List<Job> clonedList = new List<Job>();
            foreach (Job job in Jobs.Values)
            {
                byte[] jobdata = job.Serialize();
                Job clone = new Job(BigInteger.Zero);
                clone.Deserialize(jobdata);
                clone.JobPayload = new byte[0]; //Remove payload
                clonedList.Add(clone);
            }

            //compute, how many messages we have to send; each message contains a maximum of 5 jobs without its payload
            int messageCount = clonedList.Count / 5;
            if (clonedList.Count % 5 > 0)
            {
                messageCount++;
            }

            //create and send messages
            for (int i = 0; i < messageCount; i++)
            {                
                ResponseJobListMessage responseJobList = new ResponseJobListMessage();
                if (clonedList.Count > 5)
                {
                    responseJobList.Jobs = clonedList.Take(5).ToList();
                    clonedList.RemoveRange(0, 5);
                }
                else
                {
                    responseJobList.Jobs = clonedList;
                }                                
                responseJobList.MessageHeader.CertificateData = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
                responseJobList.MessageHeader.SenderName = CertificateService.GetCertificateService().OwnName;
                responseJobList.MessageHeader.WorldName = String.Empty;
                byte[] data = responseJobList.Serialize(true);
                ConnectionManager.SendData(data, peerID);
            }
        }

        /// <summary>
        /// Sends a RequestJobMessage to peer with peerID requesting a job with the given JobId
        /// if peerID == null it sends the message to every neighbor
        /// </summary>
        /// <param name="peerID"></param>
        internal void SendRequestJobMessage(byte[] peerID, BigInteger jobId)
        {
            RequestJobMessage requestJobMessage = new RequestJobMessage();
            requestJobMessage.MessageHeader.CertificateData = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
            requestJobMessage.MessageHeader.SenderName = CertificateService.GetCertificateService().OwnName;
            requestJobMessage.MessageHeader.WorldName = String.Empty;
            requestJobMessage.JobId = jobId;
            byte[] data = requestJobMessage.Serialize(true);
            ConnectionManager.SendData(data, peerID);
        }

        /// <summary>
        /// Sends a ResponseJobMessage to peer with peerID containing the job with the given JobId
        /// if peerID == null it sends the message to every neighbor
        /// </summary>
        /// <param name="peerID"></param>
        internal void SendResponseJobMessage(byte[] peerID, Job job)
        {
            ResponseJobMessage responseJobMessage = new ResponseJobMessage();
            responseJobMessage.MessageHeader.CertificateData = CertificateService.GetCertificateService().OwnCertificate.GetRawCertData();
            responseJobMessage.MessageHeader.SenderName = CertificateService.GetCertificateService().OwnName;
            responseJobMessage.MessageHeader.WorldName = String.Empty;
            responseJobMessage.Job = job;
            byte[] data = responseJobMessage.Serialize(true);
            ConnectionManager.SendData(data, peerID);
        }

        /// <summary>
        /// Sets the last execution time of all RequestJobListOperations to the minimum value forcing them to be executed
        /// </summary>
        internal void RefreshJobList()
        {
            foreach (Operation operation in Operations)
            {
                if (operation is RequestJobListOperation)
                {
                    RequestJobListOperation requestJobListOperation = (RequestJobListOperation)operation;
                    requestJobListOperation.ForceExecution();
                }
            }
        }

        /// <summary>
        /// Request the given job from any of the other peers
        /// </summary>
        /// <param name="jobId"></param>
        internal void RequestJob(BigInteger jobId)
        {
            SendRequestJobMessage(null, jobId);
        }

        /// <summary>
        /// Deletes the job, only if the job exists and the user is admin or the job was created by the user
        /// </summary>
        /// <param name="jobID"></param>
        internal void DeleteJob(BigInteger jobId)
        {
            if (Jobs[jobId] == null)
            {
                //Job does not exist
                return;
            }                       
            Job job = Jobs[jobId];
            // GenerateCreatorDeletionSignature returns false if job was not created by user and user is not admin
            if (job.GenerateDeletionSignature()) 
            {
                foreach (Operation operation in Operations)
                {
                    if (operation is JobsSerializationOperation)
                    {
                        //we deleted the job, thus, we force to serialize that immediately
                        ((JobsSerializationOperation)operation).ForceSerialization();
                    }
                }
                //Send the job to everyone; telling them that it is deleted
                SendResponseJobMessage(null, job);
                OnJobListChanged();
            }            
        }
    }    
}

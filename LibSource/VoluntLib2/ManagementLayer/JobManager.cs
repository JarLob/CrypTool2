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
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private bool Running = false;
        private Thread ReceivingThread;
        private Thread WorkerThread;

        //ConnectionManager is responsible for the core network communication and p2p overlay
        //The JobManager uses it to send and receive messages
        private ConnectionManager ConnectionManager;

        //a queue containing all operations
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();        

        //a dictionary containing all the jobs
        internal ConcurrentDictionary<BigInteger, Job> Jobs = new ConcurrentDictionary<BigInteger, Job>();

        public JobManager(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
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

            //This operation is responsible for answering HelloMessages:
            Operations.Enqueue(new TestOperation() { JobManager = this });

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
                    catch (VoluntLib2MessageDeserializationException vl2mdex)
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
    }
}

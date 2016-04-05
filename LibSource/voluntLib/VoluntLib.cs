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
using NLog;
using NLog.Config;
using voluntLib.common;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;
using voluntLib.communicationLayer;
using voluntLib.communicationLayer.communicator;
using voluntLib.communicationLayer.communicator.networkBridgeCommunicator;
using voluntLib.logging;
using voluntLib.managementLayer;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;
using voluntLib.managementLayer.localStateManagement.states.config;
using voluntLib.utilities;
using voluntLib.communicationLayer.protrocolExtensions;

#endregion

namespace voluntLib
{
    /// <summary>
    ///   VoluntLib Facade
    /// </summary>
    public class VoluntLib
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private const int DefaultWorkingListTimeout = 600000;
        private const int DefaultMaximumBackoffTime = 1000;
        private const int DefaultPort = 13337;
        private const string DefaultMulticastGroup = "224.0.7.1";
        private readonly string defaultLocalStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),  "VoluntLib" + Path.DirectorySeparatorChar + "VoluntLibStore.xml");

        #region Configuration Properties

        /// <summary>
        ///   Ip of the localEndPoint, that will be used for receiving packages.
        ///   Default: All localEndpoints (Ipv6 if Multicast Address is IpV6)
        /// </summary>
        public IPAddress LocalEndPointIPAddress { get; set; }

        /// <summary>
        ///   Encoding used for description.
        ///   Default: Encoding.UTF8
        /// </summary>
        /// <value>
        ///   The default encoding.
        /// </value>
        public Encoding DefaultEncoding { get; set; }

        /// <summary>
        ///   State configuration template for new network jobs.
        ///   Default: Epochstate with bit mask width of 5000.
        /// </summary>
        /// <value>
        ///   The default state configuration.
        /// </value>
        public EpochStateConfig DefaultStateConfig { get; set; }

        /// <summary>
        ///   Specifies the time after which a peer will be removed from the WorkingPeers list.
        ///   Default: 10 minutes
        /// </summary>
        /// <value>
        ///   The working list timeout.
        /// </value>
        public int WorkingListTimeout { get; set; }

        /// <summary>
        ///   For many response-messages, VoluntLib waits a random time between 0 and MaximumBackoffTime
        ///   and sends the response iff none response had occurred.
        ///   Default: 1 second
        /// </summary>
        /// <value>
        ///   The maximum backoff time.
        /// </value>
        public int MaximumBackoffTime { get; set; }

        #region Network Configuration

        /// <summary>
        ///   Multicast Group, that will be used for message exchanging.
        ///   Default: 224.0.7.1
        /// </summary>
        /// <value>
        ///   The multicast group.
        /// </value>
        public string MulticastGroup { get; set; }

        /// <summary>
        ///   Receiving Port
        ///   Default: 13337
        /// </summary>
        /// <value>
        ///   The port.
        /// </value>
        public int Port { get; set; }

        #endregion Network Configuration

        #region Persistence Configuration

        /// <summary>
        ///   Gets or sets a value indicating whether the persistence should be enabled within
        ///   the <see cref="Init(X509Certificate2,X509Certificate2)" />
        ///   method.
        ///   If enabled, the lib will store each NetworkJob, its localState and its resultList
        ///   in a local File determine by <see cref="LocalStoragePath" />.
        /// </summary>
        /// <value>
        ///   <c>true</c> if persistence should be enabled otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="IsPersistenceEnabled" />
        /// <seealso cref="LoadDataFromLocalStorage" />
        public bool EnablePersistence { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether NetworkJobs, their localState and their
        ///   resultList should be loaded from a local File (determine by <see cref="LocalStoragePath" />)
        ///   within the <see cref="Init(X509Certificate2,X509Certificate2)" /> method.
        /// </summary>
        /// <value>
        ///   <c>true</c> if NetworkJob, its localState and its resultList should be loaded otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="EnablePersistence" />
        public bool LoadDataFromLocalStorage { get; set; }

        /// <summary>
        ///   Gets or sets the local storage path.
        ///   Default: "voluntLibStore.xml"
        ///   <para> <see cref="EnablePersistence" /> will enable continues persisting. </para>
        ///   <para> <see cref="LoadDataFromLocalStorage" /> will load the data on start up. </para>
        /// </summary>
        /// <value>
        ///   The local storage path.
        /// </value>
        /// <seealso cref="IsPersistenceEnabled" />
        public string LocalStoragePath { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether the local storage should be cleaned on startup
        /// </summary>
        /// <seealso cref="EnablePersistence" />
        /// <seealso cref="LoadDataFromLocalStorage" />
        public bool ClearLocalStorageOnStartUp { get; set; }

        #endregion Persistence Configuration

        /// <summary>
        ///   Sets the mode for logging
        ///   LogMode.NLogConfig: uses the nlog config file to determin the log targets
        ///   LogMode.EventBased: invokes the ApplicationLog-event 
        ///   Default: LogMode.NLogConfig;
        /// </summary> 
        public LogMode LogMode { get; set; }

        /// <summary>
        /// A list containing all certificate ids or usernames that are considerd to have admin privileges
        /// An entry for the certificate with the subjectname hans has be N:hans
        /// An entry for the certificate with the serialnumeber 1234 has be SN:1234
        /// </summary>
        public List<string> AdminCertificateList
        {
            get { return adminCertificateList; }
            set
            {
                adminCertificateList = value;
                if (CertificateService != null)
                {
                    CertificateService.AdminCertificateList = adminCertificateList;
                }
            }
        }
        
        /// <summary>
        /// A list containing all certificate ids or usernames that are banned within the current network
        /// An entry for the certificate with the subjectname hans has be N:hans
        /// An entry for the certificate with the serialnumeber 1234 has be SN:1234
        /// </summary>
        public List<string> BannedCertificateList
        {
            get { return bannedCertificateList; }
            set
            {
                bannedCertificateList = value;
                if (CertificateService != null)
                {
                    CertificateService.BannedCertificateList = bannedCertificateList;
                }
            }
        }

        #endregion

        #region State Properties

        /// <summary>
        ///   Indicates whether, the lib has been started
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is started]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsStarted { get; private set; }

        /// <summary>
        ///   Indicates whether, the lib has been Initialized
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is initialized]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsInitialized { get; private set; }

        /// <summary>
        ///   Indicating whether persistence is enabled.
        ///   If enabled, the lib will store each NetworkJob, its localState and its resultList in a local File.
        /// </summary>
        /// <value>
        ///   <c>true</c> if persistence is enabled otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsPersistenceEnabled { get; private set; }

        public virtual bool IsNATFreeNetworkBridge { get; private set; }
        public virtual bool IsNetworkBridge { get; private set; }

        #endregion

        public string CertificateName { get; private set; }

        #region internal Members

        private readonly Random randomGen = new Random();
        private readonly List<SendingTCPCommunicator> tcpCommunicatorToAdd = new List<SendingTCPCommunicator>();
        private ReceivingTCPCommunicator receivingTCPCom;

        private List<AExtension> Extension = new List<AExtension>();
        protected CommunicationLayer CommunicationLayer { get; private set; }
        protected CommunicationLayer FileComLayer { get; private set; }

        protected ManagementLayer ManagementLayer { get; private set; }
        protected NetworkBridgeCommunicationLayer NetworkBridgeCommunicationLayer { get; private set; }
        protected NetworkBridgeManagementLayer NetworkBridgeManagementLayer { get; private set; }

        protected CertificateService CertificateService;

        private List<string> adminCertificateList = new List<string>();
        private List<string> bannedCertificateList = new List<string>();

        #endregion internal Members

        /// <summary>
        ///   Creates a new instance of the <see cref="VoluntLib" /> class.
        /// </summary>
        public VoluntLib()
        {
            DefaultEncoding = Encoding.UTF8;
            DefaultStateConfig = new EpochStateConfig {BitMaskWidth = 5000};
            WorkingListTimeout = DefaultWorkingListTimeout;
            MaximumBackoffTime = DefaultMaximumBackoffTime;
            Port = DefaultPort;
            MulticastGroup = DefaultMulticastGroup;
            LocalStoragePath = defaultLocalStoragePath;
            LocalEndPointIPAddress = null;
            IsStarted = false;
            IsInitialized = false;
            AdminCertificateList = new List<string>();
            LogMode = LogMode.NLogConfig;
        }

        #region start and stop

        /// <summary>
        ///   Starts this instance.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Thrown, if the object has not been initialized.</exception>
        public virtual void Start()
        {
            ThrowErrorIfNotInitialized();
            CommunicationLayer.Start();

            if (NetworkBridgeCommunicationLayer != null)
                NetworkBridgeCommunicationLayer.Start();

            IsStarted = true;
            logger.Info("VoluntLib has been started as user {0}", CertificateName);
        }

        /// <summary>
        ///   Stops this instance.
        ///   Note, that an stopped instance should not be restarted.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual void Stop()
        {
            ThrowErrorIfNotStarted();
            CommunicationLayer.Stop();

            if (FileComLayer != null) FileComLayer.Stop();

            ManagementLayer.Stop();
            IsStarted = false;

            logger.Info("VoluntLib has been stopped");
        }

        /// <summary>
        ///   Initializes this instance with the current configuration properties.
        ///   After initialization this instance is started.
        /// </summary>
        /// <param name="caCertificate">The CA Certificate.</param>
        /// <param name="ownCertificate">Own Certificate.</param>
        public virtual void InitAndStart(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            Init(caCertificate, ownCertificate);
            Start();
        }

        #endregion

        #region config and init

        public virtual void EnableNATFreeNetworkBridge(int inboundPort, IPAddress localEndPoint = null)
        {
            if (IsInitialized) 
                throw new NotSupportedException("Can only be done if not already been initialized");

            receivingTCPCom = new ReceivingTCPCommunicator(inboundPort, localEndPoint);
        }

        /// <summary>
        ///   Adds a network bridge.
        /// </summary>
        /// <param name="networkBridgeIP">The network bridge ip.</param>
        /// <param name="networkBridgePort">The network bridge port.</param>
        /// <exception cref="System.NotSupportedException">Can only add NetworkBridges if not already been initialized</exception>
        public virtual void AddNetworkBridge(string networkBridgeIP, int networkBridgePort)
        {
            if (IsInitialized) 
                throw new NotSupportedException("Can only add NetworkBridges if not already been initialized");

            tcpCommunicatorToAdd.Add(new SendingTCPCommunicator(networkBridgeIP, networkBridgePort));
        }

        /// <summary>
        ///   Initializes this instance with the current configuration properties
        /// </summary>
        /// <param name="caCertificate">The CA Certificate.</param>
        /// <param name="ownCertificate">Own Certificate.</param>
        public virtual void Init(X509Certificate2 caCertificate, X509Certificate2 ownCertificate)
        {
            Init(caCertificate, ownCertificate, new MulticastCommunicator(Port, MulticastGroup, LocalEndPointIPAddress));
        }

        /// <summary>
        ///   Initializes this instance with the current configuration properties
        /// </summary>
        /// <param name="caCertificate">The CA Certificate.</param>
        /// <param name="ownCertificate">Own Certificate.</param>
        /// <param name="communicator">A communicator.</param>
        public virtual void Init(X509Certificate2 caCertificate, X509Certificate2 ownCertificate, MulticastCommunicator communicator)
        {
            if (ownCertificate.SubjectName.Name != null)
                CertificateName = ownCertificate.SubjectName.Name.Split('=').Last();

            ManagementLayer = new ManagementLayer
            {
                CertificateName = CertificateName,
                MaximumBackoffTime = MaximumBackoffTime
            };

            CertificateService = new CertificateService(caCertificate, ownCertificate)
            {
                AdminCertificateList = AdminCertificateList,
                BannedCertificateList = BannedCertificateList
            };

            CommunicationLayer = new CommunicationLayer(ManagementLayer, CertificateService, communicator);
            Extension.ForEach(ext => CommunicationLayer.Extensions.Add(ext));
 
            //adding outbounding NetworkBridges
            if (tcpCommunicatorToAdd.Capacity > 0) 
                SetupNetworkBridgeCommunicators();

            ManagementLayer.NetworkCommunicationLayer = CommunicationLayer;
            ManagementLayer.WorkingPeers.RemoveAfterMS = WorkingListTimeout;

            //file communicator
            if (LoadDataFromLocalStorage || EnablePersistence || ClearLocalStorageOnStartUp)
                SetupFileCommunicator(CertificateService);

            //adding NATFree NetworkBridge
            if (receivingTCPCom != null)
                SetupNATFreeNetworkBridge(CertificateService);

            if (LogMode == LogMode.EventBased)
                EnableEventBasedLogging();

            RegisterPublicEvents();
            IsInitialized = true;
        }


        public void AddExtension(AExtension extension)
        {           
            Extension.Add(extension);
        }

        private void EnableEventBasedLogging()
        {
            const string targetName = "eventBasedTarget";

            var config = new LoggingConfiguration();
            var eventBasedTarget = new EventBasedTarget();
            var logRule = new LoggingRule("*", LogLevel.Warn, eventBasedTarget);

            config.AddTarget(targetName, eventBasedTarget);
            config.LoggingRules.Add(logRule);

            LogManager.Configuration = config;
            eventBasedTarget.ApplicationLog += OnApplicationLog;
              
        }


        /// <summary>
        /// Quiet mode disables trace, debug and info logs. Only warnings and error logs will be shown.
        /// </summary>
        /// <param name="logLevel"></param>
        public void EnableQuietMode()
        {
            try
            {
                if (LogManager.Configuration != null && LogManager.Configuration.LoggingRules != null)
                {
                    foreach (var rule in LogManager.Configuration.LoggingRules)
                    {
                        rule.DisableLoggingForLevel(LogLevel.Trace);
                        rule.DisableLoggingForLevel(LogLevel.Debug);
                        rule.DisableLoggingForLevel(LogLevel.Info);
                    }
                    LogManager.ReconfigExistingLoggers();
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }

        #region init-Helper
        private void SetupNATFreeNetworkBridge(CertificateService certificateService)
        {
            NetworkBridgeManagementLayer = new NetworkBridgeManagementLayer(ManagementLayer);
            NetworkBridgeCommunicationLayer = new NetworkBridgeCommunicationLayer(NetworkBridgeManagementLayer, certificateService, receivingTCPCom);
            Extension.ForEach(ext => NetworkBridgeCommunicationLayer.Extensions.Add(ext));

            NetworkBridgeManagementLayer.NetworkCommunicationLayer = NetworkBridgeCommunicationLayer;
            IsNATFreeNetworkBridge = true;
            IsNetworkBridge = true;
        }

        private void SetupNetworkBridgeCommunicators()
        {
            IsNetworkBridge = true;
            foreach (var tcpCom in tcpCommunicatorToAdd)
            {
                CommunicationLayer.AddCommunicator(tcpCom.RemoteNetworkBridgeIP, tcpCom);
            }
        }

        private void SetupFileCommunicator(CertificateService certificateService)
        {
            var fileCom = new FileCommunicator(LocalStoragePath, LoadDataFromLocalStorage, EnablePersistence,ClearLocalStorageOnStartUp);
            FileComLayer = new CommunicationLayer(ManagementLayer, certificateService, fileCom);

            ManagementLayer.FileCommunicationLayer = FileComLayer;
            fileCom.Start();
            IsPersistenceEnabled = EnablePersistence;
        }

        private void RegisterPublicEvents()
        {
            ManagementLayer.JobListChanged += OnJobListChanged;
            ManagementLayer.WorldListChanged += OnWorldListChanged;
            ManagementLayer.JobFinished += OnJobFinished;
            ManagementLayer.JobProgress += OnJobProgress;
            ManagementLayer.TaskStarted += OnTaskStarted;
            ManagementLayer.TaskProgress += OnTaskProgress;
            ManagementLayer.TaskStopped += OnTaskStopped;
        }

        #endregion

        #endregion

        #region public actions

        /// <summary>
        ///   Stops the calculation on the given job.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        public virtual void StopCalculation(BigInteger jobID)
        {
            ThrowErrorIfNotStarted();
            if (ManagementLayer.LocalStates.ContainsKey(jobID))
                ManagementLayer.LocalStates[jobID].CalculationLayer.Stop();
        }

        /// <summary>
        ///   Sends an WorldList-Request message to each member of the multicast group.
        ///   Whenever a response is received, <see cref="WorldsChanged" /> will be invoked.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual void RefreshWorldList()
        {
            ThrowErrorIfNotStarted();
            CommunicationLayer.RequestWorldList(IPAddress.Any);
        }

        /// <summary>
        ///   Sends an JobList-Request message for the given world to each member of the multicast group.
        ///   Whenever a response is received, <see cref="JobListChanged" /> will be invoked.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual void RefreshJobList(string world)
        {
            ThrowErrorIfNotStarted();
            CommunicationLayer.RequestJobList(world, IPAddress.Any);
        }

        /// <summary>
        ///   Sends an JobDetail-Request message for the given job to each member of the multicast group.
        ///   Whenever a response is received, <see cref="JobListChanged" /> will be invoked.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual void RequestJobDetails(NetworkJob job)
        {
            ThrowErrorIfNotStarted();
            CommunicationLayer.RequestJobDetails(job.JobID, job.World, IPAddress.Any);
        }

        /// <summary>
        ///   Joins a network job.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <param name="template">The template.</param>
        /// <param name="amountOfWorker">The amount of worker.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual bool JoinNetworkJob(BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {
            ThrowErrorIfNotStarted();
            return ManagementLayer.JoinNetworkJob(jobID, template, amountOfWorker);
        }

        /// <summary>
        ///   Creates a network job.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="description">The description.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="numberOfBlocks">The number of blocks.</param>
        /// <param name="jobID">The job identifier.</param>
        /// <returns>The job identifier of the new job</returns>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual BigInteger CreateNetworkJob(string world, string jobType, string jobName, string description, byte[] payload, BigInteger numberOfBlocks,
            BigInteger jobID)
        {
            ThrowErrorIfNotStarted();
            var copyOfDefaultStateConfig = new EpochStateConfig
            {
                BitMaskWidth = DefaultStateConfig.BitMaskWidth,
                NumberOfBlocks = numberOfBlocks
            };

            var job = new NetworkJob(jobID)
            {
                World = world,
                JobName = jobName,
                JobType = jobType,
                StateConfig = copyOfDefaultStateConfig,
                JobDescription = DefaultEncoding.GetBytes(description),
                JobPayload = payload,
                Creator = CertificateName
            };

            ManagementLayer.CreateNetworkJob(job);
            return job.JobID;
        }

        public virtual void DeleteNetworkJob(BigInteger jobID)
        {
            ManagementLayer.DeleteNetworkJob(jobID);
        }

        #region jobID overloads

        /// <summary>
        ///   Joins a network job.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <param name="template">The template.</param>
        /// <param name="amountOfWorker">The amount of worker.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual void JoinNetworkJob(byte[] jobID, ACalculationTemplate template, int amountOfWorker)
        {
            JoinNetworkJob(new BigInteger(jobID), template, amountOfWorker);
        }

        /// <summary>
        ///   Creates a network job.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="description">The description.</param>
        /// <param name="payload"></param>
        /// <param name="numberOfBlocks">The number of blocks.</param>
        /// <param name="jobID"></param>
        /// <returns>The job identifier of the new job</returns>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual BigInteger CreateNetworkJob(string world, string jobType, string jobName, string description, byte[] payload, BigInteger numberOfBlocks,
            byte[] jobID)
        {
            return CreateNetworkJob(world, jobType, jobName, description, payload, numberOfBlocks, new BigInteger(jobID ?? GetRandom()));
        }

        /// <summary>
        ///   Creates a network job.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="description">The description.</param>
        /// <param name="payload"></param>
        /// <param name="numberOfBlocks">The number of blocks.</param>
        /// <returns>The job identifier of the new job</returns>
        /// <exception cref="System.NotSupportedException">
        ///   Thrown, if the object has not
        ///   been initialized or not been started.
        /// </exception>
        public virtual BigInteger CreateNetworkJob(string world, string jobType, string jobName, string description, byte[] payload, BigInteger numberOfBlocks)
        {
            return CreateNetworkJob(world, jobType, jobName, description, payload, numberOfBlocks, new BigInteger(GetRandom()));
        }

        #endregion

        #endregion public actions

        #region public accessors

        /// <summary>
        ///   Get the job by its identifier.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <returns>
        ///   The NetworkJob with the given ID or null if the given ID
        ///   does not match any local job
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual NetworkJob GetJobByID(BigInteger jobID)
        {
            ThrowErrorIfNotInitialized();
            var job = ManagementLayer.Jobs.GetJob(jobID);
            return job == null || job.IsDeleted ? null : job;
        }

        /// <summary>
        ///   Returns all jobs of the given World
        /// </summary>
        /// <returns>
        ///   A list of jobs of the given world or
        ///   an empty list if no job with the given world is known
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<NetworkJob> GetJobsOfWorld(string world)
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.Jobs.GetJobsOfWorld(world).FindAll(job => !job.IsDeleted);
        }

        /// <summary>
        ///   Returns a list of all known jobs.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<NetworkJob> GetJobs()
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.Jobs.GetJobs().FindAll(job => !job.IsDeleted);
        }

      
        public virtual BigInteger GetCalculatedBlocksOfJob(BigInteger jobID)
        {
            ThrowErrorIfNotInitialized();

            LocalStateManager<EpochState> stateManager;
            ManagementLayer.LocalStates.TryGetValue(jobID, out stateManager);
            return stateManager != null ? stateManager.LocalState.NumberOfCalculatedBlocks : new BigInteger(0);
        }

        /// <summary>
        ///   Returns the result list.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <returns>The result list or an empty list the given ID does not match any local job</returns>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<byte[]> GetResultList(BigInteger jobID)
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.LocalStates.ContainsKey(jobID)
                ? ManagementLayer.LocalStates[jobID].LocalState.ResultList
                : new List<byte[]>(0);
        }

        /// <summary>
        ///   Returns the working peer list.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<WorkLog> GetWorkingPeerList(BigInteger jobID)
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.WorkingPeers.WorklogByJobID(jobID);
        }

        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual Dictionary<BigInteger, int> GetCurrentRunningWorkersPerJob()
        {
            ThrowErrorIfNotInitialized();
            var localStates = ManagementLayer.LocalStates.Values;
            var runningStates = localStates.Where(state => state.CalculationLayer != null && state.CalculationLayer.NumberOfRunningWorker > 0);
            return runningStates.ToDictionary(state => state.JobID, state => state.CalculationLayer.NumberOfRunningWorker);
        }


        /// <summary>
        ///   Returns the current world list.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<String> GetWorlds()
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.Worlds;
        }

        public EpochState GetStateOfJob(BigInteger jobId)
        {
            ThrowErrorIfNotInitialized();
            return ManagementLayer.GetStateByJobId(jobId);
        }

        #region jobID overloads

        /// <summary>
        ///   Get the job by its identifier.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <returns>
        ///   The NetworkJob with the given ID or null if the given ID
        ///   does not match any local job
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual NetworkJob GetJobByID(byte[] jobID)
        {
            return GetJobByID(new BigInteger(jobID));
        }

        /// <summary>
        ///   Returns the working peer list.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<WorkLog> GetWorkingPeerList(byte[] jobID)
        {
            return GetWorkingPeerList(new BigInteger(jobID));
        }

        /// <summary>
        ///   Returns the result list.
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <returns>The result list or an empty list the given ID does not match any local job</returns>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        public virtual List<byte[]> GetResultList(byte[] jobID)
        {
            return GetResultList(new BigInteger(jobID));
        }

        #endregion

        #endregion public accessors

        #region public events

        /// <summary>
        ///   Occurs whenever a job made progress.
        ///   It is called with <see cref="JobProgressEventArgs" /> which contains the JobID of the job,
        ///   the result list, the number of blocks and the number of calculated blocks.
        /// </summary>
        public event EventHandler<JobProgressEventArgs> JobProgress;

        /// <summary>
        ///   Occurs whenever a job is finished.
        ///   It is called with <see cref="JobProgressEventArgs" /> which contains the JobID of the job,
        ///   the result list, the number of blocks and the number of calculated blocks.
        /// </summary>
        public event EventHandler<JobProgressEventArgs> JobFinished;

        /// <summary>
        ///   Occurs when the job list has changed.
        ///   EventArg: propertyChangeEventArgs "JobList"
        /// </summary>
        public event PropertyChangedEventHandler JobListChanged;

        /// <summary>
        ///   Occurs when the world list has changed.
        ///   EventArg: propertyChangeEventArgs "WorldList"
        /// </summary>
        public event PropertyChangedEventHandler WorldsChanged;

        /// <summary>
        ///   Occurs whenever a new task has started.
        ///   It is called with <see cref="TaskEventArgs" /> which contains the JobID of the corresponding job and the blockID on
        ///   which the task works.
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskStarted;

        /// Occurs whenever a new task has made progress.
        /// It is called with
        /// <see cref="TaskEventArgs" />
        /// which contains the JobID of the corresponding job, 
        /// the blockID on which the task works and an Integer that indicates the task's progress.
        public event EventHandler<TaskEventArgs> TaskProgress;

        /// Occurs whenever a new task has made progress.   
        /// It is called with
        /// <see cref="TaskEventArgs" />
        /// which contains the JobID of the corresponding job, 
        /// the blockID on which the task worked and and an Type that indicates whether the has finished or had been stopped.
        public event EventHandler<TaskEventArgs> TaskStopped;


        /// <summary> 
        /// Set LogMode to LogMode.EventBased to enable
        /// </summary>
        public event EventHandler<LogEventInfoArg> ApplicationLog;


        #region invoker

        protected virtual void OnApplicationLog(object sender, LogEventInfoArg logEventInfoArg)
        {
            var handler = ApplicationLog;
            if (handler != null) handler(this, logEventInfoArg);
        }

        private void OnTaskStarted(object sender, TaskEventArgs e)
        {
            var handler = TaskStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTaskProgress(object sender, TaskEventArgs e)
        {
            var handler = TaskProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnTaskStopped(object sender, TaskEventArgs e)
        {
            var handler = TaskStopped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnJobListChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = JobListChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnJobFinished(object sender, JobProgressEventArgs e)
        {
            var handler = JobFinished;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void OnJobProgress(object sender, JobProgressEventArgs e)
        {
            var handler = JobProgress;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void OnWorldListChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = WorldsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
        
        #endregion public events

        #region Helper

        /// <returns>A random UInt 128.</returns>
        private byte[] GetRandom()
        {
            var buffer = new byte[16];
            randomGen.NextBytes(buffer);
            return buffer;
        }

        /// <summary>
        ///   Throws if not initialized.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been initialized.
        ///   Remember to call the init-method
        /// </exception>
        private void ThrowErrorIfNotInitialized()
        {
            if (!IsInitialized)
                throw new NotSupportedException("Object has not been initialized. Remember to call the init-method");
        }

        /// <summary>
        ///   Throws if not started.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        ///   Object has not been started.
        ///   Remember to call the start-method
        /// </exception>
        private void ThrowErrorIfNotStarted()
        {
            ThrowErrorIfNotInitialized();
            if (!IsStarted)
                throw new NotSupportedException("Object has not been started. Remember to call the start-method");
        }

        #endregion

        #region visualization
        
        public Bitmap GetVisualizationOfJobState(BigInteger jobId)
        {
            ThrowErrorIfNotStarted();
            var job = GetJobByID(jobId);
            if (job == null || ! ManagementLayer.LocalStates.ContainsKey(jobId))
            {
                return null;
            }

            var stateManager = ManagementLayer.LocalStates[jobId];
            return EpochStateVisualization.CreateImage(stateManager.LocalState);
        }

        #endregion

        public bool CanUserDeleteJob(NetworkJob job)
        {
            ThrowErrorIfNotInitialized();
            return job.Creator.Equals(CertificateName) || CertificateService.IsAdminCertificate(CertificateService.OwnCertificate);
        }

        public bool IsCertificateBanned(X509Certificate2 certificate)
        {
            ThrowErrorIfNotInitialized();
            return CertificateService.IsBannedCertificate(certificate);
        }
    }
}
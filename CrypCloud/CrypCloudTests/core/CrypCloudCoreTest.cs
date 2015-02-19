using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Controls;
using CrypCloud.Core;
using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.utils;
using Cryptool.PluginBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy; 
using voluntLib;
using voluntLib.common;
using voluntLib.common.interfaces;
using WorkspaceManager.Model;  

namespace CrypCloudTests
{
    [TestClass]
    public class CrypCloudCoreTest
    {
        private VoluntLib voluntLib;
        private CrypCloudCore cryptCloudCore;

        [TestInitialize]
        public void Setup()
        {
            voluntLib = A.Fake<VoluntLib>();
            cryptCloudCore = CrypCloudCore.Instance;
            var accessor = new PrivateObject(cryptCloudCore);
            accessor.SetFieldOrProperty("voluntLib", voluntLib);

            cryptCloudCore.DefaultWorld = "UNIT_TEST";
        }

        #region expose voluntLib's logic

        [TestMethod]
        public void IsRunning_returnsVoluntLibsIsStartedState()
        {
            A.CallTo(() => voluntLib.IsStarted).Returns(false);
            Assert.IsFalse(cryptCloudCore.IsRunning);

            A.CallTo(() => voluntLib.IsStarted).Returns(true);
            Assert.IsTrue(cryptCloudCore.IsRunning);
        }

        [TestMethod]
        public void RefreshJobs_invokeVoluntLibsMethod()
        {
            cryptCloudCore.RefreshJobList();
            A.CallTo(() => voluntLib.RefreshJobList(cryptCloudCore.DefaultWorld)).MustHaveHappened();
        }

        [TestMethod]
        public void GetJob_invokeVoluntLibsMethod()
        {
            cryptCloudCore.GetJobs();

            A.CallTo(() => voluntLib.GetJobsOfWorld(cryptCloudCore.DefaultWorld)).MustHaveHappened();
        }


        [TestMethod]
        public void StartJobs_invokeVoluntLibsMethod()
        {
            var jobId = -1;
            ACalculationTemplate template = null;

            cryptCloudCore.StartLocalCalculation(jobId, template);

            A.CallTo(() => voluntLib.JoinNetworkJob(jobId, template, cryptCloudCore.AmountOfWorker)).MustHaveHappened();
        }

        [TestMethod]
        public void StopJob_invokeVoluntLibsMethod()
        {
            var jobId = -1;

            cryptCloudCore.StopLocalCalculation(jobId);
            A.CallTo(() => voluntLib.StopCalculation(jobId)).MustHaveHappened();
        }

        #endregion

        #region Login 

        [TestMethod]
        public void Login_invokesVoluntLibInitwithGivenCertificates()
        { 
            var ownCertificate = new X509Certificate2();

            cryptCloudCore.Login(ownCertificate);

            A.CallTo(() => voluntLib.InitAndStart(new X509Certificate2(CrypCloud.Core.Properties.Resources.rootCA), ownCertificate)).MustHaveHappened();
        }

        [TestMethod]
        public void Login_AbortsAndreturnsFalse_allreadyRunning()
        { 
            A.CallTo(() => voluntLib.IsStarted).Returns(true); 
            var ownCertificate = new X509Certificate2();

            var returns = cryptCloudCore.Login(ownCertificate);

            Assert.IsFalse(returns);
            A.CallTo(() => voluntLib.InitAndStart(new X509Certificate2(CrypCloud.Core.Properties.Resources.rootCA), ownCertificate)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Login_returnTrue_onSuccessfullLogin()
        {
            var ownCertificate = new X509Certificate2();

            var returns = cryptCloudCore.Login(ownCertificate);

            Assert.IsTrue(returns);
        }

        #endregion

        #region create job

        [TestMethod]
        public void CreateJob_shouldCallVoluntLib()
        {
            var jobType = "jobtype";
            var jobName = "jobName";
            var jobDescription = "jobDescription";
            var numberOfBlocks = 1;
            var workspaceModel = CreateValidWorkspaceModel();

            bool creationSucessful = cryptCloudCore.CreateJob(jobType, jobName, jobDescription, workspaceModel,
                numberOfBlocks);

            Assert.IsTrue(creationSucessful);
            A.CallTo(
                () =>
                    voluntLib.CreateNetworkJob(cryptCloudCore.DefaultWorld, jobType, jobName, jobDescription,
                        A<Byte[]>._, numberOfBlocks)).MustHaveHappened();

        }

        [TestMethod]
        public void CreateJob_shouldRejectWorkspacesWithoutCloudPlugin()
        {
            var workspaceModel = new WorkspaceModel();

            bool creationSucessful = cryptCloudCore.CreateJob("", "", "", workspaceModel, 0);

            Assert.IsFalse(creationSucessful);
            A.CallTo(
                () =>
                    voluntLib.CreateNetworkJob(A<string>._, A<string>._, A<string>._, A<string>._, A<Byte[]>._,
                        A<long>._)).MustNotHaveHappened();

        }

        #endregion

        #region payload/workspace

        [TestMethod]
        public void GetWorkspaceOfUnknownJob_shouldReturnNull()
        {
            A.CallTo(() => voluntLib.GetJobByID(1)).Returns(null);

            var jobID = 1;
            WorkspaceModel returnedWorkspace = cryptCloudCore.GetWorkspaceOfJob(jobID);

            Assert.IsNull(returnedWorkspace);
        }

        [TestMethod]
        public void GetWorkspace_shouldReturnPayloadAsWorkspace()
        {
            var workspaceModel = new WorkspaceModel {Zoom = 10};
            FakeNetworkJobInVoluntLib(1, workspaceModel);

            var returnedWorkspace = cryptCloudCore.GetWorkspaceOfJob(1);
            Assert.AreEqual(10, returnedWorkspace.Zoom, "distigusher must match");
        }

        [TestMethod]
        [Ignore] //cant test due to pluginmodel's assembly load
        public void GetWorkspace_shouldInjectJobIDIntoComponents()
        {
            var jobID = 1;
            var workspaceModel = CreateValidWorkspaceModel();
            FakeNetworkJobInVoluntLib(jobID, workspaceModel);

            var returnedWorkspace = cryptCloudCore.GetWorkspaceOfJob(jobID);

            var cloudComponents = returnedWorkspace.GetAllPluginModels().Where((it) => it.Plugin is ACloudComponent);
            Assert.AreEqual(1, cloudComponents.Count());
            foreach (var component in cloudComponents)
            {
                Assert.AreEqual(jobID, ((ACloudComponent) component.Plugin).JobID);
            }
        }

        [TestMethod]
        public void ContainsCloudComponent()
        {
            var workspaceModel = CreateValidWorkspaceModel();
            Assert.IsTrue(workspaceModel.GetAllPluginModels().Any(it => it.Plugin is ACloudComponent), "ACloudComponent");
        }

        #endregion

        #region helperd

        private void FakeNetworkJobInVoluntLib(int jobID, WorkspaceModel workspaceModel)
        {
            var networkJob = new NetworkJob(jobID) {JobPayload = PayloadSerialization.Serialize(workspaceModel)};
            A.CallTo(() => voluntLib.GetJobByID(jobID)).Returns(networkJob);
        }

        internal class DummyCloudPlugin : ACloudComponent
        {
            public override event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

            public override ISettings Settings
            {
                get { return null; }
            }

            public override UserControl Presentation
            {
                get { return null; }
            }

            public override List<byte[]> CalculateBlock(BigInteger blockId, CancellationToken cancelToken)
            {
                return null;
            }

            public override List<byte[]> MergeBlockResults(IEnumerable<byte[]> oldResultList,
                IEnumerable<byte[]> newResultList)
            {
                return null;
            }

            public override void PreExecutionLocal()
            {
            }

            public override void StopLocal()
            {
            }

            public override void Execute()
            {
            }

            public override void Initialize()
            {
            }

            public override void PostExecution()
            {
            }

            public override event StatusChangedEventHandler OnPluginStatusChanged;
            public override event PluginProgressChangedEventHandler OnPluginProgressChanged;

            public override void Dispose()
            {
            }

            public override event PropertyChangedEventHandler PropertyChanged;
        }

        public static WorkspaceModel CreateValidWorkspaceModel()
        {
            var workspaceModel = new WorkspaceModel();
            var componentMock = new DummyCloudPlugin();
            var pluginModel = new PluginModel();
            var accessor = new PrivateObject(pluginModel);
            accessor.SetFieldOrProperty("Plugin", componentMock);
            workspaceModel.addPluginModel(pluginModel);
            return workspaceModel;
        }

        #endregion
    }
}
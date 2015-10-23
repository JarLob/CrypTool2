using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using CrypCloud.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using KeySearcher.CrypCloud;
using KeySearcher.CrypCloud.statistics;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;
using voluntLib.common;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;
using Timer = System.Timers.Timer;

namespace KeySearcher
{
    internal class CloudKeySearcher 
    {
        public int UpdateInterval = 2000;

        private readonly KeySearcher keySearcher;
        private readonly CalculationTemplate calculationTemplate;
        private readonly BigInteger jobId;
        private readonly P2PPresentationVM viewModel; 
        private readonly TaskFactory uiContext;
        
        private readonly SpeedStatistics globalSpeedStatistics = new SpeedStatistics(30);
        private readonly SpeedStatistics localSpeedStatistics = new SpeedStatistics(5);

        private Timer updateTimer;

        public CloudKeySearcher(JobDataContainer jobDataContainer, KeyPattern.KeyPattern pattern, P2PQuickWatchPresentation presentation, KeySearcher keySearcher)
        {
            this.keySearcher = keySearcher;
            CrypCloudCore.Instance.JobStateChanged += JobStateChanged; 
            CrypCloudCore.Instance.TaskHasStarted += NewTaskStarted;
            CrypCloudCore.Instance.TaskHasStopped += TaskEnded;
            CrypCloudCore.Instance.TaskProgress += TaskProgress;

            jobId = jobDataContainer.JobId;
            calculationTemplate = new CalculationTemplate(jobDataContainer, pattern);

            uiContext = presentation.UiContext;
            viewModel = presentation.ViewModel; 
            viewModel.GlobalSpeedStatistics = globalSpeedStatistics;
            viewModel.LocalSpeedStatistics = localSpeedStatistics;

            RunInUiContext(() =>
            {
                viewModel.JobID = jobId; 
                UpdatePresentation(presentation, keySearcher);
            });
        }


        private void UpdateKeyPerSecond(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var globalApproximateKeysPerSecond = globalSpeedStatistics.ApproximateKeysPerSecond();
            var localApproximateKeysPerSecond = localSpeedStatistics.ApproximateKeysPerSecond();
            RunInUiContext(() =>
            {
                viewModel.UpdateGlobalSpeed(globalApproximateKeysPerSecond);
                viewModel.UpdateLocalSpeed(localApproximateKeysPerSecond);
            });
        }

        private void TaskProgress(object sender, TaskEventArgs e)
        {
             if (new BigInteger(e.JobID) != jobId) return;

           localSpeedStatistics.AddEntry(e.TaskProgress);
           var localApproximateKeysPerSecond = localSpeedStatistics.ApproximateKeysPerSecond();
           RunInUiContext(() =>
           { 
               viewModel.UpdateLocalSpeed(localApproximateKeysPerSecond);
           });
        }

        private void NewTaskStarted(object sender, TaskEventArgs taskArgs)
        {
            if (new BigInteger(taskArgs.JobID) != jobId) return;

            if (keySearcher.WorkspaceHasBeenModified())
            {
                try
                {
                    CrypCloudCore.Instance.StopLocalCalculation(jobId);
                }
                finally
                {
                    keySearcher.GuiLogMessage("Calculation has been aborted due to changes in the workplace.", NotificationLevel.Error);
                }
                return;
            }

            RunInUiContext(
                () => viewModel.StartedLocalCalculation(taskArgs.BlockID)
            );
        }
         
        private void TaskEnded(object sender, TaskEventArgs taskArgs)
        {
            if (new BigInteger(taskArgs.JobID) != jobId) return;

            RunInUiContext(
                () => viewModel.EndedLocalCalculation(taskArgs) 
            );
        } 

        private void JobStateChanged(object sender, JobProgressEventArgs progress)
        {
            if (progress.JobId != jobId) return;

            var keyResultEntries = progress.ResultList.Select(it => new KeyResultEntry(it)).Distinct().ToList();
            keyResultEntries.Sort();
             
            RunInUiContext(
                () => viewModel.BlockHasBeenFinished(progress, keyResultEntries)
            ); 
            
            if (keyResultEntries.Count > 0)
            {
                keySearcher.SetTop1Entry(keyResultEntries[0]);
            } 
        }


        private void UpdatePresentation(P2PQuickWatchPresentation presentation, KeySearcher keySearcher)
        { 
            presentation.UpdateSettings(keySearcher, (KeySearcherSettings)keySearcher.Settings); ;
        }

        protected void RunInUiContext(Action action)
        { 
            uiContext.StartNew(action);
        }

        public void Start()
        {
            if (keySearcher.WorkspaceHasBeenModified())
            {
                keySearcher.GuiLogMessage("Calculation can not be started since the workspace was changed.", NotificationLevel.Error);
                return;
            }

            CrypCloudCore.Instance.StartLocalCalculation(jobId, calculationTemplate);

            updateTimer = new Timer(UpdateInterval);
            updateTimer.Elapsed += UpdateKeyPerSecond;
            updateTimer.Interval = UpdateInterval;
            updateTimer.Enabled = true;  
        }

        public void Stop()
        {
            try
            {
                updateTimer.Enabled = false;
                updateTimer.Stop(); 
            } catch (Exception){}

            try
            {
                CrypCloudCore.Instance.StopLocalCalculation(jobId);
            } catch(Exception e){}
        }
    }
}
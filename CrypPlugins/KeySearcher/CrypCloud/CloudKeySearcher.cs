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
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;
using voluntLib.common;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;

namespace KeySearcher
{
    internal class CloudKeySearcher 
    {
        private readonly KeySearcher keySearcher;
        private readonly CalculationTemplate calculationTemplate;
        private readonly BigInteger jobId;
        private readonly P2PPresentationVM viewModel; 
        private readonly TaskFactory uiContext;

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


            RunInUiContext(() =>
            {
                viewModel.JobID = jobId; 
                UpdatePresentation(presentation, keySearcher);
            });
        }

        private void TaskProgress(object sender, TaskEventArgs e)
        {
           // throw new NotImplementedException();
        }

        private void NewTaskStarted(object sender, TaskEventArgs taskArgs)
        {
            if (new BigInteger(taskArgs.JobID) != jobId) return;

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

            var keyResultEntries = progress.ResultList.Select(it => new KeyResultEntry(it)).ToList();
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
            CrypCloudCore.Instance.StartLocalCalculation(jobId, calculationTemplate);
        }

        public void Stop()
        {
            CrypCloudCore.Instance.StopLocalCalculation(jobId);
        }
    }
}
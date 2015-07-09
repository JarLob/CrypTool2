using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Threading;
using CrypCloud.Core;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;
using voluntLib.common.eventArgs;
using voluntLib.common.interfaces;

namespace KeySearcher
{
    internal class CloudKeySearcher 
    {
        private readonly KeySearcher keySearcher;
        private readonly CalculationTemplate calculationTemplate;
        private readonly BigInteger jobId;
        private readonly StatusContainer viewModel;
        private readonly Dispatcher dispatcher;

        public CloudKeySearcher(JobDataContainer jobDataContainer, KeyPattern.KeyPattern pattern, P2PQuickWatchPresentation presentation, KeySearcher keySearcher)
        {
            this.keySearcher = keySearcher;
            CrypCloudCore.Instance.JobStateChanged += JobStateChanged;
            jobId = jobDataContainer.JobId;
            calculationTemplate = new CalculationTemplate(jobDataContainer, pattern);

            dispatcher = presentation.Dispatcher;
            viewModel = new StatusContainer();
            viewModel.BindToView(presentation);
        }

     
        private void JobStateChanged(object sender, JobProgressEventArgs progress)
        {
            if (progress.JobId != jobId) return;

            var keyResultEntries = progress.ResultList.Select(it => new KeyResultEntry(it)).ToList();
            keyResultEntries.Sort();

            dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (SendOrPostCallback) delegate
                {
                    viewModel.GlobalProgress = (double) BigInteger.Divide(progress.NumberOfCalculatedBlocks, progress.NumberOfBlocks);
                    viewModel.TopList.Clear();
                    keyResultEntries.ForEach(it => viewModel.TopList.Add(it));
                }, null);

            if (keyResultEntries.Count > 0)
            {
                keySearcher.SetTop1Entry(keyResultEntries[0]);
            }

        }

        public void Start()
        {
            viewModel.CurrentOperation = "running";
            CrypCloudCore.Instance.StartLocalCalculation(jobId, calculationTemplate);
        }

        public void Stop()
        {
            viewModel.CurrentOperation = "stopped";
            CrypCloudCore.Instance.StopLocalCalculation(jobId);
        }
    }
}
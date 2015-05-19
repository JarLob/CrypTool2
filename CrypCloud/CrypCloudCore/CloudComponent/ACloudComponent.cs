using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace CrypCloud.Core.CloudComponent
{
    public abstract class ACloudComponent : ICrypComponent
    {
        private readonly CrypCloudCore cryptCloudCore = CrypCloudCore.Instance;
        private readonly CalculationTemplate calculationTemplate;
        private CancellationTokenSource offlineCancellation;
        
        protected ACloudComponent(BigInteger numberOfBlocks)
        {
            NumberOfBlocks = numberOfBlocks;
            JobID = -1;
            calculationTemplate = new CalculationTemplate(this);
            CurrentBestlist = new List<byte[]>();
        }

        /// <summary>
        /// Number Of Blocks that should be calculated.
        /// Has to be set before starting
        /// </summary>
        public BigInteger NumberOfBlocks { get; protected set; }

        /// <summary>
        /// JobId of bound networkjob.
        /// If its -1 its not bound to any networkjob
        /// </summary>
        public BigInteger JobID { get; set; }

        public List<byte[]> CurrentBestlist { get; private set; }
       
        public void Stop()
        {
            if (IsOnline()) 
                cryptCloudCore.StopLocalCalculation(JobID);
            else 
                offlineCancellation.Cancel(false);

            StopLocal();
        }

        public void PreExecution()
        {

            GuiLogMessage("preEx isOnline: " + IsOnline(), NotificationLevel.Error);
            PreExecutionLocal();

            if (IsOnline()) 
                cryptCloudCore.StartLocalCalculation(JobID, calculationTemplate);
            else 
                StartOfflineCalculation();
        }
         
        private void StartOfflineCalculation()
        {
            offlineCancellation = new CancellationTokenSource();
            for (var i = 0; i < NumberOfBlocks && !offlineCancellation.IsCancellationRequested; i++)
            {
                var block = CalculateBlock(i, offlineCancellation.Token);
                CurrentBestlist = MergeBlockResults(CurrentBestlist, new List<byte[]>(block));
            }
        }
        
        /// <summary>
        /// Determine if this component is linked to a Networkjob
        /// </summary>
        /// <returns></returns>
        public bool IsOnline()
        {
            return JobID != -1;
        }

        #region abstract member

        /// <summary>
        /// Represents the logic for calculation a single "cloud" block
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public abstract List<byte[]> CalculateBlock(BigInteger blockId, CancellationToken cancelToken);

        /// <summary>
        /// Merges two CalculateBlock-results.
        /// </summary> 
        /// <returns></returns>
        public abstract List<byte[]> MergeBlockResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList);

        /// <summary>
        /// Will be called once before local Calculation is started. May be used to set up data used for execution.
        /// </summary>
        public abstract void PreExecutionLocal();

        /// <summary>
        /// Will be called after the local calculation has stoped.
        /// </summary>
        public abstract void StopLocal();

        #endregion

        #region abstract member of ICrypComponent

        public abstract ISettings Settings { get; }
        public abstract UserControl Presentation { get; } 

        public abstract void Execute();
        public abstract void Initialize();
        public abstract void PostExecution();
        public abstract void Dispose();
         
        public abstract event PropertyChangedEventHandler PropertyChanged; 
        public abstract event StatusChangedEventHandler OnPluginStatusChanged;
        public abstract event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region logger

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        protected void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }
     
}

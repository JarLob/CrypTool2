using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace CrypCloud.Core.CloudComponent
{
    public abstract class ACloudComponent : ICrypComponent
    {
        private readonly CrypCloudCore cryptCloudCore = CrypCloudCore.Instance;
        private readonly CalculationTemplate calculationTemplate;

        protected ACloudComponent()
        {
            calculationTemplate = new CalculationTemplate(this); 
        }

        public BigInteger JobID { get; set; }
      
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

        #region proxy methodes 

        public void Stop()
        {
            cryptCloudCore.StopLocalCalculation(JobID);
            StopLocal();
        }

        public void PreExecution()
        {
            PreExecutionLocal();
            cryptCloudCore.StartLocalCalculation(JobID, calculationTemplate);
        }

        /// <summary>
        /// Will be called once before local Calculation is started. May be used to set up data used for execution.
        /// </summary>
        public abstract void PreExecutionLocal();

        /// <summary>
        /// Will be called after the local calculation has stoped.
        /// </summary>
        public abstract void StopLocal();

        #endregion

        #region abstract member 
        
        public abstract ISettings Settings { get; }
        public abstract UserControl Presentation { get; } 

        public abstract void Execute();
        public abstract void Initialize();
        public abstract void PostExecution();
        public abstract void Dispose();
         
        public abstract event PropertyChangedEventHandler PropertyChanged;
        public abstract event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public abstract event StatusChangedEventHandler OnPluginStatusChanged;
        public abstract event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion
    }
     
}

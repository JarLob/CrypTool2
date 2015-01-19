using System.Collections.Generic;
using voluntLib.common.interfaces;

namespace CrypCloud.Core.CloudComponent
{
    internal class CalculationTemplate : ACalculationTemplate
    {
        private readonly ACloudComponent cloudComponent;

        public CalculationTemplate(ACloudComponent cloudComponent)
        {
            this.cloudComponent = cloudComponent;
            WorkerLogic = new CloudWorker(cloudComponent);
        }

        public override List<byte[]> MergeResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList)
        {
            return cloudComponent.MergeBlockResults(oldResultList, newResultList);
        }
    }
}
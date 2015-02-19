using System.Numerics;
using System.Threading;
using voluntLib.common.interfaces;

namespace CrypCloud.Core.CloudComponent
{
    internal class CloudWorker : AWorker
    {
        private readonly ACloudComponent cloudComponent;

        public CloudWorker(ACloudComponent cloudComponent)
        {
            this.cloudComponent = cloudComponent;
        }

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockID, CancellationToken cancelToken)
        {
            return new CalculationResult
            {
                BlockID = blockID,
                LocalResults = cloudComponent.CalculateBlock(blockID, cancelToken)
            };
        }
    } 
}

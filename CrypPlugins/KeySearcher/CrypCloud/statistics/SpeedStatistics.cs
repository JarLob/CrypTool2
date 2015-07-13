using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace KeySearcher.CrypCloud.statistics
{
    class SpeedStatistics
    {
        public TimeSpan LatestAvgTime { get; set; }
        public BigInteger LatestKeysPerSecond { get; set; }
    
        private DateTime lastTick = DateTime.Now;
        private readonly CricularBuffer<long> buffer = new CricularBuffer<long>(10);

        public void Tick(BigInteger keysPerBlock)
        {
            var timeSpan = DateTime.Now.Subtract(lastTick);
            buffer.Enqueue(timeSpan.Ticks);
            LatestAvgTime = CalculateAvgTime(buffer);

            LatestKeysPerSecond = CalculateGlobalKeyPerSecond(LatestAvgTime, keysPerBlock);

            lastTick = DateTime.Now;
        }

        private static TimeSpan CalculateAvgTime(CricularBuffer<long> cricularBuffer)
        {
            var average = cricularBuffer.Average();
            return TimeSpan.FromTicks(Convert.ToInt64(average));
        }


        private static BigInteger CalculateGlobalKeyPerSecond(TimeSpan avgTickTime, BigInteger keysPerBlock)
        {
            if (avgTickTime.Ticks == 0) return 0;

            var timeTick = avgTickTime.Ticks;
            var keyPerTick = keysPerBlock.DivideAndReturnDouble(new BigInteger(timeTick));
            return new BigInteger(keyPerTick * TimeSpan.TicksPerSecond);
        }
         

    }
}

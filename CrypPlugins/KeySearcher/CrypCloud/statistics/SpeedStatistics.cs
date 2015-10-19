﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace KeySearcher.CrypCloud.statistics
{
    public class SpeedStatistics
    {
        public static int MinutesUntilEntryInvalidates = 30;

        private DateTime statisticsStartTime = DateTime.UtcNow;
        private readonly List<SpeedStatisticsEntry> calculations = new List<SpeedStatisticsEntry>();

        public void AddEntry(BigInteger numberOfKeysCalculated)
        {
            var entry = new SpeedStatisticsEntry
            {
                NumberOfKeysInBlock = numberOfKeysCalculated,
                InvalidatesAt = DateTime.UtcNow.AddMinutes(MinutesUntilEntryInvalidates)
            };

            lock (this)
            {
                calculations.Add(entry);
            }
        }


        /// <summary>
        /// Approximates the speed of the calcuation by constructing the avg of all entrys received within the last 30 minutes
        /// </summary>
        /// <returns></returns>
        public BigInteger ApproximateKeysPerSecond()
        {
            BigInteger calculatedKeys;
            lock (this)
            {
                calculations.RemoveAll(it => it.InvalidatesAt < DateTime.UtcNow);
                calculatedKeys = calculations.Aggregate(new BigInteger(0), (prev, it) => prev + it.NumberOfKeysInBlock);
            }

            var seconds = MinutesUntilEntryInvalidates*60;
            if (statisticsStartTime.AddMinutes(MinutesUntilEntryInvalidates) > DateTime.UtcNow)
            {
                var timeSpan = (DateTime.UtcNow - statisticsStartTime);
                seconds = (int) timeSpan.TotalSeconds;
            }

            if (seconds == 0)
            {
                return 0;
            }
            return calculatedKeys / seconds;
        }
    }

    internal class SpeedStatisticsEntry
    {
        public DateTime InvalidatesAt { get; set; }

        public BigInteger NumberOfKeysInBlock { get; set; }
    }

}

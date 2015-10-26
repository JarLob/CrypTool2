using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Cryptool.PluginBase.Control;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using voluntLib.common.interfaces;

namespace KeySearcher
{
    internal class CalculationTemplate : ACalculationTemplate<KeyResultEntry>
    {
        private readonly bool sortAscending;

        public CalculationTemplate(JobDataContainer jobData, KeyPattern.KeyPattern pattern, bool sortAscending)
        {
            this.sortAscending = sortAscending;
            var keysPerChunk = pattern.size() / jobData.NumberOfBlocks;
            var keyPool = new KeyPatternPool(pattern, keysPerChunk);
            WorkerLogic = new Worker(jobData, keyPool);
        }


        public override List<KeyResultEntry> MergeResults(IEnumerable<KeyResultEntry> oldResultList, IEnumerable<KeyResultEntry> newResultList)
        {
            var results = newResultList
                .Concat(oldResultList)
                .Distinct();

            results = sortAscending 
                ? results.OrderBy(it => it) 
                : results.OrderByDescending(it => it); 

            return results.Take(10).ToList();
        }
    }
}
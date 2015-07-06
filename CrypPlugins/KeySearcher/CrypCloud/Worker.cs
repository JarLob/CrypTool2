using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Data;
using Cryptool.PluginBase.Control;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using voluntLib.common.interfaces;

namespace KeySearcher
{
    internal class Worker : AWorker
    {
        private readonly JobDataContainer jobData;
        private readonly KeyPatternPool keyPool;

        public Worker(JobDataContainer jobData, KeyPatternPool keyPool)
        {
            this.jobData = jobData;
            this.keyPool = keyPool;
        }

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken)
        {
            var keySet = GetKeySetForBlock(blockId);
            var bestKeys = FindBestKeysInBlock(keySet, cancelToken);
            return CreateCalculationResult(blockId, bestKeys);
        }

        private IKeyTranslator GetKeySetForBlock(BigInteger blockId)
        {
            var keysForBlock = keyPool[blockId]; 
            var keyTranslator = jobData.CryptoAlgorithm.GetKeyTranslator();
            keyTranslator.SetKeys(keysForBlock); 
            return keyTranslator;
        }

        #region find best keys in block

        private IEnumerable<KeyResultEntry> FindBestKeysInBlock(IKeyTranslator keyTranslator, CancellationToken cancelToken)
        {
            var top10Keys = new List<KeyResultEntry>();
            while (keyTranslator.NextKey())
            {
                cancelToken.ThrowIfCancellationRequested();
                var key = keyTranslator.GetKey();
                var decryption = TryDecryption(key);
                var costs = TryCalculateCosts(decryption);
                 
                if (IsTop10Key(top10Keys, costs))
                {
                    top10Keys = CreateNewTopList(top10Keys, costs, decryption, key);
                }
            }
            return top10Keys;
        }

        private static List<KeyResultEntry> CreateNewTopList(List<KeyResultEntry> top10Keys, double costs, byte[] decryption, byte[] key)
        {
            var copyOfKey = new byte[key.Length];
            key.CopyTo(copyOfKey, 0);
            var item = new KeyResultEntry { Costs = costs, KeyBytes = copyOfKey, Decryption = decryption };

            top10Keys.Add(item);
            top10Keys.Sort();
            if (top10Keys.Count > 10)
            {
                top10Keys = top10Keys.GetRange(0, 10);
            }
            return top10Keys;
        }

        private static bool IsTop10Key(IList<KeyResultEntry> bestKeys, double costs)
        {
            return bestKeys.Count <= 10 || bestKeys[10].Costs < costs;
        }
         
        private byte[] TryDecryption(byte[] key)
        {
            try
            {
                return jobData.CryptoAlgorithm.Decrypt(jobData.Cryp, key, jobData.InitVector, jobData.BytesToUse);
            }
            catch (Exception e)
            {
                return null;
            }
        } 
        
        private double TryCalculateCosts(byte[] decryption)
        {
            try
            {
                return jobData.CostAlgorithm.CalculateCost(decryption);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static CalculationResult CreateCalculationResult(BigInteger blockId, IEnumerable<KeyResultEntry> bestKeys)
        {
            return new CalculationResult
            {
                BlockID = blockId,
                LocalResults = bestKeys.Select(entry => entry.Serialize()).ToList()
            };
        }

        #endregion

    }
}
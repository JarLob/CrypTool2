using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Cryptool.PluginBase.Control;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using voluntLib.common.interfaces;

namespace KeySearcher
{
    internal class Worker : AWorker
    {
        private const double Epsilon = 0.01d;

        private readonly JobDataContainer jobData;
        private readonly KeyPatternPool keyPool;
        private readonly RelationOperator relationOperator; 

        public Worker(JobDataContainer jobData, KeyPatternPool keyPool)
        {
            this.jobData = jobData;
            this.keyPool = keyPool;
            relationOperator = jobData.CostAlgorithm.GetRelationOperator(); 
        }

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken)
        {
            var keySet = GetKeySetForBlock(blockId);
            var bestKeys = FindBestKeysInBlock(keySet, cancelToken, blockId);
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


        private IEnumerable<KeyResultEntry> FindBestKeysInBlock(IKeyTranslator keyTranslator, CancellationToken cancelToken, BigInteger blockId)
        {
            var controlEncryption = jobData.CryptoAlgorithm;
            var costAlgorithm = jobData.CostAlgorithm;
            var ciphertext = jobData.Cryp;
            var initVector = jobData.InitVector;
            var bytesToUse = jobData.BytesToUse;
             
            var top10Keys = InitTop10Keys();
            var index = 0;
            while (keyTranslator.NextKey())
            {
                var key = keyTranslator.GetKey();
                var decryption = controlEncryption.Decrypt(ciphertext, key, initVector, bytesToUse);              
                var costs = costAlgorithm.CalculateCost(decryption);

                if (IsTopKey(costs, top10Keys[9].Costs))
                {
                    top10Keys = CreateNewTopList(top10Keys, costs, decryption, key);
                }

                index++;
                if (index%100000 == 0)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (index % 500000 == 0)
                    {
                        OnProgressChanged(blockId, 500000);
                    }
                }
                
               
            }

            OnProgressChanged(blockId, index % 500000);
            return top10Keys;
        }
        

        /// <summary>
        /// Toplist is filled with 10 entrys that will be overriden by every key
        /// This reduces the need of a couple of if-operations an so increases performance
        /// </summary>
        /// <returns></returns>
        private List<KeyResultEntry> InitTop10Keys()
        {
            var top10Keys = new List<KeyResultEntry>(11);
            var defaultCosts = relationOperator == RelationOperator.LargerThen ? double.MinValue : double.MaxValue;
            for (var i = 0; i < 10; i ++)
            {
                top10Keys.Add(new KeyResultEntry {Costs = defaultCosts, Decryption = new byte[1], KeyBytes = new byte[1]});
            }
            return top10Keys;
        }

        private bool IsTopKey(double fst, double snd)
        {
            if (relationOperator == RelationOperator.LargerThen)
            {
                return fst - snd > Epsilon;
            }
            return fst - snd < Epsilon;
        }
     
        private static List<KeyResultEntry> CreateNewTopList(List<KeyResultEntry> top10Keys, double costs,
            byte[] decryption, byte[] key)
        {
            var copyOfKey = new byte[key.Length];
            key.CopyTo(copyOfKey, 0);
            var item = new KeyResultEntry {Costs = costs, KeyBytes = copyOfKey, Decryption = decryption};

            top10Keys.Add(item);
            top10Keys.Sort();
            top10Keys = top10Keys.GetRange(0, 10);
            return top10Keys;
        } 

        private static CalculationResult CreateCalculationResult(BigInteger blockId,
            IEnumerable<KeyResultEntry> bestKeys)
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
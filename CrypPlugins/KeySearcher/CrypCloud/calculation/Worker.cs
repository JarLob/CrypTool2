using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.IO;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using OpenCLNet;
using VoluntLib2.ComputationLayer;

namespace KeySearcher
{
    /// <summary>
    /// Worker that performs keysearching
    /// </summary>
    internal class Worker : AWorker
    {
        private const double Epsilon = 0.01d;

        private readonly JobDataContainer jobData;
        private readonly KeyPatternPool keyPool;
        private readonly RelationOperator relationOperator;
        private OpenCLManager oclManager;
        private KeySearcherOpenCLCode keySearcherOpenCLCode = null;
        private KeySearcherOpenCLSubbatchOptimizer keySearcherOpenCLSubbatchOptimizer = null;
        private static object opencl_lockobject = new object();
        private bool opencl_initialized = false;        
        private int opencl_mode = 1; //normal mode
        private int opencl_deviceIndex = 0;
        private KeySearcher keysearcher = null;
        private bool enableOpenCL = false;

        public Worker(JobDataContainer jobData, KeyPatternPool keyPool, KeySearcher keysearcher, bool enableOpenCL)
        {
            this.jobData = jobData;
            this.keyPool = keyPool;
            this.keysearcher = keysearcher;
            this.enableOpenCL = enableOpenCL;
            relationOperator = jobData.CostAlgorithm.GetRelationOperator();            
            if (OpenCL.NumberOfPlatforms > 0 && enableOpenCL)
            {
                var directoryName = Path.Combine(DirectoryHelper.DirectoryLocalTemp, "KeySearcher");
                oclManager = new OpenCLManager
                {
                    AttemptUseBinaries = false,
                    AttemptUseSource = true,
                    RequireImageSupport = false,
                    BinaryPath = Path.Combine(directoryName, "openclbin"),
                    BuildOptions = "-cl-opt-disable"
                };

                for (var id = 0; id < OpenCL.GetPlatforms().Length; id++)
                {
                    oclManager.CreateDefaultContext(id, DeviceType.ALL);
                    var deviceCounter = 0;
                    if (oclManager != null)
                    {
                        var deviceIndex = 0;
                        foreach (var device in oclManager.Context.Devices)
                        {
                            if (deviceCounter == opencl_deviceIndex)
                            {
                                //we found the correct platform and correct device
                                //thus, we set the opencl_deviceIndex to the deviceIndex of the platform
                                //and stop searching. Now, we already set the correct context
                                opencl_deviceIndex = deviceIndex;
                                break;
                            }
                            deviceCounter++;
                        }
                    }
                }                

                keySearcherOpenCLCode = new KeySearcherOpenCLCode(keysearcher, jobData.Ciphertext, jobData.InitVector, jobData.CryptoAlgorithm, jobData.CostAlgorithm, 256 * 256 * 256 * 16);
                keySearcherOpenCLSubbatchOptimizer = new KeySearcherOpenCLSubbatchOptimizer(opencl_mode,
                oclManager.CQ[opencl_deviceIndex].Device.MaxWorkItemSizes.Aggregate(1, (x, y) => (x * (int)y)) / 8);
                opencl_initialized = true;
            }                       
        }

        public override CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken)
        {
            var keySet = GetKeySetForBlock(blockId);
            IEnumerable<KeyResultEntry> bestKeys = null;
            
            if (opencl_initialized && enableOpenCL)
            {
                if(Monitor.TryEnter(opencl_lockobject, 0)){
                    bestKeys = FindBestKeysInBlock_OpenCL(keySet, cancelToken, blockId);
                    Monitor.Exit(opencl_lockobject);
                }
                else
                {
                    bestKeys = FindBestKeysInBlock(keySet, cancelToken, blockId);
                }
            }
            else
            {
                bestKeys = FindBestKeysInBlock(keySet, cancelToken, blockId);
            }
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
            var ciphertext = jobData.Ciphertext;
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
        /// This method is used to bruteforce using OpenCL.
        /// </summary>
        private unsafe IEnumerable<KeyResultEntry> FindBestKeysInBlock_OpenCL(
            IKeyTranslator keyTranslator, 
           // object[] parameters,
            CancellationToken cancelToken,
            BigInteger blockId)
        {            
            var bestkeys = new List<KeyResultEntry>();
            try
            {
                var bruteforceKernel = keySearcherOpenCLCode.GetBruteforceKernel(oclManager, keyTranslator);                

                Mem userKey;
                var key = keyTranslator.GetKey();
                fixed (byte* ukp = key)
                    userKey = oclManager.Context.CreateBuffer(MemFlags.USE_HOST_PTR, key.Length, new IntPtr((void*)ukp));

                int subbatches = keySearcherOpenCLSubbatchOptimizer.GetAmountOfSubbatches(keyTranslator);
                int subbatchSize = keyTranslator.GetOpenCLBatchSize() / subbatches;

                float[] costArray = new float[subbatchSize];
                Mem costs = oclManager.Context.CreateBuffer(MemFlags.READ_WRITE, costArray.Length * 4);

                IntPtr[] globalWorkSize = { (IntPtr)subbatchSize, (IntPtr)1, (IntPtr)1 };

                keySearcherOpenCLSubbatchOptimizer.BeginMeasurement();

                try
                {
                    for (int i = 0; i < subbatches; i++)
                    {
                        bruteforceKernel.SetArg(0, userKey);
                        bruteforceKernel.SetArg(1, costs);
                        bruteforceKernel.SetArg(2, i * subbatchSize);
                        oclManager.CQ[opencl_deviceIndex].EnqueueNDRangeKernel(bruteforceKernel, 3, null, globalWorkSize, null);
                        oclManager.CQ[opencl_deviceIndex].EnqueueBarrier();

                        Event e;
                        fixed (float* costa = costArray)
                            oclManager.CQ[opencl_deviceIndex].EnqueueReadBuffer(costs, true, 0, costArray.Length * 4, new IntPtr((void*)costa), 0, null, out e);

                        e.Wait();                        
                        bestkeys.AddRange(checkOpenCLResults(keyTranslator, costArray, jobData.CryptoAlgorithm, i * subbatchSize));
                        bestkeys.Sort();
                        if (bestkeys.Count > 10)
                        {
                            bestkeys.RemoveRange(10, bestkeys.Count - 10);
                        }
                        if (i > 0 && i % 5 == 0)
                        {
                            OnProgressChanged(blockId, subbatchSize * 5);
                        }
                        cancelToken.ThrowIfCancellationRequested();
                    }
                    keySearcherOpenCLSubbatchOptimizer.EndMeasurement();
                    OnProgressChanged(blockId, (subbatches % 5) * subbatchSize);                    
                }
                finally
                {
                    costs.Dispose();
                }
            }
            catch (Exception ex)
            {                          
                throw new Exception("Bruteforcing with OpenCL failed!", ex);
            }
            return bestkeys;
        }

        private IEnumerable<KeyResultEntry> checkOpenCLResults(IKeyTranslator keyTranslator, float[] costArray, IControlEncryption sender, int add)
        {
            var bestkeys = new List<KeyResultEntry>();
            double valueThreshold = 0;
            if (relationOperator == RelationOperator.LessThen)
            {
                valueThreshold = double.MaxValue;
            }
            else
            {
                valueThreshold = double.MinValue;
            }
            for (int i = 0; i < costArray.Length; i++)
            {
                float cost = costArray[i];
                if (((relationOperator == RelationOperator.LargerThen) && (cost > valueThreshold))
                    || (relationOperator == RelationOperator.LargerThen) && (cost < valueThreshold))
                {
                    var key = keyTranslator.GetKeyFromRepresentation(keyTranslator.GetKeyRepresentation(i + add));
                    var entry = new KeyResultEntry { Costs = cost,
                                                     KeyBytes = key,
                                                     Decryption = sender.Decrypt(jobData.Ciphertext, key, jobData.InitVector, jobData.BytesToUse)
                    };                                        
                    bestkeys.Add(entry);
                    bestkeys.Sort();
                    if (bestkeys.Count > 10)
                    {
                        bestkeys.RemoveAt(10);
                    }
                    valueThreshold = bestkeys.Last().Costs;
                }
            }
            return bestkeys;
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
                top10Keys.Add(new KeyResultEntry { Costs = defaultCosts, Decryption = new byte[1], KeyBytes = new byte[1]});
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
        
        private List<KeyResultEntry> CreateNewTopList(List<KeyResultEntry> top10Keys, double costs,
            byte[] decryption, byte[] key)
        {
            var copyOfKey = new byte[key.Length];
            key.CopyTo(copyOfKey, 0);
            var item = new KeyResultEntry {Costs = costs, KeyBytes = copyOfKey, Decryption = decryption};
            top10Keys.Add(item);

            var orderByDescending = (relationOperator == RelationOperator.LessThen) 
                ? top10Keys.OrderBy(it => it)
                : top10Keys.OrderByDescending(it => it);

            return orderByDescending.Take(10).ToList();
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
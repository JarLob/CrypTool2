using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCAPathFinder.UI.Tutorial3;

namespace DCAPathFinder.Logic.Cipher3
{
    public class Cipher3PathFinder : IPathFinder
    {
        public event EventHandler<SearchResult> AttackSearchResultOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private Characteristic _currentGlobalMax;
        public bool Stop = false;
        public CancellationTokenSource Cts = new CancellationTokenSource();
        private int _procMultiplier = 2;
        public double _maxProgress;

        /// <summary>
        /// Applies a single SBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort ApplySingleSBox(ushort data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray zeroToThree = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            ushort zeroToThreeInt = BitConverter.ToUInt16(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher3Configuration.SBOX[zeroToThreeInt];

            return zeroToThreeInt;
        }

        /// <summary>
        /// calculates the loop border
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <returns></returns>
        public int CalculateLoopBorder(bool[] activeSBoxes)
        {
            int border = 0;
            for (ushort i = 0; i < activeSBoxes.Length; i++)
            {
                if (activeSBoxes[i])
                {
                    border++;
                }
            }

            border = (int) Math.Pow(2, (border * Cipher3Configuration.BITWIDTHCIPHER2));
            return border;
        }

        /// <summary>
        /// Analyzes a SBox
        /// </summary>
        /// <returns></returns>
        public List<Differential> CountDifferentialsSingleSBox()
        {
            List<Differential> result = new List<Differential>();

            for (ushort i = 0; i < 16; i++)
            {
                for (ushort j = 0; j < 16; j++)
                {
                    ushort inputDiff = (ushort) (i ^ j);
                    ushort outputDiff = (ushort) (ApplySingleSBox(i) ^ ApplySingleSBox(j));
                    bool found = false;

                    foreach (var curDiff in result)
                    {
                        if (curDiff.InputDifferential == inputDiff && curDiff.OutputDifferential == outputDiff)
                        {
                            curDiff.Count++;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        result.Add(new Differential()
                        {
                            Count = 1,
                            InputDifferential = inputDiff,
                            OutputDifferential = outputDiff
                        });
                    }
                }
            }

            foreach (var curDiff in result)
            {
                curDiff.Probability = curDiff.Count / 16.0;
            }

            return result;
        }

        /// <summary>
        /// Implementation of depth-first search to find best characteristic for given SBoxes
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        public List<Characteristic>[] FindAllDifferentialsDepthSearch(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            ushort round = (ushort) roundConfiguration.Round;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);
            double increment = _maxProgress / (loopBorder - 1);

            DateTime startTime = DateTime.Now;

            SearchResult e = new SearchResult();
            e.activeSBoxes = roundConfiguration.ActiveSBoxes;
            e.round = roundConfiguration.Round;
            e.startTime = startTime;
            e.currentAlgorithm = Algorithms.Cipher3;
            e.result = null;

            if (AttackSearchResultOccured != null)
            {
                AttackSearchResultOccured.Invoke(this, e);
            }

            //result list
            List<Characteristic>[] resultList = new List<Characteristic>[loopBorder];

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 1.0) * _procMultiplier));
            //for(int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, po, i =>
            {
                ProgressEventArgs ev = new ProgressEventArgs()
                {
                    Increment = increment
                };

                ProgressChangedOccured.Invoke(this, ev);

                //expected difference
                ushort expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (ushort) i);

                bool skip = false;

                for (ushort j = 0; j < Cipher3Configuration.SBOXNUM; j++)
                {
                    if (roundConfiguration.ActiveSBoxes[j])
                    {
                        if (GetSubBlockFromBlock(expectedDifference, j) == 0)
                        {
                            skip = true;
                        }
                    }
                }

                if (skip)
                {
                    return;
                    //continue;
                }

                //start depth-first search
                List<Characteristic> retVal = FindCharacteristics(expectedDifference, round, differentialsList);

                resultList[i] = retVal;
            });

            if (Stop)
            {
                return null;
            }

            DateTime endTime = DateTime.Now;

            e = new SearchResult();
            e.activeSBoxes = roundConfiguration.ActiveSBoxes;
            e.round = roundConfiguration.Round;
            e.startTime = startTime;
            e.endTime = endTime;
            e.currentAlgorithm = Algorithms.Cipher3;

            for (int i = 0; i < resultList.Length; i++)
            {
                if (resultList[i] != null)
                {
                    foreach (var chara in resultList[i])
                    {
                        Cipher3CharacteristicUI data = new Cipher3CharacteristicUI()
                        {
                            InputDiffInt = chara.InputDifferentials[0],
                            InputDiff =
                                Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                            InputDiffR1Int = chara.InputDifferentials[0],
                            InputDiffR1 = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            OutputDiffR1Int = chara.OutputDifferentials[0],
                            OutputDiffR1 = Convert.ToString(chara.OutputDifferentials[0], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            InputDiffR2Int = chara.InputDifferentials[1],
                            InputDiffR2 = Convert.ToString(chara.InputDifferentials[1], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            OutputDiffR2Int = chara.OutputDifferentials[1],
                            OutputDiffR2 = Convert.ToString(chara.OutputDifferentials[1], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            InputDiffR3Int = chara.InputDifferentials[2],
                            InputDiffR3 = Convert.ToString(chara.InputDifferentials[2], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            OutputDiffR3Int = chara.OutputDifferentials[2],
                            OutputDiffR3 = Convert.ToString(chara.OutputDifferentials[2], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            InputDiffR4Int = chara.InputDifferentials[3],
                            InputDiffR4 = Convert.ToString(chara.InputDifferentials[3], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            OutputDiffR4Int = chara.OutputDifferentials[3],
                            OutputDiffR4 = Convert.ToString(chara.OutputDifferentials[3], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            InputDiffR5Int = chara.InputDifferentials[4],
                            InputDiffR5 = Convert.ToString(chara.InputDifferentials[4], 2).PadLeft(16, '0')
                                .Insert(8, " "),
                            Probability = string.Format("{0:0.000000}", chara.Probability)
                        };

                        e.result.Add(data);
                    }
                }
            }

            if (Stop)
            {
                return null;
            }

            if (AttackSearchResultOccured != null) AttackSearchResultOccured.Invoke(this, e);

            return resultList;
        }

        /// <summary>
        /// Implementation to find all characteristics with given expected difference
        /// </summary>
        /// <param name="expectedDiff"></param>
        /// <param name="round"></param>
        /// <param name="differentialNumList"></param>
        /// <returns></returns>
        private List<Characteristic> FindCharacteristics(ushort expectedDiff, ushort round,
            List<Differential> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new Cipher3Characteristic();
            inputObj.InputDifferentials[round] = expectedDiff;

            //calculate previous difference
            ushort outputDiffPreviousRound = ReversePBoxBlock(expectedDiff);

            //start depth-first search
            List<Characteristic> retVal =
                FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);

            if (Stop)
            {
                return null;
            }

            return retVal;
        }

        /// <summary>
        /// Implementation to find all characteristics with specified output difference to add characteristics to a differential
        /// </summary>
        /// <param name="round"></param>
        /// <param name="differentialsList"></param>
        /// <param name="outputDiff"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private List<Characteristic> FindAllCharacteristics(UInt16 round, List<Differential> differentialsList,
            UInt16 outputDiff, Characteristic res)
        {
            if (Stop)
            {
                return null;
            }

            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < Cipher3Configuration.PROBABILITYBOUNDDIFFERENTIALSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[Cipher3Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher3Configuration.SBOXNUM];
            for (UInt16 i = 0; i < Cipher3Configuration.SBOXNUM; i++)
            {
                outputDiffs[i] = GetSubBlockFromBlock(outputDiff, i);
                if (outputDiffs[i] > 0)
                {
                    activeSBoxes[i] = true;
                }
                else
                {
                    activeSBoxes[i] = false;
                }
            }

            //resultList
            List<Characteristic> diffList = new List<Characteristic>();

            //prepare the arrayOfDifferentialLists
            List<Differential>[] arrayOfDifferentialLists = new List<Differential>[Cipher3Configuration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < Cipher3Configuration.SBOXNUM; b++)
            {
                if (activeSBoxes[b])
                {
                    arrayOfDifferentialLists[b] = new List<Differential>(differentialsList.Count);
                    differentialsList.ForEach((item) =>
                    {
                        arrayOfDifferentialLists[b].Add((Differential) item.Clone());
                    });

                    List<Differential> diffsToRemove = new List<Differential>();
                    for (int j = 0; j < arrayOfDifferentialLists[b].Count; j++)
                    {
                        if (arrayOfDifferentialLists[b][j].OutputDifferential != outputDiffs[b])
                        {
                            diffsToRemove.Add(arrayOfDifferentialLists[b][j]);
                        }
                    }

                    foreach (var curDiff in diffsToRemove)
                    {
                        arrayOfDifferentialLists[b].Remove(curDiff);
                    }

                    comb *= arrayOfDifferentialLists[b].Count;
                }
                else
                {
                    arrayOfDifferentialLists[b] = new List<Differential>();
                }
            }

            for (int c = 0; c < comb; c++)
            {
                Differential[] curDiffSBoxes = new Differential[Cipher3Configuration.SBOXNUM];

                //calc indices
                int indexNo = 0;
                int j = c;
                while (j > 0)
                {
                    if (arrayOfDifferentialLists[indexNo].Count > 0)
                    {
                        int index = j % arrayOfDifferentialLists[indexNo].Count;
                        j = j / arrayOfDifferentialLists[indexNo].Count;
                        curDiffSBoxes[indexNo] = arrayOfDifferentialLists[indexNo][index];
                    }

                    indexNo++;
                }

                //zero case
                if (c == 0)
                {
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher3Configuration.SBOXNUM; z++)
                {
                    if (curDiffSBoxes[z] == null)
                    {
                        curDiffSBoxes[z] = new Differential()
                        {
                            Count = 0,
                            InputDifferential = 0,
                            OutputDifferential = 0,
                            Probability = -1
                        };
                    }
                }

                //calc conditions
                bool satisfied = true;
                for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                {
                    if (curDiffSBoxes[i].OutputDifferential != outputDiffs[i])
                    {
                        satisfied = false;
                    }
                }

                //check if conditions are satisfied
                if (!satisfied)
                {
                    continue;
                }

                //copy object
                Characteristic characteristic = res.Clone() as Characteristic;

                //calculate inputDifference
                int inputDiff = 0;
                for (int i = Cipher3Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher3Configuration.BITWIDTHCIPHER2;
                    }
                }

                //outputDifference for previous round
                UInt16 outputDiffPreviousRound = ReversePBoxBlock((UInt16) inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }

                        characteristic.Probability = characteristic.Probability * (curDiffSBoxes[i].Count / 16.0);
                    }
                }
                else
                {
                    double value = 1.0;
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }

                        value = value * (curDiffSBoxes[i].Count / 16.0);
                    }

                    if (characteristic != null) characteristic.Probability = value;
                }

                //store result
                if (characteristic != null)
                {
                    characteristic.InputDifferentials[round - 1] = (UInt16) inputDiff;
                    characteristic.OutputDifferentials[round - 1] = outputDiff;

                    //go one round deeper
                    List<Characteristic> retval = FindAllCharacteristics((UInt16) (round - 1), differentialsList,
                        outputDiffPreviousRound, characteristic);

                    //check if there is a result
                    if (retval != null)
                    {
                        diffList.AddRange(retval);
                    }
                }
            }

            //search for the best result
            Characteristic best = new Cipher3Characteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return diffList;
        }

        /// <summary>
        /// Implementation of depth-first search to find best characteristic for given SBoxes
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <param name="abortingPolicy"></param>
        /// <returns></returns>
        public List<Characteristic> FindBestCharacteristicsDepthSearch(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList,
            AbortingPolicy abortingPolicy)
        {
            //clean possible older result 
            if (abortingPolicy == AbortingPolicy.GlobalMaximum)
            {
                _currentGlobalMax = null;
            }

            DateTime startTime = DateTime.Now;

            SearchResult e = new SearchResult();
            e.activeSBoxes = roundConfiguration.ActiveSBoxes;
            e.round = roundConfiguration.Round;
            e.startTime = startTime;
            e.currentAlgorithm = Algorithms.Cipher2;
            e.result = null;

            if (AttackSearchResultOccured != null)
            {
                AttackSearchResultOccured.Invoke(this, e);
            }

            UInt16 round = (UInt16) roundConfiguration.Round;

            //Decrement round for recursive call
            round--;

            //result list
            List<Characteristic> resultList = new List<Characteristic>();

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            double increment = _maxProgress / 2 / (loopBorder - 1);

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 1.0) * _procMultiplier));

            //for (int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, po, i =>
            {

                Characteristic inputObj = new Cipher3Characteristic();

                ProgressEventArgs ev = new ProgressEventArgs()
                {
                    Increment = increment
                };

                ProgressChangedOccured.Invoke(this, ev);

                //expected difference
                UInt16 expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (UInt16) i);
                UInt16 outputDifferencePreviousRound = ReversePBoxBlock(expectedDifference);

                bool skip = false;

                for (ushort j = 0; j < Cipher3Configuration.SBOXNUM; j++)
                {
                    if (roundConfiguration.ActiveSBoxes[j])
                    {
                        if (GetSubBlockFromBlock(expectedDifference, j) == 0)
                        {
                            skip = true;
                        }
                    }
                }

                if (skip)
                {
                    return;
                    //continue;
                }

                inputObj.InputDifferentials[round] = expectedDifference;

                //start depth-first search
                Characteristic retVal = FindBestCharacteristic(round, differentialsList, outputDifferencePreviousRound,
                    inputObj, abortingPolicy);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retVal.Probability != -1)
                {
                    _semaphoreSlim.Wait();
                    try
                    {
                        if (abortingPolicy == AbortingPolicy.Threshold)
                        {
                            resultList.Add(retVal);
                        }
                        else
                        {
                            if (_currentGlobalMax == null)
                            {
                                _currentGlobalMax = retVal;
                            }
                            else
                            {
                                if (_currentGlobalMax.Probability < retVal.Probability)
                                {
                                    _currentGlobalMax = retVal;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
            });

            if (Stop)
            {
                return null;
            }

            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();

            e = new SearchResult();
            e.activeSBoxes = roundConfiguration.ActiveSBoxes;
            e.round = roundConfiguration.Round;
            e.startTime = startTime;
            e.currentAlgorithm = Algorithms.Cipher2;

            if (abortingPolicy == AbortingPolicy.Threshold)
            {
                foreach (var characteristic in sorted)
                {
                    Cipher3CharacteristicUI data = new Cipher3CharacteristicUI()
                    {
                        InputDiffInt = characteristic.InputDifferentials[0],
                        InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR1Int = characteristic.InputDifferentials[0],
                        InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR1Int = characteristic.OutputDifferentials[0],
                        OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR2Int = characteristic.InputDifferentials[1],
                        InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR2Int = characteristic.OutputDifferentials[1],
                        OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR3Int = characteristic.InputDifferentials[2],
                        InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR3Int = characteristic.OutputDifferentials[2],
                        OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR4Int = characteristic.InputDifferentials[3],
                        InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR4Int = characteristic.OutputDifferentials[3],
                        OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR5Int = characteristic.InputDifferentials[4],
                        InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        Probability = string.Format("{0:0.000000}", characteristic.Probability)
                    };
                    e.result.Add(data);
                }

                if (AttackSearchResultOccured != null) AttackSearchResultOccured.Invoke(this, e);

                return sorted;
            }
            else
            {
                sorted.Add(_currentGlobalMax);

                foreach (var characteristic in sorted)
                {
                    Cipher3CharacteristicUI data = new Cipher3CharacteristicUI()
                    {
                        InputDiffInt = characteristic.InputDifferentials[0],
                        InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR1Int = characteristic.InputDifferentials[0],
                        InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR1Int = characteristic.OutputDifferentials[0],
                        OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR2Int = characteristic.InputDifferentials[1],
                        InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR2Int = characteristic.OutputDifferentials[1],
                        OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR3Int = characteristic.InputDifferentials[2],
                        InputDiffR3 = Convert.ToString(characteristic.InputDifferentials[2], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR3Int = characteristic.OutputDifferentials[2],
                        OutputDiffR3 = Convert.ToString(characteristic.OutputDifferentials[2], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR4Int = characteristic.InputDifferentials[3],
                        InputDiffR4 = Convert.ToString(characteristic.InputDifferentials[3], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        OutputDiffR4Int = characteristic.OutputDifferentials[3],
                        OutputDiffR4 = Convert.ToString(characteristic.OutputDifferentials[3], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        InputDiffR5Int = characteristic.InputDifferentials[4],
                        InputDiffR5 = Convert.ToString(characteristic.InputDifferentials[4], 2).PadLeft(16, '0')
                            .Insert(8, " "),
                        Probability = string.Format("{0:0.000000}", characteristic.Probability)
                    };
                    e.result.Add(data);
                }

                if (AttackSearchResultOccured != null) AttackSearchResultOccured.Invoke(this, e);
                return sorted;
            }
        }

        /// <summary> 
        /// Recursive search for the best characteristic with given expected output difference
        /// </summary>
        /// <param name="round"></param>
        /// <param name="differentialsList"></param>
        /// <param name="outputDiff"></param>
        /// <param name="res"></param>
        /// <param name="abortingPolicy"></param>
        /// <returns></returns>
        public Characteristic FindBestCharacteristic(UInt16 round, List<Differential> differentialsList,
            UInt16 outputDiff, Characteristic res, AbortingPolicy abortingPolicy)
        {
            if (Stop)
            {
                return null;
            }

            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((abortingPolicy == AbortingPolicy.Threshold) &&
                (res.Probability < Cipher3Configuration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH) &&
                (res.Probability != -1))
                return null;

            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((abortingPolicy == AbortingPolicy.GlobalMaximum) && (_currentGlobalMax != null) &&
                (res.Probability != -1) && (_currentGlobalMax.Probability > res.Probability))
                return null;

            //end of rekursion
            if (round == 0)
                return res;

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[Cipher3Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher3Configuration.SBOXNUM];
            for (UInt16 i = 0; i < Cipher3Configuration.SBOXNUM; i++)
            {
                outputDiffs[i] = GetSubBlockFromBlock(outputDiff, i);
                if (outputDiffs[i] > 0)
                {
                    activeSBoxes[i] = true;
                }
                else
                {
                    activeSBoxes[i] = false;
                }
            }

            //resultList
            List<Characteristic> diffList = new List<Characteristic>();

            //prepare the arrayOfDifferentialLists
            List<Differential>[] arrayOfDifferentialLists = new List<Differential>[Cipher3Configuration.SBOXNUM];
            UInt16 comb = 1;
            for (UInt16 b = 0; b < Cipher3Configuration.SBOXNUM; b++)
            {
                if (activeSBoxes[b])
                {
                    arrayOfDifferentialLists[b] = new List<Differential>(differentialsList.Count);
                    differentialsList.ForEach((item) =>
                    {
                        arrayOfDifferentialLists[b].Add((Differential) item.Clone());
                    });

                    List<Differential> diffsToRemove = new List<Differential>();
                    for (UInt16 j = 0; j < arrayOfDifferentialLists[b].Count; j++)
                    {
                        if (arrayOfDifferentialLists[b][j].OutputDifferential != outputDiffs[b])
                        {
                            diffsToRemove.Add(arrayOfDifferentialLists[b][j]);
                        }
                    }

                    foreach (var curDiff in diffsToRemove)
                    {
                        arrayOfDifferentialLists[b].Remove(curDiff);
                    }

                    comb *= (UInt16) arrayOfDifferentialLists[b].Count;
                }
                else
                {
                    arrayOfDifferentialLists[b] = new List<Differential>();
                }
            }

            for (int c = 0; c < comb; c++)
            {
                Differential[] curDiffSBoxes = new Differential[Cipher3Configuration.SBOXNUM];

                //calc indices
                int indexNo = 0;
                int j = c;
                while (j > 0)
                {
                    if (arrayOfDifferentialLists[indexNo].Count > 0)
                    {
                        int index = j % arrayOfDifferentialLists[indexNo].Count;
                        j = j / arrayOfDifferentialLists[indexNo].Count;
                        curDiffSBoxes[indexNo] = arrayOfDifferentialLists[indexNo][index];
                    }

                    indexNo++;
                }

                //zero case
                if (c == 0)
                {
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher3Configuration.SBOXNUM; z++)
                {
                    if (curDiffSBoxes[z] == null)
                    {
                        curDiffSBoxes[z] = new Differential()
                        {
                            Count = 0,
                            InputDifferential = 0,
                            OutputDifferential = 0,
                            Probability = -1
                        };
                    }
                }

                //calc conditions
                bool satisfied = true;
                for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                {
                    if (curDiffSBoxes[i].OutputDifferential != outputDiffs[i])
                    {
                        satisfied = false;
                    }
                }

                //check if conditions are satisfied
                if (!satisfied)
                {
                    continue;
                }

                //copy object
                Characteristic characteristic = res.Clone() as Characteristic;

                //calculate inputDifference
                int inputDiff = 0;
                for (int i = Cipher3Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher3Configuration.BITWIDTHCIPHER2;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock((UInt16) inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }

                        characteristic.Probability = characteristic.Probability * (curDiffSBoxes[i].Count / 16.0);
                    }
                }
                else
                {
                    double value = 1.0;
                    for (int i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }

                        value = value * (curDiffSBoxes[i].Count / 16.0);
                    }

                    if (characteristic != null) characteristic.Probability = value;
                }

                //store result
                if (characteristic != null)
                {
                    characteristic.InputDifferentials[round - 1] = (UInt16) inputDiff;
                    characteristic.OutputDifferentials[round - 1] = outputDiff;

                    //go one round deeper
                    Characteristic retVal = FindBestCharacteristic((UInt16) (round - 1), differentialsList,
                        (UInt16) outputDiffPreviousRound, characteristic, abortingPolicy);

                    //check if there is a result
                    if (retVal != null)
                    {
                        diffList.Add(retVal);
                    }
                }
            }

            //search for the best result
            Characteristic best = new Cipher3Characteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return best;
        }

        public List<Characteristic> FindBestCharacteristicsHeuristic(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Search to find characteristics with specified input and output difference
        /// </summary>
        /// <param name="inputDiff"></param>
        /// <param name="outputDiff"></param>
        /// <param name="round"></param>
        /// <param name="differentialNumList"></param>
        /// <returns></returns>
        public List<Characteristic> FindSpecifiedDifferentialDepthSearch(UInt16 inputDiff, UInt16 outputDiff,
            UInt16 round, List<Differential> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new Cipher3Characteristic();

            //calculate previous difference
            UInt16 outputDiffPreviousRound = ReversePBoxBlock(outputDiff);

            //start depth-first search
            List<Characteristic> retVal =
                FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            retVal.RemoveAll(item => ((item.Probability == -1.0) || (item.InputDifferentials[0] != inputDiff)));

            if (Stop)
            {
                return null;
            }

            foreach (var curItem in retVal)
            {
                curItem.InputDifferentials[round] = outputDiff;
            }

            return retVal;
        }

        /// <summary>
        /// Generates a attack configuration
        /// </summary>
        /// <param name="round"></param>
        /// <param name="sBoxesToAttack"></param>
        /// <param name="abortingPolicy"></param>
        /// <param name="searchPolicy"></param>
        /// <param name="diffListOfSBox"></param>
        /// <returns></returns>
        public DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack,
            AbortingPolicy abortingPolicy, SearchPolicy searchPolicy, List<Differential> diffListOfSBox)
        {
            DifferentialAttackRoundConfiguration result = new DifferentialAttackRoundConfiguration
            {
                ActiveSBoxes = sBoxesToAttack,
                Round = round,
                AbortingPolicy = abortingPolicy,
                SearchPolicy = searchPolicy
            };

            int inputDifference = -1;
            int expectedDifference = -1;
            double probabilityAccumulated = 0.0;
            List<Characteristic> bestCharacteristics = new List<Characteristic>();

            switch (round)
            {
                case 5:
                {
                    result.IsLast = true;
                    result.IsBeforeLast = false;
                    result.IsFirst = false;
                }
                    break;
                case 4:
                {
                    result.IsLast = false;
                    result.IsBeforeLast = true;
                    result.IsFirst = false;
                }
                    break;
                case 3:
                {
                    result.IsLast = false;
                    result.IsBeforeLast = false;
                    result.IsFirst = false;
                }
                    break;
            }

            if (Stop)
            {
                return null;
            }

            switch (result.SearchPolicy)
            {
                case SearchPolicy.FirstAllCharacteristicsDepthSearch:
                {
                    //search for all differentials to find the best one on the given SBoxes
                    List<Characteristic>[] allCharacteristics = FindAllDifferentialsDepthSearch(result, diffListOfSBox);

                    if (Stop)
                    {
                        return null;
                    }

                    ParallelOptions po = new ParallelOptions();
                    Cts = new CancellationTokenSource();
                    po.CancellationToken = Cts.Token;
                    po.CancellationToken.ThrowIfCancellationRequested();
                    po.MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 1.0) * _procMultiplier));

                    //calculate the results to find the best differential
                    //for (int i = 1; i < allCharacteristics.Length; i++)
                    Parallel.For(1, allCharacteristics.Length, po, i =>
                    {
                        if (allCharacteristics[i] == null)
                        {
                            return;
                            //continue;
                        }

                        foreach (var characteristicToComp in allCharacteristics[i])
                        {
                            bool possible = true;

                            for (int j = 0; j < Cipher3Configuration.SBOXNUM; j++)
                            {
                                if (sBoxesToAttack[j])
                                {
                                    if (GetSubBlockFromBlock(characteristicToComp.InputDifferentials[round - 1],
                                            (ushort) j) == 0)
                                    {
                                        possible = false;
                                    }
                                }
                            }

                            if (!possible)
                            {
                                continue;
                            }

                            double roundProb = 0.0;
                            List<Characteristic> roundCharacteristics = new List<Characteristic>();

                            foreach (var characteristic in allCharacteristics[i])
                            {
                                if (characteristicToComp.InputDifferentials[0] == characteristic.InputDifferentials[0])
                                {
                                    roundProb += characteristic.Probability;
                                    roundCharacteristics.Add(characteristic);
                                }
                            }

                            _semaphoreSlim.Wait();
                            try
                            {
                                if (roundProb > probabilityAccumulated)
                                {
                                    probabilityAccumulated = roundProb;
                                    bestCharacteristics = roundCharacteristics;
                                }
                            }
                            finally
                            {
                                _semaphoreSlim.Release();
                            }
                        }
                    });

                    if (Stop)
                    {
                        return null;
                    }

                    inputDifference = bestCharacteristics[0].InputDifferentials[0];
                    expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];
                }
                    break;

                case SearchPolicy.FirstBestCharacteristicDepthSearch:
                {
                    SearchResult e = new SearchResult();

                    //search for THE best characteristic on the given SBoxes
                    List<Characteristic> characteristics =
                        FindBestCharacteristicsDepthSearch(result, diffListOfSBox, abortingPolicy);

                    if (Stop)
                    {
                        return null;
                    }

                    //Delete Characteristics which are not usable
                    List<Characteristic> toDelete = new List<Characteristic>();
                    foreach (var curCharacteristic in characteristics)
                    {
                        bool[] conditionArray = new bool[Cipher3Configuration.SBOXNUM];

                        for (ushort i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                        {
                            conditionArray[i] = true;

                            if (sBoxesToAttack[i])
                            {
                                if (GetSubBlockFromBlock(curCharacteristic.InputDifferentials[round - 1], i) == 0)
                                {
                                    conditionArray[i] = false;
                                }
                            }
                        }

                        for (ushort i = 0; i < Cipher3Configuration.SBOXNUM; i++)
                        {
                            if (conditionArray[i] == false)
                            {
                                toDelete.Add(curCharacteristic);
                            }
                        }
                    }

                    //delete unusable characteristics
                    foreach (var characteristicToDelete in toDelete)
                    {
                        characteristics.Remove(characteristicToDelete);
                    }

                        double inrement = _maxProgress / 2 / (characteristics.Count);

                        ParallelOptions po = new ParallelOptions();
                    Cts = new CancellationTokenSource();
                    po.CancellationToken = Cts.Token;
                    po.CancellationToken.ThrowIfCancellationRequested();
                    po.MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 1.0) * _procMultiplier));

                    //check for other useable characteristics
                    //foreach (Characteristic characteristic in characteristics)
                    Parallel.ForEach(characteristics, po, (characteristic) =>
                    {
                        List<Characteristic> differentialList =
                            FindSpecifiedDifferentialDepthSearch(characteristic.InputDifferentials[0],
                                characteristic.InputDifferentials[round - 1], (UInt16) round, diffListOfSBox);

                        if (differentialList == null || differentialList.Count == 0)
                        {
                            return;
                            //continue;
                        }

                        ProgressEventArgs ev = new ProgressEventArgs()
                        {
                            Increment = inrement
                        };

                        this.ProgressChangedOccured.Invoke(this, ev);

                        double testProbability = 0.0;

                        foreach (var curCharacteristic in differentialList)
                        {
                            testProbability += curCharacteristic.Probability;
                        }

                        _semaphoreSlim.Wait();
                        try
                        {
                            e = new SearchResult();
                            e.startTime = DateTime.MinValue;
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.currentAlgorithm = Algorithms.Cipher3;

                            foreach (var chara in differentialList)
                            {
                                if (characteristic.InputDifferentials[1] == chara.InputDifferentials[1] &&
                                    characteristic.InputDifferentials[2] == chara.InputDifferentials[2] &&
                                    characteristic.InputDifferentials[3] == chara.InputDifferentials[3] &&
                                    characteristic.InputDifferentials[4] == chara.InputDifferentials[4] &&
                                    characteristic.OutputDifferentials[0] == chara.OutputDifferentials[0] &&
                                    characteristic.OutputDifferentials[1] == chara.OutputDifferentials[1] &&
                                    characteristic.OutputDifferentials[2] == chara.OutputDifferentials[2] &&
                                    characteristic.OutputDifferentials[3] == chara.OutputDifferentials[3])
                                {
                                    continue;
                                }

                                Cipher3CharacteristicUI data = new Cipher3CharacteristicUI()
                                {
                                    InputDiffInt = chara.InputDifferentials[0],
                                    InputDiff = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR1Int = chara.InputDifferentials[0],
                                    InputDiffR1 = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR1Int = chara.OutputDifferentials[0],
                                    OutputDiffR1 = Convert.ToString(chara.OutputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR2Int = chara.InputDifferentials[1],
                                    InputDiffR2 = Convert.ToString(chara.InputDifferentials[1], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR2Int = chara.OutputDifferentials[1],
                                    OutputDiffR2 = Convert.ToString(chara.OutputDifferentials[1], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR3Int = chara.InputDifferentials[2],
                                    InputDiffR3 = Convert.ToString(chara.InputDifferentials[2], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR3Int = chara.OutputDifferentials[2],
                                    OutputDiffR3 = Convert.ToString(chara.OutputDifferentials[2], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR4Int = chara.InputDifferentials[3],
                                    InputDiffR4 = Convert.ToString(chara.InputDifferentials[3], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR4Int = chara.OutputDifferentials[3],
                                    OutputDiffR4 = Convert.ToString(chara.OutputDifferentials[3], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR5Int = chara.InputDifferentials[4],
                                    InputDiffR5 = Convert.ToString(chara.InputDifferentials[4], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    Probability = string.Format("{0:0.000000}", chara.Probability)
                                };
                                e.result.Add(data);
                            }

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }

                            //if(differentialList.Count > bestCharacteristics.Count)
                            if (testProbability > probabilityAccumulated)
                            {
                                probabilityAccumulated = testProbability;
                                bestCharacteristics = differentialList;
                            }
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    });

                    if (Stop)
                    {
                        return null;
                    }

                    inputDifference = bestCharacteristics[0].InputDifferentials[0];
                    expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];

                    DateTime endTime = DateTime.Now;
                    e = new SearchResult();
                    e.startTime = DateTime.MinValue;
                    e.activeSBoxes = result.ActiveSBoxes;
                    e.round = result.Round;
                    e.currentAlgorithm = Algorithms.Cipher3;
                    e.result = null;
                    e.endTime = endTime;

                    if (AttackSearchResultOccured != null)
                    {
                        AttackSearchResultOccured.Invoke(this, e);
                    }
                }
                    break;
                case SearchPolicy.FirstBestCharacteristicHeuristic:
                {
                    if (Stop)
                    {
                        return null;
                    }
                }
                    break;
            }

            if (Stop)
            {
                return null;
            }

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            result.Probability = probabilityAccumulated;

            return result;
        }

        /// <summary>
        /// Generates a value depending on active SBoxes
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort GenerateValue(bool[] activeSBoxes, ushort data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(32, false);

            UInt16 currentActiveBitPosition = 0;
            for (UInt16 i = 0; i < Cipher3Configuration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (UInt16 j = 0; j < Cipher3Configuration.BITWIDTHCIPHER2; j++, currentActiveBitPosition++)
                    {
                        result[(i * Cipher3Configuration.BITWIDTHCIPHER2) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[4];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToUInt16(bytesOfResult, 0);
        }

        /// <summary>
        /// returns the specified sub block from the input block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="subblockNum"></param>
        /// <returns></returns>
        public ushort GetSubBlockFromBlock(ushort block, ushort subblockNum)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(block));
            BitArray resultBits = new BitArray(4);

            switch (subblockNum)
            {
                case 0:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i];
                    }
                }
                    break;
                case 1:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 4];
                    }
                }
                    break;
                case 2:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 8];
                    }
                }
                    break;
                case 3:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 12];
                    }
                }
                    break;
            }

            byte[] resultBytes = new byte[4];
            resultBits.CopyTo(resultBytes, 0);

            ushort resultInt = BitConverter.ToUInt16(resultBytes, 0);
            return resultInt;
        }

        /// <summary>
        /// Reverses the permutation
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort ReversePBoxBlock(ushort data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher3Configuration.PBOXREVERSE[i]] = bitsOfBlock[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            ushort outputBlock = BitConverter.ToUInt16(bytes, 0);
            return outputBlock;
        }
    }
}
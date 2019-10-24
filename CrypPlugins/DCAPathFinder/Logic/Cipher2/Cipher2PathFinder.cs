﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DCAPathFinder.UI.Models;
using DCAPathFinder.UI.Tutorial2;
using DCAPathFinder.Util;
using Newtonsoft.Json;

namespace DCAPathFinder.Logic.Cipher2
{
    public class Cipher2PathFinder : IPathFinder
    {
        public event EventHandler<SearchResult> AttackSearchResultOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private Characteristic _currentGlobalMax = null;
        public bool Stop;
        public CancellationTokenSource Cts = new CancellationTokenSource();
        public int threadCount;
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
            zeroToThreeInt = Cipher2Configuration.SBOX[zeroToThreeInt];

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

            border = (int) Math.Pow(2, (border * Cipher2Configuration.BITWIDTHCIPHER2));
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
        /// Search for the best differential
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        public List<Characteristic>[] FindAllCharacteristicsDepthSearch(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            ushort round = (ushort) roundConfiguration.Round;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);
            double increment = _maxProgress / (loopBorder - 1);

            //result list
            List<Characteristic>[] resultList = new List<Characteristic>[loopBorder];

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = threadCount;

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

                for (ushort j = 0; j < Cipher2Configuration.SBOXNUM; j++)
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
                }

                //start depth-first search
                List<Characteristic> retVal = FindCharacteristics(expectedDifference, round, differentialsList);

                resultList[i] = retVal;
            });

            if (Stop)
            {
                return null;
            }

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
            Characteristic inputObj = new Cipher2Characteristic();
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

#if DEBUG
            Console.WriteLine("Found " + retVal.Count + " paths with expectedDifference = " + expectedDiff);
#endif

            return retVal;
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
            po.MaxDegreeOfParallelism = threadCount;

            for(int i = 1; i < loopBorder;i++)
            //Parallel.For(1, loopBorder, po, i =>
            {
                Characteristic inputObj = new Cipher2Characteristic();

                ProgressEventArgs ev = new ProgressEventArgs()
                {
                    Increment = increment
                };

                if (ProgressChangedOccured != null)
                {
                    ProgressChangedOccured.Invoke(this, ev);
                }

                //expected difference
                UInt16 expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (UInt16) i);
                UInt16 outputDifferencePreviousRound = ReversePBoxBlock(expectedDifference);

                bool skip = false;

                for (ushort j = 0; j < Cipher2Configuration.SBOXNUM; j++)
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
                    //return;
                    continue;
                }

                inputObj.InputDifferentials[round] = expectedDifference;

                //start depth-first search
                Characteristic retVal = FindBestCharacteristic(round, differentialsList, outputDifferencePreviousRound,
                    inputObj, abortingPolicy);

                if (Stop)
                {
                    //return;
                    continue;
                }

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
            }//);

            if (Stop)
            {
                return null;
            }

            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();

            if (abortingPolicy == AbortingPolicy.Threshold)
            {
                return sorted;
            }
            else
            {
                sorted.Add(_currentGlobalMax);
            }

            return sorted;
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
                (res.Probability < Cipher2Configuration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH) &&
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
            bool[] activeSBoxes = new bool[Cipher2Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher2Configuration.SBOXNUM];
            for (UInt16 i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
            List<Differential>[] arrayOfDifferentialLists = new List<Differential>[Cipher2Configuration.SBOXNUM];
            UInt16 comb = 1;
            for (UInt16 b = 0; b < Cipher2Configuration.SBOXNUM; b++)
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
                Differential[] curDiffSBoxes = new Differential[Cipher2Configuration.SBOXNUM];

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
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher2Configuration.SBOXNUM; z++)
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
                for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
                for (int i = Cipher2Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher2Configuration.BITWIDTHCIPHER2;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock((UInt16) inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
            Characteristic best = new Cipher2Characteristic();
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
            int round = roundConfiguration.Round;

            List<Characteristic> resultList = new List<Characteristic>();

            round--;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            double increment = _maxProgress / 2 / (loopBorder - 1);

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = threadCount;

            //for(int i = 1; i < loopBorder; i++)
            Parallel.For(1, loopBorder, po, i =>
            {
                Characteristic inputObj = new Cipher2Characteristic();

                //expected difference
                ushort expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (ushort) i);

                inputObj.InputDifferentials[round] = expectedDifference;
                inputObj.OutputDifferentials[round - 1] = ReversePBoxBlock(expectedDifference);

                Characteristic retVal = FindBestPredecessorDifference(round, inputObj, differentialsList);

                ProgressEventArgs ev = new ProgressEventArgs()
                {
                    Increment = increment
                };

                if (ProgressChangedOccured != null)
                {
                    ProgressChangedOccured.Invoke(this, ev);
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retVal.Probability != -1)
                {
                    _semaphoreSlim.Wait();
                    try
                    {
                        resultList.Add(retVal);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
            });

            //Sort by probability
            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();

            return sorted;
        }

        /// <summary>
        /// Searches for best predecessor difference from a given difference
        /// </summary>
        /// <param name="round"></param>
        /// <param name="inputCharacteristic"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        private Characteristic FindBestPredecessorDifference(int round, Characteristic inputCharacteristic,
            List<Differential> differentialsList)
        {
            //end of rekursion
            if (round == 0)
                return inputCharacteristic;

            //check active sboxes
            int zeroToThreeRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 0);
            int fourToSevenRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 1);
            int eightToElevenRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 2);
            int twelveToFifteenRoundOutput =
                GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 3);

            //resultList
            List<Characteristic> charList = new List<Characteristic>();

            //copy object
            Characteristic characteristic = inputCharacteristic.Clone() as Characteristic;

            double bestValueSBox4 = 0.0;
            double bestValueSBox3 = 0.0;
            double bestValueSBox2 = 0.0;
            double bestValueSBox1 = 0.0;

            double probabilityAccumulated = 1.0;

            int inputDiffSBox4 = 0;
            int inputDiffSBox3 = 0;
            int inputDiffSBox2 = 0;
            int inputDiffSBox1 = 0;

            //check if SBox4 is active
            if (zeroToThreeRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == zeroToThreeRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox4)
                        {
                            bestValueSBox4 = curDiff.Count / 16.0;
                            inputDiffSBox4 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox4 != 0)
            {
                probabilityAccumulated *= bestValueSBox4;
            }

            //check if SBox3 is active
            if (fourToSevenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == fourToSevenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox3)
                        {
                            bestValueSBox3 = curDiff.Count / 16.0;
                            inputDiffSBox3 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox3 != 0)
            {
                probabilityAccumulated *= bestValueSBox3;
            }

            //check if SBox2 is active
            if (eightToElevenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == eightToElevenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox2)
                        {
                            bestValueSBox2 = curDiff.Count / 16.0;
                            inputDiffSBox2 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox2 != 0)
            {
                probabilityAccumulated *= bestValueSBox2;
            }

            //check if SBox1 is active
            if (twelveToFifteenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == twelveToFifteenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox1)
                        {
                            bestValueSBox1 = curDiff.Count / 16.0;
                            inputDiffSBox1 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox1 != 0)
            {
                probabilityAccumulated *= bestValueSBox1;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (characteristic != null && characteristic.Probability != -1)
            {
                characteristic.Probability *= probabilityAccumulated;
            }
            else
            {
                if (characteristic != null) characteristic.Probability = probabilityAccumulated;
            }

            if (characteristic != null)
            {
                characteristic.InputDifferentials[round - 1] =
                    (ushort) BuildBlockFromPartialBlocks(inputDiffSBox1, inputDiffSBox2, inputDiffSBox3,
                        inputDiffSBox4);

                if (round - 2 >= 0)
                {
                    ushort t = ReversePBoxBlock(characteristic.InputDifferentials[round - 1]);
                    characteristic.OutputDifferentials[round - 2] = t;
                }

                Characteristic retVal = FindBestPredecessorDifference(round - 1, characteristic, differentialsList);

                charList.Add(retVal);
            }

            //search for the best result
            Characteristic best = new Cipher2Characteristic();
            foreach (var curChar in charList)
            {
                if (best.Probability < curChar.Probability)
                {
                    best = curChar;
                }
            }

            return best;
        }

        /// <summary>
        /// Search to find characteristics with specified input and output difference
        /// </summary>
        /// <param name="inputDiff"></param>
        /// <param name="outputDiff"></param>
        /// <param name="round"></param>
        /// <param name="differentialNumList"></param>
        /// <returns></returns>
        public List<Characteristic> FindSpecifiedCharacteristicsDepthSearch(UInt16 inputDiff, UInt16 outputDiff,
            UInt16 round, List<Differential> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new Cipher2Characteristic();

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
            if ((res.Probability < Cipher2Configuration.PROBABILITYBOUNDDIFFERENTIALSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[Cipher2Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher2Configuration.SBOXNUM];
            for (UInt16 i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
            List<Differential>[] arrayOfDifferentialLists = new List<Differential>[Cipher2Configuration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < Cipher2Configuration.SBOXNUM; b++)
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
                Differential[] curDiffSBoxes = new Differential[Cipher2Configuration.SBOXNUM];

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
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher2Configuration.SBOXNUM; z++)
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
                for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
                for (int i = Cipher2Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher2Configuration.BITWIDTHCIPHER2;
                    }
                }

                //outputDifference for previous round
                UInt16 outputDiffPreviousRound = ReversePBoxBlock((UInt16) inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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
            Characteristic best = new Cipher2Characteristic();
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
        /// returns a block build from the arguments pb3, pb2, pb1, pb1
        /// </summary>
        /// <param name="pb3"></param>
        /// <param name="pb2"></param>
        /// <param name="pb1"></param>
        /// <param name="pb0"></param>
        /// <returns></returns>
        public int BuildBlockFromPartialBlocks(int pb3, int pb2, int pb1, int pb0)
        {
            BitArray zeroToThree = new BitArray(BitConverter.GetBytes(pb0));
            BitArray fourToSeven = new BitArray(BitConverter.GetBytes(pb1));
            BitArray eightToEleven = new BitArray(BitConverter.GetBytes(pb2));
            BitArray twelveToFifteen = new BitArray(BitConverter.GetBytes(pb3));

            BitArray resultBits = new BitArray(16);

            for (int i = 0; i < 4; i++)
            {
                resultBits[i] = zeroToThree[i];
                resultBits[i + 4] = fourToSeven[i];
                resultBits[i + 8] = eightToEleven[i];
                resultBits[i + 12] = twelveToFifteen[i];
            }

            byte[] resultBytes = new byte[4];
            resultBits.CopyTo(resultBytes, 0);

            int resultInt = BitConverter.ToInt32(resultBytes, 0);
            return resultInt;
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
        public DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack, bool useOfflinePaths,
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
                case 3:
                {
                    result.IsLast = true;
                    result.IsBeforeLast = false;
                    result.IsFirst = false;
                }
                    break;
                case 2:
                {
                    result.IsLast = false;
                    result.IsBeforeLast = true;
                    result.IsFirst = false;
                }
                    break;
            }

            if (Stop)
            {
                return null;
            }

            if (!useOfflinePaths)
            {
                switch (result.SearchPolicy)
                {
                    case SearchPolicy.FirstAllCharacteristicsDepthSearch:
                        {
                            DateTime startTime = DateTime.Now;

                            SearchResult e = new SearchResult();
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.startTime = startTime;
                            e.currentAlgorithm = Algorithms.Cipher2;
                            e.result = null;

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }

                            //search for all differentials to find the best one on the given SBoxes
                            List<Characteristic>[] allCharacteristics = FindAllCharacteristicsDepthSearch(result, diffListOfSBox);

                            if (Stop)
                            {
                                return null;
                            }

                            ParallelOptions po = new ParallelOptions();
                            Cts = new CancellationTokenSource();
                            po.CancellationToken = Cts.Token;
                            po.CancellationToken.ThrowIfCancellationRequested();
                            po.MaxDegreeOfParallelism = threadCount;

                            //calculate the results to find the best differential
                            Parallel.For(1, allCharacteristics.Length, po, i =>
                            //for (int i = 1; i < AllCharacteristics.Length; i++)
                            {
                                if (allCharacteristics[i] == null)
                                {
                                    return;
                                }

                                foreach (var characteristicToComp in allCharacteristics[i])
                                {
                                    bool possible = true;

                                    for (int j = 0; j < Cipher2Configuration.SBOXNUM; j++)
                                    {
                                        if (sBoxesToAttack[j])
                                        {
                                            if (GetSubBlockFromBlock(characteristicToComp.InputDifferentials[round - 1],
                                                    (ushort)j) == 0)
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

                            if (bestCharacteristics != null && bestCharacteristics.Count > 0)
                            {
                                inputDifference = bestCharacteristics[0].InputDifferentials[0];
                                expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];
                            }

                            e = new SearchResult();
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.startTime = startTime;
                            e.currentAlgorithm = Algorithms.Cipher2;

                            for (int i = 0; i < allCharacteristics.Length; i++)
                            {
                                if (allCharacteristics[i] != null)
                                {
                                    foreach (var characteristic in allCharacteristics[i])
                                    {
                                        Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                                        {
                                            InputDiffInt = characteristic.InputDifferentials[0],
                                            InputDiff = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            InputDiffR1Int = characteristic.InputDifferentials[0],
                                            InputDiffR1 = Convert.ToString(characteristic.InputDifferentials[0], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            OutputDiffR1Int = characteristic.OutputDifferentials[0],
                                            OutputDiffR1 = Convert.ToString(characteristic.OutputDifferentials[0], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            InputDiffR2Int = characteristic.InputDifferentials[1],
                                            InputDiffR2 = Convert.ToString(characteristic.InputDifferentials[1], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            OutputDiffR2Int = characteristic.OutputDifferentials[1],
                                            OutputDiffR2 = Convert.ToString(characteristic.OutputDifferentials[1], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            ExpectedDiffInt = characteristic.InputDifferentials[2],
                                            ExpectedDiff = Convert.ToString(characteristic.InputDifferentials[2], 2)
                                                .PadLeft(16, '0')
                                                .Insert(8, " "),
                                            Probability = string.Format("{0:0.000000}", characteristic.Probability),
                                            ColBackgroundColor = "White"
                                        };

                                        foreach (var bestCharacteristic in bestCharacteristics)
                                        {
                                            if (characteristic.InputDifferentials[0] ==
                                                bestCharacteristic.InputDifferentials[0] &&
                                                characteristic.InputDifferentials[1] ==
                                                bestCharacteristic.InputDifferentials[1] &&
                                                characteristic.InputDifferentials[2] ==
                                                bestCharacteristic.InputDifferentials[2] &&
                                                characteristic.OutputDifferentials[0] ==
                                                bestCharacteristic.OutputDifferentials[0] &&
                                                characteristic.OutputDifferentials[1] ==
                                                bestCharacteristic.OutputDifferentials[1])
                                            {
                                                data.ColBackgroundColor = "LimeGreen";
                                                break;
                                            }
                                        }

                                        e.result.Add(data);
                                    }
                                }
                            }

                            DateTime endTime = DateTime.Now;
                            e.endTime = endTime;

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }
                        }
                        break;

                    case SearchPolicy.FirstBestCharacteristicDepthSearch:
                        {
                            DateTime startTime = DateTime.Now;

                            SearchResult e = new SearchResult();
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.startTime = startTime;
                            e.currentAlgorithm = Algorithms.Cipher2;
                            e.result = null;

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }

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
                                bool[] conditionArray = new bool[Cipher2Configuration.SBOXNUM];

                                for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                                for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                            double increment = _maxProgress / 2 / (characteristics.Count);

                            ParallelOptions po = new ParallelOptions();
                            Cts = new CancellationTokenSource();
                            po.CancellationToken = Cts.Token;
                            po.CancellationToken.ThrowIfCancellationRequested();
                            po.MaxDegreeOfParallelism = threadCount;

                            List<Characteristic> allFoundCharacteristics = new List<Characteristic>();
                            allFoundCharacteristics.AddRange(characteristics);

                            //check for other useable characteristics
                            //foreach (Characteristic characteristic in characteristics)
                            Parallel.ForEach(characteristics, po, (characteristic) =>
                            {
                                List<Characteristic> differentialList =
                                    FindSpecifiedCharacteristicsDepthSearch(characteristic.InputDifferentials[0],
                                        characteristic.InputDifferentials[round - 1], (UInt16)round, diffListOfSBox);

                                if (differentialList == null)
                                {
                                    return;
                                    //continue;
                                }

                                if (differentialList.Count == 0)
                                {
                                    differentialList.Add(characteristic);
                                }

                                ProgressEventArgs ev = new ProgressEventArgs()
                                {
                                    Increment = increment
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
                                    List<Characteristic> toAdd = new List<Characteristic>();

                                    foreach (var curProbNewChar in differentialList)
                                    {
                                        bool found = false;
                                        foreach (var curChar in allFoundCharacteristics)
                                        {
                                            if (curChar.InputDifferentials[1] == curProbNewChar.InputDifferentials[1] &&
                                                curChar.InputDifferentials[2] == curProbNewChar.InputDifferentials[2] &&
                                                curChar.OutputDifferentials[0] == curProbNewChar.OutputDifferentials[0] &&
                                                curChar.OutputDifferentials[1] == curProbNewChar.OutputDifferentials[1])
                                            {
                                                found = true;
                                            }
                                        }

                                        if (!found)
                                        {
                                            toAdd.Add(curProbNewChar);
                                        }
                                    }

                                    allFoundCharacteristics.AddRange(toAdd);

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

                            if (bestCharacteristics != null && bestCharacteristics.Count > 0)
                            {
                                inputDifference = bestCharacteristics[0].InputDifferentials[0];
                                expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];
                            }

                            DateTime endTime = DateTime.Now;
                            e = new SearchResult();
                            e.startTime = DateTime.MinValue;
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.currentAlgorithm = Algorithms.Cipher2;
                            e.result = new List<CharacteristicUI>();
                            e.endTime = endTime;

                            foreach (var chara in allFoundCharacteristics)
                            {
                                Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                                {
                                    InputDiffInt = chara.InputDifferentials[0],
                                    InputDiff = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR1Int = chara.InputDifferentials[0],
                                    InputDiffR1 = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR1Int = chara.OutputDifferentials[0],
                                    OutputDiffR1 = Convert.ToString(chara.OutputDifferentials[0], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR2Int = chara.InputDifferentials[1],
                                    InputDiffR2 = Convert.ToString(chara.InputDifferentials[1], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR2Int = chara.OutputDifferentials[1],
                                    OutputDiffR2 = Convert.ToString(chara.OutputDifferentials[1], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    ExpectedDiffInt = chara.InputDifferentials[2],
                                    ExpectedDiff = Convert.ToString(chara.InputDifferentials[2], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    Probability = string.Format("{0:0.000000}", chara.Probability),
                                    ColBackgroundColor = "White"
                                };

                                foreach (var bestCharacteristic in bestCharacteristics)
                                {
                                    if (chara.InputDifferentials[0] == bestCharacteristic.InputDifferentials[0] &&
                                        chara.InputDifferentials[1] == bestCharacteristic.InputDifferentials[1] &&
                                        chara.InputDifferentials[2] == bestCharacteristic.InputDifferentials[2] &&
                                        chara.OutputDifferentials[0] == bestCharacteristic.OutputDifferentials[0] &&
                                        chara.OutputDifferentials[1] == bestCharacteristic.OutputDifferentials[1])
                                    {
                                        data.ColBackgroundColor = "LimeGreen";
                                        break;
                                    }
                                }

                                e.result.Add(data);
                            }

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

                            DateTime startTime = DateTime.Now;

                            SearchResult e = new SearchResult();
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.startTime = startTime;
                            e.currentAlgorithm = Algorithms.Cipher2;
                            e.result = null;

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }

                            List<Characteristic> characteristics = FindBestCharacteristicsHeuristic(result, diffListOfSBox);

                            //Delete Characteristics which are not usable
                            List<Characteristic> toDelete = new List<Characteristic>();
                            foreach (var curCharacteristic in characteristics)
                            {
                                bool[] conditionArray = new bool[Cipher2Configuration.SBOXNUM];

                                for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                                for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                            List<Characteristic> allFoundCharacteristics = new List<Characteristic>();
                            allFoundCharacteristics.AddRange(characteristics);

                            double inrement = _maxProgress / 2 / (characteristics.Count);

                            //check for other useable characteristics
                            //foreach (Characteristic characteristic in characteristics)
                            Parallel.ForEach(characteristics, (characteristic) =>
                            {
                                List<Characteristic> differentialList =
                                    FindSpecifiedCharacteristicsDepthSearch(characteristic.InputDifferentials[0],
                                        characteristic.InputDifferentials[round - 1], (ushort)round, diffListOfSBox);

                                ProgressEventArgs ev = new ProgressEventArgs()
                                {
                                    Increment = inrement
                                };

                                if (ProgressChangedOccured != null)
                                {
                                    ProgressChangedOccured.Invoke(this, ev);
                                }

                                if (differentialList.Count == 0)
                                {
                                    return;
                                }

                                double testProbability = 0.0;

                                foreach (var curCharacteristic in differentialList)
                                {
                                    testProbability += curCharacteristic.Probability;
                                }

                                _semaphoreSlim.Wait();
                                try
                                {
                                    List<Characteristic> toAdd = new List<Characteristic>();
                                    foreach (var curProbNewChar in differentialList)
                                    {
                                        bool found = false;
                                        foreach (var curChar in allFoundCharacteristics)
                                        {
                                            if (curChar.InputDifferentials[0] == curProbNewChar.InputDifferentials[0] &&
                                                curChar.InputDifferentials[1] == curProbNewChar.InputDifferentials[1] &&
                                                curChar.InputDifferentials[2] == curProbNewChar.InputDifferentials[2] &&
                                                curChar.OutputDifferentials[0] == curProbNewChar.OutputDifferentials[0] &&
                                                curChar.OutputDifferentials[1] == curProbNewChar.OutputDifferentials[1])
                                            {
                                                found = true;
                                            }
                                        }

                                        if (!found)
                                        {
                                            toAdd.Add(curProbNewChar);
                                        }
                                    }

                                    allFoundCharacteristics.AddRange(toAdd);

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

                            if (bestCharacteristics != null && bestCharacteristics.Count > 0)
                            {
                                inputDifference = bestCharacteristics[0].InputDifferentials[0];
                                expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];
                            }
                            else if (characteristics != null && characteristics.Count > 0)
                            {
                                Characteristic best = characteristics.OrderByDescending(curElement => curElement.Probability).ToList()[0];
                                bestCharacteristics.Add(best);
                                inputDifference = best.InputDifferentials[0];
                                expectedDifference = best.InputDifferentials[round - 1];
                                probabilityAccumulated = best.Probability;
                            }

                            DateTime endTime = DateTime.Now;
                            e = new SearchResult();
                            e.startTime = DateTime.MinValue;
                            e.activeSBoxes = result.ActiveSBoxes;
                            e.round = result.Round;
                            e.currentAlgorithm = Algorithms.Cipher2;
                            e.result = new List<CharacteristicUI>();
                            e.endTime = endTime;

                            foreach (var chara in allFoundCharacteristics)
                            {
                                Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                                {
                                    InputDiffInt = chara.InputDifferentials[0],
                                    InputDiff = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR1Int = chara.InputDifferentials[0],
                                    InputDiffR1 = Convert.ToString(chara.InputDifferentials[0], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR1Int = chara.OutputDifferentials[0],
                                    OutputDiffR1 = Convert.ToString(chara.OutputDifferentials[0], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    InputDiffR2Int = chara.InputDifferentials[1],
                                    InputDiffR2 = Convert.ToString(chara.InputDifferentials[1], 2).PadLeft(16, '0')
                                        .Insert(8, " "),
                                    OutputDiffR2Int = chara.OutputDifferentials[1],
                                    OutputDiffR2 = Convert.ToString(chara.OutputDifferentials[1], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    ExpectedDiffInt = chara.InputDifferentials[2],
                                    ExpectedDiff = Convert.ToString(chara.InputDifferentials[2], 2)
                                        .PadLeft(16, '0')
                                        .Insert(8, " "),
                                    Probability = string.Format("{0:0.000000}", chara.Probability),
                                    ColBackgroundColor = "White"
                                };

                                foreach (var bestCharacteristic in bestCharacteristics)
                                {
                                    if (chara.InputDifferentials[0] == bestCharacteristic.InputDifferentials[0] &&
                                        chara.InputDifferentials[1] == bestCharacteristic.InputDifferentials[1] &&
                                        chara.InputDifferentials[2] == bestCharacteristic.InputDifferentials[2] &&
                                        chara.OutputDifferentials[0] == bestCharacteristic.OutputDifferentials[0] &&
                                        chara.OutputDifferentials[1] == bestCharacteristic.OutputDifferentials[1])
                                    {
                                        data.ColBackgroundColor = "LimeGreen";
                                        break;
                                    }
                                }

                                e.result.Add(data);
                            }

                            if (AttackSearchResultOccured != null)
                            {
                                AttackSearchResultOccured.Invoke(this, e);
                            }
                        }
                        break;
                }
            }
            else
            {
                switch (result.SearchPolicy)
                {
                    case SearchPolicy.FirstAllCharacteristicsDepthSearch:
                        string resName = "Cipher2_AllCharacteristics_R" + round + "_SBoxes" + Helper.BoolArrayToString(sBoxesToAttack) + "_Reduced.json";
                        result = Helper.LoadConfigurationFromDisk(resName);

                        DateTime tTime = DateTime.Now;
                        SearchResult e = new SearchResult();
                        e.activeSBoxes = result.ActiveSBoxes;
                        e.round = result.Round;
                        e.startTime = tTime;
                        e.endTime = tTime;
                        e.currentAlgorithm = Algorithms.Cipher2;

                        foreach (var curCharacteristic in result.Characteristics)
                        {
                            Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                            {
                                InputDiffInt = curCharacteristic.InputDifferentials[0],
                                InputDiff = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR1Int = curCharacteristic.InputDifferentials[0],
                                InputDiffR1 = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR1Int = curCharacteristic.OutputDifferentials[0],
                                OutputDiffR1 = Convert.ToString(curCharacteristic.OutputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR2Int = curCharacteristic.InputDifferentials[1],
                                InputDiffR2 = Convert.ToString(curCharacteristic.InputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR2Int = curCharacteristic.OutputDifferentials[1],
                                OutputDiffR2 = Convert.ToString(curCharacteristic.OutputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                ExpectedDiffInt = curCharacteristic.InputDifferentials[2],
                                ExpectedDiff = Convert.ToString(curCharacteristic.InputDifferentials[2], 2).PadLeft(16, '0').Insert(8, " "),
                                Probability = string.Format("{0:0.000000}", curCharacteristic.Probability),
                                ColBackgroundColor = "LimeGreen"
                            };

                            e.result.Add(data);
                        }

                        DateTime endTime = DateTime.Now;
                        e.endTime = endTime;

                        if (AttackSearchResultOccured != null)
                        {
                            AttackSearchResultOccured.Invoke(this, e);
                        }

                        break;
                    case SearchPolicy.FirstBestCharacteristicDepthSearch:

                        //check aborting policy
                        if (abortingPolicy == AbortingPolicy.Threshold)
                        {
                            resName = "Cipher2_BestCharacteristicDepthSearch_globalThreshold_R" + round + "_SBoxes" + Helper.BoolArrayToString(sBoxesToAttack) + "_Reduced.json";
                            result = Helper.LoadConfigurationFromDisk(resName);
                        }
                        else
                        {
                            resName = "Cipher2_BestCharacteristicDepthSearch_globalMaximum_R" + round + "_SBoxes" + Helper.BoolArrayToString(sBoxesToAttack) + "_Reduced.json";
                            result = Helper.LoadConfigurationFromDisk(resName);
                        }
                        
                        tTime = DateTime.Now;
                        e = new SearchResult();
                        e.activeSBoxes = result.ActiveSBoxes;
                        e.round = result.Round;
                        e.startTime = tTime;
                        e.endTime = tTime;
                        e.currentAlgorithm = Algorithms.Cipher2;

                        foreach (var curCharacteristic in result.Characteristics)
                        {
                            Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                            {
                                InputDiffInt = curCharacteristic.InputDifferentials[0],
                                InputDiff = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR1Int = curCharacteristic.InputDifferentials[0],
                                InputDiffR1 = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR1Int = curCharacteristic.OutputDifferentials[0],
                                OutputDiffR1 = Convert.ToString(curCharacteristic.OutputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR2Int = curCharacteristic.InputDifferentials[1],
                                InputDiffR2 = Convert.ToString(curCharacteristic.InputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR2Int = curCharacteristic.OutputDifferentials[1],
                                OutputDiffR2 = Convert.ToString(curCharacteristic.OutputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                ExpectedDiffInt = curCharacteristic.InputDifferentials[2],
                                ExpectedDiff = Convert.ToString(curCharacteristic.InputDifferentials[2], 2).PadLeft(16, '0').Insert(8, " "),
                                Probability = string.Format("{0:0.000000}", curCharacteristic.Probability),
                                ColBackgroundColor = "LimeGreen"
                            };

                            e.result.Add(data);
                        }

                        endTime = DateTime.Now;
                        e.endTime = endTime;

                        if (AttackSearchResultOccured != null)
                        {
                            AttackSearchResultOccured.Invoke(this, e);
                        }

                        break;
                    case SearchPolicy.FirstBestCharacteristicHeuristic:

                        resName = "Cipher2_BestCharacteristicHeuristic_R" + round + "_SBoxes" + Helper.BoolArrayToString(sBoxesToAttack) + "_Reduced.json";
                        result = Helper.LoadConfigurationFromDisk(resName);

                        tTime = DateTime.Now;
                        e = new SearchResult();
                        e.activeSBoxes = result.ActiveSBoxes;
                        e.round = result.Round;
                        e.startTime = tTime;
                        e.endTime = tTime;
                        e.currentAlgorithm = Algorithms.Cipher2;

                        foreach (var curCharacteristic in result.Characteristics)
                        {
                            Cipher2CharacteristicUI data = new Cipher2CharacteristicUI()
                            {
                                InputDiffInt = curCharacteristic.InputDifferentials[0],
                                InputDiff = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR1Int = curCharacteristic.InputDifferentials[0],
                                InputDiffR1 = Convert.ToString(curCharacteristic.InputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR1Int = curCharacteristic.OutputDifferentials[0],
                                OutputDiffR1 = Convert.ToString(curCharacteristic.OutputDifferentials[0], 2).PadLeft(16, '0').Insert(8, " "),
                                InputDiffR2Int = curCharacteristic.InputDifferentials[1],
                                InputDiffR2 = Convert.ToString(curCharacteristic.InputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                OutputDiffR2Int = curCharacteristic.OutputDifferentials[1],
                                OutputDiffR2 = Convert.ToString(curCharacteristic.OutputDifferentials[1], 2).PadLeft(16, '0').Insert(8, " "),
                                ExpectedDiffInt = curCharacteristic.InputDifferentials[2],
                                ExpectedDiff = Convert.ToString(curCharacteristic.InputDifferentials[2], 2).PadLeft(16, '0').Insert(8, " "),
                                Probability = string.Format("{0:0.000000}", curCharacteristic.Probability),
                                ColBackgroundColor = "LimeGreen"
                            };

                            e.result.Add(data);
                        }

                        endTime = DateTime.Now;
                        e.endTime = endTime;

                        if (AttackSearchResultOccured != null)
                        {
                            AttackSearchResultOccured.Invoke(this, e);
                        }

                        break;
                }

                //exit method with deserialized result 
                return result;
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
            for (UInt16 i = 0; i < Cipher2Configuration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (UInt16 j = 0; j < Cipher2Configuration.BITWIDTHCIPHER2; j++, currentActiveBitPosition++)
                    {
                        result[(i * Cipher2Configuration.BITWIDTHCIPHER2) + j] = bitsOfValue[currentActiveBitPosition];
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
                pboxedArray[Cipher2Configuration.PBOXREVERSE[i]] = bitsOfBlock[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            ushort outputBlock = BitConverter.ToUInt16(bytes, 0);
            return outputBlock;
        }

        //Methoden zur offline Speicherung 15.08.2019

        public DifferentialAttackRoundConfiguration GenerateOfflineConfiguration(int round, bool[] sBoxesToAttack, List<Differential> diffListOfSBox, AbortingPolicy abPol)
        {
            DifferentialAttackRoundConfiguration result = new DifferentialAttackRoundConfiguration
            {
                ActiveSBoxes = sBoxesToAttack,
                Round = round,
                AbortingPolicy = abPol,
                SearchPolicy = SearchPolicy.FirstBestCharacteristicHeuristic
            };

            int inputDifference = -1;
            int expectedDifference = -1;
            double probabilityAccumulated = 0.0;
            List<Characteristic> bestCharacteristics = new List<Characteristic>();

            switch (round)
            {
                case 3:
                {
                    result.IsLast = true;
                    result.IsBeforeLast = false;
                    result.IsFirst = false;
                }
                    break;
                case 2:
                {
                    result.IsLast = false;
                    result.IsBeforeLast = true;
                    result.IsFirst = false;
                }
                    break;
            }

            if(result.SearchPolicy == SearchPolicy.FirstAllCharacteristicsDepthSearch)
            {
                //search for all differentials to find the best one on the given SBoxes
                List<Characteristic>[] allCharacteristics = FindAllDifferentialsDepthSearchOffline(result, diffListOfSBox);

                ParallelOptions po = new ParallelOptions();
                Cts = new CancellationTokenSource();
                po.CancellationToken = Cts.Token;
                po.CancellationToken.ThrowIfCancellationRequested();
                po.MaxDegreeOfParallelism = threadCount;

                //calculate the results to find the best differential
                Parallel.For(1, allCharacteristics.Length, po, i =>
                //for (int i = 1; i < AllCharacteristics.Length; i++)
                {
                    if (allCharacteristics[i] == null)
                    {
                        return;
                    }

                    foreach (var characteristicToComp in allCharacteristics[i])
                    {
                        bool possible = true;

                        for (int j = 0; j < Cipher2Configuration.SBOXNUM; j++)
                        {
                            if (sBoxesToAttack[j])
                            {
                                if (GetSubBlockFromBlock(characteristicToComp.InputDifferentials[round - 1],
                                        (ushort)j) == 0)
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
            }
            else if(result.SearchPolicy == SearchPolicy.FirstBestCharacteristicDepthSearch)
            {
                //search for THE best characteristic on the given SBoxes
                List<Characteristic> characteristics =
                    FindBestCharacteristicsDepthSearchOffline(result, diffListOfSBox, result.AbortingPolicy);

                //Delete Characteristics which are not usable
                List<Characteristic> toDelete = new List<Characteristic>();
                foreach (var curCharacteristic in characteristics)
                {
                    bool[] conditionArray = new bool[Cipher2Configuration.SBOXNUM];

                    for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                    for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                ParallelOptions po = new ParallelOptions();
                Cts = new CancellationTokenSource();
                po.CancellationToken = Cts.Token;
                po.CancellationToken.ThrowIfCancellationRequested();
                po.MaxDegreeOfParallelism = threadCount;

                //check for other useable characteristics
                //foreach (Characteristic characteristic in characteristics)
                Parallel.ForEach(characteristics, po, (characteristic) =>
                {
                    List<Characteristic> differentialList =
                        FindSpecifiedCharacteristicsDepthSearch(characteristic.InputDifferentials[0],
                            characteristic.InputDifferentials[round - 1], (UInt16)round, diffListOfSBox);

                    if (differentialList == null || differentialList.Count == 0)
                    {
                        return;
                        //continue;
                    }

                    double testProbability = 0.0;

                    foreach (var curCharacteristic in differentialList)
                    {
                        testProbability += curCharacteristic.Probability;
                    }

                    _semaphoreSlim.Wait();
                    try
                    {
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
            }else if (result.SearchPolicy ==SearchPolicy.FirstBestCharacteristicHeuristic)
            {
                List<Characteristic> characteristics = FindBestCharacteristicsHeuristicOffline(result, diffListOfSBox);

                //Delete Characteristics which are not usable
                List<Characteristic> toDelete = new List<Characteristic>();
                foreach (var curCharacteristic in characteristics)
                {
                    bool[] conditionArray = new bool[Cipher2Configuration.SBOXNUM];

                    for (ushort i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                    for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
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

                //check for other useable characteristics
                //foreach (Characteristic characteristic in characteristics)
                Parallel.ForEach(characteristics, (characteristic) =>
                {
                    List<Characteristic> differentialList =
                        FindSpecifiedCharacteristicsDepthSearch(characteristic.InputDifferentials[0],
                            characteristic.InputDifferentials[round - 1], (ushort)round, diffListOfSBox);

                    if (differentialList.Count == 0)
                    {
                        return;
                    }

                    double testProbability = 0.0;

                    foreach (var curCharacteristic in differentialList)
                    {
                        testProbability += curCharacteristic.Probability;
                    }

                    _semaphoreSlim.Wait();
                    try
                    {
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
            }

            inputDifference = bestCharacteristics[0].InputDifferentials[0];
            expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            result.Probability = probabilityAccumulated;

            return result;
        }

        public List<Characteristic>[] FindAllDifferentialsDepthSearchOffline(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            ushort round = (ushort) roundConfiguration.Round;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //result list
            List<Characteristic>[] resultList = new List<Characteristic>[loopBorder];

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = threadCount;

            //for(int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, po, i =>
            {
                //expected difference
                ushort expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (ushort) i);

                bool skip = false;

                for (ushort j = 0; j < Cipher2Configuration.SBOXNUM; j++)
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
                }

                //start depth-first search
                List<Characteristic> retVal = FindCharacteristics(expectedDifference, round, differentialsList);

                resultList[i] = retVal;
            });

            return resultList;
        }

        public List<Characteristic> FindBestCharacteristicsDepthSearchOffline(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList,
            AbortingPolicy abortingPolicy)
        {
            //clean possible older result 
            if (abortingPolicy == AbortingPolicy.GlobalMaximum)
            {
                _currentGlobalMax = null;
            }

            UInt16 round = (UInt16)roundConfiguration.Round;

            //Decrement round for recursive call
            round--;

            //result list
            List<Characteristic> resultList = new List<Characteristic>();

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = threadCount;

            //for(int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, po, i =>
            {
                Characteristic inputObj = new Cipher2Characteristic();

                //expected difference
                UInt16 expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (UInt16)i);
                UInt16 outputDifferencePreviousRound = ReversePBoxBlock(expectedDifference);

                bool skip = false;

                for (ushort j = 0; j < Cipher2Configuration.SBOXNUM; j++)
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
                }

                inputObj.InputDifferentials[round] = expectedDifference;

                //start depth-first search
                Characteristic retVal = FindBestCharacteristic(round, differentialsList, outputDifferencePreviousRound,
                    inputObj, abortingPolicy);

                if (Stop)
                {
                    return;
                }

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

            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();

            if (abortingPolicy == AbortingPolicy.Threshold)
            {
                return sorted;
            }
            else
            {
                sorted.Add(_currentGlobalMax);
            }

            return sorted;
        }

        public List<Characteristic> FindBestCharacteristicsHeuristicOffline(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            int round = roundConfiguration.Round;

            List<Characteristic> resultList = new List<Characteristic>();

            round--;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //for(int i = 1; i < loopBorder; i++)
            Parallel.For(1, loopBorder, i =>
            {
                Characteristic inputObj = new Cipher2Characteristic();

                //expected difference
                ushort expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, (ushort)i);

                inputObj.InputDifferentials[round] = expectedDifference;
                inputObj.OutputDifferentials[round - 1] = ReversePBoxBlock(expectedDifference);

                Characteristic retVal = FindBestPredecessorDifference(round, inputObj, differentialsList);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retVal.Probability != -1)
                {
                    _semaphoreSlim.Wait();
                    try
                    {
                        resultList.Add(retVal);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
            });

            //Sort by probability
            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();

            return sorted;
        }




    }
}
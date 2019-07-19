﻿/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DCAKeyRecovery.UI.Models;

namespace DCAKeyRecovery.Logic.Cipher2
{
    class Cipher2KeyRecovery : IKeyRecovery
    {
        public event EventHandler<EventArgs> NeedMessagePairOccured;
        public event EventHandler<ResultViewLastRoundRoundResultEventArgs> ResultViewRefreshRoundFinishedOccured;
        public event EventHandler<ResultViewLastRoundEventArgs> LastRoundResultViewRefreshOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;
        public event EventHandler<ResultViewAnyRoundEventArgs> AnyRoundResultViewRefreshOccured;
        public event EventHandler<ResultViewAnyRoundKeyResultEventArgs> AnyRoundResultViewKeyResultsRefreshOccured;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public AutoResetEvent DataReceivedEvent;
        private List<Pair> _plainTextPairList;
        private List<Pair> _cipherTextPairList;
        private int usedPairCount = 0;
        public bool stop;
        public CancellationTokenSource Cts = new CancellationTokenSource();
        private int _procMultiplier = 2;

        /*
        public AutoResetEvent SkipStepEvent;
        public AutoResetEvent SkipMessageEvent;
        public AutoResetEvent SkipKeyEvent;
        */

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2KeyRecovery()
        {
            stop = false;
            _plainTextPairList = new List<Pair>();
            _cipherTextPairList = new List<Pair>();
            DataReceivedEvent = new AutoResetEvent(false);

            /*
            SkipStepEvent = new AutoResetEvent(false);
            SkipMessageEvent = new AutoResetEvent(false);
            SkipKeyEvent = new AutoResetEvent(false);
            */
        }

        /// <summary>
        /// Adds new pairs for attacking first round
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="cipherText"></param>
        public void AddNewPairs(Pair plaintext, Pair cipherText)
        {
            usedPairCount++;
            usedPairCount++;
            _plainTextPairList.Add(plaintext);
            _cipherTextPairList.Add(cipherText);
            DataReceivedEvent.Set();
        }

        /// <summary>
        /// Attacks last round to get k0 and k1
        /// </summary>
        /// <param name="attack"></param>
        /// <returns></returns>
        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack)
        {
            Cipher2DifferentialKeyRecoveryAttack cipher2Attack = attack as Cipher2DifferentialKeyRecoveryAttack;
            DifferentialAttackLastRoundResult result = new DifferentialAttackLastRoundResult();
            ResultViewLastRoundEventArgs lastRoundEventArgsIterationResultViewLastRound = null;

            int decryptionCounter = 0;
            usedPairCount = 2;
            int roundCounter = 0;
            double progress = 0.0;
            ProgressEventArgs e;

            List<int> possibleKeyList = new List<int>();
            for (int i = 0; i < 65535; i++)
            {
                possibleKeyList.Add(i);
            }

            bool found = false;

            if (cipher2Attack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            DateTime startTime = DateTime.Now;

            //recover k1
            while (!found)
            {
                if (stop)
                {
                    return null;
                }

                roundCounter++;

                if (_plainTextPairList.Count == 0)
                {
                    DataReceivedEvent.Reset();
                    if (NeedMessagePairOccured != null)
                    {
                        NeedMessagePairOccured.Invoke(this, null);
                    }
                    DataReceivedEvent.WaitOne();
                }

                if (stop)
                {
                    return null;
                }

                //generate plaintext pair
                Pair inputPair = new Pair()
                {
                    LeftMember = _plainTextPairList[0].LeftMember,
                    RightMember = _plainTextPairList[0].RightMember
                };

                //generate ciphertext pair
                Pair encryptedPair = new Pair()
                {
                    LeftMember = _cipherTextPairList[0].LeftMember,
                    RightMember = _cipherTextPairList[0].RightMember
                };

                _plainTextPairList.Clear();
                _cipherTextPairList.Clear();

                UInt16 expectedDiff = (UInt16) (inputPair.LeftMember ^ inputPair.RightMember);
                var temp = possibleKeyList.ToList();
                int keysToTest = temp.Count;
                int refreshCounter = 0;
                foreach (var item in temp)
                {
                    if (stop)
                    {
                        return null;
                    }

                    UInt16 decryptedLeftMember =
                        (UInt16) (ReverseSBoxBlock(ReversePBoxBlock(PartialDecrypt(attack, encryptedPair.LeftMember))) ^
                                  item);
                    UInt16 decryptedRightMember =
                        (UInt16) (ReverseSBoxBlock(
                                      ReversePBoxBlock(PartialDecrypt(attack, encryptedPair.RightMember))) ^ item);
                    decryptionCounter++;
                    decryptionCounter++;

                    decryptedLeftMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedLeftMember));
                    decryptedRightMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedRightMember));

                    int decryptedBlocksDiff = decryptedLeftMember ^ decryptedRightMember;

                    if (refreshCounter % 30 == 0)
                    {
                        //refresh UI
                        lastRoundEventArgsIterationResultViewLastRound = new ResultViewLastRoundEventArgs()
                        {
                            startTime = startTime,
                            endTime = DateTime.MinValue,
                            currentPlainText = "m0: " + inputPair.LeftMember.ToString("X") + " m1: " +
                                               inputPair.RightMember.ToString("X"),
                            currentCipherText = "c0: " + encryptedPair.LeftMember.ToString("X") + " c1: " +
                                                encryptedPair.RightMember.ToString("X"),
                            currentKeyCandidate = Convert.ToString((ushort) item, 2).PadLeft(16, '0'),
                            expectedDifference = expectedDiff.ToString("X"),
                            round = 1,
                            currentKeysToTestThisRound = keysToTest,
                            remainingKeyCandidates = possibleKeyList.Count,
                            examinedPairCount = usedPairCount
                        };

                        if(LastRoundResultViewRefreshOccured != null)
                        {
                            LastRoundResultViewRefreshOccured.Invoke(this, lastRoundEventArgsIterationResultViewLastRound);
                        }
                    }

                    refreshCounter++;

                    if (decryptedBlocksDiff != expectedDiff)
                    {
                        possibleKeyList.Remove(item);
                    }

                    if (possibleKeyList.Count % 800 == 0)
                    {
                        progress += 0.01;

                        e = new ProgressEventArgs()
                        {
                            Increment = 0.01
                        };
                        if (ProgressChangedOccured != null)
                        {
                            ProgressChangedOccured.Invoke(this, e);
                        }
                        
                    }
                }

                //Prepare event args
                RoundResult roundResult = new RoundResult()
                {
                    RemainingKeys = possibleKeyList.Count,
                    RoundNumber = roundCounter
                };

                ResultViewLastRoundRoundResultEventArgs eventArgsLastRoundRound =
                    new ResultViewLastRoundRoundResultEventArgs()
                    {
                        RoundResult = roundResult
                    };

                //invoke event
                if (ResultViewRefreshRoundFinishedOccured != null)
                {
                    ResultViewRefreshRoundFinishedOccured.Invoke(this, eventArgsLastRoundRound);
                }


                //check if k1 is recovered
                if (possibleKeyList.Count == 1)
                {
                    found = true;

                    lastRoundEventArgsIterationResultViewLastRound = new ResultViewLastRoundEventArgs()
                    {
                        startTime = startTime,
                        endTime = DateTime.MinValue,
                        currentPlainText = "m0: " + inputPair.LeftMember.ToString("X") + " m1: " +
                                           inputPair.RightMember.ToString("X"),
                        currentCipherText = "c0: " + encryptedPair.LeftMember.ToString("X") + " c1: " +
                                            encryptedPair.RightMember.ToString("X"),
                        currentKeyCandidate = Convert.ToString((ushort) possibleKeyList[0], 2).PadLeft(16, '0'),
                        expectedDifference = expectedDiff.ToString("X"),
                        round = 1,
                        currentKeysToTestThisRound = 1,
                        remainingKeyCandidates = 1,
                        examinedPairCount = usedPairCount
                    };

                    if (LastRoundResultViewRefreshOccured != null)
                    {
                        LastRoundResultViewRefreshOccured.Invoke(this, lastRoundEventArgsIterationResultViewLastRound);
                    }
                    
                }
                else if (possibleKeyList.Count == 0)
                {
                    return null;
                }
            }

            double inc = 0.9 - progress;
            progress = 0.9;
            e = new ProgressEventArgs()
            {
                Increment = inc
            };

            if (ProgressChangedOccured != null)
            {
                ProgressChangedOccured.Invoke(this, e);
            }


            result.SubKey1 = (ushort) possibleKeyList[0];

            if (_plainTextPairList.Count == 0)
            {
                DataReceivedEvent.Reset();

                if (NeedMessagePairOccured != null)
                {
                    NeedMessagePairOccured.Invoke(this, null);
                }
                
                DataReceivedEvent.WaitOne();
            }

            Pair inputPair2 = new Pair()
            {
                LeftMember = _plainTextPairList[0].LeftMember,
                RightMember = _plainTextPairList[0].RightMember
            };

            Pair encryptedPair2 = new Pair()
            {
                LeftMember = _cipherTextPairList[0].LeftMember,
                RightMember = _cipherTextPairList[0].RightMember
            };

            if (stop)
            {
                return null;
            }

            //recover k0
            UInt16 plainText = inputPair2.LeftMember;
            UInt16 cipherText = encryptedPair2.LeftMember;

            cipherText = (UInt16) (ReverseSBoxBlock(ReversePBoxBlock(PartialDecrypt(attack, cipherText))) ^
                                   result.SubKey1);
            cipherText = ReverseSBoxBlock(ReversePBoxBlock(cipherText));

            result.SubKey0 = (ushort) (cipherText ^ plainText);

            inc = 1.0 - progress;
            progress = 1.0;
            e = new ProgressEventArgs()
            {
                Increment = inc
            };

            if (ProgressChangedOccured != null)
            {
                ProgressChangedOccured.Invoke(this, e);
            }

            decryptionCounter++;

            //refresh UI
            if (lastRoundEventArgsIterationResultViewLastRound != null)
            {
                lastRoundEventArgsIterationResultViewLastRound.endTime = DateTime.Now;
                if (LastRoundResultViewRefreshOccured != null)
                {
                    LastRoundResultViewRefreshOccured.Invoke(this, lastRoundEventArgsIterationResultViewLastRound);
                }
            }

            result.DecryptionCounter = decryptionCounter;

            return result;
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
        /// Generates the next value depending on activeSBoxes
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public UInt16 GenerateValue(bool[] activeSBoxes, UInt16 data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(16, false);

            int currentActiveBitPosition = 0;
            for (int i = 0; i < Cipher2Configuration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (int j = 0; j < Cipher2Configuration.BITWIDTHCIPHER2; j++, currentActiveBitPosition++)
                    {
                        result[(i * Cipher2Configuration.BITWIDTHCIPHER2) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[2];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToUInt16(bytesOfResult, 0);
        }

        /// <summary>
        /// Recovers key information with the given configuration
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack,
            DifferentialAttackRoundConfiguration configuration)
        {
            if (configuration.ActiveSBoxes == null)
            {
                throw new ArgumentException("activeSBoxes should contain at least one active SBox");
            }

            ResultViewAnyRoundEventArgs anyRoundEventArgs = null;
            ResultViewAnyRoundKeyResultEventArgs eventArgsKeyResult = null;
            ProgressEventArgs e = null;

            Cipher2DifferentialKeyRecoveryAttack c2Attack = attack as Cipher2DifferentialKeyRecoveryAttack;
            int partialKey = 0;

            switch (configuration.Round)
            {
                case 3:
                {
                    partialKey = c2Attack.subkey3;
                }
                    break;
                case 2:
                {
                    partialKey = c2Attack.subkey2;
                }
                    break;
            }


            DateTime startTime = DateTime.Now;

            DifferentialAttackRoundResult roundResult = new DifferentialAttackRoundResult();

            //Generate border for the loop
            int loopBorder = CalculateLoopBorder(configuration.ActiveSBoxes);

            double increment = 1.0 / (double) loopBorder;

            ParallelOptions po = new ParallelOptions();
            Cts = new CancellationTokenSource();
            po.CancellationToken = Cts.Token;
            po.CancellationToken.ThrowIfCancellationRequested();
            po.MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 1.0) * _procMultiplier));

            //for(int i = 0; i < loopBorder; i++)
            Parallel.For(0, loopBorder, po, i =>
            {
                UInt16 guessedKey = GenerateValue(configuration.ActiveSBoxes, (UInt16) i);
                //Console.WriteLine(Convert.ToString(guessedKey, 2).PadLeft(16, '0').Insert(8, " "));

                if (stop)
                {
                    return;
                }

                if (!configuration.IsLast)
                {
                    guessedKey = ApplyPBoxToBlock(guessedKey);
                    //Console.WriteLine(guessedKey + " " + Convert.ToString(guessedKey, 2).PadLeft(16, '0').Insert(8, " "));
                }

                KeyProbability curTry = new KeyProbability() {Counter = 0, Key = guessedKey};

                anyRoundEventArgs = new ResultViewAnyRoundEventArgs()
                {
                    startTime = startTime,
                    endTime = DateTime.MinValue,
                    round = configuration.Round,
                    currentExpectedProbability = configuration.Probability,
                    currentKeyCandidate = Convert.ToString((ushort) curTry.Key, 2).PadLeft(16, '0'),
                    currentKeysToTestThisRound = loopBorder,
                    currentRecoveredRoundKey = Convert.ToString((ushort) partialKey, 2).PadLeft(16, '0'),
                    expectedDifference =
                        Convert.ToString((ushort) configuration.ExpectedDifference, 2).PadLeft(16, '0'),
                    expectedHitCount = (int) (configuration.Probability * configuration.FilteredPairList.Count),
                    messagePairCountToExamine = configuration.FilteredPairList.Count
                };

                //synchronize access to resultList
                _semaphoreSlim.Wait();
                try
                {
                    if (AnyRoundResultViewRefreshOccured != null)
                    {
                        AnyRoundResultViewRefreshOccured.Invoke(this, anyRoundEventArgs);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                foreach (var curPair in configuration.EncrypedPairList)
                {
                    if (stop)
                    {
                        return;
                    }

                    Pair encryptedPair = new Pair()
                        {LeftMember = curPair.LeftMember, RightMember = curPair.RightMember};

                    encryptedPair.LeftMember = PartialDecrypt(attack, encryptedPair.LeftMember);
                    encryptedPair.RightMember = PartialDecrypt(attack, encryptedPair.RightMember);

                    //reverse round with the guessed key
                    UInt16 leftMemberSingleDecrypted = DecryptSingleRound(encryptedPair.LeftMember, curTry.Key,
                        configuration.IsBeforeLast, configuration.IsLast);
                    UInt16 rightMemberSingleDecrypted = DecryptSingleRound(encryptedPair.RightMember, curTry.Key,
                        configuration.IsBeforeLast, configuration.IsLast);

                    if (configuration.IsLast)
                    {
                        leftMemberSingleDecrypted = ReverseSBoxBlock(leftMemberSingleDecrypted);
                        rightMemberSingleDecrypted = ReverseSBoxBlock(rightMemberSingleDecrypted);
                    }
                    else
                    {
                        leftMemberSingleDecrypted = ReversePBoxBlock(leftMemberSingleDecrypted);
                        leftMemberSingleDecrypted = ReverseSBoxBlock(leftMemberSingleDecrypted);

                        rightMemberSingleDecrypted = ReversePBoxBlock(rightMemberSingleDecrypted);
                        rightMemberSingleDecrypted = ReverseSBoxBlock(rightMemberSingleDecrypted);
                    }

                    UInt16 differentialToCompare = (UInt16) (leftMemberSingleDecrypted ^ rightMemberSingleDecrypted);

                    if (differentialToCompare == configuration.ExpectedDifference)
                    {
                        curTry.Counter++;
                    }
                }

                //synchronize access to resultList
                _semaphoreSlim.Wait();
                try
                {
                    roundResult.KeyCandidateProbabilities.Add(curTry);
                    e = new ProgressEventArgs()
                    {
                        Increment = increment
                    };
                    
                    if (ProgressChangedOccured != null)
                    {
                        ProgressChangedOccured.Invoke(this, e);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            });

            if (stop)
            {
                return null;
            }

            eventArgsKeyResult = new ResultViewAnyRoundKeyResultEventArgs();

            //sort by counter
            KeyProbability bestPossibleKey = new KeyProbability() {Counter = 0, Key = 0};
            foreach (var curKey in roundResult.KeyCandidateProbabilities)
            {
                eventArgsKeyResult.keyResults.Add(new KeyResult()
                {
                    Key = curKey.Key,
                    BinaryKey = Convert.ToString(curKey.Key, 2).PadLeft(16, '0'),
                    HitCount = curKey.Counter,
                    Probability = (curKey.Counter / (double) configuration.FilteredPairList.Count)
                });

                if (curKey.Counter > bestPossibleKey.Counter)
                {
                    bestPossibleKey = curKey;
                }
            }

            //refresh UI
            if (anyRoundEventArgs != null)
            {
                anyRoundEventArgs.endTime = DateTime.Now;
                anyRoundEventArgs.currentRecoveredRoundKey =
                    Convert.ToString((ushort) partialKey ^ bestPossibleKey.Key, 2).PadLeft(16, '0');
                
                if (AnyRoundResultViewRefreshOccured != null)
                {
                    AnyRoundResultViewRefreshOccured.Invoke(this, anyRoundEventArgs);
                }
            }

            //fill data into table
            if (AnyRoundResultViewKeyResultsRefreshOccured != null)
            {
                AnyRoundResultViewKeyResultsRefreshOccured.Invoke(this, eventArgsKeyResult);
            }

            roundResult.PossibleKey = bestPossibleKey.Key;
            roundResult.Probability = bestPossibleKey.Counter / (double) configuration.UnfilteredPairList.Count;
            roundResult.ExpectedProbability = configuration.Probability;
            roundResult.KeyCandidateProbabilities = roundResult.KeyCandidateProbabilities
                .OrderByDescending(item => item.Counter).ToList();

            return roundResult;
        }

        /// <summary>
        /// Reverses a 16 bit SBox block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort ReverseSBoxBlock(UInt16 data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[2];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[2];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[2];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[2];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            UInt16 zeroToThreeInt = BitConverter.ToUInt16(zeroToThreeBytes, 0);
            UInt16 fourToSevenInt = BitConverter.ToUInt16(fourToSevenBytes, 0);
            UInt16 eightToElevenInt = BitConverter.ToUInt16(eightToElevenBytes, 0);
            UInt16 twelveToFifteenInt = BitConverter.ToUInt16(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher2Configuration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = Cipher2Configuration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = Cipher2Configuration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = Cipher2Configuration.SBOXREVERSE[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            UInt16 combined = BitConverter.ToUInt16(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Applies the SBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort SBox(UInt16 data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[2];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[2];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[2];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[2];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            UInt16 zeroToThreeInt = BitConverter.ToUInt16(zeroToThreeBytes, 0);
            UInt16 fourToSevenInt = BitConverter.ToUInt16(fourToSevenBytes, 0);
            UInt16 eightToElevenInt = BitConverter.ToUInt16(eightToElevenBytes, 0);
            UInt16 twelveToFifteenInt = BitConverter.ToUInt16(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher2Configuration.SBOX[zeroToThreeInt];
            fourToSevenInt = Cipher2Configuration.SBOX[fourToSevenInt];
            eightToElevenInt = Cipher2Configuration.SBOX[eightToElevenInt];
            twelveToFifteenInt = Cipher2Configuration.SBOX[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            UInt16 combined = BitConverter.ToUInt16(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Decrypts a block with all recovered subkeys
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public ushort PartialDecrypt(DifferentialKeyRecoveryAttack attack, ushort block)
        {
            Cipher2DifferentialKeyRecoveryAttack cipher2Attack = attack as Cipher2DifferentialKeyRecoveryAttack;

            UInt16 result = block;

            if (cipher2Attack != null && cipher2Attack.recoveredSubkey3)
            {
                result = DecryptSingleRound(result, cipher2Attack.subkey3, false, true);
            }

            if (cipher2Attack != null && cipher2Attack.recoveredSubkey2)
            {
                result = DecryptSingleRound(result, cipher2Attack.subkey2, true, false);
            }

            return result;
        }

        /// <summary>
        /// Decrypts a single round with the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <param name="beforeLast"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
        public ushort DecryptSingleRound(ushort block, ushort key, bool beforeLast, bool isLast)
        {
            UInt16 result = block;

            //if is last round
            if (isLast)
            {
                result = (UInt16) (result ^ key);
                return result;
            }

            if (beforeLast)
            {
                result = ReverseSBoxBlock(result);
                result = (UInt16) (result ^ key);
                return result;
            }

            //use the pbox
            result = ReversePBoxBlock(result);

            //undo the sbox
            result = ReverseSBoxBlock(result);

            //use the key to decrypt
            result = (UInt16) (result ^ key);

            return result;
        }

        /// <summary>
        /// Reverses the permutation on a block
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

            byte[] bytes = new byte[2];
            pboxedArray.CopyTo(bytes, 0);

            UInt16 outputBlock = BitConverter.ToUInt16(bytes, 0);
            return outputBlock;
        }

        /// <summary>
        /// Applies the permutation to data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort ApplyPBoxToBlock(ushort data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher2Configuration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[2];
            pboxedArray.CopyTo(bytes, 0);

            UInt16 outputBlock = BitConverter.ToUInt16(bytes, 0);

            return outputBlock;
        }
    }
}
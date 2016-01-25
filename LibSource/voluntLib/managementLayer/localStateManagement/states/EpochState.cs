// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using NLog;
using voluntLib.common.utils;
using voluntLib.managementLayer.localStateManagement.states.config;

#endregion

namespace voluntLib.managementLayer.localStateManagement.states
{
    public class EpochState : ALocalState
    {
        #region private members

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Random randomGenerator = new Random();
        private EpochStateConfig config;

        #endregion

        #region public members and properties

        public BigInteger maximumEpoch;
        public BigInteger EpochNumber { get; set; }
        public BitArray BitMask { get; set; } //TODO replace BitArray with own implementation due performance issues

        #endregion
       
        public EpochState() : this(new EpochStateConfig {NumberOfBlocks = 8, BitMaskWidth = 8}) {}
        public EpochState(int bitMaskWidth) : this(new EpochStateConfig {BitMaskWidth = bitMaskWidth}) {}
        public EpochState(EpochStateConfig config) : base(EpochStateID)
        {
            ApplyConfig(config);
        }

        /// <summary>
        ///   Applies a configuration to the current state Object
        ///   Also calls FinalizeValues on the configuration object
        /// </summary>
        /// <param name="stateConfig">The state configuration.</param>
        public void ApplyConfig(EpochStateConfig stateConfig)
        {
            stateConfig.FinalizeValues();
            config = stateConfig;
            maximumEpoch = config.MaximumEpoch;
            EpochNumber = 0;
            BitMask = new BitArray(config.BitMaskWidth, false);
        }

        public BitArray GetCopyOfBitmask()
        {
           return new BitArray(BitMask);
        }
    
        public override StateRelation CompareWith(ALocalState candidate)
        {
            var candidateAsEpochState = candidate as EpochState;
            if (candidateAsEpochState == null)
            {
                return StateRelation.DifferentStateType;
            }

            if (EpochNumber != candidateAsEpochState.EpochNumber)
            {
                return (EpochNumber > candidateAsEpochState.EpochNumber)
                    ? StateRelation.IsSuperSet
                    : StateRelation.IsProperSubset;
            }

            if (BitMask.Length != candidateAsEpochState.BitMask.Length)
            {
                return StateRelation.DifferentStateConfig;
            }

            if (AreEqual(candidateAsEpochState.BitMask, BitMask))
            {
                return StateRelation.Equal;
            }

            var thisOrCandidate = new BitArray(BitMask).Or(candidateAsEpochState.BitMask);
            if (AreEqual(thisOrCandidate, BitMask))
            {
                return StateRelation.IsSuperSet;
            }

            if (AreEqual(thisOrCandidate, candidateAsEpochState.BitMask))
            {
                return StateRelation.IsProperSubset;
            }

            return StateRelation.OutOfSync;
        }

        /// <summary>
        ///   Merges the meta data.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        public override void MergeMetaData(ALocalState candidate)
        {
            var candidateAsEpochState = candidate as EpochState;
            if (candidateAsEpochState == null || candidateAsEpochState.EpochNumber != EpochNumber)
            {
                return;
            }

            BitMask.Or(candidateAsEpochState.BitMask);

            if (IsFinished())
            {
                Logger.Info("Finished");
            }
        }

        public override bool ContainsMoreInformationThan(ALocalState candidate)
        {
            var candidateAsEpochState = candidate as EpochState;
            if (candidateAsEpochState == null || candidateAsEpochState.EpochNumber > EpochNumber)
            {
                return false;
            }

            if (candidateAsEpochState.EpochNumber < EpochNumber)
            {
                return true;
            }

            //compair bitmasks 
            return GetNumberOfCalculatedBlocksInCurrentEpoch() > candidateAsEpochState.GetNumberOfCalculatedBlocksInCurrentEpoch();
        }

        public override BigInteger GetFreeBlock(List<BigInteger> workingOnSet)
        {
            if (IsFinished()) return -1;

            lock (this)
            {
                var amountOfZeros = config.BitMaskWidth - GetNumberOfCalculatedBlocksInCurrentEpoch();
                var workingOnInEpoch = workingOnSet.FindAll(bID => (bID / (ulong)config.BitMaskWidth).Equals(EpochNumber));
                if (amountOfZeros - workingOnInEpoch.Count() <= 0)
                {
                    // we are already working on each missing block.
                    return -1;
                }

                //pick random zero 
                var chosenZero = randomGenerator.Next(amountOfZeros - workingOnInEpoch.Count());
                var blockID = FindBlockIdOfChosenZero(chosenZero, workingOnInEpoch);

                var freeBlock = (ulong)blockID + (EpochNumber * (ulong)config.BitMaskWidth);
                return freeBlock;
            }
        }

        private int FindBlockIdOfChosenZero(int chosenZero, List<BigInteger> workingOnInEpoch)
        {
            var blockID = 0;
            for (; chosenZero >= 0; chosenZero--)
            {
                //skip while block is 1
                while (BitMask[blockID] || workingOnInEpoch.Contains(((ulong)blockID + (ulong) config.BitMaskWidth * EpochNumber)))
                {
                    blockID++;
                }

                //count zero
                blockID++;
            }
            //undo last count
            blockID--;
            return blockID;
        }

        public override void MarkBlockAsCalculated(BigInteger blockID)
        {
            var epoch = CalculateEpochOfBlock(blockID);
            if (epoch < EpochNumber)
            {
                return;
            } 
            if (epoch > EpochNumber)
            {
                SwitchToEpoch(epoch);
            }

            var bit = CalculateBitOfBlock(blockID, epoch);
            BitMask[(int) (ulong) bit] = true;

            if (BitMaskIsFinished())
            {
                SwitchToEpoch(EpochNumber + 1);
            }
            if (IsInLastEpoch())
            {
                SetUnusedBits();
            }

            if (IsFinished())
            {
                Logger.Info("Finished");
            }
        }

        #region helper

        private void SetUnusedBits()
        {
            for (var i = 1; i <= config.UnusedBitsOfLastEpoch; i++)
            {
                BitMask[BitMask.Length - i] = true;
            }
        }


        private bool IsInLastEpoch()
        {
            return EpochNumber == maximumEpoch - 1 && config != null;
        }

        private void SwitchToEpoch(BigInteger epoch)
        {
            // switch to epoch if current epoch is old
            EpochNumber = (ulong) epoch;
            BitMask.SetAll(false);
        }

        private BigInteger CalculateBitOfBlock(BigInteger blockID, BigInteger epoch)
        {
            return blockID - epoch * config.BitMaskWidth;
        }

        private BigInteger CalculateEpochOfBlock(BigInteger blockID)
        {
            return (blockID / config.BitMaskWidth);
        }

        public override string ToString()
        {
            var str = "\tEpochNumber: " + EpochNumber + " Bitmask: ";
            
            try
            {
                return GetCopyOfBitmask().Cast<bool>().Aggregate(str, (current, bit) => current + (bit ? "1" : "0"));
            }
            catch (Exception)
            {
                return str;
            }
        }


        /// <summary>
        ///   Gets the cardinality.
        ///   according to http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
        /// </summary>
        /// <returns></returns>
        private static Int32 GetCardinality(BitArray a)
        {
            var ints = new Int32[(a.Count >> 5) + 1];
            a.CopyTo(ints, 0);
            var count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (a.Count%32));

            for (var i = 0; i < ints.Length; i++)
            {
                var c = ints[i];
                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F)*0x1010101) >> 24;
                }
                count += c;
            }

            return count;
        }

        public Int32 GetNumberOfCalculatedBlocksInCurrentEpoch()
        {           
            return GetCardinality(BitMask);
        }

        /// <summary>
        ///   Determines whether all bits within the bitmask are true.
        /// </summary>
        /// <returns></returns>
        private bool BitMaskIsFinished()
        {
            return BitMask.Cast<bool>().Aggregate(true, (current, bit) => current & bit);
        }

        private static bool AreEqual(BitArray a, BitArray b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            if (GetCardinality(a) != GetCardinality(b))
            {
                return false;
            }

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region getter

        /// <summary>
        ///   Gets the number of calculated blocks.
        /// </summary>
        /// <returns></returns>
        protected override BigInteger GetNumberOfCalculatedBlocks()
        {
            lock (this)
            {
                var numberOfCalculatedBlocks = EpochNumber * config.BitMaskWidth + GetNumberOfCalculatedBlocksInCurrentEpoch();
                if (IsInLastEpoch())
                {
                    numberOfCalculatedBlocks -= config.UnusedBitsOfLastEpoch;
                }
                return numberOfCalculatedBlocks;
            }
        }

        /// <summary>
        ///   Gets the number of blocks.
        /// </summary>
        /// <returns></returns>
        protected override BigInteger GetNumberOfBlocks()
        {
            return config.NumberOfBlocks;
        }

        /// <summary>
        ///   Determines whether the given is calculated.
        /// </summary>
        /// <param name="blockID">The block identifier.</param>
        /// <returns></returns>
        public override bool IsBlockCalculated(BigInteger blockID)
        {
            var epoch = blockID / config.BitMaskWidth;
            var bit = (int)(blockID - epoch * config.BitMaskWidth);
            return epoch < EpochNumber || BitMask.Get(bit);
        }

        /// <summary>
        ///   Determines whether this instance is finished.
        /// </summary>
        /// <returns></returns>
        public override bool IsFinished()
        {
            return EpochNumber >= maximumEpoch;
        }

        #endregion

        #region Serialize

        public override byte[] Serialize()
        {
            var baseBytes = base.Serialize();
            var byteArray = new byte[(int)Math.Ceiling((double) BitMask.Length / 8)];
            BitMask.CopyTo(byteArray, 0);
            return baseBytes
                .Concat(SerializationHelper.SerializeBigInt(maximumEpoch))
                .Concat(SerializationHelper.SerializeBigInt(EpochNumber))
                .Concat(BitConverter.GetBytes((uint) byteArray.Length))
                .Concat(byteArray).ToArray();
        }

        public override int Deserialize(byte[] data, int startIndex = 0)
        {
            startIndex += base.Deserialize(data, startIndex);
            int bytesLength;
            maximumEpoch = SerializationHelper.DeserializeBigInt(data, startIndex, out bytesLength);
            startIndex += bytesLength;

            EpochNumber = SerializationHelper.DeserializeBigInt(data, startIndex, out bytesLength);
            startIndex += bytesLength;

            var bitArrayContainerLength = (int) BitConverter.ToUInt32(data, startIndex);
            startIndex += 4;
            BitMask = new BitArray(data.Skip(startIndex).Take(bitArrayContainerLength).ToArray());

            if (config != null && config.IsFinal)
            {
                SetUnusedBits();
            }

            return startIndex;
        }

        #endregion
    }
}
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
using System.Linq;
using System.Numerics;
using NLog;
using voluntLib.common.utils;

#endregion

namespace voluntLib.managementLayer.localStateManagement.states.config
{
    /// <summary>
    ///   Represents a configuration container for the epoch state.
    ///   Contains initalisation information such as  NumberOfBlocks and the bitMaskwith
    /// </summary>
    public class EpochStateConfig : IStateConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly int MaximumBitMaskWidth = 491520; // 60 kbyte

        public EpochStateConfig(byte[] serialize, int index) : this()
        {
            Deserialize(serialize, index);
        }

        public EpochStateConfig()
        {
            NumberOfBlocks = BigInteger.MinusOne;
            MaximumEpoch = BigInteger.MinusOne;
            BitMaskWidth = -1;
            UnusedBitsOfLastEpoch = 0;
        }

        public bool IsFinal { get; private set; }
        public int BitMaskWidth { get; set; }
        public BigInteger MaximumEpoch { get; private set; }
        public int UnusedBitsOfLastEpoch { get; private set; }
        public BigInteger NumberOfBlocks { get; set; }

        /// <summary>
        ///   Calculates missing values. This Method is automatically called whenever this object is applied to a state
        /// </summary>
        /// <returns></returns>
        public bool FinalizeValues()
        {
            try
            {
                if (NumberOfBlocks == -1)
                {
                    //infinite blocks
                    BitMaskWidth = (BitMaskWidth == -1) ? MaximumBitMaskWidth : BitMaskWidth;
                    IsFinal = true;
                    return true;
                }
                if (BitMaskWidth == -1)
                {
                    BitMaskWidth = NumberOfBlocks <= MaximumBitMaskWidth ? (int) NumberOfBlocks : MaximumBitMaskWidth;
                }

                if (BitMaskWidth > NumberOfBlocks)
                {
                    BitMaskWidth = (int) NumberOfBlocks;
                }

                if (BitMaskWidth == 0) return false;

                BigInteger remainder;
                MaximumEpoch = BigInteger.DivRem(NumberOfBlocks, BitMaskWidth, out remainder);
                if (remainder != 0)
                {
                    MaximumEpoch++;
                    UnusedBitsOfLastEpoch = (int) remainder;
                }
                IsFinal = true;
                return true;
            } catch (Exception e)
            {
                Logger.Fatal("Wrong Configuration: " + e.Message + e.StackTrace);
            }
            return false;
        }

        #region ISerialize

        public byte[] Serialize()
        {
            FinalizeValues();
            var numberBytes = SerializationHelper.SerializeBigInt(NumberOfBlocks);
            var bitMaskWidthBytes = BitConverter.GetBytes(BitMaskWidth);
            return numberBytes.Concat(bitMaskWidthBytes).ToArray();
        }

        public int Deserialize(byte[] data, int startIndex = 0)
        {
            int byteLength;
            NumberOfBlocks = SerializationHelper.DeserializeBigInt(data, startIndex, out byteLength);
            startIndex += byteLength;
            BitMaskWidth = BitConverter.ToInt32(data, startIndex);
            FinalizeValues();
            return startIndex;
        }

        #endregion
    }
}
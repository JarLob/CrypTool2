﻿/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{
    public class Bitmask : IVoluntLibSerializable
    {
        public const int DEFAULT_MASKSIZE = 35840; //35 kiB
        //public const int DEFAULT_MASKSIZE = 10;
        private Random random;
        public uint MaskSize { get; private set; }

        private byte[] mask;

        /// <summary>
        /// Lookup table for fast counting set bits in a byte array
        /// </summary>
        private uint[] BIT_COUNT_MAP = new uint[]
        {
            0x0, 0x1, 0x1, 0x2, 0x1, 0x2, 0x2, 0x3,
            0x1, 0x2, 0x2, 0x3, 0x2, 0x3, 0x3, 0x4,
            0x1, 0x2, 0x2, 0x3, 0x2, 0x3, 0x3, 0x4,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x1, 0x2, 0x2, 0x3, 0x2, 0x3, 0x3, 0x4,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x1, 0x2, 0x2, 0x3, 0x2, 0x3, 0x3, 0x4,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x4, 0x5, 0x5, 0x6, 0x5, 0x6, 0x6, 0x7,
            0x1, 0x2, 0x2, 0x3, 0x2, 0x3, 0x3, 0x4,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x4, 0x5, 0x5, 0x6, 0x5, 0x6, 0x6, 0x7,
            0x2, 0x3, 0x3, 0x4, 0x3, 0x4, 0x4, 0x5,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x4, 0x5, 0x5, 0x6, 0x5, 0x6, 0x6, 0x7,
            0x3, 0x4, 0x4, 0x5, 0x4, 0x5, 0x5, 0x6,
            0x4, 0x5, 0x5, 0x6, 0x5, 0x6, 0x6, 0x7,
            0x4, 0x5, 0x5, 0x6, 0x5, 0x6, 0x6, 0x7,
            0x5, 0x6, 0x6, 0x7, 0x6, 0x7, 0x7, 0x8
        };

        /// <summary>
        /// Create a new empty bitmask
        /// </summary>
        public Bitmask(uint masksize = DEFAULT_MASKSIZE)
        {
            MaskSize = masksize;
            mask = new byte[MaskSize];
            random = new Random(Guid.NewGuid().GetHashCode());            
        }

        /// <summary>
        /// Returns a copy of the internal byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            byte[] bytes = new byte[MaskSize + 4];
            byte[] maskSizeBytes = BitConverter.GetBytes((UInt32)MaskSize);
            Array.Copy(maskSizeBytes, 0, bytes, 0, 4);
            Array.Copy(mask, 0, bytes, 4, MaskSize);
            return bytes;
        }

        /// <summary>
        /// Copys the values of the given byte array to this bitmask
        /// </summary>
        /// <param name="bytes"></param>
        public void Deserialize(byte[] bytes)
        {
            MaskSize = BitConverter.ToUInt32(bytes, 0);
            mask = new byte[MaskSize];
            Array.Copy(bytes, 4, mask, 0, MaskSize);
        }

        /// <summary>
        /// Returns the or: returnBitmask = bitmaskA | bitmaskB
        /// </summary>
        /// <param name="bitmaskA"></param>
        /// <param name="bitmaskB"></param>
        /// <returns></returns>
        public static Bitmask operator | (Bitmask bitmaskA, Bitmask bitmaskB)
        {
            if (bitmaskA.MaskSize != bitmaskB.MaskSize)
            {
                throw new Exception(String.Format("Can not or bitmasks. bitmaskA size is {0} and bitmaskB size is {1}", bitmaskA.MaskSize, bitmaskB.MaskSize));
            }

            Bitmask newMask = new Bitmask(bitmaskA.MaskSize);
            newMask.Deserialize(bitmaskA.Serialize());
            for (int i = 0; i < newMask.MaskSize; i++)
            {
                newMask.mask[i] = (byte)(newMask.mask[i] | bitmaskB.mask[i]);
            }
            return newMask;
        }        

        /// <summary>
        /// Returns the number of free bits in the bitmask
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBits()
        {
            return MaskSize * 8 - GetSetBitsCount();
        }

        /// <summary>
        /// Returns the number of set bits in bitmask
        /// </summary>
        /// <returns></returns>
        public uint GetSetBitsCount()
        {
            uint count = 0;
            foreach (byte b in mask)
            {
                count += BIT_COUNT_MAP[b];
            }
            return count;
        }

        /// <summary>
        /// Sets the bit defined by the given offset to the given bit value
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="bit"></param>
        public void SetBit(uint offset, bool bit)
        {
            if (offset > MaskSize * 8)
            {
                throw new ArgumentException(String.Format("Selected offset {0} to set bit in bitmask was greater than the bitmask's size {1}!", offset, MaskSize));
            }
            uint bytevalue = offset / 8;
            uint bitvalue = (uint)Math.Pow(2, offset % 8);
            byte value = (byte)(mask[bytevalue] & (255 - bitvalue));
            if (bit)
            {
                value = (byte)(value | bitvalue);
            }
            mask[bytevalue] = value;
        }

        /// <summary>
        /// Returns true, if the bit at the offset is 1; otherwise returns false
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool GetBit(uint offset)
        {
            if (offset > MaskSize * 8)
            {
                throw new ArgumentException(String.Format("Selected offset {0} to get bit in bitmask was greater than the bitmask's size {1}!", offset, MaskSize));
            }
            uint bytevalue = offset / 8;
            uint bitvalue = (uint)Math.Pow(2, offset % 8);
            byte value = (byte)(mask[bytevalue] & bitvalue);
            return value != 0;
        }

        /// <summary>
        /// Returns the offset of a random unset bit of this bitmask
        /// </summary>
        /// <returns></returns>
        public int GetRandomFreeBit()
        {
            uint freebits = MaskSize * 8 - GetSetBitsCount();
            if (freebits == 0)
            {
                return -1;
            }
            uint randomnumber = (uint)random.Next(0, (int)freebits-1);
            uint counter = 0;
            uint position = 0;
            for (uint i = 0; i < MaskSize; i++)
            {
                counter += (8 - BIT_COUNT_MAP[mask[i]]);
                position += 8;
                if (counter >= randomnumber)
                {
                    counter -= (8 - BIT_COUNT_MAP[mask[i]]);
                    position -= 8;
                    for (uint j = 1; j <= 128; j *= 2)
                    {
                        if ((mask[i] & j) != j)
                        {
                            counter++;
                            if (counter == randomnumber)
                            {
                                return (int)position;
                            }
                        }
                        position++;
                    }
                }
            }
            return -1;
        }
    }
}

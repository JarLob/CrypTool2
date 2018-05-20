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
        private const int MASK_SIZE = 30720; //30 kiB
        private byte[] mask;

        /// <summary>
        /// Lookup table for counting set bits in a byte
        /// </summary>
        private int[] BIT_COUNT_MAP = new int[]{
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

        public Bitmask()
        {
            mask = new byte[MASK_SIZE];
        }

        public byte[] Serialize()
        {
            byte[] bytes = new byte[MASK_SIZE];
            Array.Copy(mask, bytes, MASK_SIZE);
            return bytes;
        }

        public void Deserialize(byte[] bytes)
        {
            Array.Copy(bytes, mask, MASK_SIZE);
        }

        public static Bitmask operator |(Bitmask bitmaskA, Bitmask bitmaskB)
        {
            Bitmask newMask = new Bitmask();
            newMask.Deserialize(bitmaskA.Serialize());
            for (int i = 0; i < MASK_SIZE; i++)
            {
                newMask.mask[i] = (byte)(newMask.mask[i] | bitmaskB.mask[i]);
            }
            return newMask;
        }        

        public int GetFreeBits()
        {
            int count = 0;
            foreach (byte b in mask)
            {
                count += BIT_COUNT_MAP[b];
            }
            return count;
        }

        public void SetBit(uint offset, bool bit)
        {
            if (offset > MASK_SIZE * 8)
            {
                throw new Exception(String.Format("Selected offset {0} to set bit in Bitmask was greater than the Bitmask's size {1}!", offset, MASK_SIZE));
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

        public bool GetBit(int offset)
        {
            return false;
        }
    }
}
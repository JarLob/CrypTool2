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
using System.Numerics;
using System.Text;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{
    public class EpochState : IVoluntLibSerializable, ICloneable
    {
        public BigInteger EpochNumber { get; set; }
        public IEnumerable<byte[]> ResultList { get; set; }
        public Bitmask Bitmask { get; set; }

        /// <summary>
        /// Creates a new Empty Epoch State
        /// </summary>
        public EpochState()
        {
            Bitmask = new Bitmask();
            EpochNumber = 0;
            ResultList = new List<byte[]>();
        }

        /// <summary>
        /// Serialize to byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            int size = 0;
            //serialize epoch number
            byte[] epochNumberBytes = EpochNumber.ToByteArray();
            byte[] epochNumberLengthBytes = BitConverter.GetBytes((ushort)epochNumberBytes.Length);
            size += epochNumberLengthBytes.Length;
            size += epochNumberBytes.Length;
            //serialize result list
            byte[] resultListCountBytes = BitConverter.GetBytes((ushort)ResultList.Count());
            size += resultListCountBytes.Length;
            byte[][] resultListEntriesSizeBytes = new byte[ResultList.Count()][];
            for (int i = 0; i < ResultList.Count(); i++)
            {
                byte[] entry = ResultList.ElementAt(i);
                resultListEntriesSizeBytes[i] = BitConverter.GetBytes((ushort)entry.Length);
                size += resultListEntriesSizeBytes[i].Length;
                size += entry.Length;
            }
            //serialize bitmask
            byte[] mask = Bitmask.Serialize();
            size += mask.Length;
            //now, copy everything to new array and return it
            byte[] data = new byte[size];
            int offset = 0;
            //copy epoch number
            Array.Copy(epochNumberLengthBytes, 0, data, offset, epochNumberLengthBytes.Length);
            offset += epochNumberLengthBytes.Length;
            Array.Copy(epochNumberBytes, 0, data, offset, epochNumberBytes.Length);
            offset += epochNumberBytes.Length;
            //copy result list
            Array.Copy(resultListCountBytes, 0, data, offset, resultListCountBytes.Length);
            offset += resultListCountBytes.Length;
            for (int i = 0; i < ResultList.Count(); i++)
            {
                Array.Copy(resultListEntriesSizeBytes[i], 0, data, offset, resultListEntriesSizeBytes[i].Length);
                offset += resultListEntriesSizeBytes[i].Length;
                byte[] entry = ResultList.ElementAt(i);
                Array.Copy(entry, 0, data, offset, entry.Length);
                offset += entry.Length;
            }
            //copy bitmask
            Array.Copy(mask, 0, data, offset, mask.Length);
            return data;
        }

        /// <summary>
        /// Deserialize from byte array
        /// </summary>
        /// <param name="data"></param>
        public void Deserialize(byte[] data)
        {
            int offset = 0;
            //deserialize epoch number
            ushort epochNumberLength = BitConverter.ToUInt16(data, 0);
            offset += 2;
            byte[] epochNumberBytes = new byte[epochNumberLength];
            Array.Copy(data, offset, epochNumberBytes, 0, epochNumberLength);
            EpochNumber = new BigInteger(epochNumberBytes);
            offset += epochNumberBytes.Length;
            //deserialize result list
            ushort listLength = BitConverter.ToUInt16(data, offset);
            offset += 2;
            ResultList = new List<byte[]>();
            for (int i = 0; i < listLength; i++)
            {
                ushort resultListEntrySize = BitConverter.ToUInt16(data, offset);
                offset += 2;
                byte[] entry = new byte[resultListEntrySize];
                Array.Copy(data, offset, entry, 0, resultListEntrySize);
                ((List<byte[]>)ResultList).Add(entry);
                offset += entry.Length;
            }
            //deserialize bitmask
            byte[] mask = new byte[data.Length - offset];
            Array.Copy(data, offset, mask, 0, mask.Length);
            Bitmask = new Bitmask();
            Bitmask.Deserialize(mask);
        }

        /// <summary>
        /// Clones this EpochState
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Bitmask mask = new Bitmask();
            var data = Serialize();
            mask.Deserialize(data);
            return mask;
        }
    }      
}

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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using voluntLib.common.interfaces;

#endregion

namespace voluntLib.managementLayer.localStateManagement.states
{
    /// <summary>
    ///   Represents a State of the current Network calculation.
    /// </summary>
    public abstract class ALocalState : IMessageData
    {
        public const byte NullStateID = 0;
        public const byte EpochStateID = 1;
        public static readonly byte SlidingWindowStateID = 2;

        protected ALocalState(byte stateID)
        {
            StateID = stateID;
            ResultList = new List<byte[]>();
        }

        public byte StateID { get; set; }
        public List<byte[]> ResultList { get; set; }

        public BigInteger NumberOfBlocks
        {
            get { return GetNumberOfBlocks(); }
        }

        public BigInteger NumberOfCalculatedBlocks
        {
            get { return GetNumberOfCalculatedBlocks(); }
        }

        #region IMessageData

        public virtual byte[] Serialize()
        {
            IEnumerable<byte> resultListBytes = new byte[] {};

            //Serialize resultlist
            resultListBytes = ResultList.Aggregate(resultListBytes,
                (current, item) => current.Concat(BitConverter.GetBytes((ushort) item.Length)).Concat(item));

            return new[] {StateID, (byte) ResultList.Count}.Concat(resultListBytes).ToArray();
        }

        public virtual int Deserialize(byte[] data, int startIndex = 0)
        {
            StateID = data[startIndex];
            var resultListLength = data[startIndex + 1];
            startIndex += 2;
            ResultList.Clear();
            for (var i = 0; i < resultListLength; i++)
            {
                var resultLength = BitConverter.ToUInt16(data, startIndex);
                startIndex += 2;
                ResultList.Add(data.Skip(startIndex).Take(resultLength).ToArray());
                startIndex += resultLength;
            }
            return startIndex;
        }

        #endregion

        public abstract StateRelation CompareWith(ALocalState candidate);
        public abstract void MergeMetaData(ALocalState candidate);
        public abstract bool ContainsMoreInformationThan(ALocalState candidate);
        public abstract BigInteger GetFreeBlock(List<BigInteger> workingOnSet);
        public abstract bool IsBlockCalculated(BigInteger blockID);
        public abstract void MarkBlockAsCalculated(BigInteger blockID);
        public abstract bool IsFinished();

        protected abstract BigInteger GetNumberOfBlocks();
        protected abstract BigInteger GetNumberOfCalculatedBlocks();
    }

    public enum StateRelation
    {
        IsProperSubset,
        Equal,
        OutOfSync,
        IsSuperSet,
        DifferentStateType
    }
}
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
using System.Numerics;

#endregion

namespace voluntLib.managementLayer.localStateManagement.states
{
    public class NullState : ALocalState
    {
        public NullState() : base(NullStateID) {}

        public override byte[] Serialize()
        {
            return new byte[0];
        }

        public override StateRelation CompareWith(ALocalState candidate)
        {
            throw new NotImplementedException();
        }

        public override void MergeMetaData(ALocalState candidate) {}

        public override bool ContainsMoreInformationThan(ALocalState candidate)
        {
            throw new NotImplementedException();
        }

        public override BigInteger GetFreeBlock(List<BigInteger> workingOnSet)
        {
            throw new NotImplementedException();
        }

        public override bool IsBlockCalculated(BigInteger blockID)
        {
            throw new NotImplementedException();
        }

        public override void MarkBlockAsCalculated(BigInteger blockID) {}

        public override bool IsFinished()
        {
            return false;
        }

        protected override BigInteger GetNumberOfBlocks()
        {
            throw new NotImplementedException();
        }

        protected override BigInteger GetNumberOfCalculatedBlocks()
        {
            throw new NotImplementedException();
        }

        public static bool IsNullState(byte[] bytes)
        {
            return bytes.Length == new NullState().Serialize().Length;
        }
    }
}
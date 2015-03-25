﻿// Copyright 2014 Christopher Konze, University of Kassel
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

namespace voluntLib.common.eventArgs
{
    public class JobProgressEventArgs : EventArgs
    {
        private readonly BigInteger jobId;
        private readonly BigInteger numberOfBlocks;
        private readonly BigInteger numberOfCalculatedBlocks;
        private readonly List<byte[]> resultList;

        public JobProgressEventArgs(BigInteger jobId, List<byte[]> resultList, BigInteger numberOfBlocks, BigInteger numberOfCalculatedBlocks)
        {
            this.jobId = jobId;
            this.resultList = resultList;
            this.numberOfBlocks = numberOfBlocks;
            this.numberOfCalculatedBlocks = numberOfCalculatedBlocks;
        }

        public BigInteger NumberOfBlocks
        {
            get { return numberOfBlocks; }
        }

        public BigInteger JobId
        {
            get { return jobId; }
        }

        public List<byte[]> ResultList
        {
            get { return resultList; }
        }

        public BigInteger NumberOfCalculatedBlocks
        {
            get { return numberOfCalculatedBlocks; }
        }
    }
}
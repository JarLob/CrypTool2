/*
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

namespace VoluntLib2.ComputationLayer
{
    /// <summary>
    /// Event args for job progress events
    /// </summary>
    public class JobProgressEventArgs : EventArgs
    {
        private readonly BigInteger jobId;
        private readonly BigInteger numberOfBlocks;
        private readonly BigInteger numberOfCalculatedBlocks;
        private readonly List<byte[]> resultList;

        /// <summary>
        /// Creates a new JobProgressEventArgs
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="resultList"></param>
        /// <param name="numberOfBlocks"></param>
        /// <param name="numberOfCalculatedBlocks"></param>
        public JobProgressEventArgs(BigInteger jobId, List<byte[]> resultList, BigInteger numberOfBlocks, BigInteger numberOfCalculatedBlocks)
        {
            this.jobId = jobId;
            this.resultList = resultList;
            this.numberOfBlocks = numberOfBlocks;
            this.numberOfCalculatedBlocks = numberOfCalculatedBlocks > numberOfBlocks ? numberOfBlocks : numberOfCalculatedBlocks;
        }

        /// <summary>
        /// Number of blocks of the referenced job
        /// </summary>
        public BigInteger NumberOfBlocks
        {
            get { return numberOfBlocks; }
        }

        /// <summary>
        /// JobId of the referenced job
        /// </summary>
        public BigInteger JobId
        {
            get { return jobId; }
        }

        /// <summary>
        /// Current best list of the referenced job
        /// </summary>
        public List<byte[]> ResultList
        {
            get { return resultList; }
        }

        /// <summary>
        /// Number of calculated blocks of the referenced job
        /// </summary>
        public BigInteger NumberOfCalculatedBlocks
        {
            get { return numberOfCalculatedBlocks; }
        }
    }
}

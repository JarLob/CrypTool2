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

using System.Collections.Generic;
using System.Numerics;
using voluntLib.common;

#endregion

namespace voluntLib.managementLayer.dataStructs
{
    public class JobContainer
    {
        public JobContainer()
        {
            JobMap = new Dictionary<BigInteger, NetworkJob>();
            WorldMap = new Dictionary<string, List<NetworkJob>>();
        }

        private Dictionary<BigInteger, NetworkJob> JobMap { get; set; }
        private Dictionary<string, List<NetworkJob>> WorldMap { get; set; }

        public void AddJob(NetworkJob job)
        {
            if (!WorldMap.ContainsKey(job.World))
            {
                WorldMap.Add(job.World, new List<NetworkJob>());
            }

            if (!JobMap.ContainsKey(job.JobID))
            {
                JobMap.Add(job.JobID, job);
                WorldMap[job.World].Add(job);
            }
        }

        public void AddJobRange(IEnumerable<NetworkJob> job)
        {
            foreach (var networkJob in job)
            {
                AddJob(networkJob);
            }
        }

        public List<NetworkJob> GetJobsOfWorld(string world)
        {
            return WorldMap.ContainsKey(world) ? WorldMap[world] : new List<NetworkJob>();
        }

        public List<NetworkJob> GetJobs()
        {
            return new List<NetworkJob>(JobMap.Values);
        }

        /// <summary>
        ///   Gets the job or null if job not exists
        /// </summary>
        /// <param name="jobID">The job identifier.</param>
        /// <returns></returns>
        public NetworkJob GetJob(BigInteger jobID)
        {
            return JobMap.ContainsKey(jobID) ? JobMap[jobID] : null;
        }
    }
}
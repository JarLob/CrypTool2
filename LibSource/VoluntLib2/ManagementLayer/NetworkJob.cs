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
using VoluntLib2.ComputationLayer;

namespace VoluntLib2.ManagementLayer
{
    public class NetworkJob : IEquatable<NetworkJob>
    {        
        public NetworkJob(BigInteger jobID)
        {
            JobID = jobID;
        }

        public BigInteger JobID { get; set; }
        public string JobName { get; set; }
        public string WorldName { get; set; }        
        public string Creator { get; set; }
        public string JobType { get; set; }
        public string JobDescription { get; set; }
        public byte[] JobPayload { get; set; }
        //public EpochStateConfig StateConfig { get; set; }
        public bool IsDeleted { get; set; }

        public bool Equals(NetworkJob other)
        {
            return other.JobID.Equals(JobID);
        }

        public bool HasPayload()
        {
            return JobPayload != null;
        }

        public override int GetHashCode()
        {
            return JobID.GetHashCode();
        }

        public byte[] Serialize()
        {
            byte[] data = new byte[0];



            return data;
        }

        public void Deserialize()
        {

        }
    }
}

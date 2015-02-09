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
using System.Numerics;

#endregion

namespace voluntLib.common
{
    public class WorkLog
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public BigInteger JobID { get; set; }
        public DateTime LastReceivedMessage { get; set; }

        public override string ToString()
        {
            return Name + "@" + Hostname;
        }

        #region equals, hashCode overrides

        protected bool Equals(WorkLog other)
        {
            return string.Equals(Name, other.Name)
                   && string.Equals(Hostname, other.Hostname)
                   && Equals(JobID, other.JobID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WorkLog) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Hostname != null ? Hostname.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (JobID != null ? JobID.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
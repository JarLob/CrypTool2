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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using voluntLib.common;
using voluntLib.communicationLayer.messages.messageWithCertificate;

#endregion

namespace voluntLib.managementLayer.dataStructs
{
    public class WorkingPeerList
    {
        private readonly ConcurrentDictionary<BigInteger, List<WorkLog>> workingPeers;

        public WorkingPeerList()
        {
            workingPeers = new ConcurrentDictionary<BigInteger, List<WorkLog>>();
            RemoveAfterMS = -1;
        }

        public long RemoveAfterMS { get; set; }

        public void AddOrUpdate(PropagateStateMessage message)
        {
            var header = message.Header;
            var workLog = new WorkLog
            {
                Hostname = header.HostName,
                JobID = header.JobID,
                LastReceivedMessage = DateTime.Now,
                Name = header.SenderName
            };
            AddOrUpdate(workLog);
        }

        public void AddOrUpdate(WorkLog log)
        {
            var list = workingPeers.GetOrAdd(log.JobID, new List<WorkLog>());
            lock (list)
            {
                var find = list.Find(workLog => workLog.Equals(log));
                if (find != null)
                {
                    find.LastReceivedMessage = log.LastReceivedMessage;
                } else
                {
                    list.Add(log);
                }
            }
        }

        public List<WorkLog> WorklogByJobID(BigInteger i)
        {
            List<WorkLog> workLogs;
            if (!workingPeers.TryGetValue(i, out workLogs))
            {
                return new List<WorkLog>();
            }

            if (RemoveAfterMS == -1)
            {
                return new List<WorkLog>(workLogs);
            }

            lock (workLogs)
            {
                workLogs.RemoveAll(log => ((DateTime.Now - log.LastReceivedMessage).TotalMilliseconds > RemoveAfterMS));
            }
            return new List<WorkLog>(workLogs);
        }
    }
}
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

#endregion

namespace voluntLib.common.interfaces
{
    public abstract class ACalculationTemplate
    {
        public AWorker WorkerLogic { get; protected set; }
        public abstract List<byte[]> MergeResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList);
    }

    public abstract class ACalculationTemplate<T> : ACalculationTemplate where T : ISerializable, new()
    {
        public override List<byte[]> MergeResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList)
        {
            Func<byte[], T> byteToT = entry => 
            {
                var t = new T();
                t.Deserialize(entry);
                return t;
            };

            var oldResultsAsT = oldResultList.Select(byteToT);
            var newResultsAsT = newResultList.Select(byteToT);

            var mergeResultsAsT = MergeResults(oldResultsAsT, newResultsAsT);

            var mergeResults = mergeResultsAsT.Select(entry => entry.Serialize()).ToList();
            return mergeResults;
        }

        public abstract List<T> MergeResults(IEnumerable<T> oldResultList, IEnumerable<T> newResultList);
     
    }
}
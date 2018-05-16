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
using System.Threading;
using System.Threading.Tasks;

namespace VoluntLib2.ComputationLayer
{
    public abstract class AWorker
    {
        public byte[] JobId { get; set; }
        public abstract CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken);

        public event EventHandler<TaskEventArgs> ProgressChanged;

        protected virtual void OnProgressChanged(BigInteger blockID, int progress)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var handler = ProgressChanged;
                    if (handler != null)
                    {
                        handler(this, new TaskEventArgs(JobId, blockID, TaskEventArgType.Progress) { TaskProgress = progress });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}

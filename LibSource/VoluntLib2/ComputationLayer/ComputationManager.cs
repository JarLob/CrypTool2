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
using System.Text;
using System.Threading.Tasks;
using VoluntLib2.ManagementLayer;

namespace VoluntLib2.ComputationLayer
{
    internal class ComputationManager
    {

        public ComputationManager(VoluntLib voluntLib, JobManager jobManager)
        {

        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        internal bool JoinJob(System.Numerics.BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {            
            return false;
        }

        internal void StopJob(System.Numerics.BigInteger jobID)
        {
           
        }
    }
}

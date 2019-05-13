/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org
   Simulated Annealing algorithms by George Lasry

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

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// This class implements the fixed temperature simulated annealing
    /// and the Cowan churn simulated annealing
    /// </summary>
    public class SimulatedAnnealing
    {
        private Random _random = new Random();
        private double _fixedTemperature = 0;

        public SimulatedAnnealing(double fixedTemperature)
        {
            _fixedTemperature = fixedTemperature;
        }       

        /// <summary>
        /// Simulated Annealing Acceptance Function – Constant _temperature
        /// </summary>
        /// <param name="newKeyScore"></param>
        /// <param name="currentKeyScore"></param>
        /// <returns></returns>
        public bool AcceptWithConstantTemperature(double newKeyScore, double currentKeyScore)
        {
            // Always accept better keys
            if (newKeyScore > currentKeyScore)
            {
                return true;
            }
            // Degradation between current key and new key.
            double degradation = currentKeyScore - newKeyScore;
            double acceptanceProbability = Math.Pow(Math.E, -degradation / _fixedTemperature);
            return acceptanceProbability > 0.0085 && _random.NextDouble() < acceptanceProbability;
        }           
    }
}

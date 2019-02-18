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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// This class implements the fixed temperature simulated annealing
    /// and the Cowan churn simulated annealing
    /// </summary>
    public class SimulatedAnnealing
    {
        private Random _random = new Random();

        // Fixed temperature optimized for hexagram scoring
        public const double FIXED_TEMPERATURE = 20000;
        // Size of degradation threshold lookup table.
        private const int LOOKUP_TABLE_SIZE = 100;
        // The churn algorithm lookup table of degradation thresholds.
        private readonly double[] _degradationLookupTable;

        public SimulatedAnnealing()
        {
            _degradationLookupTable = new double[LOOKUP_TABLE_SIZE];
            ComputeDegradationLookupTable();
        }

        /// <summary>
        /// Compute the churn algorithm lookup table of degradation thresholds
        /// </summary>
        private void ComputeDegradationLookupTable() 
        {
            for (int index = 0; index < LOOKUP_TABLE_SIZE; index++)
            {
                _degradationLookupTable[index] = FIXED_TEMPERATURE * Math.Log(LOOKUP_TABLE_SIZE / (index + 1));
            }
        }

        /// <summary>
        /// Simulated Annealing Acceptance Function – Constant Temperature
        /// </summary>
        /// <param name="newKeyScore"></param>
        /// <param name="currentKeyScore"></param>
        /// <returns></returns>
        public bool AcceptWithConstantTemperature(double newKeyScore, double currentKeyScore)
        {
            // Always AcceptWithChurn better keys
            if (newKeyScore > currentKeyScore)
            {
                return true;
            }

            // Degradation between current key and new key
            double degradation = currentKeyScore - newKeyScore;
            double acceptanceProbability = Math.Pow(Math.E, - degradation / FIXED_TEMPERATURE);
            return _random.NextDouble() < acceptanceProbability;
        }

        
        /// <summary>
        /// Simulated Annealing acceptance function - Churn implementation
        /// </summary>
        /// <param name="newKeyScore"></param>
        /// <param name="currentKeyScore"></param>
        /// <returns></returns>
        public bool AcceptWithChurn(double newKeyScore, double currentKeyScore)
        {
            // Always AcceptWithChurn better keys
            if (newKeyScore > currentKeyScore)
            {
                return true;
            }
            // Fetch a Random degradation threshold from the lookup table
            int randomIndex = _random.Next(LOOKUP_TABLE_SIZE);
            double degradationRandomThreshold = _degradationLookupTable[randomIndex];
            // Degradation between current key and new key
            double degradation = currentKeyScore - newKeyScore;
            return degradation < degradationRandomThreshold;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PlayfairAnalysis.Churn
{

    public class FixedTemperatureSimulatedAnnealing
    {

        private Random random = new Random();

        // Fixed temperature optimized for hexagram scoring
        private static double FIXED_TEMPERATURE = 20_000.0;

        /**
         * Simulated annealing acceptance function.
         *
         * @param newKeyScore - score for the ney key
         * @param currentKeyScore - score for the current key
         * @return true if new key should be accepted
         */
        bool accept(double newKeyScore, double currentKeyScore)
        {

            // Always accept better keys
            if (newKeyScore > currentKeyScore)
            {
                return true;
            }

            // Degradation between current key and new key.
            double degradation = currentKeyScore - newKeyScore;

            double acceptanceProbability =
                    Math.Pow(Math.E, -degradation / FIXED_TEMPERATURE);

            return random.NextDouble() < acceptanceProbability;
        }
    }

}

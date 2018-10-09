﻿using System;
using System.Collections.Generic;
using System.Text;
using common;

namespace ADFGVXAnalyzer
{
    public class SimulatedAnnealing
    {
        private static Random random = new Random();

        public static bool accept(double newScore, double currLocalScore, double temperature)
        {

            double diffScore = newScore - currLocalScore;
            if (diffScore > 0)
            {
                return true;
            }
            double prob = Math.Pow(Math.E, diffScore / temperature);
            if (prob < 0.0085)
            {
                return false;
            }

            double probThreshold = random.NextDouble(); // 0.0 to 1.0
            return prob > probThreshold;

        }
    }
}

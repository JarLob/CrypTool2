using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class ResultViewAnyRoundEventArgs : EventArgs
    {
        public DateTime startTime;
        public DateTime endTime;
        public int round;
        public double currentExpectedProbability;
        public string expectedDifference;
        public int expectedHitCount;
        public string currentKeyCandidate;
        public int messagePairCountToExamine;
        public int messagePairCountFilteredToExamine;
        public string currentRecoveredRoundKey;
        public int currentKeysToTestThisRound;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class ResultViewLastRoundEventArgs : EventArgs
    {
        public DateTime startTime;
        public DateTime endTime;
        public string currentPlainText;
        public string currentCipherText;
        public string currentKeyCandidate;
        public string expectedDifference;
        public int round;
        public int currentKeysToTestThisRound;
        public int remainingKeyCandidates;
        public int examinedPairCount;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResultViewLastRoundEventArgs()
        {
            
        }
    }
}

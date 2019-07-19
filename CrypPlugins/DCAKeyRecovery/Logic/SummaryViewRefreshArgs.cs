using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class SummaryViewRefreshArgs
    {
        public int currentRound;
        public bool firstEvent;
        public bool lastEvent;
        public Algorithms currentAlgorithm;
        public SummaryLastRound lastRoundSummary = null;
        public SummaryAnyRound anyRoundSummary = null;
    }
}

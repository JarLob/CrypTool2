using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class SummaryAnyRound
    {
        public DateTime startTime;
        public DateTime endTime;
        public int messageCount;
        public int decryptionCount;
        public int testedKeys;
        public string recoveredSubKey;
    }
}

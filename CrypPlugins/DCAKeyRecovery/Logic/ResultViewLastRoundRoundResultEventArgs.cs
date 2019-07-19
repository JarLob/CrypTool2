using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class ResultViewLastRoundRoundResultEventArgs : EventArgs
    {
        private RoundResult _roundResult;

        /// <summary>
        /// Property for round result
        /// </summary>
        public RoundResult RoundResult
        {
            get { return _roundResult; }
            set { _roundResult = value; }
        }
    }
}

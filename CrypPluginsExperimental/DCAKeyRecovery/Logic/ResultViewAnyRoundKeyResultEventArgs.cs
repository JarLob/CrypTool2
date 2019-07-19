using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCAKeyRecovery.UI.Models;

namespace DCAKeyRecovery.Logic
{
    public class ResultViewAnyRoundKeyResultEventArgs
    {
        public List<KeyResult> keyResults;

        /// <summary>
        /// Constructor
        /// </summary>
        public ResultViewAnyRoundKeyResultEventArgs()
        {
            keyResults = new List<KeyResult>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public abstract class DifferentialKeyRecoveryAttack
    {
        public List<DifferentialAttackRoundConfiguration> RoundConfigurations;
        public List<DifferentialAttackRoundResult> RoundResults;
        public DifferentialAttackLastRoundResult LastRoundResult;

        public abstract string printRecoveredSubkeyBits();
    }
}

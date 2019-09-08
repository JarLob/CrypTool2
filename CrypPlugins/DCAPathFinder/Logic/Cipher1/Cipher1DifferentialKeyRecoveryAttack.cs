using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.Logic.Cipher1
{
    public class Cipher1DifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        //indicates if a subkey is recovered
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        public Cipher1DifferentialKeyRecoveryAttack()
        {
            recoveredSubkey1 = false;
            recoveredSubkey0 = false;
        }
    }
}
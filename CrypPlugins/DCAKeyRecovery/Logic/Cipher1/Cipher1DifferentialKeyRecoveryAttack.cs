using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic.Cipher1
{
    public class Cipher1DifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        public int subkey1;
        public int subkey0;

        public override string printRecoveredSubkeyBits()
        {
            throw new NotImplementedException();
        }
    }
}

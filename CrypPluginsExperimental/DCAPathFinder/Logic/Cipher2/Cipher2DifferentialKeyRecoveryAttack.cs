using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.Logic.Cipher2
{
    class Cipher2DifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        //saves the already attacked SBoxes
        public bool[] attackedSBoxesRound3;
        public bool[] attackedSBoxesRound2;

        //indicates if a subkey is recovered
        public bool recoveredSubkey3;
        public bool recoveredSubkey2;
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2DifferentialKeyRecoveryAttack()
        {
            attackedSBoxesRound3 = new bool[4];
            attackedSBoxesRound2 = new bool[4];
        }
    }
}

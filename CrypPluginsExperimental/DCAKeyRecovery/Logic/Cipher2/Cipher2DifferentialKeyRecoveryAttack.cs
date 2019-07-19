using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic.Cipher2
{
    public class Cipher2DifferentialKeyRecoveryAttack : DifferentialKeyRecoveryAttack
    {
        //saves the already attacked SBoxes
        public bool[] attackedSBoxesRound3;
        public bool[] attackedSBoxesRound2;

        //indicates if a subkey is recovered
        public bool recoveredSubkey3;
        public bool recoveredSubkey2;
        public bool recoveredSubkey1;
        public bool recoveredSubkey0;

        //saves the recovered subkeys
        public UInt16 subkey3;
        public UInt16 subkey2;
        public UInt16 subkey1;
        public UInt16 subkey0;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2DifferentialKeyRecoveryAttack()
        {
            RoundConfigurations = new List<DifferentialAttackRoundConfiguration>();
            RoundResults = new List<DifferentialAttackRoundResult>();

            attackedSBoxesRound3 = new bool[4];
            attackedSBoxesRound2 = new bool[4];
        }

        public override string printRecoveredSubkeyBits()
        {
            throw new NotImplementedException();
        }
    }
}

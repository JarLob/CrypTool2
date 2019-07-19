using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic.Cipher2
{
    public static class Cipher2Configuration
    {
        public static readonly UInt16 BITWIDTHCIPHER2 = 4;
        public static readonly UInt16 SBOXNUM = 4;
        public static readonly UInt16[] SBOX = { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 };
        public static readonly UInt16[] SBOXREVERSE = { 1, 8, 14, 5, 13, 7, 4, 11, 15, 2, 0, 12, 10, 9, 3, 6 };
        public static readonly UInt16[] PBOX = { 12, 9, 6, 3, 0, 13, 10, 7, 4, 1, 14, 11, 8, 5, 2, 15 };
        public static readonly UInt16[] PBOXREVERSE = { 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11, 0, 5, 10, 15 };
        public static readonly double PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = 0.001;
        public static readonly double PROBABILITYBOUNDDIFFERENTIALSEARCH = 0.0001;
    }
}

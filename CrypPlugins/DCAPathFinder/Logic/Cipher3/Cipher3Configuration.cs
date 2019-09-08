using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.Logic.Cipher3
{
    public static class Cipher3Configuration
    {
        public static readonly UInt16 BITWIDTHCIPHER2 = 4;
        public static readonly UInt16 SBOXNUM = 4;
        public static readonly UInt16[] SBOX = {6, 4, 12, 5, 0, 7, 2, 14, 1, 15, 3, 13, 8, 10, 9, 11};
        public static readonly UInt16[] PBOX = {0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15};
        public static readonly UInt16[] SBOXREVERSE = {4, 8, 6, 10, 1, 3, 0, 5, 12, 14, 13, 15, 2, 11, 7, 9};
        public static readonly UInt16[] PBOXREVERSE = {0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15};
        public static readonly double PROBABILITYBOUNDBESTCHARACTERISTICSEARCH = 0.001;
        public static readonly double PROBABILITYBOUNDDIFFERENTIALSEARCH = 0.0001;
    }
}
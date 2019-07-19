using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic.Cipher1
{
    static class Cipher1Configuration
    {
        public static readonly int BITWIDTHCIPHERFOUR = 16;
        public static readonly int SBOXNUM = 4;
        public static readonly int KEYNUM = 2;
        public static readonly UInt16[] SBOX = { 6, 4, 12, 5, 0, 7, 2, 14, 1, 15, 3, 13, 8, 10, 9, 11 };
        public static readonly UInt16[] SBOXREVERSE = { 4, 8, 6, 10, 1, 3, 0, 5, 12, 14, 13, 15, 2, 11, 7, 9 };
    }
}

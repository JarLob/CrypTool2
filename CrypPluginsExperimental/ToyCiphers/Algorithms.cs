using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCiphers
{
    /// <summary>
    /// Cipher1 = 16 bit blocksize, 2 subkeys, 32 bit key
    /// Cipher2 = 16 bit blocksize, 4 subkeys, 64 bit key
    /// Cipher3 = 16 bit blocksize, 6 subkeys, 96 bit key
    /// Cipher4 = 4 bit blocksize, 4 subkeys, 16 bit key
    /// </summary>
    public enum Algorithms
    {
        Cipher1,
        Cipher2,
        Cipher3,
        Cipher4
    }
}

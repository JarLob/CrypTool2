using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Cryptool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupCipherText
    {
        public ECPoint U1 { get; set; }// kG1
        public ECPoint U2 { get; set; }// kG2
        //public byte[] E { get; set; }// H(kH) xor m

        //Verify
        public ECPoint V { get; set; }// kC+kaD
    }
}

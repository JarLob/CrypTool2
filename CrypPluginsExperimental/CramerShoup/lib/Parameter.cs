using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math.EC;

namespace Cryptool.Plugins.CramerShoup.lib
{
    public abstract class ECCramerShoupParameter
    {
        public LongDigest Digest { get; set; }
        public ECCurve E { get; set; }
        public BigInteger Q { get; set; }
        public ECPoint G1 { get; set; }
        public ECPoint G2 { get; set; }
    }
}

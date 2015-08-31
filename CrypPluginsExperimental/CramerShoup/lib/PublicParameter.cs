using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace Cryptool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupPublicParameter : ECCramerShoupParameter
    {
        public ECPoint C { get; set; }
        public ECPoint D { get; set; }
        public ECPoint H { get; set; }
    }
}

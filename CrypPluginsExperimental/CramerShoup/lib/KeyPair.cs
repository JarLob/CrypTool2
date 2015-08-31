using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupKeyPair
    {
        public ECCramerShoupPrivateParameter Priv { get; set; }
        public ECCramerShoupPublicParameter Public { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.HKDFSHA256
{
    class TooMuchOutputRequestedException : Exception
    {
        public TooMuchOutputRequestedException(string message) : base(message)
        {

        }
    }
}

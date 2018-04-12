using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.KPFSHA256
{
    class ToMuchOutputRequestedException : Exception
    {
        public ToMuchOutputRequestedException(string message) : base(message)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoluntLib2.Tools
{
    public class ConnectionsNumberChangedEventArgs : EventArgs
    {
        public int ConnectionsNumber { get; set; }
    }
}

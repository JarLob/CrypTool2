using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoluntLib2.ConnectionLayer;

namespace VoluntLib2.Tools
{
    public class ConnectionsNumberChangedEventArgs : EventArgs
    {
        public List<Contact> Contacts { get; set; }
    }
}

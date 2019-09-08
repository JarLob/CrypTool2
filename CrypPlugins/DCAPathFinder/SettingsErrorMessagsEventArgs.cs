using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder
{
    public class SettingsErrorMessagsEventArgs : EventArgs
    {
        public string message;

        public SettingsErrorMessagsEventArgs()
        {
        }

        public SettingsErrorMessagsEventArgs(string message)
        {
            this.message = message;
        }
    }
}
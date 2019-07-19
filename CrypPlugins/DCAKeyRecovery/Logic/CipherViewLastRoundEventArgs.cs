using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    public class CipherViewLastRoundEventArgs : EventArgs
    {
        public string M1XORM2;
        public string C1XORC2;
        public string UBits;
        public string SBoxUBits;
        public string VBits;
        public string K1Bits;
        public ObservableCollection<int> KeyCandidates;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using voluntLib.common;

namespace CrypCloud.Manager.ViewModels.Pocos
{
    public class NetworkJobItem
    {
        public BigInteger NumberOfBlocks { get; set; }
        public string Creator { get; set; }
        public string Name { get; set; }
        public BigInteger Id { get; set; }
        public bool UserCanDeleteJob { get; set; }
        public bool HasWorkspace { get; set; }
        public bool DownloadingWorkspace { get; set; }
    }
}

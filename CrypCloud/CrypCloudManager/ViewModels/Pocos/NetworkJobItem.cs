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
        public BigInteger Id { get; set; }
        public BigInteger TotalNumberOfBlocks { get; set; }
        public BigInteger FinishedNumberOfBlocks { get; set; }

        public string Creator { get; set; }
        public string Name { get; set; }
        public bool UserCanDeleteJob { get; set; }
        public bool HasWorkspace { get; set; }
        public bool DownloadingWorkspace { get; set; }

        public string PrintableID
        {
            get { return Id.ToString("X"); }
            set {}
        }

        public string ProgressPercentage
        {
            get
            {
                var tmp = BigInteger.Multiply(FinishedNumberOfBlocks, 100);
                var progress = BigInteger.Divide(tmp, TotalNumberOfBlocks);
                return progress.ToString();
            }
            set{} //for binding only
        }

        public string ProgressTotal
        {
            get
            {
                return FinishedNumberOfBlocks + "/" + TotalNumberOfBlocks;
            }
            set { } //for binding only
        }  
    }
}

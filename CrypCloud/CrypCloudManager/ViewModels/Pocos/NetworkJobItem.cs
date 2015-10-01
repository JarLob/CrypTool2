using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media.Imaging;
using voluntLib.common;

namespace CrypCloud.Manager.ViewModels.Pocos
{
    public class NetworkJobItem
    {
        private const int LenghtJobid = 32;

        public BigInteger Id { get; set; }
        public BigInteger TotalNumberOfBlocks { get; set; }
        public BigInteger FinishedNumberOfBlocks { get; set; }

        public string Creator { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BigInteger Epoch { get; set; }
        public BitmapSource Visualization{ get; set; }
        public bool UserCanDeleteJob { get; set; }
        public bool HasWorkspace { get; set; }
        public bool DownloadingWorkspace { get; set; }

        public string PrintableID
        {
            get
            {
                var idAsHexString = Id.ToString("X");
                return idAsHexString.Length == LenghtJobid ? idAsHexString 
                                                           : idAsHexString.PadLeft(LenghtJobid, '0');
            }
            set {}
        }

        public string ProgressPercentage
        {
            get
            {
                if (TotalNumberOfBlocks == 0)
                {
                    return "";
                }

                var tmp = BigInteger.Multiply(FinishedNumberOfBlocks, 100);
                var progress = BigInteger.Divide(tmp, TotalNumberOfBlocks);
                return progress.ToString();
            }
            set{} //for binding only
        }

        public string EpochProgress
        {
            get
            {
                return Epoch + "/" + MaxEpoch;
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

        public BigInteger MaxEpoch { get; set; }
    }
}

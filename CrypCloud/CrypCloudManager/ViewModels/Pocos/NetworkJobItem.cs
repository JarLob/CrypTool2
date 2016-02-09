using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        public string ShortID
        {
            get
            {
                if (PrintableID.Length <= 5) return PrintableID;

                return PrintableID.Substring(0, 5);
            }
            set { }
        }

        private sealed class IdEqualityComparer : IEqualityComparer<NetworkJobItem>
        {
            public bool Equals(NetworkJobItem x, NetworkJobItem y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(NetworkJobItem obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<NetworkJobItem> IdComparerInstance = new IdEqualityComparer();

        public static IEqualityComparer<NetworkJobItem> IdComparer
        {
            get { return IdComparerInstance; }
        }

        protected bool Equals(NetworkJobItem other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NetworkJobItem) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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

   
        public string ProgressTotal
        {
            get
            {
                var doneBlocks = FinishedNumberOfBlocks.ToString("N0", new CultureInfo("de-DE"));
                var totalBlocks = TotalNumberOfBlocks.ToString("N0", new CultureInfo("de-DE"));

                return string.Format("{0} / {1}", doneBlocks, totalBlocks);
            }
            set { } //for binding only
        }

        public BigInteger MaxEpoch { get; set; }

        public double EpochProgress { get; set; }

        public string EpochProgressAsString
        {
            get { return Epoch + "/" + MaxEpoch; }
            set { }
        }
    }
}

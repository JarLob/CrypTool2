using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CrypCloud.Manager.ViewModel
{
    public class NetworkJobVM
    {
        public string Name { get; set; }
        public string Creator { get; set; }
        public string LocalFilePath { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public BigInteger NumberOfBlocks { get; set; }
        public BigInteger Id { get; set; }

        public override string ToString()
        {
            return "Name: " + Name +
                    "id: " + Id +
                    "Creator: " + Creator +
                    "Type: " + Type + 
                    "LocalFilePath: " + LocalFilePath +
                    "NumberOfBlocks: " + NumberOfBlocks + 
                    "Descropton: " + Description;
        }
    }
}

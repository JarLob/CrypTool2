using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.RandNumGen
{
    class LCG : IrndNum
    {
        public LCG(BigInteger Seed, BigInteger Modul, BigInteger a, BigInteger b, BigInteger OutputLength) : base()
        {
            this.Seed = Seed;
            this.Modulus = Modul;
            this.A = a;
            this.B = b;
            this.OutputLength = OutputLength;
            //RandNo takes value of the seed
            this.RandNo = this.Seed;
        }

        /// <summary>
        /// generates the output
        /// </summary>
        /// <returns></returns>
        public override byte[] generateRNDNums()
        {
            byte[] res = new byte[(int)OutputLength];
            for (int j = 0; j < OutputLength; j++)
            {
                int curByte = 0;
                int tmp = 128;
                for (int i = 0; i < 8; i++)
                {
                    this.randomize();
                    if (randBit() != 0)
                    {
                        curByte += tmp;
                    }
                    tmp /= 2;
                }
                res[j] = Convert.ToByte(curByte);
            }
            return res;
        }

        /// <summary>
        /// returns next random bit
        /// </summary>
        /// <returns></returns>
        public override BigInteger randBit()
        {
            return (RandNo > Modulus / 2) ? 1 : 0;
        }

        /// <summary>
        /// randomize RandNo
        /// </summary>
        public override void randomize()
        {
            RandNo= (A * RandNo + B) % Modulus;
        }
    }
}

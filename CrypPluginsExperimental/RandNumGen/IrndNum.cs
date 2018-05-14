using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.RandNumGen
{

    /// <summary>
    /// abstract class for randomnumber generators
    /// </summary>
    public abstract class IrndNum
    {
        /// <summary>
        /// needed attributes
        /// </summary>
        private BigInteger _seed;
        private BigInteger _modulus;
        private BigInteger _randNo;
        private BigInteger _a;
        private BigInteger _b;
        private BigInteger _outputLength;

        /// <summary>
        /// getter, setter for the seed
        /// </summary>
        public BigInteger Seed
        {
            set
            {
                _seed = value;
            }
            get
            {
                return _seed;
            }
        }

        /// <summary>
        /// getter, setter for modulus
        /// </summary>
        public BigInteger Modulus
        {
            set
            {
                _modulus = value;
            }
            get
            {
                return _modulus;
            }
        }

        /// <summary>
        /// getter, setter for randNo
        /// </summary>
        public BigInteger RandNo
        {
            set
            {
                _randNo = value;
            }
            get
            {
                return _randNo;
            }
        }

        /// <summary>
        /// getter, setter for a
        /// </summary>
        public BigInteger A
        {
            set
            {
                _a = value;
            }
            get
            {
                return _a;
            }
        }

        /// <summary>
        /// getter, setter for b
        /// </summary>
        public BigInteger B
        {
            set
            {
                _b = value;
            }
            get
            {
                return _b;
            }
        }

        /// <summary>
        /// getter, setter for OutputLength
        /// </summary>
        public BigInteger OutputLength
        {
            get 
            { 
                return _outputLength; 
            }
            set 
            {
                _outputLength = value; 
            }
        }

        /// <summary>
        /// must be implemented in inherting class
        /// </summary>
        public abstract void randomize();

        /// <summary>
        /// must be implemented in inherting class
        /// </summary>
        /// <returns></returns>
        public abstract BigInteger randBit();

        /// <summary>
        /// must be implemented in inherting class
        /// </summary>
        /// <returns></returns>
        public abstract byte[] generateRNDNums();
        
    }
}

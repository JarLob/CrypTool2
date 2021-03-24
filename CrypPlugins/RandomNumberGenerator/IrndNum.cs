﻿/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>
   Author: Christian Bender, Universität Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/


using System.Numerics;

namespace CrypTool.Plugins.RandomNumberGenerator
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

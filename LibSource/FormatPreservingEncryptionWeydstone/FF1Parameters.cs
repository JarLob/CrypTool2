﻿/**
 * Format-Preserving Encryption
 * 
 * Copyright (c) 2016 Weydstone LLC dba Sutton Abinger
 * 
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Sutton Abinger licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of the License
 * at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
/*
* Converted and modified by Alexander Hirsch <hirsch@cryptool.org>
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using static FormatPreservingEncryptionWeydstone.Common;

namespace FormatPreservingEncryptionWeydstone
{
    public class FF1Parameters : FFXParameters
    {

        public FF1Parameters(int radix, Ciphers ciphers)
        {
            this.ciphers = ciphers;
            this.radix = radix;
            ff1Round = new FF1RoundFunction(radix, ciphers);
        }

        /**
	     * The radix specified in this parameter set.
	     */
        readonly int radix;

        /**
	     * Instances of AES ciphers for PRF and CIPH algorithms.
	     */
        readonly Ciphers ciphers;

        /**
	     * Split function for FF1.
	     */
        readonly SplitFunction ff1Splitter = new FF1SplitFunction();
        class FF1SplitFunction : SplitFunction
        {
            public int split(int n)
            {
                // validate n
                if (n < Constants.MINLEN || n > Constants.MAXLEN)
                    throw new ArgumentException(
                            "n must be in the range [" + Constants.MINLEN + ".." + Constants.MAXLEN + "].");
                return floor(n / 2.0);
            }
        }

        /**
	     * Function to determine the number of Feistel rounds for FF1.
	     */
        readonly RoundCounter ff1RoundCounter = new FF1RoundCounter();
        class FF1RoundCounter : RoundCounter
        {
            public int rnds(int n)
            {
                return 10;
            }
        }

        /**
	     * Round function F for FF1, derived from NIST SP 800-38G.
	     */

        readonly RoundFunction ff1Round;
        class FF1RoundFunction : RoundFunction
        {
            public FF1RoundFunction(int radix, Ciphers ciphers)
            {
                this.ciphers = ciphers;
                this.radix = radix;
            }

            private int radix;

            private Ciphers ciphers;

            public bool validKey(byte[] K)
            {
                // validate K
                if (K == null)
                    return false;
                //TODO
                //if (!K.getAlgorithm().equals("AES"))
                // return false;
                return true;
            }


            public int[] F(byte[] K, int n, byte[] T, int i, int[] B)
            {

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Round #" + i + "\n");
                }

                // value of t for readability
                int t = T.Length;

                // 1. Let u = floor(n/2); v = n â€“ u.
                int u = floor(n / 2.0);
                int v = n - u;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 1\n\tu is " + u + ", v is " + v);
                }

                // 2. Let A = X[1..u]; B = X[u + 1..n].
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 2\n\tB is " + intArrayToString(B));
                }

                // 3. Let b = ceiling(ceiling(v * LOG(radix))/8).
                int b = ceiling(ceiling(v * log2(radix)) / 8.0);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 3\n\tb is " + b);
                }

                // 4. Let d = 4 * ceiling(b/4) + 4.
                int d = 4 * ceiling(b / 4.0) + 4;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4\n\td is " + d);
                }

                // 5. Let P = [1]^1 || [2]^1 || [1]^1 || [radix]^3 || [10]^1 ||
                // [u mod 256]^1 || [n]^4 || [t]^4 .
                byte[] tbr = bytestring(radix, 3);
                byte[] fbn = bytestring(n, 4);
                byte[] fbt = bytestring(t, 4);
                byte[] P = { (byte) 0x01, (byte) 0x02, (byte) 0x01, tbr[0], tbr[1], tbr[2], (byte) 0x0A,
                        (byte) (mod(u, 256) & 0xFF), fbn[0], fbn[1], fbn[2], fbn[3], fbt[0], fbt[1], fbt[2], fbt[3] };
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 5\n\tP is " + unsignedByteArrayToString(P));
                }

                // i. Let Q = T || [0]^(-t-b-1) mod 16 || [i]^1 || [NUMradix
                // (B)]^b
                byte[] Q = concatenate(T, bytestring(0, mod(-t - b - 1, 16)));
                Q = concatenate(Q, bytestring(i, 1));
                Q = concatenate(Q, bytestring(num(B, radix), b));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 6.i.\n\tQ is " + unsignedByteArrayToString(Q));
                }

                // ii. Let R = PRF(P || Q).
                byte[] R = ciphers.prf(K, concatenate(P, Q));
                // byte[] R = concatenate(prf(K, concatenate(P, Q)), P);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 6.ii.\n\tR is " + unsignedByteArrayToString(R));
                }

                // iii. Let S be the first d bytes of the following string of
                // ceiling(d/16) blocks: R || CIPH K (R xor [1]^16 ) || CIPH K
                // (R xor [2]^16 ) â€¦ CIPH K (R xor [ceiling(d/16)â€“1]^16 ).
                byte[] S = R;
                for (int j = 1; j <= ceiling(d / 16.0) - 1; j++)
                {
                    S = concatenate(S, ciphers.ciph(K, xor(R, bytestring(j, 16))));
                }
                //TODO SUPER CRITICAL
                // padding
                //S = Arrays.copyOf(S, d);
                S = S.Take(d).ToArray();
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 6.iii.\n\tS is " + byteArrayToHexString(S));
                }

                // iv. Let y = NUM(S).
                BigInteger y = num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 6.iv.\n\ty is " + y);
                }

                // v. If i is even, let m = u; else, let m = v.
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 6.v.\n\tm is " + m);
                }

                // constrain y to the range [0..radix^m]
                y = mod(y, BigInteger.Pow(radix, m));

                // Step 7. Let Y = STR m radix (y).
                int[] Y = str(y, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 7.\n\tY is " + intArrayToString(Y) + "\n");
                }

                return Y;
            }
        };

        /**
	     * Construct a new FF1Parameters instance with the specified radix.
	     * 
	     * @param radix
	     *            the radix for FF1 operations
	     */
        public FF1Parameters(int radix)
        {
            this.radix = radix;
            ciphers = new Ciphers();
            ff1Round = new FF1RoundFunction(radix, ciphers);
        }


        public int getRadix()
        {
            return radix;
        }


        public int getMinLen()
        {
            return Constants.MINLEN;
        }


        public int getMaxLen()
        {
            return Constants.MAXLEN;
        }


        public int getMinTLen()
        {
            return 0;
        }


        public int getMaxTLen()
        {
            return Constants.MAXLEN;
        }


        public ArithmeticFunction getArithmeticFunction()
        {
            return FFX.getBlockwiseArithmeticFunction(radix);
        }


        public FeistelMethod getFeistelMethod()
        {
            return FeistelMethod.TWO;
        }


        public SplitFunction getSplitter()
        {
            return ff1Splitter;
        }


        public RoundCounter getRoundCounter()
        {
            return ff1RoundCounter;
        }


        public RoundFunction getRoundFunction()
        {
            return ff1Round;
        }
    }
}

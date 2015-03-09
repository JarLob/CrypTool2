﻿/*
   Copyright 2010 Sven Rech (svenrech at cryptool dot org), University of Duisburg-Essen

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

/*
 * This class provides some additional functionality for the BigInteger class.
 * The parser stuff is written by Sven Rech and Nils Kopal.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Security.Cryptography;
using System.Globalization;

namespace Cryptool.PluginBase.Miscellaneous
{
    public static class BigIntegerHelper
    {
        #region internal stuff of expression parser

        delegate BigInteger FunctionDelegate(BigInteger[] args);
        delegate BigInteger Function2Delegate(BigInteger a, BigInteger b);

        private struct FunctionInfo
        {
            public FunctionDelegate function;
            public string pattern;
            public int numargs;
            public FunctionInfo(FunctionDelegate function, string pattern, int numargs)
            {
                this.function = function; this.pattern = pattern; this.numargs = numargs;
            }
        }

        private struct OperatorInfo
        {
            public Function2Delegate function;
            public Priority priority;
            public int numargs;
            public bool left_associative;
            public OperatorInfo(Function2Delegate function, int numargs, Priority priority, bool left_associative = true)
            {
                this.function = function; this.priority = priority; this.numargs = numargs; this.left_associative = left_associative;
            }
        }

        private struct TOKEN
        {
            public enum Ttype { INTEGER, MULTIPLY, DIVIDE, PLUS, MINUS, POW, BRACKETOPEN, BRACKETCLOSE, EXCLAMATION, PERCENT, COMMA, MOD, HASH, FUNCTION };
            public enum Tfunc { GCD, LCM, MODINV, MODPOW, SQRT, NCR, NPR, NEXTPRIME, PREVPRIME, ISPRIME, PHI, ABS, CROSSSUM, DIVSUM, DIVNUM, PI, PRIME, DLOG };
            public Ttype ttype;
            public BigInteger integer;
            public Tfunc function;
        }

        static Dictionary<TOKEN.Tfunc, FunctionInfo> prefixFunctions = new Dictionary<TOKEN.Tfunc, FunctionInfo>
        {   
            { TOKEN.Tfunc.ABS, new FunctionInfo( args => { return BigInteger.Abs(args[0]); }, "abs", 1 ) },
            { TOKEN.Tfunc.SQRT, new FunctionInfo( args => { return BigIntegerHelper.Sqrt(args[0]); }, "sqrt", 1 ) },
            { TOKEN.Tfunc.CROSSSUM, new FunctionInfo( args => { return BigIntegerHelper.CrossSum(args[0],args[1]); }, "crosssum", 2 ) },
            { TOKEN.Tfunc.GCD, new FunctionInfo( args => { return BigIntegerHelper.GCD(args[0], args[1]); }, "(gcd|ggt)", 2 ) },
            { TOKEN.Tfunc.LCM, new FunctionInfo( args => { return BigIntegerHelper.LCM(args[0], args[1]); }, "(lcm|kgv)", 2 ) },
            { TOKEN.Tfunc.MODINV, new FunctionInfo( args => { return BigIntegerHelper.ModInverse(args[0], args[1]); }, "modinv", 2 ) },
            { TOKEN.Tfunc.MODPOW, new FunctionInfo( args => { return BigInteger.ModPow(args[0], args[1], args[2]); }, "modpow", 3 ) },
            { TOKEN.Tfunc.DLOG, new FunctionInfo( args => { return BigIntegerHelper.DiscreteLogarithm(args[0],args[1],args[2]); }, "dlog", 3 ) },
            { TOKEN.Tfunc.NPR, new FunctionInfo( args => { return BigIntegerHelper.nPr(args[0], args[1]); }, "npr", 2 ) },
            { TOKEN.Tfunc.NCR, new FunctionInfo( args => { return BigIntegerHelper.nCr(args[0], args[1]); }, "ncr", 2 ) },
            { TOKEN.Tfunc.PHI, new FunctionInfo( args => { return BigIntegerHelper.Phi(args[0]); }, "phi", 1 ) },
            { TOKEN.Tfunc.DIVSUM, new FunctionInfo( args => { return BigIntegerHelper.SumOfDivisors(args[0]); }, "divsum", 1 ) },
            { TOKEN.Tfunc.DIVNUM, new FunctionInfo( args => { return BigIntegerHelper.NumberOfDivisors(args[0]); }, "divnum", 1 ) },
            { TOKEN.Tfunc.PI, new FunctionInfo( args => { return BigIntegerHelper.NumberOfPrimes(args[0]); }, "pi", 1 ) },
            { TOKEN.Tfunc.PRIME, new FunctionInfo( args => { return BigIntegerHelper.PrimeNumber(args[0]); }, "prime", 1 ) },
            { TOKEN.Tfunc.NEXTPRIME, new FunctionInfo( args => { return BigIntegerHelper.NextProbablePrime(args[0]); }, "nextprime", 1 ) },
            { TOKEN.Tfunc.PREVPRIME, new FunctionInfo( args => { return BigIntegerHelper.PreviousProbablePrime(args[0]); }, "prevprime", 1 ) },
            { TOKEN.Tfunc.ISPRIME, new FunctionInfo( args => { return BigIntegerHelper.IsProbablePrime(args[0])?1:0; }, "isprime", 1 ) },
        };

        static Dictionary<TOKEN.Ttype, OperatorInfo> infixOperators = new Dictionary<TOKEN.Ttype, OperatorInfo>
        {   
            { TOKEN.Ttype.PLUS, new OperatorInfo( (a,b) => { return a + b; }, 2, Priority.ADD ) },
            { TOKEN.Ttype.MINUS, new OperatorInfo( (a,b) => { return a - b; }, 2, Priority.SUB ) },
            { TOKEN.Ttype.MULTIPLY, new OperatorInfo( (a,b) => { return a * b; }, 2, Priority.MULT ) },
            { TOKEN.Ttype.DIVIDE, new OperatorInfo( (a,b) => { return a / b; }, 2, Priority.DIV ) },
            { TOKEN.Ttype.POW, new OperatorInfo( (a,b) => { return BigIntegerHelper.Pow(a,b); }, 2, Priority.POW, false ) },
            { TOKEN.Ttype.EXCLAMATION, new OperatorInfo( (a,b) => { return BigIntegerHelper.Factorial(a); }, 1, Priority.FACTORIAL ) },
            { TOKEN.Ttype.MOD, new OperatorInfo( (a,b) => { return a % b; }, 2, Priority.MOD ) },
            { TOKEN.Ttype.PERCENT, new OperatorInfo( (a,b) => { return a % b; }, 2, Priority.MOD ) },
            { TOKEN.Ttype.HASH, new OperatorInfo( (a,b) => { return BigIntegerHelper.Primorial(a); }, 1, Priority.FACTORIAL ) },
        };

        private static Stack<TOKEN> Scan(string expr)
        {
            TOKEN t = new TOKEN();
            int startIndex = 0;
            if (expr == "")
                return new Stack<TOKEN>();
            switch (expr[0])
            {
                case ' ':
                    return Scan(expr.Substring(1));
                case '(':
                    t.ttype = TOKEN.Ttype.BRACKETOPEN;
                    startIndex = 1;
                    break;
                case ')':
                    t.ttype = TOKEN.Ttype.BRACKETCLOSE;
                    startIndex = 1;
                    break;
                case '+':
                    t.ttype = TOKEN.Ttype.PLUS;
                    startIndex = 1;
                    break;
                case '-':
                    t.ttype = TOKEN.Ttype.MINUS;
                    startIndex = 1;
                    break;
                case '*':
                    if (expr.Length > 1 && expr[1] == '*')
                    {
                        t.ttype = TOKEN.Ttype.POW;
                        startIndex = 2;
                    }
                    else
                    {
                        t.ttype = TOKEN.Ttype.MULTIPLY;
                        startIndex = 1;
                    }
                    break;
                case '/':
                    t.ttype = TOKEN.Ttype.DIVIDE;
                    startIndex = 1;
                    break;
                case '^':
                    t.ttype = TOKEN.Ttype.POW;
                    startIndex = 1;
                    break;
                case '!':
                    t.ttype = TOKEN.Ttype.EXCLAMATION;
                    startIndex = 1;
                    break;
                case '%':
                    t.ttype = TOKEN.Ttype.PERCENT;
                    startIndex = 1;
                    break;
                case ',':
                    t.ttype = TOKEN.Ttype.COMMA;
                    startIndex = 1;
                    break;
                    
                default:

                    Match m;
                    bool found = false;
                    foreach (var f in prefixFunctions)
                    {
                        m = Regex.Match(expr, "^" + f.Value.pattern, RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            t.ttype = TOKEN.Ttype.FUNCTION;
                            t.function = f.Key;
                            startIndex = m.Length;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;

                    if (Regex.IsMatch(expr, "^mod", RegexOptions.IgnoreCase))
                    {
                        t.ttype = TOKEN.Ttype.MOD;
                        startIndex = 3;
                        break;
                    }

                    // try to parse as hexadecimal, decimal, octal or binary number
                    
                    m = Regex.Match(expr, @"^([0-9a-z]+)\(([0-9]+)\)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        int basis = int.Parse(m.Groups[2].Value);
                        t.integer = Parse(m.Groups[1].Value, basis);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = m.Groups[0].Value.Length;
                        break;
                    }

                    m = Regex.Match(expr, @"^[#hx]([0-9a-f]+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        //t.integer = BigInteger.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier);
                        t.integer = BigIntegerHelper.Parse(m.Groups[1].Value,16);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = m.Groups[0].Value.Length;
                        break;
                    }

                    m = Regex.Match(expr, @"^([0-9]+)");
                    if (m.Success)
                    {
                        t.integer = BigInteger.Parse(m.Groups[1].Value);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = m.Groups[0].Value.Length;
                        break;
                    }

                    m = Regex.Match(expr, @"^o([0-7]+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        t.integer = BigIntegerHelper.Parse(m.Groups[1].Value,8);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = m.Groups[0].Value.Length;
                        break;
                    }

                    m = Regex.Match(expr, @"^b([01]+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        t.integer = BigIntegerHelper.Parse(m.Groups[1].Value,2);
                        t.ttype = TOKEN.Ttype.INTEGER;
                        startIndex = m.Groups[0].Value.Length;
                        break;
                    }

                    if(expr[0]=='#') {
                        t.ttype = TOKEN.Ttype.HASH;
                        startIndex = 1;
                        break;
                    }

                    throw new ParseException("parsing failed at character " + expr[0]);
            }
            Stack<TOKEN> st = Scan(expr.Substring(startIndex));
            st.Push(t);
            return st;
        }

        private enum Priority { ALL=0, ADD, SUB, MULT, DIV, MOD, POW, FACTORIAL, SIGN };

        private static void ConsumeToken(Stack<TOKEN> stack, TOKEN.Ttype ttype, string errmsg)
        {
            if (stack.Count==0 || stack.Peek().ttype != ttype) 
                throw new ParseException(errmsg);
            stack.Pop();
        }

        private static BigInteger[] ParseArguments(Stack<TOKEN> stack, int count = -1)
        {
            List<BigInteger> arguments = new List<BigInteger>();

            ConsumeToken(stack, TOKEN.Ttype.BRACKETOPEN, "opening bracket expected");

            while (stack.Count > 0 && stack.Peek().ttype != TOKEN.Ttype.BRACKETCLOSE)
            {
                if (arguments.Count > 0)
                    ConsumeToken(stack, TOKEN.Ttype.COMMA, "comma expected");
                arguments.Add(Parse(stack, Priority.ALL));
            }

            ConsumeToken(stack, TOKEN.Ttype.BRACKETCLOSE, "closing bracket expected");

            if (count >= 0 && arguments.Count != count)
                throw new ParseException("unexpected number of arguments");

            return arguments.ToArray();
        }

        private static BigInteger Parse(Stack<TOKEN> stack, Priority priority)
        {
            BigInteger v = 0;

            if (stack.Count == 0)
                throw new ParseException("empty stack");

            TOKEN t = stack.Pop();

            // Parse prefix operators
            switch (t.ttype)
            {
                case TOKEN.Ttype.MINUS:
                    v = -Parse(stack, Priority.SIGN);
                    break;
                case TOKEN.Ttype.PLUS:
                    v = Parse(stack, Priority.SIGN);
                    break;
                case TOKEN.Ttype.INTEGER:
                    v = t.integer;
                    break;
                case TOKEN.Ttype.BRACKETOPEN:
                    v = Parse(stack, Priority.ALL);
                    ConsumeToken(stack, TOKEN.Ttype.BRACKETCLOSE, "closing bracket expected");
                    break;
                case TOKEN.Ttype.FUNCTION:
                    FunctionInfo fi = prefixFunctions[t.function];
                    v = fi.function(ParseArguments(stack, fi.numargs));
                    break;
                default:
                    throw new ParseException("unexpected prefix");
            }

            // Parse infix operators
            while (stack.Count != 0)
            {
                TOKEN.Ttype ttype = stack.Peek().ttype;

                if (ttype == TOKEN.Ttype.BRACKETCLOSE || ttype == TOKEN.Ttype.COMMA)
                    break;

                OperatorInfo io = infixOperators[ttype];
                if ((priority > io.priority) || (priority == io.priority && io.left_associative)) return v;
                stack.Pop();
                BigInteger b = (io.numargs == 2) ? Parse(stack, io.priority) : 0;
                v = io.function(v, b);
            }

            return v;
        }

        #endregion
        
        /*         
         * Parses a math expression (example: (2+2)^(17-5) ) 
         * and returns a BigInteger based on this expression
         * 
         * throws an exception when expression is not valid or the Number gets too big
         */
        public static BigInteger ParseExpression(string expr)
        {
            Stack<TOKEN> stack;
            BigInteger i=0;

            try
            {
                stack = Scan(expr);

                if (stack.Count > 0)
                    i = Parse(stack, Priority.ALL);

                if (stack.Count != 0)
                    throw new ParseException("unexpected remainder on stack");
            }
            catch (ParseException ex)
            {
                throw new Exception(string.Format("Parsing error ({0})", ex.Message));
            }

            return i;
        }

        static string digits = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static BigInteger Parse(string expr, int basis)
        {
            if (basis < 2)
                throw new Exception(string.Format("Illegal base {0} (must be >=2).", basis));

            BigInteger result = 0;

            //int numdigits = Math.Min(1000, expr.Length / 2);
            //BigInteger multibasis = BigInteger.Pow(basis, numdigits);

            BigInteger multibasis = basis;
            int numdigits = 1;
            while (numdigits < expr.Length/2 && multibasis < limit)
            {
                multibasis *= basis;
                numdigits++;
            }

            int t = expr.Length % numdigits;
            if (t == 0) t = numdigits;
            for (int f = 0; f < expr.Length; t += numdigits)
            {
                result = result * multibasis + Parse2(expr.Substring(f, t-f), basis);
                f = t;
            }

            return result;
        }

        public static BigInteger Parse2(string expr, int basis)
        {
            if (basis < 2)
                throw new Exception(string.Format("Illegal base {0} (must be >=2).", basis));

            BigInteger result = 0;

            foreach (var c in expr)
            {
                int d = digits.IndexOf(c.ToString().ToLower());
                if (d < 0 || d >= basis)
                    throw new Exception(string.Format("Unexpected character '{0}' in base {1} expression.", c, basis));
                result = result * basis + d;
            }

            return result;
        }

        static private BigInteger limit = BigInteger.Pow(10, 100);

        public static string ToBaseString(this BigInteger n, int basis)
        {
            if (basis < 2)
                throw new Exception(string.Format("Illegal base {0} (must be >=2).", basis));

            StringBuilder result = new StringBuilder();

            int sign = (n < 0) ? -1 : 1;
            n = BigInteger.Abs(n);

            BigInteger multibasis = basis;
            int exp = 1;
            while (multibasis < n && multibasis < limit)
            {
                multibasis *= basis;
                exp++;
            }

            do
            {
                try
                {
                    //result.Insert(0, digits[(int)(n % basis)]);
                    ////result.Append(digits[(int)(n % basis)]);
                    //result = digits[(int)(n % basis)] + result;
                    result.Insert(0, ToBaseString_NumDigits(n % multibasis,basis,(n<multibasis)?-1:exp));
                    n /= multibasis;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new Exception(string.Format("Can't convert input to base {0}.", basis));
                }
                //n /= basis;
            } while (n != 0);

            //if (sign == -1) result = "-" + result;
            if (sign == -1) result.Insert(0, '-');
            //if (sign == -1) result.Append('-');

            //char[] chars = new char[result.Length];
            //result.CopyTo(0, chars, 0, chars.Length);
            //Array.Reverse(chars);
            //return new string(chars);

            return result.ToString();
        }

        public static string ToBaseString_NumDigits(this BigInteger n, int basis, int numdigits)
        {
            if (basis < 2)
                throw new Exception(string.Format("Illegal base {0} (must be >=2).", basis));

            StringBuilder result = new StringBuilder();

            int sign = (n < 0) ? -1 : 1;
            n = BigInteger.Abs(n);

            try
            {
                for (int i = 0; ; i++)
                {
                    result.Insert(0, digits[(int)(n % basis)]);
                    n /= basis;
                    if ((numdigits < 0 && n == 0) || i == numdigits - 1) break;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(string.Format("Can't convert input to base {0}.", basis));
            }

            if (sign == -1) result.Insert(0, '-');

            return result.ToString();
        }

        public static BigInteger Pow(BigInteger b, BigInteger e)
        {
            if (b < 0) return (e % 2 == 0) ? Pow(-b, e) : -Pow(-b, e);
            if (e < 0) return (b > 1) ? 0 : (BigInteger)1 / Pow(b, -e);
            if (b <= 1) return b;

            try
            {
                return BigInteger.Pow(b, (int)e);
            }
            catch
            {
                throw new OverflowException();
            }
        }
        
        /// <summary>
        /// Returns the modulo inverse of input.
        /// Throws ArithmeticException if the inverse does not exist (i.e. gcd(this, modulus) != 1) or the modulus is smaller than 2.
        /// </summary>
        public static BigInteger ModInverse(BigInteger input, BigInteger modulus)
        {
            if (modulus < 2)
                throw (new ArithmeticException(String.Format("Modulus must be >= 2, is {0}.", modulus)));
            
            BigInteger x, y;
            BigInteger g = ExtEuclid(((input % modulus) + modulus) % modulus, modulus, out x, out y);

            if (g != 1)
                throw (new ArithmeticException(String.Format("{0} has no inverse modulo {1}.", input, modulus)));

            return ((x % modulus) + modulus) % modulus;
        }

        /// <summary>
        /// Extended Euclidean Algorithm.
        /// Returns the GCD of a and b and finds integers x and y that satisfy x*a + y*b = gcd(a,b)
        /// </summary>
        public static BigInteger ExtEuclid(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            BigInteger xx, t, q, r;

            BigInteger aa = BigInteger.Abs(a);
            BigInteger bb = BigInteger.Abs(b);
            xx = 0; x = 1;

            while (bb > 0)
            {
                q = BigInteger.DivRem(aa, bb, out r);
                aa = bb; bb = r;
                t = x - xx * q; x = xx; xx = t;
            }

            x *= a.Sign;
            y = (b == 0) ? 0 : (aa - x * a) / b;

            return aa;
        }

        /// <summary>
        /// Greatest Common Divisor
        /// Returns the GCD of a and b
        /// </summary>
        public static BigInteger GCD(this BigInteger a, BigInteger b)
        {
            return BigInteger.GreatestCommonDivisor(a, b);
        }

        /// <summary>
        /// Least Common Multiple
        /// Returns the LCM of a and b
        /// </summary>
        public static BigInteger LCM(this BigInteger a, BigInteger b)
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(a, b);
            return (gcd != 0) ? ((a * b) / gcd) : 0;
        }

        public static BigInteger SetBit(BigInteger b, int i)
        {
            if( i>=0 ) b |= (((BigInteger)1) << i);
            return b;
        }

        /// <summary>
        /// Returns a random prime with 'bits' bits and the MSB set.
        /// You need this if you want to create primes with a given bitlength, just
        /// calling RandomPrimeBits would not guarantee the bitlength of the prime.
        /// </summary>
        public static BigInteger RandomPrimeMSBSet(int bits)
        {
            if (bits <= 1) throw new ArithmeticException("No primes with this bitcount");

            BigInteger limit = ((BigInteger)1) << bits;

            while (true)
            {
                var p = NextProbablePrime(SetBit(RandomIntBits(bits - 1), bits - 1));
                if (p < limit) return p;
            }
        }

        /// <summary>
        /// Returns a random integer with 'bits' bits and the MSB set.
        /// </summary>
        public static BigInteger RandomIntMSBSet(int bits)
        {
            return SetBit( RandomIntBits(bits - 1), bits - 1 );
        }

        /// <summary>
        /// Returns a random prime less than limit
        /// </summary>
        public static BigInteger RandomPrimeLimit(this BigInteger limit)
        {
            if (limit <= 2) throw new ArithmeticException("No primes below this limit");

            while( true ) {
                var p = NextProbablePrime(RandomIntLimit(limit));
                if( p < limit ) return p;
            }
        }

        /// <summary>
        /// Returns a random prime less than 2^bits
        /// </summary>
        public static BigInteger RandomPrimeBits(int bits)
        {
            if (bits < 0) throw new ArithmeticException("Enter a positive bitcount");
            return RandomPrimeLimit( ((BigInteger)1) << bits );
        }

        /// <summary>
        /// Returns a random integer less than limit
        /// </summary>
        public static BigInteger RandomIntLimit(this BigInteger limit)
        {
            if (limit <= 0) throw new ArithmeticException("Enter a positive limit");

            byte[] buffer = limit.ToByteArray();
            int n = buffer.Length;
            byte msb = buffer[n - 1];
            int mask = 0;

            while (mask < msb)
                mask = (mask << 1) + 1;

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            while (true)
            {
                rng.GetBytes(buffer);
                buffer[n - 1] &= (byte)mask;
                var p = new BigInteger(buffer);
                if (p < limit) return p;
            }
        }

        /// <summary>
        /// Returns a random integer less than 2^bits
        /// </summary>
        public static BigInteger RandomIntBits(int bits)
        {
            if (bits < 0) throw new ArithmeticException("Enter a positive bitcount");
            return RandomIntLimit( ((BigInteger)1) << bits );
        }

        #region primesBelow2000
        // primes smaller than 2000 to test the generated prime number (taken from BigInteger class written by Chew Keong TAN)

        private static readonly int[] primesBelow2000 = {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
            101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,
	        211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293,
	        307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,
	        401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499,
	        503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599,
	        601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691,
	        701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797,
	        809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887,
	        907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,
	        1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097,
	        1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193,
	        1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297,
	        1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399,
	        1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499,
	        1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597,
	        1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699,
	        1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789,
	        1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,
	        1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999 };

        #endregion

        public static BigInteger NextProbablePrime( this BigInteger n )
        {
            if (n < 0) throw new ArithmeticException("NextProbablePrime cannot be called on value < 0");
            if (n <= 2) return 2;
            if (n.IsEven) n++;
            if (n == 3) return 3;
            BigInteger r = n % 6;
            if (r == 3) n += 2;
            if (r == 1) { if (IsProbablePrime(n)) return n; else n += 4; }
            
            // at this point n mod 6 = 5

            while (true)
            {
                if (IsProbablePrime(n)) return n;
                n += 2;
                if (IsProbablePrime(n)) return n;
                n += 4;
            }

        }

        public static BigInteger PreviousProbablePrime(this BigInteger n)
        {
            if (n < 2) throw new ArithmeticException("PreviousProbablePrime cannot be called on value < 2");
            if (n == 2) return 2;
            if (n.IsEven) n--;
            if (n == 3) return 3;
            BigInteger r = n % 6;
            if (r == 3) n -= 2;
            if (r == 5) { if (IsProbablePrime(n)) return n; else n -= 4; }

            // at this point n mod 6 = 1

            while (true)
            {
                if (IsProbablePrime(n)) return n;
                n -= 2;
                if (IsProbablePrime(n)) return n;
                n -= 4;
            }

        }

        /*
         * This code is heavily inspired by the code from the BigInteger class written by Chew Keong TAN
         */
        public static bool IsProbablePrime(this BigInteger thisVal)
        {
            thisVal = BigInteger.Abs(thisVal);

            //test small numbers
            if (thisVal == 0 || thisVal == 1)
                return false;
            if (thisVal == 2 || thisVal == 3)
                return true;            

            if (thisVal.IsEven)     // even numbers
                return false;

            // test for divisibility by primes < 2000
            for (int p = 0; p < primesBelow2000.Length; p++)
            {
                BigInteger divisor = primesBelow2000[p];

                if (divisor >= thisVal)
                    break;

                BigInteger resultNum = thisVal % divisor;
                if (resultNum == 0)                
                    return false;                
            }

            // Perform BASE 2 Rabin-Miller Test

            // calculate values of s and t
            int s = 0;

            BigInteger t = thisVal - 1;
            while ((t & 0x01) == 0)     //TODO: This could be implemented more efficient
            {
                t = t >> 1;
                s++;
            }

            BigInteger a = 2;

            // b = a^t mod p
            BigInteger b = BigInteger.ModPow(a, t, thisVal);

            if (b == 1)         // a^t mod p = 1
                return true;

            BigInteger p_sub1 = thisVal - 1;
            for (int j = 0; j < s; j++)
            {
                if (b == p_sub1)         // a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
                    return true;

                b = (b * b) % thisVal;
            }

            /*  TODO: Implement this:
            // if number is strong pseudoprime to base 2, then do a strong lucas test
            if (result)
                result = LucasStrongTestHelper(thisVal);
            */

            return false;
        }


        /// <summary>
        /// Because it is often necessary to convert a reversed byte array to a positive BigInteger, without having a the highest significant bit 
        /// set to zero for indicating the positiveness, this method can be used.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static BigInteger FromPositiveReversedByteArray(byte[] p)
        {
            byte[] b = new byte[p.Length+1];      //b has one more byte than p
            for (int i = 0; i < p.Length; i++)
                b[i] = p[p.Length-i-1];
            return new BigInteger(b);
        }

        /// <summary>
        /// Returns the number of bits that are needed to represent the integer argument.
        /// </summary>
        public static int BitCount(this BigInteger b)
        {
            if (b < 0) b = -b;

            byte[] buffer = b.ToByteArray();

            // ignore leading zero bytes
            int i = buffer.Length - 1;
            while( i > 0 && buffer[i] == 0 ) i--;
            
            // ignore leading zero bits
            byte mask = 0x80;
            int j = 8;
            while( j>0 && (buffer[i] & mask) == 0 )
            {
                j--;
                mask >>= 1;
            }

            return 8 * i + j;
        }

        public static BigInteger Factorial(this BigInteger n)
        {
            if (n < 0) throw new ArithmeticException("The factorial of a negative number is not defined");
            if (n > 1000000000) throw new OverflowException();

            BigInteger result = 1;
            BigInteger counter = n;

            while (counter > 1)
            {
                result *= counter;
                counter--;
            }

            return result;
        }

        public static BigInteger Primorial(this BigInteger n)
        {
            if (n < 0) throw new ArithmeticException("The primorial of a negative number is not defined");

            BigInteger result = 1;
            BigInteger counter = n;

            while (counter > 1)
            {
                counter = counter.PreviousProbablePrime();
                result *= counter;
                counter--;
            }

            return result;
        }

        /// <summary>
        /// Calculates the number of unordered subsets with r objects of a set with n objects.
        /// </summary>
        public static BigInteger nCr(this BigInteger n, BigInteger r)
        {
            if ( n < r || r < 0 ) return 0;

            BigInteger result = 1;
            BigInteger ri = 1;
            BigInteger ni = n;

            while (ri <= r)
            {
                result *= ni;
                result /= ri;
                ni--;
                ri++;
            }

            return result;
        }

        /// <summary>
        /// Calculates the number of ordered subsets with r objects of a set with n objects.
        /// </summary>
        public static BigInteger nPr(this BigInteger n, BigInteger r)
        {
            if (n < r || r < 0) return 0;

            BigInteger result = 1;
            BigInteger ri = 1;
            BigInteger ni = n;

            while (ri <= r)
            {
                result *= ni;
                ni--;
                ri++;
            }

            return result;
        }

        /// <summary>
        /// Calculates the cross sum of the given number in its expansion in base b
        /// </summary>
        public static BigInteger CrossSum(this BigInteger n, BigInteger b)
        {
            if (b < 2) throw new ArithmeticException("The base must be >= 2");

            BigInteger result = 0;

            while (n > 0)
            {
                result += n % b;
                n /= b;
            }

            return result;
        }

        /// <summary>
        /// Returns the minimum of a set of BigIntegers
        /// </summary>
        public static BigInteger Min(BigInteger n, params BigInteger[] values)
        {
            BigInteger min = n;

            for (int i = 0; i < values.Length; i++)
                if (min > values[i]) min = values[i];

            return min;
        }

        /// <summary>
        /// Returns the maximum of a set of BigIntegers
        /// </summary>
        public static BigInteger Max(BigInteger n, params BigInteger[] values)
        {
            BigInteger max = n;

            for (int i = 0; i < values.Length; i++)
                if (max < values[i]) max = values[i];

            return max;
        }

        /// <summary>
        /// Calculates the square root of a BigInteger.
        /// If the argument is not a perfect square, this function returns the floor of the square root,
        /// i.e. the biggest number that is smaller than the actual square root of the argument.
        /// </summary>
        
        // Compute the square root of n using Heron's method (which is Newton's method applied to x^2-n)

        public static BigInteger Sqrt(this BigInteger n)
        {
            if( n<0 )
                throw (new ArithmeticException("Square root of negative number does not exist!"));

            if (n == 0) 
                return 0;

            BigInteger x = n >> (n.BitCount() / 2);     // select starting value
            BigInteger lastx;

            while (true)
            {
                lastx = x;
                x = (n / x + x) >> 1;
                int i = x.CompareTo(lastx);
                if (i == 0) return x;
                if (i < 0)
                {
                    if (lastx - x == 1 && (x * x < n) && (lastx * lastx) > n) return x;
                }
                else
                {
                    if (x - lastx == 1 && (lastx * lastx) < n && (x * x) > n) return lastx;
                }
            }
        }

        public static Dictionary<BigInteger, long> Factorize(this BigInteger n)
        {
            bool isFactorized;
            return n.Factorize(n, out isFactorized);
        }

        public static Dictionary<BigInteger, long> Factorize(this BigInteger n, BigInteger limit, out bool isFactorized)
        {
            Dictionary<BigInteger, long> factors = new Dictionary<BigInteger, long>();
            BigInteger value = (n < 0) ? -n : n;

            isFactorized = false;

            if (value == 1)
            {
                isFactorized = true;
                return factors;
            }

            if (value.IsProbablePrime())
            {
                factors[value] = 1;
                isFactorized = true;
                return factors;
            }

            for (BigInteger factor = 2; ; factor = (factor + 1).NextProbablePrime())
            {
                if (factor * factor > value)
                {
                    factors[value] = 1;
                    isFactorized = true;
                    break;
                }

                if (factor > limit)
                {
                    factors[value] = 1;
                    isFactorized = false;
                    break;
                }

                if (value % factor == 0)
                {
                    factors[factor] = 0;

                    do
                    {
                        value /= factor;
                        factors[factor]++;
                    }
                    while (value % factor == 0);

                    if (value == 1)
                    {
                        isFactorized = true;
                        break;
                    }

                    if (value.IsProbablePrime())
                    {
                        factors[value] = 1;
                        isFactorized = true;
                        break;
                    }
                }
            }

            return factors;
        }

        public static BigInteger Refactor(Dictionary<BigInteger, long> factors)
        {
            BigInteger result = 1;

            foreach (var s in factors.Keys)
                result *= BigInteger.Pow(s,(int)factors[s]);

            return result;
        }

        public static List<BigInteger> Divisors(this BigInteger n)
        {
            return Divisors(n.Factorize());
        }

        public static List<BigInteger> Divisors(Dictionary<BigInteger, long> factors)
        {
            Dictionary<BigInteger, long> f = new Dictionary<BigInteger, long>();
            List<BigInteger> keys = new List<BigInteger>();
            foreach (var key in factors.Keys)
            {
                keys.Add(key);
                f[key] = 0;
            }

            List<BigInteger> result = new List<BigInteger>();

            int i;
            do
            {
                result.Add(Refactor(f));
                for (i = keys.Count - 1; i >= 0; i--)
                {
                    f[keys[i]]++;
                    if (f[keys[i]] <= factors[keys[i]]) break;
                }
                for (int j = i + 1; j < keys.Count; j++) f[keys[j]] = 0;
            }
            while (i >= 0);

            return result;
        }

        public static BigInteger SumOfDivisors(this BigInteger n)
        {
            return Divisors(n.Factorize()).Aggregate(BigInteger.Add);
        }

        public static BigInteger NumberOfDivisors(this BigInteger n)
        {
            Dictionary<BigInteger, long> factors = n.Factorize();
            BigInteger result = 1;

            foreach (var f in factors.Keys)
                result *= factors[f] + 1;

            return result;
        }

        public static BigInteger Phi(this BigInteger n)
        {
            if (n == 0) return 0;
            return Phi(n.Factorize());
        }

        public static BigInteger Phi(Dictionary<BigInteger, long> factors)
        {
            BigInteger phi = 1;

            foreach (var f in factors.Keys)
            {
                if (f > 0)
                    if (factors[f] > 0)
                        phi *= (f - 1) * BigInteger.Pow(f, (int)factors[f] - 1);
            }

            return phi;
        }

        public static BigInteger NumberOfPrimes(BigInteger n)
        {
            BigInteger count = 0;

            for (BigInteger i = 2; i <= n; i = NextProbablePrime(i + 1))
                count++;

            return count;
        }

        public static BigInteger PrimeNumber(BigInteger n)
        {
            BigInteger prime = 2;

            for (BigInteger i = 0; i < n; i++)
                prime = NextProbablePrime(prime + 1);

            return prime;
        }        
        
        /// <summary>
        /// Baby-step giant-step algorithm by Daniel Shanks
        /// </summary>
        public static BigInteger DiscreteLogarithm(this BigInteger residue, BigInteger basis, BigInteger modulus)
        {
            Dictionary<BigInteger, BigInteger> hashtab = new Dictionary<BigInteger, BigInteger>();

            BigInteger m = modulus.Sqrt() + 1;
            BigInteger v;

            try
            {
                // baby-steps
                v = (residue * basis) % modulus;
                for (BigInteger j = 1; j <= m; j++)
                {
                    if (hashtab.ContainsKey(v)) break;
                    hashtab.Add(v, j);
                    v = (v * basis) % modulus;
                }
            }
            catch (OutOfMemoryException ex)
            {
                m = hashtab.Count;
            }

            BigInteger M = (modulus + m - 1) / m;

            // giant-steps
            BigInteger g_m = BigInteger.ModPow(basis, m, modulus);
            v = g_m;
            for (BigInteger i = 1; i <= M; i++)
            {
                if (hashtab.ContainsKey(v))
                    return i * m - hashtab[v];

                v = (v * g_m) % modulus;

                if (v == g_m) break;
            }

            throw new ArithmeticException(string.Format("Input base {0} is not a generator of the residue class {1} modulo {2}.", basis, residue, modulus));
        }
    }
}

public class ParseException : Exception
{
    public ParseException()
    {
    }

    public ParseException(string message)
        : base(message)
    {
    }

    public ParseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
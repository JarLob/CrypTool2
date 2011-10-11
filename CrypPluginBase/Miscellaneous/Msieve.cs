﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Cryptool.PluginBase.IO;
using System.Numerics;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace Cryptool.PluginBase.Miscellaneous
{
    /// <summary>
    /// This class is responsible for loading the msieve dll and offers some functions
    /// for factorizing.
    /// </summary>
    public class Msieve
    {
        private static Assembly msieveDLL = null;
        private static Mutex msieveMutex = new Mutex();

        /// <summary>
        /// A single factor
        /// </summary>
        public struct Factor
        {
            public BigInteger factor;
            public bool prime;
            public int count;

            public Factor(BigInteger factor, bool prime, int count)
            {
                this.factor = factor;
                this.prime = prime;
                this.count = count;
            }
        }

        /// <summary>
        /// This method is mainly a singleton.
        /// </summary>
        /// <returns>The (only) msieve assembly</returns>
        public static Assembly GetMsieveDLL()
        {
            msieveMutex.WaitOne();

            if (msieveDLL == null)
            {
                string s = Directory.GetCurrentDirectory();
                string dllname;
                if (IntPtr.Size == 4)
                    dllname = "msieve.dll";
                else
                    dllname = "msieve64.dll";

                msieveDLL = Assembly.LoadFile(DirectoryHelper.BaseDirectory + "\\Lib\\" + dllname);
            }

            msieveMutex.ReleaseMutex();

            return msieveDLL;
        }

        /// <summary>
        /// This method factorizes the parameter "number" by using the "trivial" (i.e. very fast) methods that are available in msieve.
        /// This means, that the factorization doesn't take very long, but on the other hand, you can end up having some 
        /// composite factors left, because they can't be factorized efficiently.
        /// </summary>
        /// <param name="number">the number to factorize</param>
        /// <returns>A list of factors</returns>
        public static List<Factor> TrivialFactorization(BigInteger number)
        {
            msieveMutex.WaitOne();

            Type msieve = GetMsieveDLL().GetType("Msieve.msieve");

            //init msieve with callbacks:
            MethodInfo initMsieve = msieve.GetMethod("initMsieve");
            Object callback_struct = Activator.CreateInstance(msieveDLL.GetType("Msieve.callback_struct"));
            FieldInfo putTrivialFactorlistField = msieveDLL.GetType("Msieve.callback_struct").GetField("putTrivialFactorlist");
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            MethodInfo putTrivialFactorlistMethodInfo = typeof(Msieve).GetMethod("putTrivialFactorlist", flags);
            Delegate putTrivialFactorlistDel = MulticastDelegate.CreateDelegate(msieveDLL.GetType("Msieve.putTrivialFactorlistDelegate"), putTrivialFactorlistMethodInfo);
            putTrivialFactorlistField.SetValue(callback_struct, putTrivialFactorlistDel);
            initMsieve.Invoke(null, new object[1] { callback_struct });

            //start msieve:
            currentNumber = number;
            MethodInfo start = msieve.GetMethod("start");
            start.Invoke(null, new object[] { number.ToString(), null });

            msieveMutex.ReleaseMutex();
            return factorlist;
        }

        #region private

        private static List<Factor> factorlist;
        private static BigInteger currentNumber;

        private static void putTrivialFactorlist(IntPtr list, IntPtr obj)
        {
            factorlist = new List<Factor>();
            
            Type msieve = GetMsieveDLL().GetType("Msieve.msieve");
            MethodInfo getPrimeFactorsMethod = msieve.GetMethod("getPrimeFactors");
            MethodInfo getCompositeFactorsMethod = msieve.GetMethod("getCompositeFactors");

            ArrayList pf = (ArrayList)(getPrimeFactorsMethod.Invoke(null, new object[] { list }));
            foreach (Object o in pf)
                AddToFactorlist(BigInteger.Parse((string)o), true);

            ArrayList cf = (ArrayList)(getCompositeFactorsMethod.Invoke(null, new object[] { list }));
            foreach (Object o in cf)
                AddToFactorlist(BigInteger.Parse((string)o), false);

            Debug.Assert(currentNumber == 1);
        }

        private static void AddToFactorlist(BigInteger factor, bool prime)
        {
            //Check if factor already in factorlist:
            foreach (Factor f in factorlist)
                if (f.factor == factor)
                    return;

            //Add to factorlist:
            int count = 0;
            while (currentNumber % factor == 0)
            {
                count++;
                currentNumber /= factor;
            }
            Debug.Assert(count != 0);
            factorlist.Add(new Factor(factor, prime, count));
        }

        #endregion

    }
}

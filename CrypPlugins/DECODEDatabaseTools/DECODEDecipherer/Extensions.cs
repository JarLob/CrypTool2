/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{

    public static class Extensions
    {
        public static bool Contains(this string[] array, string element)
        {
            foreach (var str in array)
            {
                if (str.Equals(element))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(this List<Token> list, Token token)
        {
            foreach (var token2 in list)
            {
                if (token.Equals(token2))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Contains(this List<Token> list, Symbol symbol)
        {
            foreach (var token in list)
            {
                if (token.Equals(symbol))
                {
                    return true;
                }
            }
            return false;
        }

        public static string[] Concat(this string[] arrayA, string[] arrayB)
        {
            if (arrayB == null || arrayB.Length == 0)
            {
                return arrayA;
            }
            string[] arrayC = new string[arrayA.Length + arrayB.Length];

            Array.Copy(arrayA, arrayC, arrayA.Length);
            Array.Copy(arrayB, 0, arrayC, arrayA.Length, arrayB.Length);
            return arrayC;
        }
    }

}

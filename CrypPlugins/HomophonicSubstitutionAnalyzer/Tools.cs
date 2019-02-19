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

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class Tools
    {
        /// <summary>
        /// Maps the homophones into number space
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] MapHomophoneTextNumbersIntoNumberSpace(string text)
        {
            var numbers = new int[text.Length / 2];
            var position = 0;
            for (var i = 0; i < text.Length; i += 2)
            {
                numbers[position] = int.Parse(text.Substring(i, 2));
                position++;
            }
            return numbers;            
        }

        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var i in numbers)
            {
                try
                {
                    builder.Append(alphabet[i]);
                }
                catch (IndexOutOfRangeException)
                {
                    //do nothing; letter is not in alphabet; thus, it is ignored
                }
            }
            return builder.ToString();
        }

        public static int[] MapIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (var c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }


        public static int[] Distinct(int[] text)
        {
            HashSet<int> symbols = new HashSet<int>();

            foreach (char c in text)
            {
                if (!symbols.Contains(c))
                {
                    symbols.Add(c);
                }
            }
            return symbols.ToArray();
        }

        public static int[] ChangeToConsecutiveNumbers(int[] text)
        {
            var number = 0;
            var newtext = new int[text.Length];
            Dictionary<int, int> mapping = new Dictionary<int, int>();

            for (var i = 0; i < text.Length; i++)
            {
                if (!mapping.Keys.Contains(text[i]))
                {
                    mapping.Add(text[i], number);
                    number++;
                }
                newtext[i] = mapping[text[i]];
            }
            return newtext;
        }

        public static string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var c in text)
            {
                if (alphabet.Contains(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        public static int[] MapHomophonesIntoNumberSpace(string ciphertext)
        {
            int[] numbers = new int[ciphertext.Length];
            for (int i = 0; i < ciphertext.Length; i++)
            {
                numbers[i] = (int) ciphertext[i];
            }
            return numbers;
        }
    }
}

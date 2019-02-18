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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class Statistics
    {
        private static double[, , , ,] Fivegrams;
        private static double[, , , , , ] Sixgrams;

        public static void Load6GramsGZ(string filename)
        {
            uint[, , , , ,] freq;
            uint max;
            ulong sum;
            string alphabet;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[, , , , ,])bf.Deserialize(gz);
                }
            }

            double[, , , , ,] result = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
            {
                for (int b = 0; b < alphabet.Length; b++)
                {
                    for (int c = 0; c < alphabet.Length; c++)
                    {
                        for (int d = 0; d < alphabet.Length; d++)
                        {
                            for (int e = 0; e < alphabet.Length; e++)
                            {
                                for (int f = 0; f < alphabet.Length; f++)
                                {
                                    result[a, b, c, d, e, f] = Math.Log((freq[a, b, c, d, e, f] + 0.001) / sum);
                                }
                            }
                        }
                    }
                }
            }

            Sixgrams = result;
        }

        public static void Load5GramsGZ(string filename)
        {
            uint[, , , ,] freq;
            uint max;
            ulong sum;
            string alphabet;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[, , , ,])bf.Deserialize(gz);
                }
            }

            double[, , , ,] result = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
            {
                for (int b = 0; b < alphabet.Length; b++)
                {
                    for (int c = 0; c < alphabet.Length; c++)
                    {
                        for (int d = 0; d < alphabet.Length; d++)
                        {
                            for (int e = 0; e < alphabet.Length; e++)
                            {
                                result[a, b, c, d, e] = Math.Log((freq[a, b, c, d, e] + 0.001) / sum);

                            }
                        }
                    }
                }
            }

            Fivegrams = result;
        }

        public static double Calculate5GramCost(int[] plaintext)
        {
            int end = plaintext.Length - 4;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
            {
                try
                {
                    value += Fivegrams[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3], plaintext[i + 4]];
                }
                catch (IndexOutOfRangeException)
                {
                    //do nothing; we have a letter that is not in our statistics
                }
            }

            return value / end;
        }

        public static double Calculate6GramCost(int[] plaintext)
        {
            int end = plaintext.Length - 5;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
            {
                try
                {
                    value += Sixgrams[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3], plaintext[i + 4], plaintext[i + 5]];
                }
                catch (IndexOutOfRangeException)
                {
                    //do nothing; we have a letter that is not in our statistics
                }
            }

            return value / end;
        }        
    }
}

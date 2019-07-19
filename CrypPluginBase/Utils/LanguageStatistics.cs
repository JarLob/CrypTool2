using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Properties;

namespace Cryptool.PluginBase.Utils
{
    public class LanguageStatistics
    {
        //this has to be consistent with the "SupportedLanguagesCodes"
        //and with all other arrays here
        public enum Languages
        {
            Englisch,
            German,
            Spansih,
            French,
            Italian,
            Hungarian,
            Russian,
            Czech,
            Greek,
            Latin
        }

        public static string[] SupportedLanguages
        {
            get { return new string[] { Resources.LanguageEN, Resources.LanguageDE, Resources.LanguageES, Resources.LanguageFR, Resources.LanguageIT, Resources.LanguageHU, Resources.LanguageRU, Resources.LanguageCS, Resources.LanguageEL, Resources.LanguageLA }; }
        }

        public static string[] SupportedLanguagesCodes
        {
            get { return new string[] { "en", "de", "es", "fr", "it", "hu", "ru", "cs", "el", "la" }; }
        }

        public static string LanguageCode(int n)
        {
            return SupportedLanguagesCodes[n % SupportedLanguages.Length];
        }

        public static Dictionary<string, double[]> Unigrams = new Dictionary<string, double[]>()
        {
            //Source: Wikipedia
            { "en", new double[] { 0.08167, 0.01492, 0.02782, 0.04253, 0.12702, 0.02228, 0.02015, 0.06094, 0.06966, 0.00153, 0.00772, 0.04025, 0.02406, 0.06749, 0.07507, 0.01929, 0.00095, 0.05987, 0.06327, 0.09056, 0.02758, 0.00978, 0.0236, 0.0015, 0.01974, 0.00074} }, // English
            { "fr", new double[] { 0.07636, 0.00901, 0.0326, 0.03669, 0.14715, 0.01066, 0.00866, 0.00737, 0.07529, 0.00613, 0.00049, 0.05456, 0.02968, 0.07095, 0.05796, 0.02521, 0.01362, 0.06693, 0.07948, 0.07244, 0.06311, 0.01838, 0.00074, 0.00427, 0.00128, 0.00326} }, // French
            { "de", new double[] { 0.06516, 0.01886, 0.02732, 0.05076, 0.16396, 0.01656, 0.03009, 0.04577, 0.0655, 0.00268, 0.01417, 0.03437, 0.02534, 0.09776, 0.02594, 0.0067, 0.00018, 0.07003, 0.0727, 0.06154, 0.04166, 0.00846, 0.01921, 0.00034, 0.00039, 0.01134} }, // German 
            { "es", new double[] { 0.11525, 0.02215, 0.04019, 0.0501, 0.12181, 0.00692, 0.01768, 0.00703, 0.06247, 0.00493, 0.00011, 0.04967, 0.03157, 0.06712, 0.08683, 0.0251, 0.00877, 0.06871, 0.07977, 0.04632, 0.02927, 0.01138, 0.00017, 0.00215, 0.01008, 0.00467} }, // Spanish 
            { "pt", new double[] { 0.14634, 0.01043, 0.03882, 0.04992, 0.1257, 0.01023, 0.01303, 0.00781, 0.06186, 0.00397, 0.00015, 0.02779, 0.04738, 0.04446, 0.09735, 0.02523, 0.01204, 0.0653, 0.06805, 0.04336, 0.03639, 0.01575, 0.00037, 0.00253, 6e-005, 0.0047} }, // Portuguese 
            { "eo", new double[] { 0.12117, 0.0098, 0.00776, 0.03044, 0.08995, 0.01037, 0.01171, 0.00384, 0.10012, 0.03501, 0.04163, 0.06104, 0.02994, 0.07955, 0.08779, 0.02755, 0, 0.05914, 0.06092, 0.05276, 0.03183, 0.01904, 0, 0, 0, 0.00494} }, // Esperanto 
            { "it", new double[] { 0.11745, 0.00927, 0.04501, 0.03736, 0.11792, 0.01153, 0.01644, 0.00636, 0.10143, 0.00011, 9e-005, 0.0651, 0.02512, 0.06883, 0.09832, 0.03056, 0.00505, 0.06367, 0.04981, 0.05623, 0.03011, 0.02097, 0.00033, 3e-005, 0.0002, 0.01181} }, // Italian 
            { "tr", new double[] { 0.1292, 0.02844, 0.01463, 0.05206, 0.09912, 0.00461, 0.01253, 0.01212, 0.096, 0.00034, 0.05683, 0.05922, 0.03752, 0.07987, 0.02976, 0.00886, 0, 0.07722, 0.03014, 0.03314, 0.03235, 0.00959, 0, 0, 0.03336, 0.015} }, // Turkish 
            { "sv", new double[] { 0.09383, 0.01535, 0.01486, 0.04702, 0.10149, 0.02027, 0.02862, 0.0209, 0.05817, 0.00614, 0.0314, 0.05275, 0.03471, 0.08542, 0.04482, 0.01839, 0.0002, 0.08431, 0.0659, 0.07691, 0.01919, 0.02415, 0.00142, 0.00159, 0.00708, 0.0007} }, // Swedish 
            { "pl", new double[] { 0.10503, 0.0174, 0.03895, 0.03725, 0.07352, 0.00143, 0.01731, 0.01015, 0.08328, 0.01836, 0.02753, 0.02564, 0.02515, 0.06237, 0.06667, 0.02445, 0, 0.05243, 0.05224, 0.02475, 0.02062, 0.00012, 0.05813, 4e-005, 0.03206, 0.04852} }, // Polish 
            { "nl", new double[] { 0.07486, 0.01584, 0.01242, 0.05933, 0.1891, 0.00805, 0.03403, 0.0238, 0.06499, 0.0146, 0.02248, 0.03568, 0.02213, 0.10032, 0.06063, 0.0157, 9e-005, 0.06411, 0.0373, 0.0679, 0.0199, 0.0285, 0.0152, 0.00036, 0.00035, 0.0139} }, // Dutch 
            { "da", new double[] { 0.06025, 0.02, 0.00565, 0.05858, 0.15453, 0.02406, 0.04077, 0.01621, 0.06, 0.0073, 0.03395, 0.05229, 0.03237, 0.0724, 0.04636, 0.01756, 7e-005, 0.08956, 0.05805, 0.06862, 0.01979, 0.02332, 0.00069, 0.00028, 0.00698, 0.00034} }, // Danish 
            { "is", new double[] { 0.1011, 0.01043, 0, 0.01575, 0.06418, 0.03013, 0.04241, 0.01871, 0.07578, 0.01144, 0.03314, 0.04532, 0.04041, 0.07711, 0.02166, 0.00789, 0, 0.08581, 0.0563, 0.04953, 0.04562, 0.02437, 0, 0.00046, 0.009, 0} }, // Icelandic 
            { "fi", new double[] { 0.12217, 0.00281, 0.00281, 0.01043, 0.07968, 0.00194, 0.00392, 0.01851, 0.10817, 0.02042, 0.04973, 0.05761, 0.03202, 0.08826, 0.05614, 0.01842, 0.00013, 0.02872, 0.07862, 0.0875, 0.05008, 0.0225, 0.00094, 0.00031, 0.01745, 0.00051} }, // Finnish 
            { "cs", new double[] { 0.08421, 0.00822, 0.0074, 0.03475, 0.07562, 0.00084, 0.00092, 0.01356, 0.06073, 0.01433, 0.02894, 0.03802, 0.02446, 0.06468, 0.06695, 0.01906, 1e-005, 0.04799, 0.05212, 0.05727, 0.0216, 0.05344, 0.00016, 0.00027, 0.01043, 0.01503} }, // Czech
            //Source: http://practicalcryptography.com/cryptanalysis/letter-frequencies-various-languages/
            { "ru", new double[] { 0.0804, 0.0155, 0.0475, 0.0188, 0.0295, 0.0821, 0.0022, 0.008, 0.0161, 0.0798, 0.0136, 0.0349, 0.0432, 0.0311, 0.0672, 0.1061, 0.0282, 0.0538, 0.0571, 0.0583, 0.0228, 0.0041, 0.0102, 0.0058, 0.0123, 0.0055, 0.0034, 0.0003, 0.0191, 0.0139, 0.0031, 0.0063, 0.02 } }, // Russian
            //Source: https://everything2.com/title/Letter+frequency+in+several+languages
            { "la", new double[] { 0.072, 0.012, 0.033, 0.017, 0.092, 0.009, 0.014, 0.005, 0.101, 0, 0, 0.021, 0.034, 0.06, 0.044, 0.03, 0.013, 0.068, 0.068, 0.072, 0.074, 0.007, 0, 0.006, 0, 0 } } // Latin
        };
        
        public static Dictionary<string, string> Alphabets = new Dictionary<string, string>()
        {
            { "en", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
            { "de", "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜß" },
            { "es", "ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÑÓÚÜ" },
            { "fr", "ABCDEFGHIJKLMNOPQRSTUVWXYZÀÂŒÇÈÉÊËÎÏÔÙÛ" },
            { "it", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
            { "hu", "ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÓÖŐÚÜŰ" },
            { "ru", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" },
            { "cs", "AÁBCČDĎEÉĚFGHIÍJKLMNŇOÓPQRŘSŠTŤUÚŮVWXYÝZŽ" },
            { "el", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ" },
            { "la", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
        };

        public static string Alphabet(string language, bool useSpaces = false)
        {
            if (!Alphabets.ContainsKey(language)) return null;
            if (useSpaces) return Alphabets[language] + " ";
            return Alphabets[language];
        }

        public static double[] Load1Grams(string language, out string alphabet, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 1, useSpaces ? "-sp" : "");
                return Load1GramsGZ(filename, out alphabet);
            }
            catch (Exception ex)
            {
                alphabet = null;
                return null;
            }
        }

        public static double[,] Load2Grams(string language, out string alphabet, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 2, useSpaces ? "-sp" : "");
                return Load2GramsGZ(filename, out alphabet);
            }
            catch (Exception ex)
            {
                alphabet = null;
                return null;
            }
        }

        public static double[,,] Load3Grams(string language, out string alphabet, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 3, useSpaces ? "-sp" : "");
                return Load3GramsGZ(filename, out alphabet);
            }
            catch (Exception ex)
            {
                alphabet = null;
                return null;
            }
        }

        public static double[,,,] Load4Grams(string language, out string alphabet, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 4, useSpaces ? "-sp" : "");
                return Load4GramsGZ(filename, out alphabet);
            }
            catch (Exception ex)
            {
                alphabet = null;
                return null;
            }
        }

        public static double[] Load1GramsGZ(string filename, out string alphabet)
        {
            uint[] freq;
            uint max;
            ulong sum;
            
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[])bf.Deserialize(gz);
                }
            }

            double[] result = new double[alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
                result[a] = Math.Log((freq[a] + 0.001) / max);

            return result;
        }

        public static double[,] Load2GramsGZ(string filename, out string alphabet)
        {
            uint[,] freq;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[,])bf.Deserialize(gz);
                }
            }

            double[,] result = new double[alphabet.Length, alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
                for (int b = 0; b < alphabet.Length; b++)
                        result[a, b] = Math.Log((freq[a, b] + 0.001) / max);

            return result;
        }

        public static double[,,] Load3GramsGZ(string filename, out string alphabet)
        {
            uint[,,] freq;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[,,])bf.Deserialize(gz);
                }
            }

            double[,,] result = new double[alphabet.Length, alphabet.Length, alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
                for (int b = 0; b < alphabet.Length; b++)
                    for (int c = 0; c < alphabet.Length; c++)
                        result[a, b, c] = Math.Log((freq[a, b, c] + 0.001) / max);

            return result;
        }

        public static double[,,,] Load4GramsGZ(string filename, out string alphabet)
        {
            uint[,,,] freq;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq = (uint[,,,])bf.Deserialize(gz);
                }
            }

            double[,,,] result = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            for (int a = 0; a < alphabet.Length; a++)
                for (int b = 0; b < alphabet.Length; b++)
                    for (int c = 0; c < alphabet.Length; c++)
                        for (int d = 0; d < alphabet.Length; d++)
                            result[a, b, c, d] = Math.Log((freq[a, b, c, d] + 0.001) / max);

            return result;
        }

        public static double Calculate1GramCost(double[] ngrams, int[] plaintext)
        {
            int end = plaintext.Length;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += ngrams[plaintext[i]];

            return value / end;
        }

        public static double Calculate2GramCost(double[,] ngrams, int[] plaintext)
        {
            int end = plaintext.Length - 1;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += ngrams[plaintext[i], plaintext[i + 1]];

            return value / end;
        }

        public static double Calculate3GramCost(double[,,] ngrams, int[] plaintext)
        {
            int end = plaintext.Length - 2;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += ngrams[plaintext[i], plaintext[i + 1], plaintext[i + 2]];

            return value / end;
        }

        public static double Calculate4GramCost(double[,,,] ngrams, int[] plaintext)
        {
            int end = plaintext.Length - 3;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += ngrams[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];

            return value / end;
        }

        // deprecated:

        public static double[,,] Load3Grams(string language, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.bin", language, 3, useSpaces ? "-sp" : "");
                return Load3Grams(filename, Alphabet(language, useSpaces));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static double[,,,] Load4Grams(string language, bool useSpaces = false)
        {
            try
            {
                string filename = String.Format("{0}-{1}gram-nocs{2}.bin", language, 4, useSpaces ? "-sp" : "");
                return Load4Grams(filename, Alphabet(language, useSpaces));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Load 3Gram file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="alphabet"></param>
        static double[,,] Load3Grams(string filename, string alphabet)
        {
            var _trigrams = new double[alphabet.Length, alphabet.Length, alphabet.Length];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
                for (int i = 0; i < alphabet.Length; i++)
                    for (int j = 0; j < alphabet.Length; j++)
                        for (int k = 0; k < alphabet.Length; k++)
                            _trigrams[i, j, k] = BitConverter.ToDouble(reader.ReadBytes(8), 0);

            return _trigrams;
        }

        /// <summary>
        /// Load 4Gram file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="alphabet"></param>
        static double[,,,] Load4Grams(string filename, string alphabet)
        {
            var _quadgrams = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
                for (int i = 0; i < alphabet.Length; i++)
                    for (int j = 0; j < alphabet.Length; j++)
                        for (int k = 0; k < alphabet.Length; k++)
                            for (int l = 0; l < alphabet.Length; l++)
                                _quadgrams[i, j, k, l] = BitConverter.ToDouble(reader.ReadBytes(8), 0);

            return _quadgrams;
        }
        
        /// <summary>
        /// Calculate cost value based on index of coincidence
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        static public double CalculateIoC(int[] plaintext)
        {
            Dictionary<int, UInt64> countChars = new Dictionary<int, UInt64>();

            foreach (int c in plaintext)
                if (countChars.ContainsKey(c)) countChars[c]++; else countChars.Add(c, 1);

            UInt64 value = 0;

            foreach (UInt64 cnt in countChars.Values)
                value += cnt * (cnt - 1);

            UInt64 N = (UInt64)plaintext.Length;
            return (double)value / (N * (N - 1));
        }
    }

    /// <summary>
    /// Abstract super class for nGrams classes
    /// </summary>
    public abstract class Grams
    {
        public string Alphabet { get; protected set; }
        public abstract double CalculateCost(int[] plaintext);
    }

    public class UniGrams : Grams
    {
        public float[] Frequencies;

        public UniGrams(string language, bool useSpaces = false)
        {
            string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 1, useSpaces ? "-sp" : "");
            LoadGZ(filename);
        }

        private void LoadGZ(string filename)
        {
            uint[] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[]) bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length];
                    for (int a = 0; a < Alphabet.Length; a++)
                        Frequencies[a] = (float)Math.Log((data[a] + 0.001) / max);
                }
            }

        }

        public override double CalculateCost(int[] plaintext)
        {
            int end = plaintext.Length;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += Frequencies[plaintext[i]];

            return value / end;
        }
    }

    public class BiGrams : Grams
    {
        public float[,] Frequencies;

        public BiGrams(string language, bool useSpaces = false)
        {
            string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 2, useSpaces ? "-sp" : "");
            LoadGZ(filename);
        }

        private void LoadGZ(string filename)
        {
            uint[,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                    for (int b = 0; b < Alphabet.Length; b++)
                        Frequencies[a, b] = (float)Math.Log((data[a, b] + 0.001) / max);
                }                                                
            }            
        }

        public override double CalculateCost(int[] plaintext)
        {
            int end = plaintext.Length - 1;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += Frequencies[plaintext[i], plaintext[i + 1]];

            return value / end;
        }
    }

    public class TriGrams : Grams
    {
        public float[,,] Frequencies;
        
        public TriGrams(string language, bool useSpaces = false)
        {
            string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 3, useSpaces ? "-sp" : "");
            LoadGZ(filename);
        }

        private void LoadGZ(string filename)
        {
            uint[,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,]) bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[, ,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                    for (int b = 0; b < Alphabet.Length; b++)
                    for (int c = 0; c < Alphabet.Length; c++)
                        Frequencies[a, b, c] = (float)Math.Log((data[a, b, c] + 0.001) / max);
                }
                
            }         
        }

        public override double CalculateCost(int[] plaintext)
        {
            int end = plaintext.Length - 2;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += Frequencies[plaintext[i], plaintext[i + 1], plaintext[i + 2]];

            return value / end;
        }
    }

    public class QuadGrams : Grams
    {
        public float[,,,] Frequencies;

        public QuadGrams(string language, bool useSpaces = false)
        {
            string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 4, useSpaces ? "-sp" : "");
            LoadGZ(filename);
        }

        private void LoadGZ(string filename)
        {
            uint[,,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);                
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[, , ,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                    for (int b = 0; b < Alphabet.Length; b++)
                    for (int c = 0; c < Alphabet.Length; c++)
                    for (int d = 0; d < Alphabet.Length; d++)
                        Frequencies[a, b, c, d] = (float)Math.Log((data[a, b, c, d] + 0.001) / max);
                }
            }        
        }

        public override double CalculateCost(int[] plaintext)
        {
            int end = plaintext.Length - 3;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += Frequencies[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];

            return value / end;
        }
    }

    public class PentaGrams : Grams
    {
        public float[,,,,] Frequencies;        

        public PentaGrams(string language, bool useSpaces = false)
        {
            string filename = String.Format("{0}-{1}gram-nocs{2}.gz", language, 5, useSpaces ? "-sp" : "");
            LoadGZ(filename);
        }

        private void LoadGZ(string filename)
        {
            uint[,,,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,,,]) bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[, , , ,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                    for (int b = 0; b < Alphabet.Length; b++)
                    for (int c = 0; c < Alphabet.Length; c++)
                    for (int d = 0; d < Alphabet.Length; d++)
                    for (int e = 0; e < Alphabet.Length; e++)
                        Frequencies[a, b, c, d, e] = (float)Math.Log((data[a, b, c, d, e] + 0.001) / max);
                }
            }           
        }

        public override double CalculateCost(int[] plaintext)
        {
            int end = plaintext.Length - 4;
            if (end <= 0) return 0;

            double value = 0;

            for (int i = 0; i < end; i++)
                value += Frequencies[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3], plaintext[i + 4]];

            return value / end;
        }
    }
}
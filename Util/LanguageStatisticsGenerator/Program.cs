using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace LanguageStatisticsGenerator
{
    public class NGrams
    {
        public string alphabet;

        //public uint[,,,,,,] freq7;
        public uint[,,,,,] freq6;
        public uint[,,,,] freq5;
        public uint[,,,] freq4;
        public uint[,,] freq3;
        public uint[,] freq2;
        public uint[] freq1;

        public uint max;
        public ulong sum;

        // create statistics from directory
        public NGrams(string alphabet, string path, bool useSpace)
        {
            if (!useSpace) alphabet.Replace(" ", "");
            if (useSpace && !alphabet.Contains(" ")) alphabet += " ";

            this.alphabet = alphabet;
            var char2num = Enumerable.Range(0, alphabet.Length).ToDictionary(i => alphabet[i]);

            string line;
            int counter = 0;
            int j = 0;
            string pattern = "[^" + alphabet + "]";

            //freq7 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq6 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq5 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq4 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq3 = new uint[alphabet.Length, alphabet.Length, alphabet.Length];
            freq2 = new uint[alphabet.Length, alphabet.Length];
            freq1 = new uint[alphabet.Length];

            Console.WriteLine("Creating files list");
            string[] files = Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories);
            Console.WriteLine("Files list created");

            foreach (var filename in files)
            {
                try
                {
                    Console.WriteLine(String.Format("Reading {0}", filename));
                    using (ZipArchive zipfile = ZipFile.OpenRead(filename))
                    {
                        foreach (ZipArchiveEntry entry in zipfile.Entries)
                        {
                            if (!entry.FullName.EndsWith("txt"))
                            {
                                continue;
                            }
                            using (var stream = entry.Open())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    bool startfound = false;
                                    bool endfound = false;

                                    while ((line = sr.ReadLine()) != null && !endfound)
                                    {
                                        string w = line.Substring(line.IndexOf('\t') + 1).ToUpper();
                                        
                                        if (!startfound) // search for start line
                                        {
                                            if (w.StartsWith("*** START"))
                                            {
                                                startfound = true;
                                            }
                                            continue;
                                        }
                                        if (!endfound) // search for end line
                                        {
                                            if (w.StartsWith("*** END"))
                                            {
                                                endfound = true;
                                                continue;
                                            }                                            
                                        }

                                        w = w.Replace("Á", "A");
                                        w = w.Replace("Â", "A");
                                        w = w.Replace("À", "A");

                                        w = w.Replace("É", "E");
                                        w = w.Replace("Ê", "E");
                                        w = w.Replace("È", "E");

                                        w = w.Replace("Í", "I");
                                        w = w.Replace("Î", "I");
                                        w = w.Replace("Ì", "I");

                                        w = w.Replace("Ó", "O");
                                        w = w.Replace("Ô", "O");
                                        w = w.Replace("Ò", "O");

                                        w = w.Replace("Ú", "U");
                                        w = w.Replace("Û", "U");
                                        w = w.Replace("Ù", "U");

                                        if (useSpace)
                                        {
                                            w = " " + w + " ";
                                            w = Regex.Replace(w, pattern, "");
                                            w = Regex.Replace(w, " +", " ");
                                        }
                                        else
                                        {
                                            w = Regex.Replace(w, pattern, "");
                                        }

                                        var n = w.Select(c => char2num[c]).ToArray();

                                        //for (int i = 0; i + 7 <= n.Length; i++) freq7[n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4], n[i + 5], n[i + 6]]++;
                                        for (int i = 0; i + 6 <= n.Length; i++) freq6[n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4], n[i + 5]]++;
                                        for (int i = 0; i + 5 <= n.Length; i++) freq5[n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4]]++;
                                        for (int i = 0; i + 4 <= n.Length; i++) freq4[n[i], n[i + 1], n[i + 2], n[i + 3]]++;
                                        for (int i = 0; i + 3 <= n.Length; i++) freq3[n[i], n[i + 1], n[i + 2]]++;
                                        for (int i = 0; i + 2 <= n.Length; i++) freq2[n[i], n[i + 1]]++;
                                        if (useSpace)
                                        {
                                            for (int i = 1; i + 1 <= n.Length; i++) freq1[n[i]]++;
                                        }
                                        else
                                        {
                                            for (int i = 0; i + 1 <= n.Length; i++) freq1[n[i]]++;
                                        }

                                        counter++;
                                        if (++j == 50000)
                                        {
                                            j = 0;
                                            Console.Write(counter + " lines read\r");
                                            Console.Out.Flush();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine(String.Format("{0} successfully read", filename));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("Excption during read of {0}: ", ex.Message));
                }
            }                
                
            GetMaxAndSum(4);

            Console.WriteLine(counter + " lines read");
        }

        public NGrams(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    alphabet = (string)bf.Deserialize(gz);
                    max = (uint)bf.Deserialize(gz);
                    sum = (ulong)bf.Deserialize(gz);
                    freq4 = (uint[,,,])bf.Deserialize(gz);
                }
            }
        }

        void GetMaxAndSum(int n)
        {
            sum = 0;
            max = 0;

            /*if (n == 7)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                            for (int d = 0; d < alphabet.Length; d++)
                                for (int e = 0; e < alphabet.Length; e++)
                                    for (int f = 0; f < alphabet.Length; f++)
                                        for (int g = 0; g < alphabet.Length; g++)
                                        {
                                            uint x = freq7[a, b, c, d, e, f, g];
                                            if (max < x) max = x;
                                            sum += x;
                                        }
            }
            else*/ if (n == 6)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                            for (int d = 0; d < alphabet.Length; d++)
                                for (int e = 0; e < alphabet.Length; e++)                                
                                    for (int f = 0; f < alphabet.Length; f++)
                                    {
                                        uint x = freq6[a, b, c, d, e,f];
                                        if (max < x) max = x;
                                        sum += x;
                                    }
            }
            else if (n == 5)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                            for (int d = 0; d < alphabet.Length; d++)
                                for (int e = 0; e < alphabet.Length; e++)
                                {
                                    uint x = freq5[a, b, c, d, e];
                                    if (max < x) max = x;
                                    sum += x;
                                }
            }
            else if (n == 4)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                            for (int d = 0; d < alphabet.Length; d++)
                            {
                                uint x = freq4[a, b, c, d];
                                if (max < x) max = x;
                                sum += x;
                            }
            }
            else if (n == 3)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                        for (int c = 0; c < alphabet.Length; c++)
                        {
                            uint x = freq3[a, b, c];
                            if (max < x) max = x;
                            sum += x;
                        }
            }
            else if (n == 2)
            {
                for (int a = 0; a < alphabet.Length; a++)
                    for (int b = 0; b < alphabet.Length; b++)
                    {
                        uint x = freq2[a, b];
                        if (max < x) max = x;
                        sum += x;
                    }
            }
            else if (n == 1)
            {
                for (int a = 0; a < alphabet.Length; a++)
                {
                    uint x = freq1[a];
                    if (max < x) max = x;
                    sum += x;
                }
            }
        }


        /// <summary>
        /// Magic number (ASCII string 'CTLS') for statistics file format.
        /// </summary>
        public const uint FileFormatMagicNumber = 'C' + ('T' << 8) + ('L' << 16) + ('S' << 24);

        private void WriteStatisticsFile(int gramLength, IEnumerable<float> frequencies, string outputFile)
        {
            using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (var gz = new GZipStream(fs, CompressionMode.Compress))
            using (var bw = new BinaryWriter(gz))
            {
                bw.Write(FileFormatMagicNumber);
                bw.Write(gramLength);
                bw.Write(alphabet);
                foreach (var frequencyValue in frequencies)
                {
                    bw.Write(frequencyValue);
                }
            }
        }

        private static IEnumerable<float> CalculateLogs(Array freq, ulong sum)
        {            
            return freq.Cast<uint>().Select(value => (float)Math.Log(value == 0 ? 1.0 / sum : value  / (double)sum));
        }

        public void WriteGZ(string filename)
        {
            var freqs = new Array[] { freq1, freq2, freq3, freq4, freq5, freq6/*, freq7*/ };
            for (int gramLength = 6; gramLength >= 1; gramLength--)
            {
                Console.WriteLine("Writing {0}-grams", gramLength);
                GetMaxAndSum(gramLength);
                WriteStatisticsFile(gramLength, CalculateLogs(freqs[gramLength-1], sum), string.Format(filename + ".gz", gramLength));
            }
        }
    }

    class Program
    {
        /* Source: http://wortschatz.uni-leipzig.de
        static Dictionary<string, string> sentencesFileNames = new Dictionary<string, string>
            {
                {"en", "eng_news_2015_3M-sentences.txt" },
                {"de", "deu_news_2015_3M-sentences.txt" },
                {"fr", "fra_news_2010_1M-sentences.txt" },
                {"es", "spa_news_2011_1M-sentences.txt" },
                {"it", "ita_news_2010_1M-sentences.txt" },
                {"hu", "hun_newscrawl_2011_1M-sentences.txt" },
                {"ru", "rus_news_2010_1M-sentences.txt" },
                {"cs", "ces_news_2005-2007_1M-sentences.txt" },
                {"la", "lat_wikipedia_2016_100K-sentences.txt" },
                {"el", "ell_newscrawl_2017_1M-sentences.txt" },
            };
        */

        static Dictionary<string, string> alphabets = new Dictionary<string, string>
        {
                {"en", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                {"de", "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜß" },
                {"fr", "ABCDEFGHIJKLMNOPQRSTUVWXYZÀÂŒÇÈÉÊËÎÏÔÙÛ" },
                {"es", "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ" },
                {"it", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                {"hu", "ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÓÖŐÚÜŰ" },
                {"ru", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" },
                {"cs", "AÁBCČDĎEÉĚFGHIÍJKLMNŇOÓPQRŘSŠTŤUÚŮVWXYÝZŽ" },
                {"la", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                {"el", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ" },
                {"nl", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"}
        };    
        
        static void Main(string[] args)
        {
            if(args.Length!=2)
            {
                Console.WriteLine("Usage: {0} textcorpusdirectory (alphabetfile | language selector)", System.AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("\nSpecify a predefined language selector or a file, that\ncontains the alphabet as a single line of text (UTF-8).");
                Console.WriteLine("The following alphabets are predefined:");
                foreach (var a in alphabets.OrderBy(x => x.Key))
                    Console.WriteLine("\t{0}: {1}", a.Key, a.Value);
                return;
            }

            string alphabet;

            string selector = args[1].ToLower();

            if (alphabets.ContainsKey(selector))
            {
                alphabet = alphabets[selector];
            }
            else
            {
                try
                {
                    alphabet = File.ReadAllText(args[1], Encoding.UTF8);
                    alphabet = Regex.Replace(alphabet, "[\r\n]", "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("Error while reading alphabet file '{0}'", args[1]));
                    return;
                }
                selector = "lang";
            }

            alphabet = alphabet.ToUpper();

            var duplicates = alphabet.Distinct().Where(c => alphabet.Count(d => c == d) > 1);
            if (duplicates.Count() > 0)
            {
                Console.WriteLine("Error: alphabet contains duplicate characters: '" + String.Join("", duplicates.OrderBy(c => c)) + "'");
                return;
            }

            string outname = selector + "-{0}gram-nocs";

            string sentencesFilename = args[0];

            try
            {
                Console.WriteLine("creating statistics without spaces...");
                new NGrams(alphabet, sentencesFilename, false).WriteGZ(outname);
                Console.WriteLine("creating statistics with spaces...");
                new NGrams(alphabet, sentencesFilename, true).WriteGZ(outname + "-sp");
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("Error while reading text corpus file '{0}': {1}", sentencesFilename, ex.Message));
                return;
            }
        }
    }
}
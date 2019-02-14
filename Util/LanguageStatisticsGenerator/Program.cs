using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace LanguageStatisticsGenerator
{
    class NGrams
    {
        public string alphabet;

        public uint[,,,,] freq5;
        public uint[,,,] freq4;
        public uint[,,] freq3;
        public uint[,] freq2;
        public uint[] freq1;

        public uint max;
        public ulong sum;
        const int size = 4;

        public NGrams(string alphabet, uint[,,,] freq)
        {
            this.alphabet = alphabet;
            this.freq4 = freq;
            GetMaxAndSum(4);
        }

        public NGrams(string alphabet, string text)
        {
            this.alphabet = alphabet;
            var char2num = Enumerable.Range(0, alphabet.Length).ToDictionary(i => alphabet[i]);

            freq4 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];

            for (int i = 0; i + size <= text.Length; i++)
                freq4[char2num[text[i]], char2num[text[i + 1]], char2num[text[i + 2]], char2num[text[i + 3]]]++;

            GetMaxAndSum(4);
        }

        // create statistics from data file 
        public NGrams(string alphabet, string filename, bool useSpace)
        {
            if (!useSpace) alphabet.Replace(" ", "");
            if (useSpace && !alphabet.Contains(" ")) alphabet += " ";

            this.alphabet = alphabet;
            var char2num = Enumerable.Range(0, alphabet.Length).ToDictionary(i => alphabet[i]);

            string line;
            int counter = 0;
            int j = 0;
            string pattern = "[^" + alphabet + "]";

            freq5 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq4 = new uint[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            freq3 = new uint[alphabet.Length, alphabet.Length, alphabet.Length];
            freq2 = new uint[alphabet.Length, alphabet.Length];
            freq1 = new uint[alphabet.Length];

            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string w = line.Substring(line.IndexOf('\t') + 1).ToUpper();
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

                    for (int i = 0; i + 5 <= n.Length; i++) freq5[n[i], n[i + 1], n[i + 2], n[i + 3], n[i + 4]]++;
                    for (int i = 0; i + 4 <= n.Length; i++) freq4[n[i], n[i + 1], n[i + 2], n[i + 3]]++;
                    for (int i = 0; i + 3 <= n.Length; i++) freq3[n[i], n[i + 1], n[i + 2]]++;
                    for (int i = 0; i + 2 <= n.Length; i++) freq2[n[i], n[i + 1]]++;
                    if (useSpace)
                        for (int i = 1; i + 1 <= n.Length; i++) freq1[n[i]]++;
                    else
                        for (int i = 0; i + 1 <= n.Length; i++) freq1[n[i]]++;

                    counter++;
                    if (++j == 1000)
                    {
                        j = 0;
                        Console.Write(counter + " lines read\r");
                        Console.Out.Flush();
                    }
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

            if (n == 5)
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

        public void WriteGZ(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();

            GetMaxAndSum(5);
            using (var fs = new FileStream(String.Format(filename + ".gz", 5), FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, alphabet);
                    bf.Serialize(gz, max);
                    bf.Serialize(gz, sum);
                    bf.Serialize(gz, freq5);
                }
            }

            GetMaxAndSum(4);
            using (var fs = new FileStream(String.Format(filename + ".gz", 4), FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, alphabet);
                    bf.Serialize(gz, max);
                    bf.Serialize(gz, sum);
                    bf.Serialize(gz, freq4);
                }
            }

            GetMaxAndSum(3);
            using (var fs = new FileStream(String.Format(filename + ".gz", 3), FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, alphabet);
                    bf.Serialize(gz, max);
                    bf.Serialize(gz, sum);
                    bf.Serialize(gz, freq3);
                }
            }

            GetMaxAndSum(2);
            using (var fs = new FileStream(String.Format(filename + ".gz", 2), FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, alphabet);
                    bf.Serialize(gz, max);
                    bf.Serialize(gz, sum);
                    bf.Serialize(gz, freq2);
                }
            }

            GetMaxAndSum(1);
            using (var fs = new FileStream(String.Format(filename + ".gz", 1), FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, alphabet);
                    bf.Serialize(gz, max);
                    bf.Serialize(gz, sum);
                    bf.Serialize(gz, freq1);
                }
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
                {"es", "ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÑÓÚÜ" },
                {"it", "ABCDEFGHIJKLMNOPQRSTUVWXYZÀÈÌÒÙ" },
                {"hu", "ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÓÖŐÚÜŰ" },
                {"ru", "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" },
                {"cs", "AÁBCČDĎEÉĚFGHIÍJKLMNŇOÓPQRŘSŠTŤUÚŮVWXYÝZŽ" },
                {"la", "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                {"el", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ" },
    };

        public static void WriteStatistics(string path, string filename, NGrams data)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    using (var entryStream = archive.CreateEntry(filename + ".bin").Open())
                    {
                        bf.Serialize(entryStream, data.alphabet);
                        bf.Serialize(entryStream, data.max);
                        bf.Serialize(entryStream, data.sum);
                        bf.Serialize(entryStream, data.freq4);
                    }
                }

                using (var fileStream = new FileStream(path + filename + ".zip", FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public static NGrams ReadStatistics(string filename)
        {
            NGrams data;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (ZipArchive zip = new ZipArchive(fs))
            {
                var entry = zip.Entries.First();

                using (Stream sr = entry.Open())
                {
                    string alphabet = (string)bf.Deserialize(sr);
                    uint max = (uint)bf.Deserialize(sr);
                    ulong sum = (ulong)bf.Deserialize(sr);
                    var freq = (uint[,,,])bf.Deserialize(sr);
                    data = new NGrams(alphabet, freq);
                }
            }

            return data;
        }
        
        static void Main(string[] args)
        {
            if(args.Length!=2)
            {
                Console.WriteLine("Usage: {0} textcorpusfile (alphabetfile | language selector)", System.AppDomain.CurrentDomain.FriendlyName);
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
                Console.WriteLine("Error: Alphabet contains duplicate characters: '" + String.Join("", duplicates.OrderBy(c => c)) + "'");
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
                Console.WriteLine(String.Format("Error while reading text corpus file '{0}'", sentencesFilename));
                return;
            }
        }
    }
}
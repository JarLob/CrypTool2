﻿using System;
using System.IO;
using System.Threading;

namespace PlayfairAnalysis.Common
{
    public class Stats
    {

        public static long[] monogramStats = new long[Utils.TEXT_ALPHABET_SIZE];
        public static long[] bigramStats = new long[Utils.TEXT_ALPHABET_SIZE * 32];
        private static short[] hexagramStats = null;
        public static long evaluations = 0;

        public static bool readHexagramStatsFile(String filename, CancellationToken ct)
        {
            long start = DateTime.Now.Ticks;

            CtAPI.printf("Loading hexagram stats file {0}\n",
                    filename);

            int totalShortRead = 0;

            try
            {
                //FileInputStream _is = new FileInputStream(new File(filename));

                //Buffer.BlockCopy()
                

                hexagramStats = new short[26 * 26 * 26 * 26 * 26 * 26];

                using (var reader = new BinaryReader(File.OpenRead(filename)))
                {
                    var stats = reader.ReadBytes(hexagramStats.Length * 2);
                    for (int i = 0; i < stats.Length; i += 2)
                    {
                        //Convert big endian to little endian values:
                        var tmp = stats[i];
                        stats[i] = stats[i + 1];
                        stats[i + 1] = tmp;
                    }
                    Buffer.BlockCopy(stats, 0, hexagramStats, 0, stats.Length);
                }

                /*
                int[] hist = new int[100000];
                for (short h : hexagramStats) {
                    hist[h]++;
                }
                for (int i = 0; i < hist.Length; i++) {
                    if (hist[i] > 0) {
                        Console.Out.WriteLine("%,8d %,10d/%,10d\n", i, hist[i], hexagramStats.Length);
                    }
                }
                */


            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read hexa file {0} - {1}", filename, ex.ToString());
            }
            CtAPI.printf("Hexagram stats file {0} loaded successfully ({1} seconds), size = {2} bytes\n",
                    filename, TimeSpan.FromTicks(DateTime.Now.Ticks - start).TotalSeconds, totalShortRead * 2);
            CtAPI.println("");
            CtAPI.println("");
            return true;
        }

        private static int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

        public static long evalPlaintextHexagram(int[] plaintext, int plaintextLength)
        {

            CtAPI.shutdownIfNeeded();
            Stats.evaluations++;

            int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
            long val = 0;
            for (int i = 5; i < plaintextLength; i++)
            {
                index = (index % POWER_26_5) * 26 + plaintext[i];
                val += hexagramStats[index];
            }
            return (val * 1000) / (plaintextLength - 5);

        }

        public static long evalPlaintextHexagram(int[] plaintext)
        {
            return evalPlaintextHexagram(plaintext, plaintext.Length);
        }

        public static String evaluationsSummary()
        {
            var elapsed = Utils.getElapsed();
            return $"[{elapsed.TotalSeconds:N0} sec.][{Stats.evaluations / 1000:N0}K decryptions ({Stats.evaluations / elapsed.TotalMilliseconds:N0}K/sec.)]";
        }

        static int readBigramFile(String fileName)
        {

            String line = "";
            int items = 0;

            try
            {
                using (var bufferedReader = new StreamReader(fileName))
                {
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        line = line.ToUpper();
                        String[] split = line.Split(new [] { "[ ]+" }, StringSplitOptions.None);
                        int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0][0]);
                        int l2 = Utils.TEXT_ALPHABET.IndexOf(split[0][1]);
                        if (l1 < 0 || l2 < 0)
                        {
                            continue;
                        }
                        long freq = long.Parse(split[1]);

                        bigramStats[(l1 << 5) + l2] += freq;
                        items++;
                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read bigram file {0} - {1}", fileName, ex.ToString());
            }

            CtAPI.printf("Bigram file read: {0}, items  = {1}  \n", fileName, items);

            convertToLog(bigramStats);

            return items;

        }

        static int readMonogramFile(String fileName, bool m209)
        {

            String line;
            int items = 0;

            try
            {
                using (var bufferedReader = new StreamReader(fileName))
                {
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        line = line.ToUpper();
                        String[] split = line.Split(new[] { "[ ]+" }, StringSplitOptions.None);
                        int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0][0]);
                        if (l1 < 0)
                        {
                            continue;
                        }
                        long freq = long.Parse(split[1]);

                        monogramStats[l1] += freq;
                        items++;
                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read mono file {0} - {1}", fileName, ex.ToString());
            }

            CtAPI.printf("mono file read: {0}, items  = {1}  \n", fileName, items);

            convertToLog(monogramStats);

            return items;

        }

        static int readFileForStats(String fileName, bool m209)
        {


            String line;
            int length = 0;
            String from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
            String to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();


            try
            {
                using (var bufferedReader = new StreamReader(fileName))
                {
                    int l2 = -1;
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        foreach (char c_iter in line.ToUpper())
                        {
                            var c = c_iter;
                            if (m209)
                            {
                                if (c == ' ' || c == ',' || c == '.')
                                {
                                    c = 'Z';
                                }
                            }

                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            int l1 = l2;
                            l2 = Utils.TEXT_ALPHABET.IndexOf(c);
                            if (l1 != -1 && l2 != -1)
                            {
                                monogramStats[l1]++;
                                bigramStats[(l1 << 5) + l2]++;
                                length++;
                            }
                        }
                    }

                    // Always close files.
                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read text file for stats  {0} - {1}", fileName, ex.ToString());
            }

            convertToLog(bigramStats);
            convertToLog(monogramStats);

            CtAPI.printf("Text file read for stats {0}, length = {1} \n", fileName, length);

            return length;
        }

        private static void convertToLog(long[] stats)
        {
            long minVal = long.MaxValue;
            foreach (long stat in stats)
            {
                if ((stat > 0) && (stat < minVal))
                {
                    minVal = stat;
                }
            }

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] > 0)
                {
                    stats[i] = (long)(10000.0 * Math.Log((1.0 * stats[i]) / (1.0 * minVal)));
                }
            }

        }

        public static bool load(String dirname, Language language, bool m209)
        {
            int n = 1;
            switch (language)
            {
                case Language.ENGLISH:
                    //n *= readFileForStats("book.txt", m209);
                    n *= readBigramFile(dirname + "/" + "english_bigrams.txt");
                    n *= readMonogramFile(dirname + "/" + "english_monograms.txt", m209);
                    break;
                case Language.FRENCH:
                    n *= readBigramFile(dirname + "/" + "french_bigrams.txt");
                    n *= readMonogramFile(dirname + "/" + "french_monograms.txt", m209);
                    break;
                case Language.ITALIAN:
                    n *= readFileForStats(dirname + "/" + "italianbook.txt", m209);
                    break;
                case Language.GERMAN:
                    n *= readFileForStats(dirname + "/" + "germanbook.txt", m209);
                    //n *= readBigramFile(dirname + "/" + "german_bigrams.txt");
                    //n *= readMonogramFile(dirname + "/" + "german_monograms.txt", m209);
                    break;
            }
            if (m209)
            {
                monogramStats['E' - 'A'] = Math.Max(60000, monogramStats['E' - 'A']);
                monogramStats['Z' - 'A'] = Math.Max(80000, monogramStats['Z' - 'A']);
            }

            if (n == 0)
            {
                CtAPI.goodbyeFatalError("Cannot load stats - language: " + language);
            }
            return true;
        }

    }

}

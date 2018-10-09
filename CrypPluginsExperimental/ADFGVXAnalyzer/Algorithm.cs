﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;

namespace ADFGVXAnalyzer
{
    public class IndexOfCoinzidenz
    {

        public const double GERMAN = 7.6;
        public const double ITALIA = 7.6;
        public const double FRANCE = 7.8;
        public const double SPAIN = 7.5;
        public const double ENGLISH = 6.5;

    }

    public class ThreadingHelper
    {
        public long[] decryptions;
        public int taskcount;
        public double bestOverall = 0.0;
        public object bestOverallLock = new object();
        public object decryptionsLock = new object();
        ADFGVXAnalyzer analyzer;

        public ThreadingHelper(int taskcount, ADFGVXAnalyzer analyzer)
        {
            this.taskcount = taskcount;
            this.decryptions = new long[taskcount];
            this.analyzer = analyzer;

        }

        public void UpdateDisplayEnd(int keylength, long decryptions, long alldecryptions)
        {
            alldecryptions *= this.decryptions.Length;
            analyzer.UpdateDisplayEnd(keylength, decryptions, alldecryptions);
            analyzer.ProgressChanged(decryptions, alldecryptions);
        }
    }

    public class Algorithm
    {
        private int keyLength;
        private ADFGVXVector[] ciphers;
        private Alphabet36Vector allPlain;
        private Alphabet36Vector plain;
        private ADFGVXVector interimCipher;
        private Logger log;
        private ThreadingHelper threadingHelper;
        private ADFGVXAnalyzer analyzer;


        private int taskId;


        public Algorithm(int keyLength, String[] messages, Logger log, int taskId, ThreadingHelper threadingHelper, ADFGVXAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            this.threadingHelper = threadingHelper;
            this.taskId = taskId;
            this.log = log;
            this.keyLength = keyLength;
            ciphers = new ADFGVXVector[messages.Length];
            int totalPlainLength = 0;
            int maxPlainLength = 0;
            for (int m = 0; m < messages.Length; m++)
            {
                ciphers[m] = new ADFGVXVector(messages[m].Replace(" ", ""), false);
                totalPlainLength += ciphers[m].length / 2;
                maxPlainLength = Math.Max(maxPlainLength, ciphers[m].length / 2);
            }
            allPlain = new Alphabet36Vector(totalPlainLength, true);
            plain = new Alphabet36Vector(maxPlainLength, false);
            interimCipher = new ADFGVXVector(maxPlainLength * 2, false);
        }

        private double eval(ADFGVX key)
        {
            threadingHelper.decryptions[taskId - 1]++;
            allPlain.length = 0;
            foreach (ADFGVXVector cipher in ciphers)
            {
                key.decode(cipher, interimCipher, plain);
                allPlain.append(plain);
            }
            allPlain.stats();
            return (6000.0 * allPlain.IoC1 + 180000.0 * allPlain.IoC2);
        }


        public void SANgramsIC()
        {

            AlphabetVector keepTranspositionKey = new AlphabetVector(keyLength, false);
            AlphabetVector newTranspositionKey = new AlphabetVector(keyLength, false);

            ADFGVX key = new ADFGVX("", keyLength);
            TranspositionTransformations transforms = new TranspositionTransformations(keyLength, true, true, true);
            int zyklen = 100;

            for (int cycles = 1; cycles <= zyklen; cycles++)
            {
                if (cycles % 10 == 0)
                {
                    analyzer.LogText += Environment.NewLine + "Task id: " + taskId + " starting with cycle: " + cycles;
                }
                key.randomTranspositionKey();
                double score = eval(key);

                double startTemp = 500.0;
                double endTemp = 20.0;
                double delta = 20.0;

                double temp = startTemp;
                for (int step = 0; temp >= endTemp; step++, temp -= delta)
                {
                    transforms.randomize();
                    int size = transforms.size();

                    for (int i = 0; i < size; i++)
                    {
                        keepTranspositionKey.copy(key.transpositionKey);
                        transforms.transform(keepTranspositionKey.TextInInt, newTranspositionKey.TextInInt, keyLength, i);
                        key.setTranspositionKey(newTranspositionKey);
                        double newScore = eval(key);
                        if (SimulatedAnnealing.accept(newScore, score, temp))
                        {
                            score = newScore;
                            if (score > threadingHelper.bestOverall)
                            {
                                printIfBest(key, cycles, step, score, temp);
                            }
                        }
                        else
                        {
                            key.setTranspositionKey(keepTranspositionKey);
                        }
                    }
                    // Update PresentationView
                    long alldecryptions = 0;
                    lock (threadingHelper.decryptionsLock)
                    {
                        foreach (long d in threadingHelper.decryptions)
                        {
                            alldecryptions += d;
                        }
                    }
                    threadingHelper.UpdateDisplayEnd(keyLength, alldecryptions, zyklen * (long)(startTemp / delta) * size + zyklen);
                }
            }

            //log.LogText("Task " + taskId + " Fertig", Logtype.Info);
        }


        private void printIfBest(ADFGVX key, int cycles, int step, double score, double temp)
        {

            lock (threadingHelper.bestOverallLock)
            {
                if (score > threadingHelper.bestOverall)
                {
                    threadingHelper.bestOverall = score;
                    analyzer.AddNewBestListEntry(Math.Round(threadingHelper.bestOverall, 2),
                        Math.Round(allPlain.IoC1, 2), Math.Round(allPlain.IoC2, 2), key.transpositionKey.ToString(), allPlain.ToString());
                    if (allPlain.IoC1 >= IndexOfCoinzidenz.GERMAN * 0.9)
                    {
                        analyzer.Plaintext = allPlain.ToString();
                        analyzer.Transpositionkey = key.transpositionKey.ToString();
                        analyzer.LogText += Environment.NewLine + "Task: " + taskId + Environment.NewLine + "cycle: " + cycles +
                                            Environment.NewLine + "temp: " + temp + Environment.NewLine + "trans key: " + key.transpositionKey +
                                            Environment.NewLine + "bestOverall: " + threadingHelper.bestOverall +
                                            Environment.NewLine + "IoC1 and IoC2: " + allPlain.IoC1 + " " + allPlain.IoC2;
                    }
                }
            }
        }
    }
}

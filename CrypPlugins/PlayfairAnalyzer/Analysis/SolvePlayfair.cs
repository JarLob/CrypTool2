using PlayfairAnalysis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlayfairAnalysis
{
    public class SolvePlayfair
    {
        static void solve(int taskNumber, int saCycles, int innerRounds /* 200000 */, int multiplier/*1500*/, int[] cipherText, String crib, Key simulationKey, AnalysisInstance instance)
        {
            try
            {
                var ct = instance.CancellationToken;
                var utils = new Utils((int)DateTime.Now.Ticks + taskNumber * 100);  //Different seed for every thread
                var transformations = new Transformations(instance, utils);
                var simulatedAnnealing = new SimulatedAnnealing(utils);

                long simulationOriginalScore = (simulationKey == null) ? long.MinValue : simulationKey.score;
                Key currentKey = new Key(instance, utils);
                Key newKey = new Key(instance, utils);
                Key bestKey = new Key(instance, utils);
                currentKey.setCipher(cipherText);
                newKey.setCipher(cipherText);
                bestKey.setCipher(cipherText);
                currentKey.setCrib(crib);
                newKey.setCrib(crib);
                bestKey.setCrib(crib);

                long serialCounter = 0;

                for (int cycle = 0; cycle < saCycles || saCycles == 0; cycle++)
                {
                    ct.ThrowIfCancellationRequested();
                    if (taskNumber == 0)
                    {
                        instance.CtAPI.updateProgress(cycle, saCycles);
                    }

                    transformations.randomize();
                    currentKey.random();

                    long currentScore = currentKey.eval();

                    bestKey.copy(currentKey);
                    long bestScore = bestKey.eval();
                    for (int innerRound = 0; innerRound < innerRounds; innerRound++)
                    {
                        ct.ThrowIfCancellationRequested();
                        transformations.apply(currentKey, newKey, serialCounter++);

                        long newScore = newKey.eval();

                        if (simulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier))
                        {
                            currentKey.copy(newKey);
                            currentScore = newScore;

                            if (currentScore > bestScore)
                            {
                                //                        currentKey.decrypt();
                                //                        long n8 = NGrams.eval8(currentKey.decryptionRemoveNulls, currentKey.decryptionRemoveNullsLength);
                                //
                                //                        if (CtBestList.shouldPushResult(n8)) {
                                //                            CtBestList.pushResult(n8,
                                //                                    currentKey.ToString(),
                                //                                    currentKey.ToString(),
                                //                                    Utils.getString(currentKey.fullDecryption),
                                //                                    Stats.evaluationsSummary() +
                                //                                            String.format("[%d/%d}[Task: %2d][Mult.: %,d]",
                                //                                                    currentKey.decryptionRemoveNullsLength, cipherText.Length, taskNumber, multiplier));
                                //
                                //                        }
                                //
                                bestScore = currentScore;
                                bestKey.copy(currentKey);
                                bestKey.decrypt();
                            }

                        }
                    }

                    if (instance.CtBestList.shouldPushResult(bestScore))
                    {
                        bestKey.alignAlphabet();
                        instance.CtBestList.pushResult(bestScore,
                                bestKey.ToString(),
                                bestKey.ToString(),
                                Utils.getString(bestKey.fullDecryption),
                                instance.Stats.evaluationsSummary() +
                                        $"[{bestKey.decryptionRemoveNullsLength}/{cipherText.Length}][Task: {taskNumber,2}][Mult.: {multiplier:N0}]");
                        if (currentScore == simulationOriginalScore || newKey.matchesFullCrib())
                        {
                            instance.CtAPI.printf("Key found");
                            instance.CtAPI.goodbye();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //Let thread terminate
            }
        }

        public static void solveMultithreaded(int[] cipherText, String cribString, int threads, int cycles, Key simulationKey, AnalysisInstance instance)
        {
            var threadCompletions = new List<TaskCompletionSource<bool>>();
            Key simulationKey_ = simulationKey;
            for (int t_ = 0; t_ < threads; t_++)
            {
                int t = t_;
                double factor = (cribString.Length > cipherText.Length / 2) ? 0.1 : 1.0;
                int multiplier = (int)(factor * 150_000) / cipherText.Length;

                var threadCompletion = new TaskCompletionSource<bool>();
                threadCompletions.Add(threadCompletion);
                new Thread(
                        delegate()
                        {
                            try
                            {
                                solve(t, cycles, 200_000, multiplier, cipherText, cribString, simulationKey_, instance);
                            }
                            finally
                            {
                                threadCompletion.SetResult(true);
                            }
                        }
                ).Start();
            }

            WatchThreadCompletions(threadCompletions, instance);
        }

        private static async void WatchThreadCompletions(List<TaskCompletionSource<bool>> threadCompletions, AnalysisInstance instance)
        {
            await Task.WhenAny(threadCompletions.Select(t => t.Task));
            //call "goodbye" after all threads completed to let CT2 know about it:
            instance.CtAPI.goodbye();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PlayfairAnalysis.Common
{
    public static class CtBestList
    {
        /**
         * Class encapulating a result in best list.
         */
        public class Result
        {
            public long score;
            public String keyString;
            public String keyStringShort;   // short version of the key
            public String plaintextString;
            public String commentString;
            public Result(long score, String keyString, String keyStringShort, String plaintextString, String commentString)
            {
                set(score, keyString, keyStringShort, plaintextString, commentString);
            }
            public void set(long score, String keyString, String keyStringShort, String plaintextString, String commentString)
            {
                this.score = score;
                this.keyString = keyString;
                this.keyStringShort = keyStringShort;
                this.plaintextString = plaintextString;
                this.commentString = commentString;
            }
            public string ToString(int rank)
            {
                return string.Format("{0,2};{1,12:N0};{2};{3};{4}\n", rank, score, keyStringShort, plaintextString, commentString);
            }

        }

        private static object mutex = new object();
        private static List<Result> bestResults = new List<Result>();
        private static Result originalResult = null;
        private static long lastBestListUpdateMillis = 0;
        private static bool shouldUpdateBestList = false;

        private static int maxNumberOfResults = 10;
        private static long scoreThreshold = 0;
        private static bool discardSamePlaintexts = false;
        private static bool throttle = false;

        /**
         * Set best list size.
         * @param size - max number of elements in best list.
         */
        public static void setSize(int size)
        {
            lock (mutex)
            {
                CtBestList.maxNumberOfResults = size;
                clean();
            }
        }

        /**
         * Set a score threshold, below which result will not be included in best list.
         * @param scoreThreshold - threshold value
         */
        public static void setScoreThreshold(long scoreThreshold)
        {
            lock (mutex)
            {
                CtBestList.scoreThreshold = scoreThreshold;
                clean();
            }
        }

        /**
         * If set to yes, ignore results with plaintext already seen (possibly with a different key).
         * @param discardSamePlaintexts
         */
        public static void setDiscardSamePlaintexts(bool discardSamePlaintexts)
        {
            lock (mutex)
            {
                CtBestList.discardSamePlaintexts = discardSamePlaintexts;
                clean();
            }
        }

        /**
         * If set to true, best list will be send to Cryptool no more than once every second.
         * This is useful in case there are many new results, in a short period of time, that would be one of the top best.
         * This can happen very often in hillclimbing processes which slowly progress.
         * @param throttle - if yes, throttle updates to Cryptool.
         */
        public static void setThrottle(bool throttle)
        {
            lock (mutex)
            {
                CtBestList.throttle = throttle;
                clean();
            }
        }

        /**
         * If known, keep the original key and/or plaintext, as well as the score value expected when decrypting with the
         * correct (original) key. If a new result has exactly this score value, the attack with stop.
         * @param score - expected score with the correct key.
         * @param keyString - the correct key.
         * @param keyStringShort - the correct key, short format.
         * @param plaintextString - the expected/original plaintext.
         * @param commentString - a comment
         */
        public static void setOriginal(long score, String keyString, String keyStringShort, String plaintextString, String commentString)
        {
            lock (mutex)
            {
                originalResult = new Result(score, keyString, keyStringShort, plaintextString, commentString);
            }
        }

        /**
         * Resets the best list.
         */
        public static void clear()
        {
            lock (mutex)
            {
                bestResults.Clear();
                CtAPI.displayBestList("-");
            }
        }

        /**
         * Check whether a new result has a score that would allow it to be added to the best list.
         * Useful when some processing is required before pushing the result (e.g. formatting the key string). After formatting, then
         * pushResult should be called to push the result.
         * @param score - score of a new result.
         * @return - score is higher than the lower score in the best list.
         */
        public static bool shouldPushResult(long score)
        {
            lock (mutex)
            {
                if (throttle)
                {
                    if (shouldUpdateBestList && DateTime.Now.Ticks - lastBestListUpdateMillis > TimeSpan.FromSeconds(1000).Ticks)
                    {
                        lastBestListUpdateMillis = DateTime.Now.Ticks;
                        shouldUpdateBestList = false;
                        display();
                    }
                }

                if (score < scoreThreshold)
                {
                    return false;
                }
                int size = bestResults.Count;
                return size < maxNumberOfResults || score > bestResults[size - 1].score;
            }
        }

        /**
         * Push a new result to the task list, if its score is highes that the lowest score in the best list.
         * Discard duplicate keys (and if the relevant option is set, keyes resulting in an already seen plaintext).
         * @param score
         * @param keyString
         * @param keyStringShort
         * @param plaintextString
         * @param commentString
         * @return
         */
        public static bool pushResult(long score, String keyString, String keyStringShort, String plaintextString, String commentString)
        {
            lock (mutex)
            {
                if (discardSamePlaintexts)
                {
                    foreach (Result be in bestResults)
                    {
                        if (be.plaintextString == plaintextString)
                        {
                            return false;
                        }
                    }
                }
                foreach (Result be in bestResults)
                {
                    if (be.keyString == keyString)
                    {
                        return false;
                    }
                }
                int size = bestResults.Count;
                var bestChanged = false;
                if (size == 0 || score > bestResults[0].score)
                {
                    bestChanged = true;
                }
                if (size < maxNumberOfResults)
                {
                    bestResults.Add(new Result(score, keyString, keyStringShort, plaintextString, commentString));
                }
                else if (score > bestResults[size - 1].score)
                {
                    bestResults[size - 1].set(score, keyString, keyStringShort, plaintextString, commentString);
                }
                else
                {
                    return false;
                }
                sort();
                if (bestChanged)
                {
                    Result bestResult = bestResults[0];
                    if (originalResult == null)
                    {
                        CtAPI.displayBestResult(bestResult);
                    }
                    else
                    {
                        CtAPI.displayBestResult(bestResult, originalResult);
                    }
                }
                if (throttle)
                {
                    shouldUpdateBestList = true;
                }
                else
                {
                    display();
                }
                return true;
            }
        }

        // Package private.
        static void display()
        {
            lock (mutex)
            {
                StringBuilder s = new StringBuilder();
                sort();
                for (int i = 0; i < bestResults.Count; i++)
                {
                    s.Append(bestResults[i].ToString(i + 1));
                }
                CtAPI.displayBestList(s.ToString());
            }
        }

        // Private.
        private static void clean()
        {
            lock (mutex)
            {
                sort();
                while (bestResults.Count > maxNumberOfResults)
                {
                    bestResults.RemoveAt(bestResults.Count - 1);
                }
                while (bestResults.Count > 0 && bestResults[bestResults.Count - 1].score < scoreThreshold)
                {
                    bestResults.RemoveAt(bestResults.Count - 1);
                }
            }
        }

        private class ResultComparer : IComparer<Result>
        {
            public int Compare(Result o1, Result o2)
                => (int)(o2.score - o1.score);
        }

        private static void sort()
        {
            lock (mutex)
            {
                bestResults.Sort(new ResultComparer());
            }
        }
    }
}
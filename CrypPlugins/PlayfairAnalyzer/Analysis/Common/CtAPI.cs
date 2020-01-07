using System;

namespace PlayfairAnalysis.Common
{
    public static class CtAPI
    {
        public delegate void BestResultChangedHandler(CtBestList.Result bestResult);
        public static event BestResultChangedHandler BestResultChangedHandlerEvent;

        public delegate void BestListChangedHandler(string bestList);
        public static event BestListChangedHandler BestListChangedEvent;

        public delegate void GoodbyeHandler();
        public static event GoodbyeHandler GoodbyeEvent;

        public delegate void ProgressChangedHandler(double currentValue, double maxValue);
        public static event ProgressChangedHandler ProgressChangedEvent;

        internal static void reset()
        {
            Stats.evaluations = 0;
            Utils.resetTimer();
            CtBestList.clear();
            CtBestList.setScoreThreshold(0);
            CtBestList.setDiscardSamePlaintexts(true);
            CtBestList.setSize(10);
            CtBestList.setThrottle(false);
        }

        internal static void goodbye()
        {
            GoodbyeEvent?.Invoke();
        }

        internal static void goodbyeFatalError(string format, params object[] objects)
        {
            GoodbyeEvent?.Invoke();
            throw new Exception(string.Format(format, objects));
        }

        internal static void printf(string format, params object[] objects)
        {
            Console.Out.WriteLine(format, objects);
        }

        internal static void println(string s)
        {
            Console.Out.WriteLine(s);
        }

        internal static void print(string s)
        {
            Console.Out.Write(s);
        }

        internal static void shutdownIfNeeded()
        {
        }

        internal static void updateProgress(long value, int maxValue)
        {
            if (maxValue <= 0)
            {
                ProgressChangedEvent?.Invoke(value % 100, 100);
            }
            else
            {
                ProgressChangedEvent?.Invoke(value, maxValue);
            }
        }

        internal static void openAndReadInputValues(string attackName, string attackVersion)
        {
        }

        internal static void displayBestList(string keyString)
        {
            Console.Out.WriteLine("Best List:\n"+keyString);
            BestListChangedEvent?.Invoke(keyString);
        }

        internal static void displayBestResult(CtBestList.Result bestResult)
        {
            BestResultChangedHandlerEvent?.Invoke(bestResult);
            Console.Out.WriteLine($"Score: {bestResult.score}");
            Console.Out.WriteLine($"Key: {bestResult.keyString}");
            Console.Out.WriteLine($"Plaintext: {plaintextCapped(bestResult.plaintextString)}");
        }

        internal static void displayBestResult(CtBestList.Result bestResult, CtBestList.Result originalResult)
        {
            displayBestResult(bestResult);
        }

        private static string plaintextCapped(string plaintext)
        {
            if (plaintext.Length <= 1000)
            {
                return plaintext;
            }
            return plaintext.Substring(0, Math.Min(100, plaintext.Length)) + "...";
        }
    }
}

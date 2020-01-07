using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlayfairAnalysis.Common
{

    public class NGrams
    {
        private static Dictionary<long, long> map7 = new Dictionary<long, long>();
        private static Dictionary<long, long> map8 = new Dictionary<long, long>();
        private static long MASK7 = (long)Math.Pow(26, 6);

        private static bool[] FILTER = new bool[(int)Math.Pow(26, 6)];
    private static long MASK8 = (long)Math.Pow(26, 7);



        public static long eval7(int[] text, int len)
        {
            Stats.evaluations++;
            long idx = 0;
            long score = 0;
            for (int i = 0; i < len; i++)
            {
                idx = (idx % MASK7) * 26 + text[i];
                if (i < 7 - 1)
                {
                    continue;
                }
                if (!FILTER[(int)(idx / 26)])
                {
                    continue;
                }
                long v = map7[idx];
                if (v == null)
                {
                    continue;
                }
                score += 400_000 * v;
            }

            return score / (len - 7 + 1);
        }

        public static long eval8(int[] text, int len)
        {
            Stats.evaluations++;
            long idx = 0;
            long score = 0;
            for (int i = 0; i < len; i++)
            {
                idx = (idx % MASK8) * 26 + text[i];
                if (i < 8 - 1)
                {
                    continue;
                }
                if (!FILTER[(int)(idx / (26 * 26))])
                {
                    continue;
                }
                long v = map8[idx];
                if (v == null)
                {
                    continue;
                }
                score += 400_000 * v;
            }
            return score / (len - 8 + 1);
        }

        /*
        public static bool load(String statsFilename, int ngrams)
        {
            try
            {
                FileInputStream _is = new FileInputStream(new File(statsFilename));
                var map = ngrams == 8 ? map8 : map7;
                map.Clear();


                ObjectInputStream inputStream = new ObjectInputStream(_is);
                long[] data = (long[])inputStream.readObject();
                Console.Out.WriteLine("Read %,d items from %s\n", data.Length / 2, statsFilename);
                long _using = Math.Min(data.Length / 2, 1_000_000);
                for (int i = 0; i < _using; i++) {
                    long index = data[2 * i];
                    long value = data[2 * i + 1] + 1;
                    map.Add(index, (long)(Math.Log(value) / Math.Log(2)));
                    if (ngrams == 7)
                    {
                        FILTER[(int)(index / 26)] = true;
                    }
                    else
                    {
                        FILTER[(int)(index / (26 * 26))] = true;
                    }
                }
                Console.Out.WriteLine("Using %,d items from %s\n", _using, statsFilename);

            _is.close();


                return true;
            }
            catch (Exception ex) {
                Console.Out.WriteLine(ex.StackTrace);
            }
            return false;
            }


        */
    }
}

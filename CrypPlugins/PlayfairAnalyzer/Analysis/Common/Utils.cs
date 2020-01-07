using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlayfairAnalysis.Common
{
    public enum Language { ENGLISH, FRENCH, GERMAN, ITALIAN }

    public class Utils
    {

        public static String HEXA_FILE = "hexa.bin";
        public static String NGRAMS7_FILE = "english_7grams.bin";
        public static String NGRAMS8_FILE = "english_8grams.bin";
        public static String BOOK_FILE = "book.txt";

        public static int A = getTextSymbol('A');
        public static int X = getTextSymbol('X');
        public static int Z = getTextSymbol('Z');
        public static int J = getTextSymbol('J');
        public static int I = getTextSymbol('I');
        public static int K = getTextSymbol('K');

        public static String TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.Length;

        private static int[][] perms6 = createPerms6();

        public static int getTextSymbol(char c)
        {

            if (c >= 'a' && c <= 'z')
            {
                return c - 'a';
            }
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            return -1;
        }

        public static char getTextChar(int symbol)
        {

            if ((symbol >= 0) && (symbol <= (TEXT_ALPHABET_SIZE - 1)))
                return (TEXT_ALPHABET[symbol]);
            else
                return '?';

        }

        public static int[] getText(String textString)
        {
            int[] text = new int[textString.Length];
            int len = 0;
            for (int i = 0; i < textString.Length; i++)
            {
                int c = getTextSymbol(textString[i]);
                if (c == -1)
                {
                    //continue;
                }
                text[len++] = c;
            }
            return Arrays.copyOf(text, len);
        }

        private static String from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
        private static String to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();

        public static String readTextFile(String fileName)
        {

            int[] text = new int[1000000];
            String line = "";
            int len = 0;

            try
            {
                using (var bufferedReader = new StreamReader(fileName))
                {
                    while ((line = bufferedReader.ReadLine()) != null)
                    {
                        foreach (char c_it in line)
                        {
                            var c = c_it;
                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            int index = getTextSymbol(c);
                            if (index != -1)
                            {
                                text[len] = index;
                                len++;
                            }
                        }
                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read text file {0} - {0}", fileName, ex.ToString());
            }

            String cipherStr = getString(Arrays.copyOf(text, len));

            Console.Out.WriteLine("Text file read: {0}, length = {1} \n{2}\n", fileName, len, cipherStr);
            return cipherStr;
        }

        public static int readTextSegmentFromFile(String filename, int startPosition, int[] text)
        {

            int length = 0;

            try
            {
                using (var bufferedReader = new StreamReader(filename))
                {
                    int position = 0;
                    String line = "";

                    while (((line = bufferedReader.ReadLine()) != null) && (length < text.Length))
                    {
                        if (position > startPosition)
                        {
                            for (int i = 0; i < line.Length; i++)
                            {
                                char c = line[i];
                                int rep = from.IndexOf(c);
                                if (rep != -1)
                                {
                                    c = to[rep];
                                }
                                int index = getTextSymbol(c);
                                if ((index != -1) && (length < text.Length))
                                {
                                    text[length] = index;
                                    length++;
                                }
                            }
                        }
                        position += line.Length;
                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read text file {0} - {1}", filename, ex.ToString());
            }
            Console.Out.WriteLine("Read segment from file: {0}, Position: {1} , Length: {2}\n", filename, startPosition, length);
            Console.Out.WriteLine("{0}\n\n", getString(text));

            return length;
        }

        public static int readRandomSentenceFromFile(String filename)
        {

            var lists = new List<HashSet<string>>();
            for (int l = 0; l < 10000; l++)
            {
                lists.Add(new HashSet<string>());
            }
            try
            {
                using (var bufferedReader = new StreamReader(filename))
                {
                    StringBuilder s = new StringBuilder();
                    String line = "";

                    while ((line = bufferedReader.ReadLine()) != null)
                    {
                        line += ' ';
                        for (int i = 0; i < line.Length; i++)
                        {
                            char c = line[i];
                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            if (c == ' ')
                            {
                                if (s.Length > 0 && s[s.Length - 1] != ' ')
                                {
                                    s.Append(c);
                                }
                            }
                            else if (c == '.' || c == ';' || c == ':' || c == '\"')
                            {
                                if (s.Length >= 6 && s.Length <= 50 && s[0] >= 'A' && s[0] <= 'Z')
                                {
                                    String clean = s.ToString().Replace(" ", "").ToUpper();
                                    lists[clean.Length].Add(clean);
                                }
                                s.Clear();
                            }
                            else if (c >= 'a' && c <= 'z')
                            {
                                s.Append(c);
                            }
                            else if (c >= 'A' && c <= 'Z')
                            {
                                s.Append(c);
                            }
                        }

                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read text file {0} - {1}", filename, ex.ToString());
            }
            for (int l = 0; l < lists.Count; l++)
            {
                if (lists[l].Count > 10)
                {
                    Console.Out.WriteLine("{0,5} {1,5}\n", l, lists[l].Count);
                }
                foreach (String s in lists[l])
                {
                    int[] t = Utils.getText(s);
                    if (t.Length >= 6)
                    {
                        long score = Stats.evalPlaintextHexagram(t);
                        Console.Out.WriteLine("{0,5} {1} {2}\n", l, s, score);
                    }

                }

            }
            return 0;
        }

        public static int[] readRandomSentenceFromFile(String filename, String prefix, int length, bool playfair)
        {


            var list = new List<string>();

            try
            {
                using (var bufferedReader = new StreamReader(filename))
                {
                    StringBuilder s = new StringBuilder();
                    String line = "";

                    while ((line = bufferedReader.ReadLine()) != null)
                    {
                        line += ' ';
                        for (int i = 0; i < line.Length; i++)
                        {
                            char c = line[i];
                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            if (c == ' ')
                            {
                                if (s.Length > 0 && s[s.Length - 1] != ' ')
                                {
                                    s.Append(c);
                                }
                            }
                            else if (c == '.' || c == ';' || c == ':' || c == '\"')
                            {
                                if (s.Length >= 6 && s.Length <= 1000 && s[0] >= 'A' && s[0] <= 'Z')
                                {
                                    String clean = prefix + s.ToString().Replace(" ", "").ToUpper();

                                    if (clean.Length == length && (!playfair || !clean.Contains("J")))
                                    {
                                        int[] t = Utils.getText(clean);
                                        long score = Stats.evalPlaintextHexagram(t);
                                        if (score > 2_200_000)
                                        {
                                            list.Add(clean);
                                        }
                                    }
                                }
                                s.Clear();
                            }
                            else if (c >= 'a' && c <= 'z')
                            {
                                s.Append(c);
                            }
                            else if (c >= 'A' && c <= 'Z')
                            {
                                s.Append(c);
                            }
                        }

                    }

                    bufferedReader.Close();
                }
            }
            catch (IOException ex)
            {
                CtAPI.goodbyeFatalError("Unable to read file {0} - {1}", filename, ex.ToString());
            }
            if (list.Count == 0)
            {
                CtAPI.goodbyeFatalError("Unable to read sentence from text file {0} with {1} letters", filename, length);
            }
            return Utils.getText(list[new Random().Next(list.Count)]);
        }

        public static String getString(int[] text)
        {
            return getString(text, text.Length);
        }

        public static String getString(int[] text, int length)
        {
            StringBuilder m = new StringBuilder();
            for (int i = 0; i < Math.Min(text.Length, length); i++)
            {
                m.Append(getTextChar(text[i]));
            }
            return m.ToString();
        }

        private static long startTime;

        public static void resetTimer()
        {
            startTime = DateTime.Now.Ticks;
        }

        public static TimeSpan getElapsed()
        {
            return new TimeSpan(DateTime.Now.Ticks - startTime + 1);
        }

        public static Random random = new Random();
        public static int randomNextInt(int range)
        {
            return random.Next(range);
        }
        public static int randomNextInt()
        {
            return random.Next();
        }
        public static double randomNextDouble()
        {
            return random.NextDouble();
        }

        public static int sum(int[] a)
        {
            int sum = 0;
            foreach (int i in a)
            {
                sum += i;
            }
            return sum;
        }

        public static long sum(long[] a)
        {
            long sum = 0;
            foreach (long i in a)
            {
                sum += i;
            }
            return sum;
        }

        public static bool In(int x, params int[] a) {
        foreach (int i in a) {
            if (i == x) {
                return true;
            }
        }
        return false;
    }

public static int[] randomPerm6()
{
    return perms6[random.Next(perms6.Length)];
}

private static int[][] createPerms6()
{
    int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][];
    int index = 0;
    for (int i0 = 0; i0 < 6; i0++)
    {
        for (int i1 = 0; i1 < 6; i1++)
        {
            if (i1 == i0)
            {
                continue;
            }
            for (int i2 = 0; i2 < 6; i2++)
            {
                if (i2 == i0 || i2 == i1)
                {
                    continue;
                }
                for (int i3 = 0; i3 < 6; i3++)
                {
                    if (i3 == i0 || i3 == i1 || i3 == i2)
                    {
                        continue;
                    }
                    for (int i4 = 0; i4 < 6; i4++)
                    {
                        if (i4 == i0 || i4 == i1 || i4 == i2 || i4 == i3)
                        {
                            continue;
                        }
                        for (int i5 = 0; i5 < 6; i5++)
                        {
                            if (i5 == i0 || i5 == i1 || i5 == i2 || i5 == i3 || i5 == i4)
                            {
                                continue;
                            }

                                    perms6[index] = new int[6];
                            perms6[index][0] = i0;
                            perms6[index][1] = i1;
                            perms6[index][2] = i2;
                            perms6[index][3] = i3;
                            perms6[index][4] = i4;
                            perms6[index][5] = i5;
                            index++;

                        }
                    }
                }
            }
        }
    }
    return perms6;
}

public static String readPlaintextSegmentFromFile(String dirname, Language language, int from, int requiredLength, bool m209)
{
    String filename = null;
    switch (language)
    {
        case Language.ENGLISH:
            filename = "book.txt";
            break;
        case Language.FRENCH:
            filename = "frenchbook.txt";
            break;
        case Language.ITALIAN:
            filename = "italianbook.txt";
            break;
        case Language.GERMAN:
            filename = "germanbook.txt";
            break;
    }
    return readPlaintextSegmentFromFile(dirname + "/" + filename, from, requiredLength, m209);
}
// read a plain text segment from a file at a given position and length
private static String readPlaintextSegmentFromFile(String fileName, int startPosition, int requiredLength, bool m209)
{

    StringBuilder text = new StringBuilder();
    String line;
    int position = 0;
    int fileLength = (int) File.OpenRead(fileName).Length;
    if (fileLength == 0)
    {
        CtAPI.goodbyeFatalError("Cannot open file " + fileName);
    }
    if (startPosition < 0)
    {
        startPosition = randomNextInt(80 * fileLength / 100);
    }

    try
    {
                // FileReader reads text files in the default encoding.
                using (var bufferedReader = new StreamReader(fileName))
                {

                    while (((line = bufferedReader.ReadLine()) != null) && (text.Length < requiredLength))
                    {
                        line += " ";
                        if (position > startPosition)
                        {
                            //System.out.println(line);
                            line = line.ToUpper();
                            for (int i = 0; (i < line.Length) && (text.Length < requiredLength); i++)
                            {
                                char c = line[i];
                                int rep = from.IndexOf(c);
                                if (rep != -1)
                                {
                                    c = to[rep];
                                }
                                if (getTextSymbol(c) == -1)
                                {
                                    if (m209)
                                    {
                                        if (text.Length > 0 && text[text.Length - 1] == 'Z')
                                        {
                                            continue;
                                        }
                                        c = 'Z';
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                text.Append(c);
                            }
                        }
                        position += line.Length;
                    }

                    // Always close files.
                    bufferedReader.Close();
                }
    }
    catch (IOException ex)
    {
        CtAPI.goodbyeFatalError("Unable to read book file {0} - {1}", fileName, ex.ToString());
    }

    Console.Out.WriteLine("Generated Random Plaintext - Book: {0}, Position: {1} , Length: {2}\n", fileName, startPosition, text.Length);
    Console.Out.WriteLine("{0}\n\n", text.ToString().Replace("Z", " "));


    return text.ToString();
}
}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace common
{
    public abstract class Vector
    {

        public int alphabetSize;
        public int[] TextInInt;
        public int length;
        public int cursor;

        public bool withStats;
        public bool acceptErrors = false;

        private StringBuilder alphabet;

        //calculation of IoC with monograms
        public long[] counts1;
        public double[] freqs1;
        public double IoC1 = 0.0;
        //calculation of IoC with bigrams
        private long[,] counts2;
        private double[,] freqs2;
        public double IoC2 = 0.0;



        public static Random r = new Random();

        public override String ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                s.Append("").Append(Chr(TextInInt[i]));
            }
            return s.ToString();
        }

        public Vector(StringBuilder alphabet, String s, bool withStats)

           : this(alphabet, s.Length, withStats)
        {
            for (int i = 0; i < length; i++)
            {
                TextInInt[i] = Index(s[i]);
                if (TextInInt[i] == -1)
                {
                    Console.WriteLine("Bad %d = %s\n", i, s[i]);
                }
            }
            stats();
        }

        public Vector(StringBuilder alphabet, int length, bool withStats)
        {
            this.alphabet = alphabet;
            this.length = length;
            this.withStats = withStats;
            this.alphabetSize = alphabet.Length;
            this.TextInInt = new int[length];
            if (this.withStats)
            {
                counts1 = new long[alphabetSize];
                freqs1 = new double[alphabetSize];
                counts2 = new long[alphabetSize, alphabetSize];
                freqs2 = new double[alphabetSize, alphabetSize];
            }
            if (length == alphabetSize)
            {
                Identity();
            }
        }

        public int Index(char c)
        {
            return alphabet.ToString().IndexOf(c);
        }

        public char Chr(int index)
        {
            return (index >= 0 && index < alphabetSize) ? alphabet[index] : '!';
        }

        public void Swap(int i, int j)
        {
            if (i == j)
            {
                return;
            }
            int keep = TextInInt[i];
            TextInInt[i] = TextInInt[j];
            TextInInt[j] = keep;
        }

        public void Identity()
        {
            for (int i = 0; i < length; i++)
            {
                TextInInt[i] = i;
            }
        }

        public Vector FromTranspositionKey(String keyS)
        {

            if (keyS.Length > TextInInt.Length)
            {
                throw new System.Exception("Cannot create transposition key");
            }

            for (int i = 0; i < TextInInt.Length; i++)
            {
                TextInInt[i] = -1;
            }
            //Arrays.fill(v, -1);

            length = keyS.Length;

            for (int i = 0; i < length; i++)
            {
                int minJ = -1;
                for (int j = 0; j < length; j++)
                {
                    if (TextInInt[j] != -1)
                    {
                        continue;
                    }
                    if ((minJ == -1) || (keyS[j] < keyS[minJ]))
                    {
                        minJ = j;
                    }
                }
                TextInInt[minJ] = i;

            }
            return this;
        }

        public Vector randomPermutation()
        {
            Identity();
            for (int i = 0; i < length - 1; i++)
            {
                int j = i + r.Next(length - i);
                Swap(i, j);
            }
            return this;
        }

        public void append(Vector v)
        {
            Array.Copy(v.TextInInt, 0, this.TextInInt, length, v.length);
            length += v.length;
        }

        public void append(params Vector[] TextInInts)
        {
            foreach (Vector TextInInt in TextInInts)
            {
                append(TextInInt);
            }
        }

        public Vector copy(Vector v)
        {
            length = 0;
            append(v);
            return this;
        }

        public Vector copy(Vector v, int from, int length)
        {
            Array.Copy(v.TextInInt, from, this.TextInInt, 0, length);
            this.length = length;
            return this;
        }

        public void copy(params Vector[] TextInInts)
        {
            length = 0;
            append(TextInInts);
        }

        public bool copy(String s)
        {
            length = 0;
            return append(s);
        }

        public bool copy(params String[] strings)
        {
            length = 0;
            return append(strings);
        }

        public bool append(String s)
        {
            return append(s.ToCharArray());
        }

        public bool append(params String[] strings)
        {
            foreach (String s in strings)
            {
                if (!append(s))
                {
                    return false;
                }
            }
            return true;
        }

        public bool copy(char c)
        {
            length = 0;
            return append(c);
        }

        public bool copy(params char[] chars)
        {
            length = 0;
            return append(chars);
        }

        public bool append(char c)
        {
            int i = Index(c);
            if (i == -1 && !acceptErrors)
            {
                return false;
            }
            TextInInt[length++] = i;
            return true;
        }

        public bool append(params char[] chars)
        {
            foreach (char c in chars)
            {
                if (!append(c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool copy(int i)
        {
            length = 0;
            return append(i);
        }

        public bool copy(params int[] ints)
        {
            length = 0;
            return append(ints);
        }

        public bool append(int i)
        {
            TextInInt[length++] = i;
            return true;
        }

        public bool append(params int[] ints)
        {
            foreach (int i in ints)
            {
                if (!append(i))
                {
                    return false;
                }
            }
            return true;
        }

        public bool removeIndex(int index)
        {
            if (index >= length)
            {
                return false;
            }
            else if (index == length - 1)
            {
                length--;
                return true;
            }

            Array.Copy(TextInInt, index + 1, TextInInt, index, length - index - 1);
            length--;
            return true;
        }

        public bool removeElement(int element)
        {
            for (int i = 0; i < length; i++)
            {
                if (TextInInt[i] == element)
                {
                    return removeIndex(i);
                }
            }
            return false;
        }

        public bool removeElement(char element)
        {
            return removeElement(Index(element));
        }

        public bool valid()
        {
            for (int i = 0; i < length; i++)
            {
                if (TextInInt[i] == -1)
                {
                    return false;
                }
            }
            return true;
        }

        public long hash()
        {
            long index = 0;
            for (int i = 0; i < length; i++)
            {
                index = index * alphabetSize + TextInInt[i];
            }
            return index;
        }

        public int hashShift5()
        {
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                index = (index << 5) + TextInInt[i];
            }
            return index;
        }

        public void inverseOf(Vector v)
        {
            if (v.length != alphabetSize)
            {
                //throw new RuntimeException("Length is not standard");
            }
            if (v.length != length)
            {
                throw new System.Exception("Length is not equal");
            }

            for (int i = 0; i < this.TextInInt.Length; i++)
            {
                this.TextInInt[i] = -1;
            }


            for (int i = 0; i < length; i++)
            {
                if (v.TextInInt[i] != -1)
                {
                    this.TextInInt[v.TextInInt[i]] = i;
                }
            }

        }

        public void stats()
        {
            stats(false);
        }

        public void stats(bool bigramEvenOnly)
        {
            if (!withStats)
            {
                return;
            }
            for (int i = 0; i < alphabetSize; i++)
            {
                freqs1[i] = 0.0;
                counts1[i] = 0;
            }

            IoC1 = 0.0;
            IoC2 = 0.0;

            for (int i = 0; i < alphabetSize; i++)
            {
                for (int j = 0; j < alphabetSize; j++)
                {
                    counts2[i, j] = 0;
                }
            }


            for (int i = 0; i < alphabetSize; i++)
            {
                for (int j = 0; j < alphabetSize; j++)
                {
                    freqs2[i, j] = 0.0;
                }
            }


            int lastSymbol = -1;
            for (int i = 0; i < length; i++)
            {
                int symbol = TextInInt[i];
                if (symbol != -1)
                {
                    counts1[symbol]++;
                    if (lastSymbol != -1)
                    {
                        if (bigramEvenOnly)
                        {
                            if (i % 2 == 1)
                            {
                                counts2[lastSymbol, symbol]++;
                                counts2[lastSymbol, symbol]++;
                            }
                        }
                        else
                        {
                            counts2[lastSymbol, symbol]++;
                        }
                    }
                }
                lastSymbol = symbol;
            }

            double freq;
            for (int f1 = 0; f1 < alphabetSize; f1++)
            {
                //freq = freqs1[f1] = 1.0 * counts1[f1] / length;
                //IoC1 += freq * freq;
                freq = freqs1[f1] = counts1[f1] * (counts1[f1] - 1);
                IoC1 += freq;

                //-------------------------------------------------------------------------
                double[] freqs2F1 = new double[freqs2.GetLength(1)];

                for (int i = 0; i < freqs2.GetLength(1); i++)
                {
                    freqs2F1[i] = freqs2[f1, i];
                }

                //-------------------------------------------------------------------------
                long[] counts2F1 = new long[counts2.GetLength(1)];

                for (int i = 0; i < counts2.GetLength(1); i++)
                {
                    counts2F1[i] = counts2[f1, i];
                }

                //-------------------------------------------------------------------------

                for (int f2 = 0; f2 < alphabetSize; f2++)
                {
                    //freq = freqs2F1[f2] = 1.0 * counts2F1[f2] / length;
                    freq = freqs2F1[f2] = counts2F1[f2] * (counts2F1[f2] - 1);
                    if (freq == 0)
                    {
                        continue;
                    }
                    //IoC2 += freq * freq;
                    IoC2 += freq;

                }
            }

            IoC1 = IoC1 / (length * (length - 1));
            IoC2 = IoC2 / (length * (length - 1));
            double dsize = 100;
            IoC1 *= dsize;
            IoC2 *= dsize;
        }

    }

}

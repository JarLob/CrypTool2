﻿/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// This class represents a text which constists of symbols while each symbol
    ///  consist of diffrent amounts of letters
    /// </summary>
    public class Text : ICloneable
    {
        private List<int[]> symbols = new List<int[]>();
        private int letterIterator = 0;
        private int symbolIterator = 0;
        
        /// <summary>
        /// Creates a new empty text
        /// </summary>
        public Text()
        {

        }

        /// <summary>
        /// Creates a text containing the letters defined by the given int array
        /// </summary>
        /// <param name="letters"></param>
        public Text(int[] letters)
        {
            foreach(var letter in letters)
            {
                symbols.Add(new int[] { letter });
            }
        }

        /// <summary>
        /// Get symbol at defined index OR
        /// Set symbol at defined inex
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int[] this[int index]
        {
            get
            {
                return symbols[index];
            }
            set
            {
                if(index < symbols.Count)
                {
                    symbols[index] = value;
                }
                else
                {
                    while(symbols.Count < index)
                    {
                        symbols.Add(new int[0]);
                    }
                    symbols.Add(value);
                }
            }
        }

        /// <summary>
        /// Removes symbol at defined index
        /// </summary>
        /// <param name="index"></param>
        public void RemoteSymbolAt(int index)
        {
            symbols.RemoveAt(index);
        }

        /// <summary>
        /// Resets the letter iterator
        /// </summary>
        public void ResetIterator()
        {
            letterIterator = 0;
            symbolIterator = 0;
        }

        /// <summary>
        /// Returns the next letter of the text
        /// </summary>
        /// <returns></returns>
        public int GetNextSingleLetter()
        {
            if(symbolIterator > symbols.Count)
            {
                return -1;
            }
            int letter = symbols[symbolIterator][letterIterator];
            letterIterator++;
            if(letterIterator > symbols[symbolIterator].Length)
            {
                letterIterator = 0;
                symbolIterator++;
            }
            return letter;
        }

        /// <summary>
        /// Returns the number of symbols
        /// </summary>
        /// <returns></returns>
        public int GetSymbolsCount()
        {
            return symbols.Count;
        }

        /// <summary>
        /// Returns the number of letters
        /// </summary>
        /// <returns></returns>
        public int GetLettersCount()
        {
            int length = 0;
            foreach (var symbol in symbols)
            {
                length += symbol.Length;
            }
            return length;
        }

        /// <summary>
        /// Clears this text
        /// </summary>
        public void Clear()
        {
            symbols.Clear();
            ResetIterator();
        }

        /// <summary>
        /// Exchanges two symbols of the text
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public void SwapSymbols(int i, int j)
        {
            int[] a = symbols[i];
            int[] b = symbols[j];
            symbols[i] = b;
            symbols[j] = a;
        }

        /// <summary>
        /// Clones this text
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Text text = new Text();
            text.symbols = new List<int[]>();
            foreach(var symbol in symbols)
            {
                text.symbols.Add((int[])symbol.Clone());
            }
            return text;
        }

        /// <summary>
        /// Converts the text to an array of integers
        /// </summary>
        /// <returns></returns>
        public int[] ToIntegerArray()
        {
            List<int> ints = new List<int>();
            foreach(var symbol in symbols)
            {
                foreach(var i in symbol)
                {
                    ints.Add(i);
                }
            }
            return ints.ToArray();
        }
    }
}

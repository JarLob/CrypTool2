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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class WordFinder
    {
        private int _minLength;
        private int _maxLength;
        private LetterNode _rootLetterNode = new LetterNode();
        private string _alphabet;

        /// <summary>
        /// Constructor
        /// Generates a "word tree" for fast search of words
        /// </summary>
        /// <param name="words"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <param name="alphabet"></param>
        public WordFinder(string[] words, int minLength, int maxLength, string alphabet)
        {
            _minLength = minLength;
            _maxLength = maxLength;
            _alphabet = alphabet;
                      
            foreach (string word in words)
            {
                if (word.Length < _minLength || word.Length > _maxLength)
                {
                    continue;
                }
                LetterNode node = _rootLetterNode;
                int[] letters = Tools.MapIntoNumberSpace(word.ToUpper(), alphabet);
                for (int i = 0; i < letters.Length; i++)
                {
                    bool found = false;
                    foreach (var childLetterNode in node.nodes)
                    {
                        if (childLetterNode.Letter == letters[i])
                        {
                            node = childLetterNode;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        LetterNode newNode = new LetterNode();
                        newNode.Letter = letters[i];
                        node.nodes.Add(newNode);
                        node = newNode;
                    }
                }
            }
        }      
        
        /// <summary>
        /// Checks, if a given word is in the dictionary
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool IsInDictionary(int[] word)
        {
            var node = _rootLetterNode;
            
            foreach (int letter in word)
            {
                bool found = false;
                foreach (var childLetterNode in node.nodes)
                {
                    if (childLetterNode.Letter == letter)
                    {
                        node = childLetterNode;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds all words with length between min and max lengths
        /// Returns the positions and lengths in a dictionary
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public Dictionary<int,int> FindWords(int[] plaintext)
        {
            Dictionary<int,int> _wordPositions = new Dictionary<int, int>();
            for (int i = 0; i < plaintext.Length - _maxLength; i++)
            {
                for (int length = _maxLength; length >= _minLength; length--)
                {
                    if (i > plaintext.Length - _maxLength)
                    {
                        break;
                    }
                    int[] word = GetWord(plaintext, i, length);
                    if (IsInDictionary(word))
                    {
                        string strWord = Tools.MapNumbersIntoTextSpace(word, _alphabet);
                        //Console.WriteLine(String.Format("Word found: {0}", strWord));
                        _wordPositions.Add(i, length);
                        i += word.Length;
                        
                        break;
                    }                    
                }
            }

            return _wordPositions;
        }

        /// <summary>
        /// Returns a word out of the given text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private int[] GetWord(int[] text, int index, int length)
        {
            int[] result = new int[length];
            Array.Copy(text, index, result, 0, length);
            return result;
        }
    }

    /// <summary>
    /// A node used for the "word tree"
    /// </summary>
    public class LetterNode
    {
        public int Letter { get; set; }
        public List<LetterNode> nodes = new List<LetterNode>();
    }

}

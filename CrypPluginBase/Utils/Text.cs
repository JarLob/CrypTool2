﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.PluginBase.Utils
{
    public class Text
    {
        #region Private Variables

        private string text;
        private int[] numbers;
        private bool[] isUpper;
        private Alphabet alphabet;

        public enum InvalidCharacterAction { Preserve, Delete, Replace };

        #endregion

        #region Constructor

        public Text(string text, Alphabet alphabet)
        {
            this.text = text;
            this.alphabet = alphabet;
            numbers = alphabet.StringToNumbers(text, true);
            isUpper = text.Select(c => c.ToString() == c.ToString().ToUpper()).ToArray();
        }

        #endregion

        #region Properties

        public int Length
        {
            get { return text.Length; }
        }

        public int[] ValidLetterArray 
        {
            get { return numbers.Where(c => c >= 0).ToArray(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert text to string
        /// </summary>
        public override string ToString()
        {
            return ToString(alphabet, true);
        }

        public string ToString(Alphabet alphabet, bool preserveCase, InvalidCharacterAction invalidCharacterAction = InvalidCharacterAction.Delete, char replaceChar = '?')
        {
            StringBuilder sb = new StringBuilder();

            for (int i=0;i<numbers.Length;i++)
            {
                if (numbers[i] >= 0)
                {
                    sb.Append(alphabet[numbers[i]]);
                }
                else
                {
                    switch (invalidCharacterAction)
                    {
                        case InvalidCharacterAction.Preserve: sb.Append(text[i]); break;
                        case InvalidCharacterAction.Replace: sb.Append(replaceChar); break;
                        case InvalidCharacterAction.Delete: break;
                    }
                }
            }

            return sb.ToString();
        }
        
        #endregion
    }
}
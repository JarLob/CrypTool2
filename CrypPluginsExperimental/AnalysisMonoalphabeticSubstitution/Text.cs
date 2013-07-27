﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    class Text
    {
        #region Private Variables

        private List<int> text = new List<int>();
        private List<string> notInAlphabet = new List<string>();
        private List<bool> orgCapital = new List<bool>();
        private Boolean caseSensitive = false;

        #endregion

        #region Constructor

        public Text(string text, Alphabet alpha, bool caseSense)
        {
            this.caseSensitive = caseSense;
            string curString = "";
            string c = "";

            if (this.caseSensitive == false)
            {
                for (int i = 0; i < text.Length; i++)
                {                 
                    bool status = false;

                    int j = 0;
                    do
                    {
                        j++;
                        curString = text.Substring(i, j);

                        c = curString;
                        if (char.IsUpper(c.ToCharArray()[0]))
                        {
                            status = true;
                            c = c.ToLower();
                         }
                    }
                    while (alpha.GetNumberOfLettersStartingWith(c) > 1);

                    if (alpha.GetNumberOfLettersStartingWith(c) == 1)
                    {
                        this.text.Add(alpha.GetPositionOfLetter(c));
                        this.orgCapital.Add(status);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        this.notInAlphabet.Add(curString);
                        this.text.Add(-this.notInAlphabet.Count);
                        this.orgCapital.Add(false);
                    }
                    i += j - 1;
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    int j = 0;
                    do
                    {
                        j++;
                        curString = text.Substring(i, j);
                        c = curString;
                    }
                    while (alpha.GetNumberOfLettersStartingWith(c) > 1);

                    if (alpha.GetNumberOfLettersStartingWith(c) == 1)
                    {
                        this.text.Add(alpha.GetPositionOfLetter(c));
                        this.orgCapital.Add(false);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        this.notInAlphabet.Add(curString);
                        this.text.Add(-this.notInAlphabet.Count);
                        this.orgCapital.Add(false);
                    }
                    i += j - 1;
                }
            }
        }

        public Text()
        {

        }

        #endregion

        #region Properties

        public int Length
        {
            get { return this.text.Count; }
            private set { ; }
        }

        public int[] ValidLetterArray 
        {
            get { return GetValidLetterArray(); }
            set { ; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Convert text to string
        /// </summary>
        public string ToString(Alphabet alpha)
        {
            string res = "";

            for (int i=0;i<this.text.Count;i++)
            {
                int letter = this.text[i];

                if (letter >= 0)
                {
                    if (this.caseSensitive == false && this.orgCapital[i] == true)
                    {
                        res += alpha.GetLetterFromPosition(letter).ToUpper();
                    }
                    else
                    {
                        res += alpha.GetLetterFromPosition(letter);
                    }
                }
                else
                {
                    res += this.notInAlphabet[(-1)*letter-1];
                }
            }

            return res;
        }

        /// <summary>
        /// Get letter at position
        /// </summary>
        public int GetLetterAt(int position)
        {
            if ((position >= 0) && (position < this.text.Count))
            {
                return this.text[position];
            }
            else 
            {
                return -1;
            }
        }

        /// <summary>
        /// Get letter that is not in alphabet at position
        /// </summary>
        public string GetLetterNotInAlphabetAt(int position)
        {
            if (position >= 0)
            {
                return "";  
            }
            else if (-position > this.notInAlphabet.Count)
            {
                return "";
            }
            else
            {
                return this.notInAlphabet[-position-1];
            }
        }
        
        /// <summary>
        /// Add letter to the end of the text
        /// </summary>
        public void AddLetter(string letter, Alphabet alpha)
        {
            int position;

            if (this.caseSensitive == false)
            {
                if (char.IsUpper(letter.ToCharArray()[0]) == true)
                {
                    this.orgCapital.Add(true);
                }
                else
                {
                    this.orgCapital.Add(false);
                }
                position = alpha.GetPositionOfLetter(letter.ToLower());
            }
            else
            {
                position = alpha.GetPositionOfLetter(letter);
                this.orgCapital.Add(false);
            }
 
            if (position >= 0)
            {
                this.text.Add(position);
            }
            else
            {
                this.notInAlphabet.Add(letter);
                this.text.Add(-this.notInAlphabet.Count);
            }
        }

        private void AddLetter(int letter)
        {
            this.text.Add(letter);
        }

        /// <summary>
        /// Change letter at position
        /// </summary>
        public bool ChangeLetterAt(int position, int letter)
        {
            if ((position >= 0) && (position < this.text.Count) && (this.text[position]!=-1))
            {
                this.text[position] = letter;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Copy text
        /// </summary>
        public Text CopyTo()
        {
            Text res = new Text();
            for (int i = 0; i < this.text.Count; i++)
            {
                res.AddLetter(this.text[i]);
            }
            for (int i = 0; i < this.notInAlphabet.Count; i++)
            {
                res.AddLetterNotInAlphabet(this.notInAlphabet[i]);
            }
            for (int i = 0; i < this.orgCapital.Count; i++)
            {
                res.AddOrgCapital(this.orgCapital[i]);
            }

            return res;
        }

        private void AddLetterNotInAlphabet(string letter)
        {
            this.notInAlphabet.Add(letter);
        }

        private void AddOrgCapital(bool val)
        {
            this.orgCapital.Add(val);
        }

        #endregion

        #region Helper Methods

        private int[] GetValidLetterArray()
        {
            List<int> l = new List<int>();
            for (int i = 0; i < this.text.Count; i++)
            {
                if (this.text[i] >= 0)
                {
                    l.Add(this.text[i]);
                }
            }
            return l.ToArray();
        }

        #endregion
    }
}

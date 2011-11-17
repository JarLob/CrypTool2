/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Validation;
using System.IO;
using System.Collections;
using System.ComponentModel;

namespace Cryptool.ADFGVX
{
    public class ADFGVXSettings : ISettings
    {
        #region Public ADFGVX specific interface

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private const string CIPHER_ALPHABET5 = "ADFGX";
        private const string CIPHER_ALPHABET6 = "ADFGVX";

        public string Alphabet
        {
            get; set;
        }

        public string CipherAlphabet
        {
            get; set;
        }

        public ADFGVXSettings()
        {
            updateAlphabet();
        }

        public enum ActionEnum
        {
            Encrypt, Decrypt
        }

        public enum CipherTypeEnum
        {
            ADFGX, ADFGVX
        }

        #endregion

        #region private methods

        private Random random = new Random();

        // with given alphabet
        private string createRandomPassword()
        {
            StringBuilder newPassword = new StringBuilder();
            StringBuilder remainingChars = new StringBuilder(Alphabet);

            while(remainingChars.Length > 0)
            {
                int pos = random.Next(remainingChars.Length);
                newPassword.Append(remainingChars[pos]);
                remainingChars.Remove(pos, 1);
            }

            return newPassword.ToString();
        }

        private void rebuildSubstitutionMatrix()
        {
            string value = SubstitutionPass.ToUpperInvariant();

            StringBuilder sb = new StringBuilder();
            HashSet<char> seen = new HashSet<char>();

            foreach(char c in value)
            {
                // add character to matrix if unique and part of alphabet
                if (!seen.Contains(c) && Alphabet.Contains(c))
                {
                    sb.Append(c);
                    seen.Add(c);
                }
            }

            // fill matrix with remaining characters
            foreach(char c in Alphabet)
            {
                if (!seen.Contains(c))
                    sb.Append(c);
            }

            SubstitutionMatrix = sb.ToString();
            Debug.Assert(sb.Length == Alphabet.Length, "Matrix length != Alphabet length");
        }

        private void rebuildTranspositionCleanPassword()
        {
            string value = TranspositionPass.ToUpperInvariant();

            // remove characters not part of alphabet
            List<char> cleanPassword = new List<char>();
            foreach(char c in value)
            {
                if (Alphabet.Contains(c))
                    cleanPassword.Add(c);
            }

            // copy and sort characters
            char[] keyChars = cleanPassword.ToArray();
            Array.Sort(keyChars);

            // determine column order
            int[] newColumnOrder = new int[keyChars.Length];
            for (int i = 0; i < keyChars.Length; i++)
            {
                int column = Array.IndexOf(keyChars, cleanPassword[i]);
                newColumnOrder[i] = column;
                keyChars[column] = (char)0; // make sure the same character won't be found again
            }
            this.KeyColumnOrder = newColumnOrder;

            // build nice looking string for output (note: column numbers start with 0 in array, but 1 in string)
            StringBuilder keyWord = new StringBuilder();
            if (newColumnOrder.Length >= 1)
            {
                keyWord.Append((newColumnOrder[0]+1));
                for(int i = 1; i < newColumnOrder.Length; i++)
                {
                    keyWord.Append("-" + (newColumnOrder[i]+1));
                }
            }
            this.CleanTranspositionPass = keyWord.ToString();
        }

        private void updateAlphabet()
        {
            switch (cipherType)
            {
                case CipherTypeEnum.ADFGX:
                    this.Alphabet = ALPHABET25;
                    this.CipherAlphabet = CIPHER_ALPHABET5;
                    break;
                case CipherTypeEnum.ADFGVX:
                default:
                    this.Alphabet = ALPHABET36;
                    this.CipherAlphabet = CIPHER_ALPHABET6;
                    break;
            }

            rebuildSubstitutionMatrix();
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        private ActionEnum selectedAction = ActionEnum.Encrypt;

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public ActionEnum Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        private CipherTypeEnum cipherType = CipherTypeEnum.ADFGVX;

        [TaskPane("CipherVariantCaption", "CipherVariantTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ADFGX", "ADFGVX" })]
        public CipherTypeEnum CipherType
        {
            get { return this.cipherType; }
            set
            {
                if (value != cipherType)
                {
                    this.cipherType = value;
                    OnPropertyChanged("CipherType");

                    updateAlphabet();
                }
            }
        }

        private string substitutionPass = string.Empty;

        [TaskPane("SubstitutionPassCaption", "SubstitutionPassTooltip", null, 3, false, ControlType.TextBox)]
        public string SubstitutionPass
        {
            get { return this.substitutionPass; }
            set
            {
                if (value != substitutionPass)
                {
                    this.substitutionPass = value;
                    OnPropertyChanged("SubstitutionPass");

                    rebuildSubstitutionMatrix();
                }
            }
        }

        private string substitutionMatrix = string.Empty;

        [TaskPane("SubstitutionMatrixCaption", "SubstitutionMatrixTooltip", null, 4, false, ControlType.TextBoxReadOnly)]
        public string SubstitutionMatrix
        {
            get { return substitutionMatrix; }
            set
            {
                if (value != substitutionMatrix)
                {
                    this.substitutionMatrix = value;
                    OnPropertyChanged("SubstitutionMatrix");    
                }
            }
        }

        [TaskPane("StandardMatrixCaption", "StandardMatrixTooltip", null, 5, false, ControlType.Button)]
        public void ResetKeyButton()
        {
            SubstitutionPass = string.Empty;
            TranspositionPass = string.Empty;
        }

        [TaskPane("RandomMatrixCaption", "RandomMatrixTooltip", null, 6, false, ControlType.Button)]
        public void RandomKeyButton()
        {
            SubstitutionPass = createRandomPassword();
            TranspositionPass = createRandomPassword();
        }

        private string transpositionPass = string.Empty;

        [TaskPane("TranspositionPassCaption", "TranspositionPassTooltip", null, 7, false, ControlType.TextBox)]
        public string TranspositionPass
        {
            get { return this.transpositionPass; }
            set
            {
                if (value != transpositionPass)
                {
                    this.transpositionPass = value;
                    OnPropertyChanged("TranspositionPass");

                    rebuildTranspositionCleanPassword();
                }
            }
        }

        private string cleanTranspositionPass = string.Empty;

        [TaskPane("CleanTranspositionPassCaption", "CleanTranspositionPassTooltip", null, 8, false, ControlType.TextBoxReadOnly)]
        public string CleanTranspositionPass
        {
            get { return cleanTranspositionPass; }
            set
            {
                if (value != cleanTranspositionPass)
                {
                    cleanTranspositionPass = value;
                    OnPropertyChanged("CleanTranspositionPass");
                }
            }
        }

        // Not a user setting, but used by ADFGVX processing
        public int[] KeyColumnOrder
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}

﻿/*
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Hill
{
    public class HillSettings : ISettings
    {
        #region Private Variables

        private bool action = false;    // false = encrypt, true = decrypt
        // Der Modulus der Hill-Matrix wird aus der Länge des Alphabets gewonnen, die Dimension der Matrix aus matrixString.
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string matrixString = "1,0,0,0,1,0,0,0,1";

        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public bool Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (value != action)
                {
                    this.action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("AlphabetCaption", "AlphabetTooltip", null, 2, false, ControlType.TextBox)]
        public string Alphabet
        {
            get
            {
                return this.alphabet;
            }
            set
            {
                if (value != alphabet)
                {
                    HashSet<char> isPresent = new HashSet<char>();
                    this.alphabet = "";
                    foreach (var c in value)
                    {
                        if (!isPresent.Contains(c))
                        {
                            isPresent.Add(c);
                            this.alphabet += c;
                        }
                    }
                    OnPropertyChanged("Alphabet");
                }
            }
        }

        [TaskPane("MatrixStringCaption", "MatrixStringTooltip", null, 3, false, ControlType.TextBox)]
        public string MatrixString
        {
            get
            {
                return this.matrixString;
            }
            set
            {
                if (value != matrixString)
                {
                    this.matrixString = value;
                    OnPropertyChanged("MatrixString");
                }
            }
        }

        public int Modulus
        {
            get
            {
                return new List<char>(alphabet.ToCharArray()).Distinct().Count();
            }
        }

        public void Initialize()
        {
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
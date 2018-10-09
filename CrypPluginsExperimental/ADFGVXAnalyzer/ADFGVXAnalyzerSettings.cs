/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace ADFGVXAnalyzer
{
    // HOWTO: rename class (click name, press F2)
    public class ADFGVXANalyzerSettings : ISettings
    {
        #region Private Variables

        private int keyLengthFrom = 13;
        private int keyLengthTo = 13;
        private int threads = 1;
        private char separator = ',';

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("KeyLengthFrom", "KeyLengthFromToolTip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyLengthFrom
        {
            get
            {
                return keyLengthFrom;
            }
            set
            {
                if (keyLengthFrom != value)
                {
                    keyLengthFrom = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("KeyLengthFrom");
                }
            }
        }

        [TaskPane("KeyLengthTo", "KeyLengthToToolTip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyLengthTo
        {
            get
            {
                return keyLengthTo;
            }
            set
            {
                if (keyLengthTo != value)
                {
                    keyLengthTo = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("KeyLengthTo");
                }
            }
        }

        [TaskPane("Threads", "ThreadsToolTip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int Threads
        {
            get
            {
                return threads;
            }
            set
            {
                if (threads != value)
                {
                    threads = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Threads");
                }
            }
        }

        [TaskPane("Separator", "SeparatorToolTip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public char Separator
        {
            get
            {
                return separator;
            }
            set
            {
                if (separator != value)
                {
                    separator = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Separator");
                }
            }
        }

        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        [TaskPane("Alphabet", "Alphabet", null, 3, false, ControlType.TextBox, ValidationType.RegEx, 0, Int32.MaxValue)]
        public string Alphabet
        {
            get { return this.alphabet; }
            set
            {
                if (value != alphabet)
                {

                    this.alphabet = value;
                    OnPropertyChanged("Alphabet");


                }
            }
        }

        private string encryptAlphabet = "ADFGVX";

        [TaskPane("EncryptAlphabet", "EncryptAlphabet", null, 3, false, ControlType.TextBox, ValidationType.RegEx, 0, Int32.MaxValue)]
        public string EncryptAlphabet
        {
            get { return this.encryptAlphabet; }
            set
            {
                if (value != encryptAlphabet)
                {
                    this.encryptAlphabet = value;
                    OnPropertyChanged("EncryptAlphabet");

                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}

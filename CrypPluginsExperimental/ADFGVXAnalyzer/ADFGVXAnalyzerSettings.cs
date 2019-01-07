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
using System.Collections.ObjectModel;

namespace Cryptool.ADFGVXAnalyzer
{
    public class ADFGVXAnalyzerSettings : ISettings
    {
        #region Private Variables

        private int _keyLengthFrom = 13;
        private int _keyLengthTo = 13;
        private int _threads = 1;
        private int _separator = 0;
        private string _plaintextAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private ObservableCollection<string> _coresAvailable = new ObservableCollection<string>();

        #endregion

        public ADFGVXAnalyzerSettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add((i + 1).ToString());
            }
        }

        /// <summary>
        /// Get the number of cores in a collection, used for the selection of cores
        /// </summary>
        public ObservableCollection<string> CoresAvailable
        {
            get { return _coresAvailable; }
            set
            {
                if (value != _coresAvailable)
                {
                    _coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }


        #region TaskPane Settings

        [TaskPane("FromKeylengthCaption", "FromKeylengthTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyLengthFrom
        {
            get
            {
                return _keyLengthFrom;
            }
            set
            {
                if (_keyLengthFrom != value)
                {
                    _keyLengthFrom = value;
                    OnPropertyChanged("KeyLengthFrom");
                }
            }
        }

        [TaskPane("ToKeylengthCaption", "ToKeylengthTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyLengthTo
        {
            get
            {
                return _keyLengthTo;
            }
            set
            {
                if (_keyLengthTo != value)
                {
                    _keyLengthTo = value;
                    OnPropertyChanged("KeyLengthTo");
                }
            }
        }

        [TaskPane("ThreadsCaption", "ThreadsTooltip", null, 3, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int Threads
        {
            get
            {
                return _threads;
            }
            set
            {
                if (_threads != value)
                {
                    _threads = value;
                    OnPropertyChanged("Threads");
                }
            }
        }

        [TaskPane("SeparatorCaption", "SeparatorTooltip", null, 4, false, ControlType.ComboBox,new string[]{",",";","."})]
        public int Separator
        {
            get
            {
                return _separator;
            }
            set
            {
                if (_separator != value)
                {
                    _separator = value;
                    OnPropertyChanged("Separator");
                }
            }
        }        

        [TaskPane("PlaintextAlphabetCaption", "PlaintextAlphabetTooltip", null, 3, false, ControlType.TextBox)]
        public string PlaintextAlphabet
        {
            get { return this._plaintextAlphabet; }
            set
            {
                if (value != _plaintextAlphabet)
                {

                    _plaintextAlphabet = value;
                    OnPropertyChanged("PlaintextAlphabet");


                }
            }
        }

        private string _ciphertextAlphabet = "ADFGVX";

        [TaskPane("CiphertextAlphabetCaption", "CiphertextAlphabetTooltip", null, 3, false, ControlType.TextBox)]
        public string CiphertextAlphabet
        {
            get { return _ciphertextAlphabet; }
            set
            {
                if (value != _ciphertextAlphabet)
                {
                    this._ciphertextAlphabet = value;
                    OnPropertyChanged("CiphertextAlphabet");

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

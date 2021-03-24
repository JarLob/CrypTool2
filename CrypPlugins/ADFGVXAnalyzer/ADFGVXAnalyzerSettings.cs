﻿/*
   Copyright 2018 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Collections.ObjectModel;

namespace ADFGVXAnalyzer
{
    // HOWTO: rename class (click name, press F2)
    public class ADFGVXANalyzerSettings : ISettings
    {
        #region Private Variables

        private int keyLength = 15;
        private int language = 0;
        private int separator = 0;
        private int coresUsed = 0;
        private int restarts = 100;
        private int deviation = 10;

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();


        #endregion


        public ADFGVXANalyzerSettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;
        }

        /// <summary>
        /// Get the number of cores in a collection, used for the selection of cores
        /// </summary>
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        #region TaskPane Settings

        [TaskPane("ChooseAlphabetCaption", "ChooseAlphabetTooltip", "AlphabetGroup", 1, false, ControlType.ComboBox, new string[] { "ChooseAlphabetList1", "ChooseAlphabetList2", "ChooseAlphabetList3", "ChooseAlphabetList4", "ChooseAlphabetList5"})] 
        public int Language
        {
            get { return language; }
            set { language = value; }
        }
    
        [TaskPane("KeyLength", "KeyLengthToolTip", "MessageGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyLength
        {
            get
            {
                return keyLength;
            }
            set
            {
                if (keyLength != value)
                {
                    keyLength = value;
                    OnPropertyChanged("KeyLength");
                }
            }
        }


        [TaskPane("ChooseSeparatorCaption", "ChooseSeparatorTooltip", "MessageGroup", 1, false, ControlType.ComboBox, new string[] { "ChooseSeparatorList1", "ChooseSeparatorList2", "ChooseSeparatorList3", "ChooseSeparatorList4" })]
        public int Separator
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
                    OnPropertyChanged("Separator");
                }
            }
        }

        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        [TaskPane("AlphabetCaption", "AlphabetTooltip", "MessageGroup", 3, false, ControlType.TextBox, ValidationType.RegEx, 0, Int32.MaxValue)]
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

        [TaskPane("EncryptAlphabetCaption", "EncryptAlphabetTooltip", "MessageGroup", 3, false, ControlType.TextBox, ValidationType.RegEx, 0, Int32.MaxValue)]
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

        /// <summary>
        /// Getter/Setter for the number of cores which should be used by ADFGVXAnalyzer
        /// </summary>
        [TaskPane("Threads", "ThreadsToolTip", "ParametersGroup", 1, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the number of restarts which should be used by ADFGVXAnalyzer
        /// </summary>
        [TaskPane("Restarts", "RestartsToolTip", "ParametersGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int Restarts
        {
            get { return this.restarts; }
            set
            {
                if (value != this.restarts)
                {
                    this.restarts = value;
                    OnPropertyChanged("Restarts");
                }
            }
        }

        [TaskPane("Deviation", "DeviationToolTip", "ParametersGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int Deviation
        {
            get { return this.deviation; }
            set
            {
                if (value != this.deviation)
                {
                    this.deviation = value;
                    OnPropertyChanged("Deviation");
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

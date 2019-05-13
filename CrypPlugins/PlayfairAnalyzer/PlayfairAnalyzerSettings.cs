﻿/*
   Copyright 2019 George Lasry, Nils Kopal, CrypTool 2 Team

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
using Cryptool.PluginBase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cryptool.PlayfairAnalyzer
{

    public enum Language
    {
        English
    }
    public class PlayfairAnalyzerSettings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _filename;
        private string _arguments;
        private string _resourceDirectory;
        private bool _showWindow;
        private Language _language;

        private int _threads;
        private int _cycles;

        public PlayfairAnalyzerSettings()
        {
            //fill the list for the dropdown menu with numbers from 1 to ProcessorCount
            CoresAvailable.Clear();
            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add(i.ToString());
            }
        }

        public void Initialize()
        {
            
        }      

        [TaskPane("ThreadsCaption", "ThreadsTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int Threads
        {
            get { return _threads; }
            set
            {
                if (value != _threads)
                {
                    _threads = value;
                    OnPropertyChanged("Threads");
                }
            }
        }

        [TaskPane("CyclesCaption", "CyclesTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 1000)]
        public int Cycles
        {
            get { return _cycles; }
            set
            {
                if (value != _cycles)
                {
                    _cycles = value;
                    OnPropertyChanged("Cycles");
                }
            }
        }

        [TaskPane("ShowWindowCaption", "ShowWindowTooltip", null, 3, false, ControlType.CheckBox)]
        public bool ShowWindow
        {
            get { return _showWindow; }
            set
            {
                if (value != _showWindow)
                {
                    _showWindow = value;
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 4, false, ControlType.ComboBox, new string[] { "English"})]
        public Language Language
        {
            get { return _language; }
            set
            {
                if (value != _language)
                {
                    _language = value;
                }
            }
        }

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        [DontSave]
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

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

    }
}

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.M138Analyzer
{
    // HOWTO: rename class (click name, press F2)
    public class M138AnalyzerSettings : ISettings
    {
        #region Private Variables
        private int _analyticMode = 0;
        private int _language = 0;
        private int _keyLength = 25;
        private int _minOffset = 1;
        private int _maxOffset = 26;
        private string _retries;

        #endregion

        #region TaskPane Settings

        [TaskPane("MathodCap", "MethodDes", null, 0, false, ControlType.ComboBox, new string[] { "KnownPlaintextDes", "HillClimbingDes" })]
        public int Method
        {
            get
            {
                return _analyticMode;
            }
            set
            {
                if (_analyticMode != value)
                {
                    _analyticMode = value;
                }
            }
        }

        [TaskPane("LanguageCap", "LanguageDes", null, 0, false, ControlType.ComboBox, new string[] { "EnglishDes", "GermanDes" })]
        public int LanguageSelection
        {
            get
            {
                return _language;
            }
            set
            {
                if (_language != value)
                {
                    _language = value;
                }
            }
        }

        [TaskPane("KeyLengthCap", "KeyLengthDes", null, 0, false, ControlType.TextBox)]
        public int KeyLengthUserSelection
        {
            get
            {
                return _keyLength;
            }
            set
            {
                if (_keyLength != value)
                {
                    _keyLength = value;
                }
            }
        }

        [TaskPane("MinOffsetCap", "MinOffsetDes", null, 0, false, ControlType.TextBox)]
        public int MinOffsetUserSelection
        {
            get
            {
                return _minOffset;
            }
            set
            {
                if (_minOffset != value)
                {
                    _minOffset = value;
                }
            }
        }

        [TaskPane("MaxOffsetCap", "MaxOffsetDes", null, 0, false, ControlType.TextBox)]
        public int MaxOffsetUserSelection
        {
            get
            {
                return _maxOffset;
            }
            set
            {
                if (_maxOffset != value)
                {
                    _maxOffset = value;
                }
            }
        }

        [TaskPane("HillClimbRestartsCap", "HillClimbRestartsDes", null, 0, false, ControlType.TextBox)]
        public string HillClimbRestarts
        {
            get
            {
                return _retries;
            }
            set 
            {
                if (_retries != value)
                {
                    _retries = value;
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

        public void Initialize()
        {

        }

        #endregion
    }
}

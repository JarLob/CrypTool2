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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows;

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
        private bool _fastConverge = false;

        #endregion

        #region TaskPane Settings

        [TaskPane("MethodCap", "MethodDes", null, 0, false, ControlType.ComboBox, new string[] { "KnownPlaintextDes", "HillClimbingDes", "PartKnowPlainDes" })]
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

        [TaskPane("LanguageCap", "LanguageDes", null, 4, false, ControlType.ComboBox, new string[] { "EnglishDes", "GermanDes" })]
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

        [TaskPane("KeyLengthCap", "KeyLengthDes", null, 1, false, ControlType.TextBox)]
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

        [TaskPane("MinOffsetCap", "MinOffsetDes", null, 2, false, ControlType.TextBox)]
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

        [TaskPane("MaxOffsetCap", "MaxOffsetDes", null, 3, false, ControlType.TextBox)]
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

        [TaskPane("HillClimbRestartsCap", "HillClimbRestartsDes", null, 5, false, ControlType.TextBox)]
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

        [TaskPane("FastConvergeCap", "FastConvergeDes", null, 6, false, ControlType.CheckBox)]
        public bool FastConverge
        {
            get
            {
                return _fastConverge;
            }
            set
            {
                if (_fastConverge != value)
                {
                    _fastConverge = value;
                }
            }
        }

        #endregion

        #region Events
        public void UpdateTaskPaneVisibility()
        {
            if (_analyticMode == 0)
            {
                //Known Plaintext
                SettingChanged("LanguageSelection", Visibility.Hidden);
                SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                SettingChanged("HillClimbRestarts", Visibility.Hidden);
                SettingChanged("FastConverge", Visibility.Hidden);
            }
            else if(_analyticMode == 1)
            {
                //Hill Climbing
                SettingChanged("LanguageSelection", Visibility.Visible);
                SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                SettingChanged("HillClimbRestarts", Visibility.Visible);
                SettingChanged("FastConverge", Visibility.Visible);
            }
            else if (_analyticMode == 2)
            {
                SettingChanged("LanguageSelection", Visibility.Visible);
                SettingChanged("KeyLengthUserSelection", Visibility.Visible);
                SettingChanged("MinOffsetUserSelection", Visibility.Visible);
                SettingChanged("MaxOffsetUserSelection", Visibility.Visible);
                SettingChanged("HillClimbRestarts", Visibility.Visible);
                SettingChanged("FastConverge", Visibility.Visible);
            }
            else
            {
                //Nope, this should not be possible
            }
        }
        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;


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

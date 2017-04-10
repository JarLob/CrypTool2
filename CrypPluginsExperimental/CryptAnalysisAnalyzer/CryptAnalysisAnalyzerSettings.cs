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
using System.Numerics;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    public enum FormatType { lettersOnly, digitsOnly, numbers, binaryOnly };
    public enum GenerationType { random, naturalSpeech };

    public class CryptAnalysisAnalyzerSettings : ISettings
    {
        #region Private Variables

        private BigInteger _textLength = 100;
        private int _minKeyLength = 14;
        private int _maxKeyLength = 14;
        private int _keysPerLength = 1;
        private double _correctPercentage = 100;
        private int _timeUnit = 10;
        private FormatType _keyFormat = FormatType.lettersOnly;
        private GenerationType _keyGeneration = GenerationType.random;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Minimal correct percentage", "This is a parameter tooltip", null, 1, false, ControlType.TextBox, null)]
        public double CorrectPercentage
        {
            get
            {
                return _correctPercentage;
            }
            set
            {
                _correctPercentage = value;
                OnPropertyChanged("CorrectPercentage");
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Time unit size in ms", "This is a parameter tooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int TimeUnit
        {
            get
            {
                return _timeUnit;
            }
            set
            {
                _timeUnit = value;
                OnPropertyChanged("TimeUnit");
            }
        }

        /*
        internal void UpdateTaskPaneVisibility()
        {
            settingChanged("KeyFormat", Visibility.Collapsed);

            switch (KeyGeneration)
            {
                case GenerationType.naturalSpeech: // natural speech
                    settingChanged("KeyFormat", Visibility.Visible);
                    break;
                case GenerationType.random: // random generation
                    // TODO: change to invisible when input alphabet or regex is implemented
                    settingChanged("KeyFormat", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }*/

        #endregion

        #region Events

        //public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}

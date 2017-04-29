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

namespace Cryptool.Plugins.TestVectorGenerator
{
    public enum FormatType { lettersOnly, uniqueNumbers, digitsOnly, binaryOnly, uniqueLetters };
    public enum GenerationType { regex, random, naturalSpeech };

    /// <summary>
    /// An enumaration for the different modes of dealing with dots
    /// </summary>
    public enum DotSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

    /// <summary>
    /// An enumaration for the different modes of dealing with numbers
    /// </summary>
    public enum NumbersHandlingMode { Ignore = 0, Remove = 1, ReplaceEnglish = 2, ReplaceGerman = 3 };

    public class TestVectorGeneratorSettings : ISettings
    {
        #region Private Variables
        
        // general variables
        private const int generalPaneIndex = 1;
        private const int plaintextPaneIndex = generalPaneIndex + 7;
        private const int keyPaneIndex = plaintextPaneIndex + 5;
        private int _numberOfTestRuns = 1;
        private int _textLength = 100;
        private int _maxTextLength = 100;
        private int _textLengthIncrease = 5;
        private NumbersHandlingMode _numbersHandlingMode = NumbersHandlingMode.Remove;
        private bool _showExtendedSettings = false;

        // plaintext variables
        private DotSymbolHandlingMode _dotSymbolHandlingMode = DotSymbolHandlingMode.Remove;
        private string _dotReplacer = "X";

        // key variables
        private int _minKeyLength = 14;
        private int _maxKeyLength = 14;
        private string _separator = "";
        private FormatType _keyFormat;
        private GenerationType _keyGeneration;

        #endregion

        #region General TaskPane Settings

        [TaskPane("Number of Test Runs", "NumberOfTestRunsTooltipCaption", null, generalPaneIndex, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int NumberOfTestRuns
        {
            get
            {
                return _numberOfTestRuns;
            }
            set
            {
                if (_numberOfTestRuns != value)
                {
                    _numberOfTestRuns = value;
                    OnPropertyChanged("NumberOfTestRuns");
                }
            }
        }

        [TaskPane("Uppercase Only", "UppercaseOnlyTooltipCaption", null, generalPaneIndex + 1, false, ControlType.CheckBox)]
        public bool UppercaseOnly
        {
            get;
            set;
        }

        [TaskPane("Delete Spaces", "DeleteSpacesTooltipCaption", null, generalPaneIndex + 2, false, ControlType.CheckBox)]
        public bool DeleteSpaces
        {
            get;
            set;
        }

        [TaskPane("Replace ß by SZ", "ReplaceSZTooltipCaption", null, generalPaneIndex + 3, false, ControlType.CheckBox)]
        public bool ReplaceSZ
        {
            get;
            set;
        }

        [TaskPane("Replace Umlauts", "ReplaceUmlautsTooltipCaption", null, generalPaneIndex + 4, false, ControlType.CheckBox)]
        public bool ReplaceUmlauts
        {
            get;
            set;
        }

        [TaskPane("Numbers Handling", "NumbersHandlingTooltipCaption", null, generalPaneIndex + 5, false, ControlType.ComboBox, new String[] { 
            "Ignore", "Remove", "Replace with NULL, ONE,...", "Replace with EINS, ZWEI,..."})]
        public NumbersHandlingMode NumbersHandling
        {
            get;
            set;
        }

        [TaskPane("Show extended settings", "ShowExtendedSettingsTooltipCaption", null, generalPaneIndex + 6, false, ControlType.CheckBox)]
        public bool ShowExtendedSettings
        {
            get
            {
                return _showExtendedSettings;
            }
            set
            {
                _showExtendedSettings = value;
                UpdateExtendedSettingsVisibility();
                OnPropertyChanged("ShowExtendedSettings");
            }
        }

        #endregion

        #region Plaintext TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the _settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Plaintext Length", "This is a parameter tooltipCaption", "PlaintextGroup", plaintextPaneIndex, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int TextLength
        {
            get
            {
                return _textLength;
            }
            set
            {
                _textLength = value;
                OnPropertyChanged("TextLength");
            }
        }

        [TaskPane("Maximum Plaintext Length", "This is a parameter tooltipCaption", "PlaintextGroup", plaintextPaneIndex + 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int MaxTextLength
        {
            get
            {
                return _maxTextLength;
            }
            set
            {
                _maxTextLength = value;
                OnPropertyChanged("MaxTextLength");
            }
        }

        [TaskPane("Plaintext Length Step Increase", "This is a parameter tooltipCaption", "PlaintextGroup", plaintextPaneIndex + 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int TextLengthIncrease
        {
            get
            {
                return _textLengthIncrease;
            }
            set
            {
                _textLengthIncrease = value;
                OnPropertyChanged("TextLengthIncrease");
            }
        }

        public double PlaintextsPerLength
        {
            get
            {
                if (_textLengthIncrease == 0)
                    return 0;
                if (_maxTextLength == _textLength ||
                    _textLengthIncrease > (_maxTextLength - _textLength))
                    return 1;
                double plaintextsPerLength = (double) _numberOfTestRuns / ((_maxTextLength - _textLength + _textLengthIncrease) / _textLengthIncrease);
                return plaintextsPerLength;
            }
        }

        [TaskPane("Dot Symbol Handling", "DotSymbolHandlingTooltipCaption", "PlaintextGroup", plaintextPaneIndex + 3, false, ControlType.ComboBox, new String[] { 
            "Ignore", "Remove", "Replace with:"})]
        public DotSymbolHandlingMode DotSymbolHandling
        {
            get
            {
                return this._dotSymbolHandlingMode;
            }
            set
            {
                this._dotSymbolHandlingMode = value;
                UpdateDotReplacingSettingVisibility();
                OnPropertyChanged("DotSymbolHandling");
            }
        }

        [TaskPane("Replace Dots With:", "DotReplacerTooltipCaption", "PlaintextGroup", plaintextPaneIndex + 4, false, ControlType.TextBox, null)]
        public string DotReplacer
        {
            get
            {
                return _dotReplacer;
            }
            set
            {
                _dotReplacer = value;
                OnPropertyChanged("DotReplacer");
            }
        }

        #endregion

        #region Key TaskPane Settings

        [TaskPane("keyGenerationCaption", "KeyGenerationTooltipCaption", "KeyGroup", keyPaneIndex, false, ControlType.ComboBox, new String[] { 
            "random with regex", "random", "natural speech"})]
        public GenerationType KeyGeneration
        {
            get
            {
                return this._keyGeneration;
            }
            set
            {
                if (value != _keyGeneration)
                {
                    this._keyGeneration = value;
                    UpdateKeyFormatVisibility();
                    OnPropertyChanged("KeyGeneration");
                }
            }
        }

        [TaskPane("Minimum Key Length", "Minimum length of the generated keys", "KeyGroup", keyPaneIndex+1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MinKeyLength
        {
            get
            {
                return _minKeyLength;
            }
            set
            {
                if (_minKeyLength != value)
                {
                    _minKeyLength = value;
                    OnPropertyChanged("MinKeyLength");
                }
            }
        }

        [TaskPane("Maximum Key Length", "Maximum length of the generated keys", "KeyGroup", keyPaneIndex+2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MaxKeyLength
        {
            get
            {
                return _maxKeyLength;
            }
            set
            {
                if (_maxKeyLength != value)
                {
                    _maxKeyLength = value;
                    OnPropertyChanged("MaxKeyLength");
                }
            }
        }

        public int KeysPerLength
        {
            get
            {
                if (_maxKeyLength == _minKeyLength ||
                        _numberOfTestRuns == 0 ||
                        _numberOfTestRuns == 1 ||
                        _numberOfTestRuns < (_maxKeyLength - _minKeyLength))
                    return 1;
                return _numberOfTestRuns / (_maxKeyLength - _minKeyLength);
            }
        }

        [TaskPane("Separator between key symbols", "Separator between each of the key symbols to generate", "KeyGroup", keyPaneIndex+3, false, ControlType.TextBox, null)]
        public string Separator
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

        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", "KeyGroup", keyPaneIndex + 4, false, ControlType.ComboBox, new String[] { 
            "letters", "unique numbers", "digits", "binary", "unique letters"})]
        public FormatType KeyFormatRandom
        {
            get
            {
                return this._keyFormat;
            }
            set
            {
                if (value != _keyFormat)
                {
                    this._keyFormat = value;
                    OnPropertyChanged("KeyFormatRandom");
                }
            }
        }

        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", "KeyGroup", keyPaneIndex + 4, false, ControlType.ComboBox, new String[] { 
            "letters", "unique numbers"})]
        public FormatType KeyFormatNaturalSpeech
        {
            get
            {
                return this._keyFormat;
            }
            set
            {
                if (value != _keyFormat)
                {
                    this._keyFormat = value;
                    OnPropertyChanged("KeyFormat");
                }
            }
        }

        #endregion

        #region UI Update

        internal void UpdateExtendedSettingsVisibility()
        {
            settingChanged("MaxTextLength", Visibility.Collapsed);
            settingChanged("TextLengthIncrease", Visibility.Collapsed);
            settingChanged("UppercaseOnly", Visibility.Collapsed);
            settingChanged("DeleteSpaces", Visibility.Collapsed);
            settingChanged("ReplaceSZ", Visibility.Collapsed);
            settingChanged("ReplaceUmlauts", Visibility.Collapsed);
            settingChanged("DotSymbolHandling", Visibility.Collapsed);
            settingChanged("DotReplacer", Visibility.Collapsed);
            settingChanged("NumbersHandling", Visibility.Collapsed);
            settingChanged("Separator", Visibility.Collapsed);
            if (ShowExtendedSettings)
            {
                settingChanged("MaxTextLength", Visibility.Visible);
                settingChanged("TextLengthIncrease", Visibility.Visible);
                settingChanged("UppercaseOnly", Visibility.Visible);
                settingChanged("DeleteSpaces", Visibility.Visible);
                settingChanged("ReplaceSZ", Visibility.Visible);
                settingChanged("ReplaceUmlauts", Visibility.Visible);
                settingChanged("DotSymbolHandling", Visibility.Visible);
                settingChanged("NumbersHandling", Visibility.Visible);
                settingChanged("Separator", Visibility.Visible);
                UpdateDotReplacingSettingVisibility();
            }
        }

        internal void UpdateDotReplacingSettingVisibility()
        {
            settingChanged("DotReplacer", Visibility.Collapsed);
            if (_dotSymbolHandlingMode == DotSymbolHandlingMode.Replace)
            {
                settingChanged("DotReplacer", Visibility.Visible);
            }
        }

        internal void UpdateKeyFormatVisibility()
        {
            settingChanged("KeyFormatRandom", Visibility.Collapsed);
            settingChanged("KeyFormatNaturalSpeech", Visibility.Collapsed);
            switch (KeyGeneration)
            {
                case GenerationType.naturalSpeech: // natural speech
                    _keyFormat = FormatType.lettersOnly;
                    settingChanged("KeyFormatNaturalSpeech", Visibility.Visible);
                    break;
                case GenerationType.random: // random generation
                    // TODO: change to invisible when input alphabet or regex is implemented
                    settingChanged("KeyFormatRandom", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));

            OnPropertyChanged(setting);
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            if (_keyFormat == null)
                _keyFormat = FormatType.lettersOnly;
            if (_keyGeneration == null)
                _keyGeneration = GenerationType.random;
            UpdateKeyFormatVisibility();
            UpdateExtendedSettingsVisibility();
            UpdateDotReplacingSettingVisibility();
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}

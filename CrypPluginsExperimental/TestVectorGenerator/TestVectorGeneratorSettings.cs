/*
   Copyright 2017 CrypTool 2 Team <ct2contact@cryptool.org>

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
    // enums for the selectable options in the UI
    public enum FormatType { letters, numbers, binary, inputAlphabet };
    public enum GenerationType { regex, random, naturalSpeech };

    // an enumaration for the different modes of dealing with dots
    public enum DotSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

    // an enumaration for the different modes of dealing with numbers
    public enum NumbersHandlingMode { Ignore = 0, Remove = 1, ReplaceEnglish = 2, ReplaceGerman = 3 };

    public class TestVectorGeneratorSettings : ISettings
    {
        #region Private Variables
        
        // general variables
        private const int generalPaneIndex = 1;
        private const int plaintextPaneIndex = generalPaneIndex + 7;
        private const int keyPaneIndex = plaintextPaneIndex + 5;
        private int _numberOfTestRuns = 1;
        private int _minTextLength = 100;
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
        private bool _uniqueSymbolUsage = false;
        private FormatType _keyFormat;
        private GenerationType _keyGeneration;

        #endregion

        #region General TaskPane Settings

        /// <summary>
        /// The total number of test runs for this test series.
        /// </summary>
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

        /// <summary>
        /// Modifies all letters in the plaintext and the random and natural speech keys.
        /// </summary>
        [TaskPane("Uppercase Only", "UppercaseOnlyTooltipCaption", null, generalPaneIndex + 1, false, ControlType.CheckBox)]
        public bool UppercaseOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Deletes the spaces in the plaintext and the natural speech keys.
        /// </summary>
        [TaskPane("Delete Spaces", "DeleteSpacesTooltipCaption", null, generalPaneIndex + 2, false, ControlType.CheckBox)]
        public bool DeleteSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// Replaces the letter ß with sz in the plaintext and the natural speech keys.
        /// </summary>
        [TaskPane("Replace ß by sz", "ReplaceSZTooltipCaption", null, generalPaneIndex + 3, false, ControlType.CheckBox)]
        public bool ReplaceSZ
        {
            get;
            set;
        }

        /// <summary>
        /// Replaces all umlauts (upper- or lowercase) in the plaintext and the natural speech keys.
        /// </summary>
        [TaskPane("Replace Umlauts", "ReplaceUmlautsTooltipCaption", null, generalPaneIndex + 4, false, ControlType.CheckBox)]
        public bool ReplaceUmlauts
        {
            get;
            set;
        }

        /// <summary>
        /// Ignores, removes, or replaces numbers in the plaintext and the natural speech keys.
        /// </summary>
        [TaskPane("Numbers Handling", "NumbersHandlingTooltipCaption", null, generalPaneIndex + 5, false, ControlType.ComboBox, new String[] { 
            "Ignore", "Remove", "Replace with NULL, ONE,...", "Replace with EINS, ZWEI,..."})]
        public NumbersHandlingMode NumbersHandling
        {
            get;
            set;
        }

        /// <summary>
        /// Displays or hides the extended settings.
        /// </summary>
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
                // update the visibility in the UI
                UpdateExtendedSettingsVisibility();
                OnPropertyChanged("ShowExtendedSettings");
            }
        }

        #endregion

        #region Plaintext TaskPane Settings

        /// <summary>
        /// Defines the minimum length of the generated plaintexts.
        /// </summary>
        [TaskPane("Minimum Plaintext Length", "This is a parameter tooltipCaption", "PlaintextGroup", plaintextPaneIndex, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int MinTextLength
        {
            get
            {
                return _minTextLength;
            }
            set
            {
                _minTextLength = value;
                OnPropertyChanged("MinTextLength");
            }
        }

        /// <summary>
        /// Defines the maximum length of the generated plaintexts.
        /// </summary>
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

        /// <summary>
        /// Defines the text length steps of the generated plaintexts.
        /// </summary>
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

        /// <summary>
        /// Returnes the number of plaintexts to be generated with each length.
        /// </summary>
        public double PlaintextsPerLength
        {
            get
            {
                if (_textLengthIncrease == 0)
                    return 0;
                if (_maxTextLength == _minTextLength ||
                    _textLengthIncrease > (_maxTextLength - _minTextLength))
                    return 1;
                double plaintextsPerLength = (double) _numberOfTestRuns / ((_maxTextLength - _minTextLength + _textLengthIncrease) / _textLengthIncrease);
                return plaintextsPerLength;
            }
        }

        /// <summary>
        /// Ignores, removes, or replaces dots in the plaintext and the natural speech keys.
        /// </summary>
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

        /// <summary>
        /// Specifies the dot replacing symbol.
        /// </summary>
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

        /// <summary>
        /// Specifies the type of the key generation.
        /// </summary>
        [TaskPane("keyGenerationCaption", "KeyGenerationTooltipCaption", "KeyGroup", keyPaneIndex, false, ControlType.ComboBox, new String[] { 
            "Random with regex", "Random", "Natural speech"})]
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

        /// <summary>
        /// Defines the minimum length of the generated keys.
        /// </summary>
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

        /// <summary>
        /// Defines the maximum length of the generated keys.
        /// </summary>
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

        /// <summary>
        /// Returns the number of keys to be generated with each length.
        /// </summary>
        public int KeysPerLength
        {
            get
            {
                if (_maxKeyLength == _minKeyLength)
                    return _numberOfTestRuns;
                if (_numberOfTestRuns == 0 ||
                        _numberOfTestRuns == 1 ||
                        _numberOfTestRuns < (_maxKeyLength - _minKeyLength))
                    return 1;
                return _numberOfTestRuns / (_maxKeyLength - _minKeyLength);
            }
        }

        /// <summary>
        /// Specifies the separator symbol between the key symbols.
        /// </summary>
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

        /// <summary>
        /// Specifies the key format for the random key generation.
        /// </summary>
        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", "KeyGroup", keyPaneIndex + 4, false, ControlType.ComboBox, new String[] { 
            "Letters", "Numbers", "Binary", "Use input alphabet"})]
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

        /// <summary>
        /// Switches between using each key symbol only once and using it arbitrarily often.
        /// </summary>
        [TaskPane("Unique symbol usage", "UniqueSymbolUsageTooltipCaption", "KeyGroup", keyPaneIndex + 5, false, ControlType.CheckBox)]
        public bool UniqueSymbolUsage
        {
            get
            {
                return _uniqueSymbolUsage;
            }
            set
            {
                _uniqueSymbolUsage = value;
                OnPropertyChanged("UniqueSymbolUsage");
            }
        }

        /// <summary>
        /// Specifies the key format for the natural speech key generation.
        /// </summary>
        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", "KeyGroup", keyPaneIndex + 4, false, ControlType.ComboBox, new String[] { 
            "Sentences from text", "Numeric key from text"})]
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

        /// <summary>
        /// Updates the visibility of the extended settings.
        /// </summary>
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

        /// <summary>
        /// Shows or hides the dot replacer input field.
        /// </summary>
        internal void UpdateDotReplacingSettingVisibility()
        {
            settingChanged("DotReplacer", Visibility.Collapsed);
            if (_dotSymbolHandlingMode == DotSymbolHandlingMode.Replace)
            {
                settingChanged("DotReplacer", Visibility.Visible);
            }
        }

        /// <summary>
        /// Shows the key format drop-down menu for the selected key generation type.
        /// </summary>
        internal void UpdateKeyFormatVisibility()
        {
            settingChanged("KeyFormatRandom", Visibility.Collapsed);
                    settingChanged("UniqueSymbolUsage", Visibility.Collapsed);
            settingChanged("KeyFormatNaturalSpeech", Visibility.Collapsed);
            switch (KeyGeneration)
            {
                case GenerationType.naturalSpeech: // natural speech
                    _keyFormat = FormatType.letters;
                    settingChanged("KeyFormatNaturalSpeech", Visibility.Visible);
                    break;
                case GenerationType.random: // random generation
                    // TODO: change to invisible when input alphabet or regex is implemented
                    settingChanged("KeyFormatRandom", Visibility.Visible);
                    settingChanged("UniqueSymbolUsage", Visibility.Visible);
                    break;
            }
        }

        /// <summary>
        /// Triggers TaskPaneAttributeChanged and OnPropertyChanged for the given setting.
        /// </summary>
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

        /// <summary>
        /// Sets the default values and updates the UI.
        /// </summary>
        public void Initialize()
        {
            if (_keyFormat == null)
                _keyFormat = FormatType.letters;
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

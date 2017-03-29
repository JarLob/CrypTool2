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

    public class TestVectorGeneratorSettings : ISettings
    {
        #region Private Variables

        private int _textLength = 100;
        private int _minKeyLength = 14;
        private int _maxKeyLength = 14;
        private int _keyAmountPerLength = 1;
        private FormatType _keyFormat;
        private GenerationType _keyGeneration;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the _settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Ciphertext Length", "This is a parameter tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int TextLength
        {
            get
            {
                return _textLength;
            }
            set
            {
                if (_textLength != value)
                {
                    _textLength = value;
                    OnPropertyChanged("TextLength");
                }
            }
        }

        [TaskPane("keyGenerationCaption", "KeyGenerationTooltipCaption", null, 2, true, ControlType.ComboBox, new String[] { 
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
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("KeyGeneration");
                }
            }
        }

        [TaskPane("Minimum Key Length", "Minimum length of the generated keys", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
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

        [TaskPane("Maximum Key Length", "Maximum length of the generated keys", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
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

        [TaskPane("Key Amount Per Length", "Amount of keys to generate per length", null, 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyAmountPerLength
        {
            get
            {
                return _keyAmountPerLength;
            }
            set
            {
                if (_keyAmountPerLength != value)
                {
                    _keyAmountPerLength = value;
                    OnPropertyChanged("KeyAmountPerLength");
                }
            }
        }

        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", null, 6, true, ControlType.ComboBox, new String[] { 
            "uppercase letters", "unique numbers", "digits", "binary", "unique Letters"})]
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

        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", null, 6, true, ControlType.ComboBox, new String[] { 
            "uppercase letters", "unique numbers"})]
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

        internal void UpdateTaskPaneVisibility()
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
            UpdateTaskPaneVisibility();
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}

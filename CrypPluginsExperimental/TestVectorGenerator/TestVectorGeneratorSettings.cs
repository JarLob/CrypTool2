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

namespace Cryptool.Plugins.TestVectorGenerator
{
    public enum FormatType { substitution, transformation };

    public class TestVectorGeneratorSettings : ISettings
    {
        #region Private Variables

        private int textLength = 100;
        private int minKeyLength = 14;
        private int maxKeyLength = 14;
        private int keyAmountPerLength = 1;
        private FormatType keyFormat = FormatType.substitution;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Ciphertext Length", "This is a parameter tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int TextLength
        {
            get
            {
                return textLength;
            }
            set
            {
                if (textLength != value)
                {
                    textLength = value;
                    OnPropertyChanged("TextLength");
                }
            }
        }

        [TaskPane("Minimum Key Length", "Minimum length of the generated keys", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MinKeyLength
        {
            get
            {
                return minKeyLength;
            }
            set
            {
                if (minKeyLength != value)
                {
                    minKeyLength = value;
                    OnPropertyChanged("MinKeyLength");
                }
            }
        }

        [TaskPane("Maximum Key Length", "Maximum length of the generated keys", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MaxKeyLength
        {
            get
            {
                return maxKeyLength;
            }
            set
            {
                if (maxKeyLength != value)
                {
                    maxKeyLength = value;
                    OnPropertyChanged("MaxKeyLength");
                }
            }
        }

        [TaskPane("Key Amount Per Length", "Amount of keys to generate per length", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KeyAmountPerLength
        {
            get
            {
                return keyAmountPerLength;
            }
            set
            {
                if (keyAmountPerLength != value)
                {
                    keyAmountPerLength = value;
                    OnPropertyChanged("KeyAmountPerLength");
                }
            }
        }

        [TaskPane("keyFormatCaption", "KeyFormatTooltipCaption", null, 5, true, ControlType.ComboBox, new String[] { 
            "Substitution", "Transformation"})]
        public FormatType KeyFormat
        {
            get
            {
                return this.keyFormat;
            }
            set
            {
                if (value != keyFormat)
                {
                    this.keyFormat = value;
                    //UpdateTaskPaneVisibility();
                    OnPropertyChanged("KeyFormat");
                }
            }
        }

        #endregion

        #region Events

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

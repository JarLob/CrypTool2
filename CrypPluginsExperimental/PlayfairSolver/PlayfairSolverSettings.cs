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

namespace Cryptool.Plugins.PlayfairSolver
{
    // HOWTO: rename class (click name, press F2)
    public class ExamplePluginCT2Settings : ISettings
    {
        #region Private Variables

        private int maxLength = 10000;
        private int maxCycles = 100;

        #endregion

        #region TaskPane Settings

        [TaskPane("MaxLength", "Maximum length of ciphertext", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MaxLength
        {
            get
            {
                return maxLength;
            }
            set
            {
                if (maxLength != value)
                {
                    maxLength = value;
                    OnPropertyChanged("MaxLength");
                }
            }
        }

        [TaskPane("MaxCycles", "Maximum number of cycles", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MaxCycles
        {
            get
            {
                return maxCycles;
            }
            set
            {
                if (maxCycles != value)
                {
                    maxCycles = value;
                    OnPropertyChanged("MaxCycles");
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
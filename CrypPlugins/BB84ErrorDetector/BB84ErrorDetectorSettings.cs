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

namespace Cryptool.Plugins.BB84ErrorDetector
{

    public class BB84ErrorDetectorSettings : ISettings
    {
        #region Private Variables

        private int startIndex;
        private int endIndex;
        private int thresholdValue;

        #endregion

        #region TaskPane Settings



        [TaskPane("Start Index", "res_StartIndexTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)] //######
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                if (startIndex != value)
                {
                    startIndex = value;
                    OnPropertyChanged("StartIndex");
                }
            }
        }

        [TaskPane("End Index", "res_EndIndexTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)] //######
        public int EndIndex
        {
            
            get
            {
                return endIndex;
            }
            set
            {
                if (endIndex != value)
                {
                    endIndex = value;
                    OnPropertyChanged("EndIndex");
                }
            }
        }

        [TaskPane("res_ThresholdCaption", "res_ThresholdTooltip", null, 3, false, ControlType.NumericUpDown,ValidationType.RangeInteger, 0, 100)] 
        public int ThresholdValue
        {
            get
            {
                return thresholdValue;
            }
            set
            {
                if (thresholdValue != value)
                {
                    thresholdValue = value;
                    OnPropertyChanged("ThresholdValue");
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
    }
}

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
using System.Numerics;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ObliviousTransfer1
{

    public class ObliviousTransfer1Settings : ISettings
    {
        #region Private Variables

        private int kLimit= 1;

        #endregion

        #region TaskPane Settings


        [TaskPane("KLimit", "Maximum limit of k", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int KLimit
        {
            get
            {
                return kLimit;
            }
            set
            {
                if (kLimit != value)
                {
                    if (value < 1)
                    {
                        kLimit = 1;
                    }
                    else
                    {
                        kLimit = value;
                    }

                    OnPropertyChanged("KLimit");
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

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

using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Salsa20
{
    public class Salsa20Settings : ISettings
    {
        public int rounds = 2;
        [TaskPane("RoundCaption", "RoundTooltip", null, 0, false, ControlType.ComboBox, new string[] { "8", "12", "20" })]
        public int Rounds
        {
            get { return rounds; }
            set
            {
                    rounds = value;
                    OnPropertyChanged("Rounds");
            }
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
        #endregion
    }
}

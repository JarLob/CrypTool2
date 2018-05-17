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

namespace Cryptool.Plugins.RandomNumberGenerator
{
    // HOWTO: rename class (click name, press F2)
    public class RandomNumberGeneratorSettings : ISettings
    {
        #region Private Variables

        private int _rndAlg = 0;

        #endregion

        #region TaskPane Settings

        [TaskPane("presRadioCaption", "presRadioToolTip", "presAlgoGroup", 0, false, ControlType.ComboBox, new string[] { "X^2 mod N", "LCG", "ICG" })]
        public int RndAlg
        {
            get
            {
                return this._rndAlg;
            }
            set
            {
                this._rndAlg = value;
                Console.WriteLine("Auswahl: " + _rndAlg);
                OnPropertyChanged("RndAlg");
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

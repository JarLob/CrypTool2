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

namespace Cryptool.Plugins.AESVisualization
{
    // HOWTO: rename class (click name, press F2)
    public class AESVisualizationSettings : ISettings
    {
        #region Private Variables

        private int keysize;
        private int language;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("KeysizeCaption", "KeysizeTooltip", null, 3, false, ControlType.ComboBox, new String[] { "128 Bit", "192 Bit", "256 Bit" })]
        public int Keysize
        {
            get { return this.keysize; }
            set
            {
                if (((int)value) != keysize)
                {
                    this.keysize = (int)value;
                    OnPropertyChanged("Keysize");
                }
            }
        }

        //[TaskPane("Sprache", "SpracheTooltip", null, 2, false, ControlType.ComboBox, new String[] { "Deutsch", "English"})]
        //public int Language
        //{
        //    get { return this.language; }
        //    set
        //    {
        //        if (((int)value) != language)
        //        {
        //            this.language = (int)value;
        //            OnPropertyChanged("Sprache");
        //        }
        //    }
        //}
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

﻿/*
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.IO;

namespace Cryptool.MD5
{
    public class MD5Settings : ISettings
    {

        private bool hasChanges = false;

        #region INotifyPropertyChanged Members

#pragma warning disable 67
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
#pragma warning restore


        #endregion


        private bool presentationMode = false;
        [TaskPane("Presentation mode", "With presentation mode enabled, results will not be generated ASAP. Instead, the generation steps can be individually traced from the Presentation screen.", null, 0, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool PresentationMode
        {
            get { return presentationMode; }
            set
            {
                if (value != presentationMode)
                {
                    presentationMode = value;
                    hasChanges = true;
                    OnPropertyChanged("PresentationMode");
                }
            }
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(p));
        }

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion
    }
}

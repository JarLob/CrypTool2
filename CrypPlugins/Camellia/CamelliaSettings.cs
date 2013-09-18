/*
   Copyright 2013 Nils Kopal, University of Kassel

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

namespace Camellia
{
    public class CamelliaSettings : ISettings
    {
        private int _action ; // 0=encrypt, 1=decrypt
        private int _keysize; // 0=128, 1=192, 2=256
        private int _mode; // 0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int _padding = 1; // 0="None", 1="Zeros"=default, 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5=1-0-Padding

        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this._action; }
            set
            {
                if (((int)value) != _action)
                {
                    this._action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("KeysizeCaption", "KeysizeTooltip", null, 3, false, ControlType.ComboBox, new String[] { "KeysizeList1", "KeysizeList2", "KeysizeList3" })]
        public int Keysize
        {
            get { return this._keysize; }
            set
            {
                if (((int)value) != _keysize)
                {
                    this._keysize = (int)value;
                    OnPropertyChanged("Keysize");
                }
            }
        }

        [TaskPane("ModeCaption", "ModeTooltip", null, 5, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        public int Mode
        {
            get { return _mode; }
            set
            {
                if ((value) != _mode)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("PaddingCaption", "PaddingTooltip", null, 6, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public int Padding
        {
            get { return _padding; }
            set
            {
                if ((value) != _padding)
                {
                    _padding = value;
                    OnPropertyChanged("Padding");
                }
            }
        }

        #region events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

     

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
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

namespace Cryptool.M_138
{
    public class M_138Settings : ISettings
    {
        #region Private Variables

        private int _encryptDecrypt = 0;
        private int _separatorStr = 0;
        private int _separatorOff = 0;
        private int _invalidCharacters = 0;
        private bool _caseSensitive = false;

        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return _encryptDecrypt;
            }
            set
            {
                if (_encryptDecrypt != value)
                {
                    _encryptDecrypt = value;
                }
            }
        }

        [TaskPane("SeparatorStripCaption", "SeparatorStripTooltip", null, 1, false, ControlType.ComboBox, new string[] {  ",", ".", "/" })]
        public int SeparatorStripChar
        {
            get
            {
                return _separatorStr;
            }
            set
            {
                if (_separatorStr != value)
                {
                    _separatorStr = value;
                }
            }
        }

        [TaskPane("SeparatorOffCharCaption", "SeparatorOffCharTooltip", null, 2, false, ControlType.ComboBox, new string[] { "/", ",", "." })]
        public int SeparatorOffChar
        {
            get
            {
                return _separatorOff;
            }
            set
            {
                if (_separatorOff != value)
                {
                    _separatorOff = value;
                }
            }
        }

        [TaskPane("InvalidCharacterHandlingCaption", "InvalidCharacterHandlingTooltip", null, 3, false, ControlType.ComboBox, new string[] { "InvalidCharacterHandlingList1", "InvalidCharacterHandlingList2", "InvalidCharacterHandlingList3" })]
        public int InvalidCharacterHandling
        {
            get
            {
                return _invalidCharacters;
            }
            set
            {
                if (_invalidCharacters != value)
                {
                    _invalidCharacters = value;
                }
            }
        }

        [TaskPane("CaseSensitivyCaption", "CaseSensitivyTooltip", null, 4, false, ControlType.CheckBox)]
        public bool CaseSensitivity
        {
            get
            {
                return _caseSensitive;
            }
            set
            {
                if (_caseSensitive != value)
                {
                    _caseSensitive = value;
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

        public void Initialize()
        {

        }

        #endregion
    }
}
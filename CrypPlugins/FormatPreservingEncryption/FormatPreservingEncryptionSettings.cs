﻿/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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

namespace Cryptool.Plugins.FormatPreservingEncryption
{
    //TODO isnt working as expected
    //delarce directly 
    enum Algorithms: int{ FF1, FF2, FF3, DFF};
    enum Actions { Encrypt, Decrypt};
    enum Modes { Normal, XML };

    public class FormatPreservingEncryptionSettings : ISettings
    {
        #region Private Variables and Constants

        private int action = 0; // 0 Encrypt, 1 Decrypt
        private int algorithm = 0; // FF1, FF2, FF3, DFF
        private int mode = 0; //normal, XML
        private bool passPlaintext = false;


        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt"})]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("AlgorithmCaption", "AlgorithmTooltip", null, 2, false, ControlType.ComboBox, new string[] { "FF1", "FF2", "FF3", "DFF" })]
        public int Algorithm
        {
            get { return this.algorithm; }
            set
            {
                if (((int)value) != algorithm)
                {
                    this.algorithm = (int)value;
                    OnPropertyChanged("Algorithm");
                }
            }
        }


        [TaskPane("ModeCaption", "ModeTooltip", null, 3, false, ControlType.ComboBox, new string[] { "Normal", "XML" })]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                if (((int)value) != mode)
                {
                    this.mode = (int)value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("PassPlaintextCaption", "PassPlaintextTooltip", null, 3, false, ControlType.CheckBox, new string[] {"Pass plaintext to ciphertext output"})]
        public bool PassPlaintext
        {
            get { return this.passPlaintext; }
            set
            {
                if ((value) != passPlaintext)
                {
                    this.passPlaintext = value;
                    OnPropertyChanged("PassPlaintext");
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
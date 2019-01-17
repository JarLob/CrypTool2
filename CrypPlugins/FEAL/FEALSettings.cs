﻿/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using Cryptool.Plugins.FEAL;

namespace Cryptool.Plugins.FEAL
{  
    public enum FealAlgorithmType
    {
        FEAL4,
        FEAL8
    }

    public enum Action
    {
        Encrypt,
        Decrypt
    }

    public enum BlockMode
    {
        ECB,
        CBC,
        CFB,
        OFB,
        EAX
    }

    public class FEALSettings : ISettings
    {
        #region Private Variables

        private FealAlgorithmType _fealAlgorithmType = FealAlgorithmType.FEAL8;
        private Action _action = Action.Encrypt;
        private BlockMode _blockMode = BlockMode.ECB;
        private BlockCipherHelper.PaddingType _padding = BlockCipherHelper.PaddingType.None;
        #endregion

        #region TaskPane Settings


        [TaskPane("AlgorithmTypeCaption", "AlgorithmTypeTooltip", null, 1, false, ControlType.ComboBox,
            new string[] {"FEAL4", "FEAL8"})]
        public FealAlgorithmType FealAlgorithmType
        {
            get { return _fealAlgorithmType; }
            set
            {
                if (_fealAlgorithmType != value)
                {
                    _fealAlgorithmType = value;
                    OnPropertyChanged("FealAlgorithmType");
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox,
            new string[] {"Encrypt", "Decrypt"})]
        public Action Action
        {
            get { return _action; }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("BlockModeCaption", "BlockModeTooltip", null, 3, false, ControlType.ComboBox,
            new string[] {"ECB", "CBC", "CFB", "OFB", "EAX"})]
        public BlockMode BlockMode
        {
            get { return _blockMode; }
            set
            {
                if (_blockMode != value)
                {
                    _blockMode = value;
                    OnPropertyChanged("BlockMode");
                }
            }
        }

        [TaskPane("PaddingCaption", "PaddingTooltip", null, 4, false, ControlType.ComboBox, new string[] {"None", "Zeros", "PKCS7", "ANSIX932", "ISO10126", "OneZeros"})]
        public BlockCipherHelper.PaddingType Padding
        {
            get { return _padding; }
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    OnPropertyChanged("Padding");
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

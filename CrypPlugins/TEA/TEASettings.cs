﻿/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.TEA
{
    public class TEASettings : ISettings
    {
        #region ISettings Members

        private int action = 0; // 0=encrypt, 1=decrypt
        private int version = 0; // 0="TEA"=default, 1="XTEA, 2=XXTEA"
        private int rounds = 64;
        private int mode = 0; // 0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 1; // 0="None", 1="Zeros"=default, 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5=1-0-Padding

        public BlockCipherHelper.PaddingType[] padmap = new BlockCipherHelper.PaddingType[6] { 
            BlockCipherHelper.PaddingType.None, BlockCipherHelper.PaddingType.Zeros, BlockCipherHelper.PaddingType.PKCS7,
            BlockCipherHelper.PaddingType.ANSIX923, BlockCipherHelper.PaddingType.ISO10126, BlockCipherHelper.PaddingType.OneZeros
        };

        [ContextMenu( "ActionCaption", "ActionTooltip",1, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "ActionList1", "ActionList2")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set { this.action = (int)value; }
        }

        [ContextMenu("VersionCaption", "VersionTooltip", 2, ContextMenuControlType.ComboBox, null, "VersionList1", "VersionList3", "VersionList3")]
        [TaskPane("VersionCaption", "VersionTooltip", null, 2, false, ControlType.ComboBox, new String[] { "VersionTPList1", "VersionTPList2", "VersionTPList3" })]
        public int Version
        {
            get { return this.version; }
            set
            {
                if (((int)value) != version)
                {
                    this.version = (int)value;
                    OnPropertyChanged("Padding");                    
                }
            }
        }
        
        [ContextMenu("ModeCaption", "ModeTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        [TaskPane("ModeCaption", "ModeTooltip", "", 3, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        public int Mode
        {
            get
            {
                return mode;
            }

            set
            {
                if (((int)value) != mode)
                {
                    this.mode = (int)value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 4, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6")]
        [TaskPane("PaddingTPCaption", "PaddingTPTooltip", null, 4, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5", "PaddingList6" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding)
                {
                    this.padding = (int)value;
                    OnPropertyChanged("Padding");                    
                }
            }
        }

        [TaskPane("RoundsCaption", "RoundsTooltip", "RoundsGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get { return this.rounds; }
            set
            {
                if (((int)value) != rounds)
                {
                    this.rounds = value;
                    OnPropertyChanged("Rounds");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propName);
        }

        #endregion
    }
}

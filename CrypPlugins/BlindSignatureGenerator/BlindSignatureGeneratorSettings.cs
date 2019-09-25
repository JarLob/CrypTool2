﻿/*
   Copyright 2019 Axel Wehage

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cryptool.Plugins.BlindSignatureGenerator
{  
    public class BlindSignatureGeneratorSettings : ISettings
    {

        #region Public Variables

        public enum SigningMode { RSA = 0, Paillier = 1 };

        public enum HashMode { SHA1 = 0, SHA256 = 1, SHA384 = 2, SHA512 = 3, None = 4, };

        #endregion

        #region Private Variables

        private SigningMode selectedSigningMode = SigningMode.RSA;
        private HashMode selectedHashMode = HashMode.SHA1;
        private String blindSigningSecurity = "500";
        #endregion

        #region TaskPane Settings

        public BlindSignatureGeneratorSettings()
        {
        }
  
        [TaskPane("SigningAlgorithm", "SigningAlgorithmTooltip", null, 2, false, ControlType.ComboBox, new string[] { "RSA", "Paillier" })]
        public SigningMode SigningAlgorithm
        {
            get
            {
                return this.selectedSigningMode;
            }
            set
            {
                if (value != selectedSigningMode)
                {
                    this.selectedSigningMode = value;
                    OnPropertyChanged("SigningAlgorithm");
                }
            }
        }
        [TaskPane("HashAlgorithm", "HashAlgorithmTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SHA1", "SHA256", "SHA384", "SHA512", "None" })]
        public HashMode HashAlgorithm
        {
            get
            {
                return this.selectedHashMode;
            }
            set
            {
                if (value != selectedHashMode)
                {
                    this.selectedHashMode = value;
                    OnPropertyChanged("HashAlgorithm");
                }
            }
        }
        /// <summary>
        /// Getter/Setter for the degree of security against the blind signing attack
        /// </summary>
        [TaskPane("BlindSigningSecurity", "BlindSigningSecurityTooltip", null, 3, false, ControlType.TextBox)]
        public String BlindSigningSecurity
        {
            get
            {
                return this.blindSigningSecurity;
            }
            set
            {
                if (value != blindSigningSecurity)
                {
                    if (value == null || value == "")
                    {
                        this.blindSigningSecurity = "500";
                        OnPropertyChanged("BlindSigningSecurity");
                    }
                    this.blindSigningSecurity = value;
                    OnPropertyChanged("BlindSigningSecurity");
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

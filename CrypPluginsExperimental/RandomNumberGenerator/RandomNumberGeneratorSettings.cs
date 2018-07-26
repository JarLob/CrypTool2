﻿/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>
   Author: Christian Bender, Universität Siegen
           Nils Kopal, CrypTool 2 Team

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
using System.Numerics;
using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace Cryptool.Plugins.RandomNumberGenerator
{
    /// <summary>
    /// Type of random number generator
    /// </summary>
    public enum AlgorithmType
    {
        RandomRandom = 0,               //.net Random.Random
        RNGCryptoServiceProvider = 1,   //.net RNGCryptoServiceProvider
        X2modN = 2,
        LCG = 3,
        ICG = 4
    }

    public enum OutputType
    {
        ByteArray = 0,
        CrypToolStream = 1,
        Number = 2,
        NumberArray = 3
    }
    

    public class RandomNumberGeneratorSettings : ISettings
    {
        #region Private Variables

        private readonly Dictionary<AlgorithmType, List<string>> _settingsVisibility = new Dictionary<AlgorithmType, List<string>>();
        private readonly List<string> _settingsList = new List<string>();

        private AlgorithmType _AlgorithmType = 0;
        private OutputType _OutputType = 0;

        private string _OutputLength = string.Empty;
        private string _OutputAmount = string.Empty;
        private string _Seed = string.Empty;
        private string _Modulus = string.Empty;
        private string _a = string.Empty;
        private string _b = string.Empty;
        
        #endregion

        public RandomNumberGeneratorSettings()
        {
            foreach (AlgorithmType type in Enum.GetValues(typeof(AlgorithmType)))
            {
                _settingsVisibility[type] = new List<string>();
            }            
        }

        #region TaskPane Settings

        [TaskPane("AlgorithmTypeCaption", "AlgorithmTypeTooltip", "GeneralSettingsGroup", 0, false, ControlType.ComboBox, new string[] { "Random.Random", "RNGCryptoServiceProvider", "X^2 mod N", "LCG", "ICG" })]
        public AlgorithmType AlgorithmType
        {
            get
            {
                return _AlgorithmType;
            }
            set
            {
                _AlgorithmType = value;
                UpdateTaskPaneVisibility();
            }
        }

        [TaskPane("OutputTypeCaption", "OutputTypeTooltip", "GeneralSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "Byte Array", "CryptoolStream", "Number", "Number Array" })]
        public OutputType OutputType
        {
            get
            {
                return _OutputType;
            }
            set
            {
                _OutputType = value;
                UpdateTaskPaneVisibility();
            }
        }

        [TaskPane("OutputLengthCaption", "OutputLengthTooltip", "GeneralSettingsGroup", 2, false, ControlType.TextBox)]
        public string OutputLength
        {
            get
            {
                return _OutputLength;
            }
            set
            {
                _OutputLength = value;
            }
        }

        [TaskPane("OutputAmountCaption", "OutputAmountTooltip", "GeneralSettingsGroup", 3, false, ControlType.TextBox)]
        public string OutputAmount
        {
            get
            {
                return _OutputAmount;
            }
            set
            {
                _OutputAmount = value;
            }
        }


        [TaskPane("SeedCaption", "SeedTooltip", "AlgorithmSettingsGroup,", 0, false, ControlType.TextBox)]
        public string Seed
        {
            get
            {
                return _Seed;
            }
            set
            {
                _Seed = value;
            }
        }

        [TaskPane("ModulusCaption", "ModulusTooltip", "AlgorithmSettingsGroup,", 1, false, ControlType.TextBox)]
        public string Modulus
        {
            get
            {
                return _Modulus;
            }
            set
            {
                _Modulus = value;
            }
        }

        [TaskPane("aCaption", "aTooltip", "AlgorithmSettingsGroup,", 2, false, ControlType.TextBox)]
        public string a
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
            }
        }

        [TaskPane("bCaption", "aTooltip", "AlgorithmSettingsGroup,", 3, false, ControlType.TextBox)]
        public string b
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
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
            _settingsList.Add("Seed");
            _settingsList.Add("Modulus");
            _settingsList.Add("a");
            _settingsList.Add("b");
            _settingsVisibility[AlgorithmType.RandomRandom].Add("Seed");
            _settingsVisibility[AlgorithmType.RandomRandom].Add("Modulus");
            _settingsVisibility[AlgorithmType.RNGCryptoServiceProvider].Add("Seed");
            _settingsVisibility[AlgorithmType.RNGCryptoServiceProvider].Add("Modulus");
            _settingsVisibility[AlgorithmType.X2modN].Add("Seed");
            _settingsVisibility[AlgorithmType.X2modN].Add("Modulus");
            _settingsVisibility[AlgorithmType.ICG].Add("Seed");
            _settingsVisibility[AlgorithmType.ICG].Add("Modulus");
            _settingsVisibility[AlgorithmType.ICG].Add("a");
            _settingsVisibility[AlgorithmType.ICG].Add("b");
            _settingsVisibility[AlgorithmType.LCG].Add("Seed");
            _settingsVisibility[AlgorithmType.LCG].Add("Modulus");
            _settingsVisibility[AlgorithmType.LCG].Add("a");
            _settingsVisibility[AlgorithmType.LCG].Add("b");
            UpdateTaskPaneVisibility();
        }

        private void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            foreach (var tpac in _settingsList.Select(operation => new TaskPaneAttribteContainer(operation, (_settingsVisibility[AlgorithmType].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }

            if (_OutputType == OutputType.NumberArray)
            {
                TaskPaneAttribteContainer tpac = new TaskPaneAttribteContainer("OutputAmount", Visibility.Visible);
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
            else
            {
                TaskPaneAttribteContainer tpac = new TaskPaneAttribteContainer("OutputAmount", Visibility.Collapsed);
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    }
}

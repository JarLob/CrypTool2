/*
   Copyright 2017 Nils Kopal, Applied Information Security, Uni Kassel
   http://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.ComponentModel;


namespace Cryptool.VigenereAnalyzer
{
    public enum Mode
    {
        Vigenere = 0,
        Autokey = 1
    };

    public enum Language
    {
        Englisch = 0,
        German = 1,
        Spanish = 2
    };

    public enum CostFunction
    {
        Trigrams = 0,
        Quadgrams = 1,
        Both = 2,
        IoC = 3
    }

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    };

    public enum KeyStyle
    {
        Random = 0,
        NaturalLanguage=1        
    }

    class VigenereAnalyzerSettings : ISettings
    {      
        private Mode _mode = Mode.Vigenere;
        private int _fromKeylength;
        private int _toKeyLength = 20;
        private Language _language = Language.Englisch;
        private bool _fastConverge;
        private int _restarts = 50;
        private KeyStyle _keyStyle;
        private CostFunction _costFunction = CostFunction.Quadgrams;

        // EVALUATION!
        private bool _stopIfPercentReached = false;
        private int _comparisonFrequency = 1;

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new []{"Vigenere", "VigenereAutokey"})]
        public Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("FromKeylengthCaption", "FromKeylengthTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int FromKeylength
        {
            get
            {
                return _fromKeylength;
            }
            set
            {
                if (value != _fromKeylength)
                {
                    _fromKeylength = value;
                    OnPropertyChanged("FromKeyLength");
                }
            }
        }

        [TaskPane("ToKeylengthCaption", "ToKeylengthTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int ToKeyLength
        {
            get
            {
                return _toKeyLength;
            }
            set
            {
                if (value != _toKeyLength)
                {
                    _toKeyLength = value;
                    OnPropertyChanged("ToKeyLength");
                }
            }
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int Restarts
        {
            get
            {
                return _restarts;
            }
            set
            {
                if (value != _restarts)
                {
                    _restarts = value;
                    OnPropertyChanged("Restarts");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 5, false, ControlType.ComboBox, new string[]{"English","German", "Spanish"})]
        public Language Language
        {
            get
            {
                return _language;
            }
            set
            {
                if (value != _language)
                {
                    _language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        [TaskPane("CostFunction", "CostFunctionTooltip", null, 6, false, ControlType.ComboBox, new string[] { "Trigrams", "Quadgrams", "Both", "IoC" })]
        public CostFunction CostFunction
        {
            get
            {
                return _costFunction;
            }
            set
            {
                if (value != _costFunction)
                {
                    _costFunction = value;
                    OnPropertyChanged("CostFunction");
                }
            }
        }

        [TaskPane("KeyStyleCaption", "KeyStyleTooltip", null, 7, false, ControlType.ComboBox, new string[] { "Random", "NaturalLanguage" })]
        public KeyStyle KeyStyle
        {
            get
            {
                return _keyStyle;
            }
            set
            {
                if (value != _keyStyle)
                {
                    _keyStyle = value;
                    OnPropertyChanged("KeyStyle");
                }
            }
        }

        // EVALUATION!
        [TaskPane("Stop current analysis if percent reached", "Stop the current analysis in the cryptanalytic component if entered percentage reached", null, 7, false, ControlType.CheckBox)]
        public bool StopIfPercentReached
        {
            get
            {
                return this._stopIfPercentReached;
            }
            set
            {
                this._stopIfPercentReached = value;
                OnPropertyChanged("StopIfPercentReached");
            }
        }

        // EVALUATION!
        [TaskPane("ComparisonFrequencyCaption", "ComparisonFrequencyTooltip", null, 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int ComparisonFrequency
        {
            get
            {
                return _comparisonFrequency;
            }
            set
            {
                if (value != _comparisonFrequency)
                {
                    _comparisonFrequency = value;
                    OnPropertyChanged("ComparisonFrequency");
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}

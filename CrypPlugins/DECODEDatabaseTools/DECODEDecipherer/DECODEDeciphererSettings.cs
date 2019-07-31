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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    public enum ParserType
    {
        SimpleSingleTokenParser = 0,
        NoVocabularyParser = 1,
        Vocabulary3DigitsEndingWithNull1DigitsParser = 2,
        Vocabulary3DigitsEndingWithNull2DigitsParser = 3,
        Vocabulary4DigitsWithPrefixParser = 4,
        Francia4Parser = 5,
        Francia6Parser = 6
    }

    class DECODEDeciphererSettings : ISettings
    {
        private ParserType _parserType;
        private string _nulls;
        private string _prefix;
        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ParserTypeCaption", "ParserTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] 
        {
            "SimpleSingleTokenParser",
            "NoVocabularyParser",
            "Vocabulary3DigitsEndingWithNull1DigitsParser",
            "Vocabulary3DigitsEndingWithNull2DigitsParser",
            "Vocabulary4DigitsWithPrefixParser",
            "Francia4Parser",
            "Francia6Parser"
        })]
        public ParserType ParserType
        {
            get { return _parserType; }
            set
            {
                if ((value) != _parserType)
                {
                    _parserType = value;
                    OnPropertyChanged("ParserType");
                }
            }
        }

        [TaskPane("NullsCaption", "NullsTooltip", null, 2, false, ControlType.TextBox)]
        public string Nulls
        {
            get { return _nulls; }
            set
            {
                if ((value) != _nulls)
                {
                    _nulls = value;
                    OnPropertyChanged("Nulls");
                }
            }
        }

        [TaskPane("PrefixCaption", "PrefixTooltip", null, 3, false, ControlType.TextBox)]
        public string Prefix
        {
            get { return _prefix; }
            set
            {
                if ((value) != _prefix)
                {
                    _prefix = value;
                    OnPropertyChanged("Prefix");
                }
            }
        }

        public void Initialize()
        {
            
        }

        public string[] GetNulls()
        {
            if (string.IsNullOrEmpty(_nulls)){
                return new string[0];
            }
            string[] nulls = _nulls.Split(',');
            for(int i = 0; i < nulls.Length; i++)
            {
                nulls[i] = nulls[i].Trim();
            }
            return nulls;
        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}

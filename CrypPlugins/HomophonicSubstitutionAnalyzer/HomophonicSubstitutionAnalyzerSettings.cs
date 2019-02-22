/*
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

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    public enum CiphertextFormat
    {
        Letters,
        NumberGroups
    }

    public enum AnalysisMode
    {
        SemiAutomatic,
        FullAutomatic
    }

    public class HomophonicSubstitutionAnalyzerSettings : ISettings
    {
        #region Private Variables

        private int _language;
        private bool _useSpaces;
        private CiphertextFormat _ciphertextFormat = CiphertextFormat.Letters;
        private int _wordCountToFind = 5;
        private int _minWordLength = 8;
        private int _maxWordLength = 10;
        private AnalysisMode _analysisMode;
        private int _cycles = 50000;
        private int _iterations = 1000;

        #endregion

        #region TaskPane Settings

        [TaskPane("LanguageCaption", "LanguageTooltip", "LanguageSettingsGroup", 0, false, ControlType.LanguageSelector)]
        public int Language
        {
            get { return _language; }
            set { _language = value; }
        }

        [TaskPane("UseSpacesCaption", "UseSpacesTooltip", "LanguageSettingsGroup", 1, false, ControlType.CheckBox)]
        public bool UseSpaces
        {
            get { return _useSpaces; }
            set { _useSpaces = value; }
        }

        [TaskPane("CiphertextFormatCaption", "CiphertextFormatTooltip", "TextFormatGroup", 2, false, ControlType.ComboBox, new[] { "Letters", "NumberGroups" })]
        public CiphertextFormat CiphertextFormat
        {
            get { return _ciphertextFormat; }
            set { _ciphertextFormat = value; }
        }

        [TaskPane("WordCountToFindCaption", "WordCountToFindTooltip", "WordLockerGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int WordCountToFind
        {
            get { return _wordCountToFind; }
            set { _wordCountToFind = value; }
        }

        [TaskPane("MinWordLengthCaption", "MinWordLengthTooltip", "WordLockerGroup", 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MinWordLength
        {
            get { return _minWordLength; }
            set { _minWordLength = value; }
        }

        [TaskPane("MaxWordLengthCaption", "MaxWordLengthTooltip", "WordLockerGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int MaxWordLength
        {
            get { return _maxWordLength; }
            set { _maxWordLength = value; }
        }

        [TaskPane("AnalysisModeCaption", "AnalaysisModeTooltip", "AlgorithmSettingsGroup", 6, false, ControlType.ComboBox, new string[]{"SemiAutomatic", "FullAutomatic"})]
        public AnalysisMode AnalysisMode
        {
            get { return _analysisMode; }
            set { _analysisMode = value; }
        }

        [TaskPane("CyclesCaption", "CyclesTooltip", "AlgorithmSettingsGroup", 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Cycles
        {
            get { return _cycles; }
            set { _cycles = value; }
        }

        [TaskPane("IterationsCaption", "IterationsTooltip", "AlgorithmSettingsGroup", 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
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

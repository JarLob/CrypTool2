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
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Threading;
using Cryptool.PluginBase.Utils;
using System;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.HomophonicSubstitutionAnalyzer.Properties.Resources","PluginCaption", "PluginTooltip", "HomophonicSubstitutionAnalyzer/userdoc.xml", "HomophonicSubstitutionAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class HomophonicSubstitutionAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly HomophonicSubstitutionAnalyzerSettings _settings = new HomophonicSubstitutionAnalyzerSettings();
        private readonly HomophoneSubstitutionAnalyzerPresentation _presentation = new HomophoneSubstitutionAnalyzerPresentation();
        private bool _running = true;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HomophonicSubstitutionAnalyzer()
        {
            _presentation.NewBestValue += PresentationOnNewBestValue;
            _presentation.UserChangedText += PresentationOnUserChangedText;
            _presentation.Progress += PresentationOnProgress;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "DictionaryCaption", "DictionaryTooltip", false)]
        public string[] Dictionary
        {
            get;
            set;
        }
  
        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip")]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "FoundWordsCaption", "FoundWordsTooltip")]
        public string FoundWords
        {
            get;
            set;
        }
        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            Dictionary = null;
            Ciphertext = null;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {            
            ProgressChanged(0, 1);

            //set separator for ciphertext letter separation
            char separator;
            switch (_settings.Separator)
            {
                case Separator.Comma:
                    separator = ',';
                    break;
                case Separator.FullStop:
                    separator = '.';
                    break;
                case Separator.Semicolon:
                    separator = ';';
                    break;
                default:
                    separator = ' ';
                    break;
            }
            _presentation.LoadLangStatistics(_settings.Language, _settings.UseSpaces);
            _presentation.AddCiphertext(Ciphertext, _settings.CiphertextFormat, separator, _settings.CostFactorMultiplicator, _settings.FixedTemperature);
            GenerateLetterLimits();
            _presentation.AnalyzerConfiguration.WordCountToFind = _settings.WordCountToFind;
            _presentation.AnalyzerConfiguration.MinWordLength = _settings.MinWordLength;
            _presentation.AnalyzerConfiguration.MaxWordLength = _settings.MaxWordLength;
            _presentation.AnalyzerConfiguration.Cycles = _settings.Cycles;
            _presentation.AnalyzerConfiguration.AnalysisMode = _settings.AnalysisMode;
            _presentation.AnalyzerConfiguration.Restarts = _settings.Restarts;
            _presentation.AddDictionary(Dictionary);                        

            _presentation.EnableUI();
            _running = true;

            if (_settings.AnalysisMode == AnalysisMode.FullAutomatic)
            {
                _presentation.StartAnalysis();
            }

            while (_running)
            {
                Thread.Sleep(100);
            }

            _presentation.DisableUIAndStop();
            
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Generate the letter limits list based on language
        /// </summary>
        private void GenerateLetterLimits()
        {
            int languageId = _settings.Language;
            string languageCode = LanguageStatistics.SupportedLanguagesCodes[languageId];
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Clear();
            string alphabet = LanguageStatistics.Alphabets[languageCode];

            double[] unigrams;
            if (LanguageStatistics.Unigrams.ContainsKey(languageCode))
            {
                unigrams = LanguageStatistics.Unigrams[languageCode];
            }
            else
            {
                //if we have no unigram stats, we just equally distribute the letters 
                unigrams = new double[alphabet.Length];
                for(int i=0;i< alphabet.Length; i++)
                {
                    unigrams[i] = 1.0 / alphabet.Length;
                }
            }
            
            for(int i = 0; i < alphabet.Length; i++)
            {
                int minvalue = 1;
                int maxvalue = 2;
                if (i < unigrams.Length)
                {
                    minvalue = (int)Math.Ceiling(unigrams[i] * alphabet.Length);
                    maxvalue = minvalue * 2;
                }
                _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = i, MinValue = minvalue, MaxValue = maxvalue });
            }            
            if (_settings.UseSpaces)
            {
                _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = alphabet.Length, MinValue = 2, MaxValue = 3 });   //SPACE/NULLS
            }
            _presentation.GenerateKeyLetterLimitsListView();
        }

        /// <summary>
        /// Progress of analyzer changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progressChangedEventArgs"></param>
        private void PresentationOnProgress(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            if(!_running)
            {
                return;
            }
            ProgressChanged(progressChangedEventArgs.Percentage, 1);
        }

        /// <summary>
        /// Analyzer found a new best value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newBestValueEventArgs"></param>
        private void PresentationOnNewBestValue(object sender, NewBestValueEventArgs newBestValueEventArgs)
        {
            if(!_running)
            {
                return;
            }
            if (newBestValueEventArgs.NewTopEntry)
            {
                Plaintext = newBestValueEventArgs.Plaintext;
                OnPropertyChanged("Plaintext");
                if (newBestValueEventArgs.FoundWords != null && newBestValueEventArgs.FoundWords.Count > 0)
                {
                    StringBuilder wordBuilder = new StringBuilder();
                    foreach (var word in newBestValueEventArgs.FoundWords)
                    {
                        wordBuilder.AppendLine(word);
                    }

                    FoundWords = wordBuilder.ToString();
                    OnPropertyChanged("FoundWords");
                }
                if (!string.IsNullOrWhiteSpace(newBestValueEventArgs.SubstitutionKey))
                {
                    Key = newBestValueEventArgs.SubstitutionKey;
                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        /// User changed a homophone plaintext mapping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="userChangedTextEventArgs"></param>
        private void PresentationOnUserChangedText(object sender, UserChangedTextEventArgs userChangedTextEventArgs)
        {
            Plaintext = userChangedTextEventArgs.Plaintext;
            OnPropertyChanged("Plaintext");
            Key = userChangedTextEventArgs.SubstitutionKey;
            OnPropertyChanged("Key");
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _presentation.DisableUIAndStop();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {            
            _running = false;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}

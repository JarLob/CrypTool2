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
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Threading;
using Cryptool.PluginBase.Utils;

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
            _presentation.Progress += PresentationOnProgress;
        }

        #region Data Properties
       
        /// </summary>
        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get;
            set;
        }

        /// </summary>
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
            _presentation.LoadLangStatistics(_settings.Language, _settings.UseSpaces);
            _presentation.AddCiphertext(Ciphertext, _settings.CiphertextFormat);
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
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Clear();
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 0, MinValue = 3, MaxValue = 5 });   //A
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 1, MinValue = 1, MaxValue = 2 });   //B
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 2, MinValue = 1, MaxValue = 2 });   //C
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 3, MinValue = 1, MaxValue = 2 });   //D
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 4, MinValue = 4, MaxValue = 6 });   //E
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 5, MinValue = 1, MaxValue = 2 });   //F
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 6, MinValue = 1, MaxValue = 2 });   //G
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 7, MinValue = 1, MaxValue = 2 });   //H
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 8, MinValue = 3, MaxValue = 5 });   //I
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 9, MinValue = 1, MaxValue = 2 });   //J
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 10, MinValue = 1, MaxValue = 2 });   //K
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 11, MinValue = 1, MaxValue = 2 });   //L
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 12, MinValue = 1, MaxValue = 2 });   //M
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 13, MinValue = 2, MaxValue = 3 });   //N
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 14, MinValue = 3, MaxValue = 5 });   //O
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 15, MinValue = 1, MaxValue = 2 });   //P
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 16, MinValue = 1, MaxValue = 2 });   //Q
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 17, MinValue = 1, MaxValue = 2 });   //R
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 18, MinValue = 1, MaxValue = 2 });   //S
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 19, MinValue = 3, MaxValue = 5 });   //T
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 20, MinValue = 3, MaxValue = 5 });   //U
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 21, MinValue = 1, MaxValue = 2 });   //V
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 22, MinValue = 1, MaxValue = 2 });   //W
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 23, MinValue = 1, MaxValue = 2 });   //X
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 24, MinValue = 1, MaxValue = 2 });   //Y
            _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 25, MinValue = 1, MaxValue = 2 });   //Z

            //_presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 26, MinValue = 1, MaxValue = 1 });   //Ä
            //_presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 27, MinValue = 1, MaxValue = 1 });   //Ö
            //_presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 28, MinValue = 1, MaxValue = 1 });   //Ü
            //_presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 29, MinValue = 1, MaxValue = 1 });   //ß            

            if (_settings.UseSpaces)
            {
                _presentation.AnalyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 26, MinValue = 2, MaxValue = 3 });   //SPACE       
            }
            _presentation.GenerateKeyLetterLimitsListView();
        }

        /// <summary>
        /// Progress of analyzer changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <param name="e"></param>
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
            }
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

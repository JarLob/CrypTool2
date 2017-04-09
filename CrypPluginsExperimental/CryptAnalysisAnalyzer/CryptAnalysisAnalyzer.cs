﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Numerics;
using System.Windows.Threading;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CryptAnalysisAnalyzer", "Subtract one number from another", "CryptAnalysisAnalyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CryptAnalysisAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly CryptAnalysisAnalyzerSettings _settings = new CryptAnalysisAnalyzerSettings();

        private string _textInput;
        private int _seedInput;
        private string _plaintextInput;
        private string _keyInput;
        private string _bestPlaintextInput;
        private string _bestKeyInput;
        private EvaluationContainer _evaluationInput;

        private string _plaintextOutput;
        private string _keyOutput;
        private string _bestPlaintextOutput;
        private string _bestKeyOutput;

        private int _keyCount = 0;
        private int _progress;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description")]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                this._textInput = value;
                OnPropertyChanged("TextInput");
            }
        }

        [PropertyInfo(Direction.InputData, "SeedInput", "SeedInput tooltip description")]
        public string SeedInput
        {
            get { return this._seedInput.ToString(); }
            set
            {
                int seed = value.GetHashCode();
                if (_seedInput != seed)
                {
                    this._seedInput = seed;
                    OnPropertyChanged("SeedInput");
                }

            }
        }

        [PropertyInfo(Direction.InputData, "KeyInput", "KeyInput tooltip description", true)]
        public string KeyInput
        {
            get { return this._keyInput; }
            set
            {
                this._keyInput = value;
                OnPropertyChanged("KeyInput");
            }
        }

        [PropertyInfo(Direction.InputData, "PlaintextInput", "PlaintextInput tooltip description", true)]
        public string PlaintextInput
        {
            get { return this._plaintextInput; }
            set
            {
                this._plaintextInput = value;
                OnPropertyChanged("PlaintextInput");
            }
        }

        [PropertyInfo(Direction.InputData, "BestKeyInput", "BestKeyInput tooltip description")]
        public string BestKeyInput
        {
            get { return this._bestKeyInput; }
            set
            {
                this._bestKeyInput = value;
                OnPropertyChanged("BestKeyInput");
            }
        }

        [PropertyInfo(Direction.InputData, "BestPlaintextInput", "BestPlaintextInput tooltip description")]
        public string BestPlaintextInput
        {
            get { return this._bestPlaintextInput; }
            set
            {
                this._bestPlaintextInput = value;
                OnPropertyChanged("BestPlaintextInput");
            }
        }

        [PropertyInfo(Direction.InputData, "EvaluationInput", "EvaluationInput tooltip description")]
        public EvaluationContainer EvaluationInput
        {
            get { return this._evaluationInput; }
            set
            {
                this._evaluationInput = value;
                OnPropertyChanged("EvaluationInput");
            }
        }


        [PropertyInfo(Direction.OutputData, "TriggerNextKey", "TriggerNextKey tooltip description")]
        public string TriggerNextKey { get; set; }

        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutput tooltip description")]
        public string KeyOutput
        {
            get { return this._keyOutput; }
            set
            {
                this._keyOutput = value;
                OnPropertyChanged("KeyOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get { return this._plaintextOutput; }
            set
            {
                this._plaintextOutput = value;
                OnPropertyChanged("PlaintextOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "BestKeyOutput", "BestKeyOutput tooltip description")]
        public string BestKeyOutput
        {
            get { return this._bestKeyOutput; }
            set
            {
                this._bestKeyOutput = value;
                OnPropertyChanged("BestKeyOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "BestPlaintextOutput", "BestPlaintextOutput tooltip description")]
        public string BestPlaintextOutput
        {
            get { return this._bestPlaintextOutput; }
            set
            {
                this._bestPlaintextOutput = value;
                OnPropertyChanged("BestPlaintextOutput");
            }
        }
        
        #endregion

        #region General Methods

        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        public int ComputeLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))            
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return string.IsNullOrEmpty(source) ? 0 : source.Length;
            
            int sourceLength = source.Length;
            int targetLength = target.Length;

            int[,] distance = new int[sourceLength + 1, targetLength + 1];

            // Step 1
            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetLength; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    // Step 2
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 3
                    distance[i, j] = Math.Min(
                                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        /// <summary> 
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source))            
                return string.IsNullOrEmpty(target) ? 1: 0;            

            if (string.IsNullOrEmpty(target))            
                return string.IsNullOrEmpty(source) ? 1 : 0;            

            double stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - (stepsToSame / (double)Math.Max(source.Length, target.Length)));
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
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _keyCount = 0;
            _plaintextOutput = "";
            _keyOutput = null;
            _progress = 0;
        }

        public bool checkVariables()
        {
            if (KeyInput.Length == 0 || String.IsNullOrEmpty(KeyInput))
            {
                // TESTING!!!
                KeyInput = "KEYWORDX";

                //GuiLogMessage("The key input is empty!", NotificationLevel.Error);
                //return false;
            }

            if (String.IsNullOrEmpty(PlaintextInput))
            {
                GuiLogMessage("The plaintext input is empty!", NotificationLevel.Error);
                return false;
            }

            if (String.IsNullOrEmpty(SeedInput))
            {
                GuiLogMessage("The seed input is empty! It is required for logging purposes.", NotificationLevel.Warning);
            }

            return true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!checkVariables())
            {
                GuiLogMessage("CAA: Execute: checkVariables failed!", NotificationLevel.Balloon);
                return;
            }

            if (PlaintextInput != PlaintextOutput ||
                KeyInput != KeyOutput)
            {
                PlaintextOutput = PlaintextInput;
                KeyOutput = KeyInput;
            }

            if (_evaluationInput != null && _evaluationInput.hasValueSet)
            {
                string evaluationString = _evaluationInput.ToString();
                Console.WriteLine(evaluationString);

                double percentCorrect = 0;
                if (!String.IsNullOrEmpty(BestKeyInput) && !String.IsNullOrEmpty(BestPlaintextInput))
                {

                    GuiLogMessage("Execute() Best Key: " + BestKeyInput, NotificationLevel.Debug);
                    GuiLogMessage("Execute() Best Plaintext: " + BestPlaintextInput.Substring(0,
                        BestPlaintextInput.Length > 50 ? 50 : BestPlaintextInput.Length), NotificationLevel.Debug);

                    percentCorrect = CalculateSimilarity(_bestPlaintextInput, _plaintextInput);
                    GuiLogMessage("percentCorrect: " + percentCorrect, NotificationLevel.Debug);
                    
                }

                BigInteger decryptions = EvaluationInput.GetDecryptions();
                TimeSpan runtime;
                if (EvaluationInput.GetRuntime(out runtime)) {
                    double divisor = runtime.TotalMilliseconds / 10;
                    double decryptionsPerTimeUnit = Math.Round((double) decryptions / divisor, 4);

                    GuiLogMessage("Decryptions per time unit: " + decryptionsPerTimeUnit, NotificationLevel.Debug);

                }

                TriggerNextKey = KeyInput;
                OnPropertyChanged("TriggerNextKey");

                ProgressChanged(1, 1);
            }

            
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
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
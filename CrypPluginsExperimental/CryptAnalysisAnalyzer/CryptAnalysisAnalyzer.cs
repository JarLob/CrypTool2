/*
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

using System;
using System.Numerics;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Controls;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;

using PercentageSimilarity;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Applied Information Security - University of Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CryptAnalysisAnalyzer", "Analyse CryptAnalysis methods", "CryptAnalysisAnalyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
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
        private int _evaluationCount = 0;
        private int _totalKeysInput = 0;
        private int _progress;
        private EvaluationContainer _lastEval;

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
        
        [PropertyInfo(Direction.InputData, "TotalKeysInput", "TotalKeysInput tooltip description", true)]
        public int TotalKeysInput
        {
            get { return this._totalKeysInput; }
            set
            {
                this._totalKeysInput = value;
                OnPropertyChanged("TotalKeysInput");
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

        [PropertyInfo(Direction.OutputData, "MinimalCorrectPercentage", "MinimalCorrectPercentage tooltip description")]
        public double MinimalCorrectPercentage
        {
            get { return _settings.CorrectPercentage; }
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

        public void SendTestVectorToEncryption()
        {
            _keyCount++;

            OnPropertyChanged("MinimalCorrectPercentage");
            PlaintextOutput = PlaintextInput;
            KeyOutput = KeyInput;
        }

        #endregion

        #region Evaluation

        public void CollectEvaluationData()
        {
            // Key number - _keyCount
            // Seed - SeedInput
            // EvaluationInput
            // Key - KeyInput
            // Key space - ???
            // Plaintext - PlaintextInput
            // Ciphertext - ???
            // Best key - BestKeyInput
            // Best plaintext - BestPlaintextInput
            // % correct - percentCorrect
            // Necessary decryptions - decryptions
            // Runtime - runtime
            // Restarts - EvaluationInput.GetRestarts()
            // DecryptionsPerTimeUnit - decryptionsPerTimeUnit
            // Success probability - to be calculated!
            // Population size - EvaluationInput.GetPopulationSize()
            // Tabu set size - EvaluationInput.GetTabuSetSize()

            if (_evaluationCount == 0)
                Console.WriteLine("------- EpochSeed: " + SeedInput + " -------");

            Console.WriteLine("----- Key: " + _keyCount + " -----");

            string evaluationString = _evaluationInput.ToString();
            Console.WriteLine(evaluationString);
            GuiLogMessage(evaluationString, NotificationLevel.Balloon);

            Console.WriteLine("Key: " + KeyInput);
            Console.WriteLine("Plaintext: " + PlaintextInput.Substring(0,
                PlaintextInput.Length > 50 ? 50 : PlaintextInput.Length));
            Console.WriteLine("Best Key: " + BestKeyInput);
            Console.WriteLine("Best Plaintext: " + BestPlaintextInput.Substring(0,
                BestPlaintextInput.Length > 50 ? 50 : BestPlaintextInput.Length));

            double percentCorrect = _bestPlaintextInput.CalculateSimilarity(_plaintextInput) * 100;
            Console.WriteLine("percentCorrect: " + percentCorrect);
            Console.WriteLine("success: " + (percentCorrect >= _settings.CorrectPercentage ? "yes!" : "no."));

            BigInteger decryptions = EvaluationInput.GetDecryptions();
            TimeSpan runtime;
            if (EvaluationInput.GetRuntime(out runtime))
            {
                double divisor = runtime.TotalMilliseconds / _settings.TimeUnit;
                double decryptionsPerTimeUnit = Math.Round((double)decryptions / divisor, 4);

                Console.WriteLine("Decryptions per time unit: " + decryptionsPerTimeUnit);

            }

            if (_totalKeysInput == null ||
                _totalKeysInput > 0 ||
                _keyCount < _totalKeysInput)
            {
                TriggerNextKey = KeyInput;
                OnPropertyChanged("TriggerNextKey");
            }

            _evaluationCount++;

            BestPlaintextInput = "";
            BestKeyInput = "";
            EvaluationInput = new EvaluationContainer();
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
            ProgressChanged(0, 1);
            Console.WriteLine("--------------------------------------------");
        }

        public bool checkVariables()
        {
            if (String.IsNullOrEmpty(PlaintextInput) &&
                (String.IsNullOrEmpty(KeyInput) || KeyInput.Length == 0))
            {
                if (String.IsNullOrEmpty(KeyInput) || KeyInput.Length == 0)
                {
                    // TESTING!!!
                    //KeyInput = "KEYWORDX";

                    GuiLogMessage("The key input is empty!", NotificationLevel.Error);
                    return false;
                }

                if (String.IsNullOrEmpty(PlaintextInput))
                {
                    GuiLogMessage("The plaintext input is empty!", NotificationLevel.Error);
                    return false;
                }
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
           
            if (!checkVariables())
            {
                return;
            }

            // If both plaintext and key are new,
            // send them to the output
            if (!String.IsNullOrEmpty(PlaintextInput) &&
                !String.IsNullOrEmpty(KeyInput) &&
                PlaintextInput != PlaintextOutput &&
                KeyInput != KeyOutput)
            {
                // Send the plaintext and key to the encryption method
                SendTestVectorToEncryption();

                if (_totalKeysInput > 0)
                    ProgressChanged(_keyCount-0.9, _totalKeysInput);
            }

            // Wait for the analysis method to send evaluation data.
            // If the evaluation input is set, together with the best key
            // and best plaintext, do the evaluation for that calculation
            if (_evaluationInput != null && _evaluationInput.hasValueSet &&
                (_lastEval == null || !_evaluationInput.Equals(_lastEval)) &&
                /*_evaluationCount < _keyCount &&*/
                _keyCount <= _totalKeysInput &&
                !String.IsNullOrEmpty(BestKeyInput) &&
                !String.IsNullOrEmpty(BestPlaintextInput) &&
                BestKeyInput != " " &&
                BestPlaintextInput != " ")
            {
                _lastEval = _evaluationInput;

                // Gather all available evaluation data
                CollectEvaluationData();

                if (_totalKeysInput > 0)
                    ProgressChanged(_keyCount, _totalKeysInput);
                else
                    ProgressChanged(1, 1);
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _plaintextInput = "";
            _keyInput = "";
            _bestPlaintextInput = "";
            _bestKeyInput = "";
            _evaluationInput = new EvaluationContainer();
            _lastEval = null;

            _plaintextOutput = "";
            _keyOutput = "";

            _keyCount = 0;
            _evaluationCount = 0;
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

    public class ExtendedEvaluationContainer : EvaluationContainer
    {
        /*
        // Key number - _keyCount
        // Seed - SeedInput
        // EvaluationInput
        // Key - KeyInput
        // Key space - ???
        // Plaintext - PlaintextInput
        // Ciphertext - ???
        // Best key - BestKeyInput
        // Best plaintext - BestPlaintextInput
        // % correct - percentCorrect
        // Necessary decryptions - decryptions
        // Runtime - runtime
        // Restarts - EvaluationInput.GetRestarts()
        // DecryptionsPerTimeUnit - decryptionsPerTimeUnit
        // Success probability - to be calculated!
        // Population size - EvaluationInput.GetPopulationSize()
        // Tabu set size - EvaluationInput.GetTabuSetSize()
        */

        private BigInteger _epochSeed;
        private int _keyNumber;
        private string _key;
        // private BigInteger _keySpace; ?
        private string _plaintext;
        private string _bestKey;
        private string _bestPlaintext;
        private double _minimalDecryption;
        private double _percentDecrypted;
        private bool _successful;
        // DecryptionsPerTimeUnit ? 

        ExtendedEvaluationContainer(EvaluationContainer e)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(e);
        }

        ExtendedEvaluationContainer(EvaluationContainer e, BigInteger epochSeed, 
            int keyNumber, string key, string plaintext, string bestKey,
            string bestPlaintext, double minimalDecryption, double percentDecrypted,
            bool successfull)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(e);
            this._epochSeed = epochSeed;
            this._keyNumber = keyNumber;
            this._key = key;
            this._plaintext = plaintext;
            this._bestKey = bestKey;
            this._bestPlaintext = bestPlaintext;
            this._minimalDecryption = minimalDecryption;
            this._percentDecrypted = percentDecrypted;
            this._successful = successfull;
        }

        public bool hasEpochSeed()
        {
            return this._epochSeed != null && this._epochSeed != 0;
        }

        public bool hasKeyNumber()
        {
            return this._keyNumber != null && this._keyNumber != 0;
        }

        public bool hasKey()
        {
            return !string.IsNullOrEmpty(this._key);
        }

        public bool hasPlaintext()
        {
            return !string.IsNullOrEmpty(this._plaintext);
        }

        public bool hasBestKey()
        {
            return !string.IsNullOrEmpty(this._bestKey);
        }

        public bool hasBestPlaintext()
        {
            return !string.IsNullOrEmpty(this._bestPlaintext);
        }

        public bool hasMinimalDecryption()
        {
            return this._minimalDecryption != null && this._minimalDecryption != 0;
        }

        public bool hasPercentDecrypted()
        {
            return this._percentDecrypted != null && this._percentDecrypted != 0;
        }

        public BigInteger GetEpochSeed()
        {
            if (hasEpochSeed())
                return this._epochSeed;

            return 0;
        }

        public int GetKeyNumber()
        {
            if (hasKeyNumber())
                return this._keyNumber;

            return 0;
        }

        public string GetKey()
        {
            if (hasKey())
                return this._key;

            return null;
        }

        public string GetPlaintext()
        {
            if (hasPlaintext())
                return this._plaintext;

            return null;
        }

        public string GetBestKey()
        {
            if (hasBestKey())
                return this._bestKey;

            return null;
        }

        public string GetBestPlaintext()
        {
            if (hasBestPlaintext())
                return this._bestPlaintext;

            return null;
        }

        public double GetMinimalDecryption()
        {
            if (hasMinimalDecryption())
                return this._minimalDecryption;

            return 0;
        }

        public double GetPercentDecrypted()
        {
            if (hasPercentDecrypted())
                return this._percentDecrypted;

            return 0;
        }

        public bool GetSuccessfull()
        {
            return this._successful;
        }
    }
}

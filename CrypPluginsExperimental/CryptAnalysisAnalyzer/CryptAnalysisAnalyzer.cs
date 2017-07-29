/*
   Copyright 2017 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Linq;
using System.Numerics;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Controls;
using CryptAnalysisAnalyzer.Properties;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Applied Information Security - University of Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CryptAnalysisAnalyzer.Properties.Resources", "CAAcaption", "CAAtooltip", "CryptAnalysisAnalyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CryptAnalysisAnalyzer : ICrypComponent
    {
        public CryptAnalysisAnalyzer()
        {
            // initialize the settings with the current instance to be able
            // to call methods in this instance from the settings
            _settings = new CryptAnalysisAnalyzerSettings(this);
        }

        #region Private Variables

        private readonly CryptAnalysisAnalyzerSettings _settings;

        private string _textInput;
        private string _seedInput;
        private string _plaintextInput;
        private string _ciphertextInput;
        private string _keyInput;
        private string _bestPlaintextInput;
        private string _bestKeyInput;
        private EvaluationContainer _evaluationInput;

        private bool _newPlaintext = false;
        private bool _newCiphertext = false;
        private bool _newKey = false;
        private bool _newBestPlaintext = false;
        private bool _newBestKey = false;
        private bool _newEvaluation = false;

        private string _plaintextOutput;
        private string _keyOutput;
        private string _gnuPlotScriptOutput;
        private string _gnuPlotDataOutput;
        private string _evaluationOutput;

        private int _keyCount = 0;
        private int _evaluationCount = 0;
        private int _totalKeysInput = 0;
        private int _progress;
        private Dictionary<int, ExtendedEvaluationContainer> _testRuns;

        private string NewLine = System.Environment.NewLine;
        private string _originalNumberDecimalSeparator;

        #endregion

        #region Evaluation Variables

        // count and helper variables
        private int _successCount;
        private double _decryptedCount;
        private double _decryptionsCount;
        private double _runtimeCount;
        private bool _noRuntime;
        private double _restarts;
        private bool _noRestarts;
        private double _populationSize;
        private bool _noPopulationSize;
        private double _tabuSetSize;
        private bool _noTabuSetSize;
        private string _testSeriesSeed;
        private double _normalizedAverageYValues;
        private double _lowestXValue;
        private double _highestXValue;
        private double _lowestYValue;
        private double _highestYValue;
        private double[] _xValuesArray;
        private double[] _yValuesArray;

        // evaluation key values
        private Dictionary<int, int> _keyLengths;
        private Dictionary<int, int> _ciphertextLengths;
        private Dictionary<double, int> _runtimes;

        // evaluation detailed values
        // key length
        private Dictionary<int, double> _successPerKeyLength;
        private Dictionary<int, double> _percentDecryptedPerKeyLength;
        private Dictionary<int, double> _decryptionsPerKeyLength;
        private Dictionary<int, double> _restartsPerKeyLength;
        private Dictionary<int, double> _tabuSizesPerKeyLength;
        private Dictionary<int, double> _populationSizesPerKeyLength;
        private Dictionary<int, double> _runtimePerKeyLength;
        // ciphertext length
        private Dictionary<int, double> _successPerCiphertextLength;
        private Dictionary<int, double> _percentDecryptedPerCiphertextLength;
        private Dictionary<int, double> _decryptionsPerCiphertextLength;
        private Dictionary<int, double> _restartsPerCiphertextLength;
        private Dictionary<int, double> _tabuSizesPerCiphertextLength;
        private Dictionary<int, double> _populationSizesPerCiphertextLength;
        private Dictionary<int, double> _runtimePerCiphertextLength;
        // runtime
        private Dictionary<double, double> _successPerRuntime;
        private Dictionary<double, double> _percentDecryptedPerRuntime;
        private Dictionary<double, double> _decryptionsPerRuntime;
        private Dictionary<double, double> _restartsPerRuntime;
        private Dictionary<double, double> _tabuSizesPerRuntime;
        private Dictionary<double, double> _populationSizesPerRuntime;
        // sorted
        IOrderedEnumerable<KeyValuePair<double, int>> _sortedRuntimes;

        // average values
        private double _successPercentage;
        private double _averagePercentDecrypted;
        private double _averageDecryptions;
        private double _averageRestarts;
        private double _averageTabuSetSize;
        private double _averagePopulationSize;
        private double _averageRuntime;

        #endregion

        #region GnuPlot Variables

        private string _evalMethod;
        private string _keyValue;
        private string _val1;
        private string _val2;
        private string _val3;
        private string _ylabel;

        #endregion

        #region Data Properties

        /// <summary>
        /// The input text from which the plaintexts are taken (in the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltipCaption")]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                this._textInput = value;
                OnPropertyChanged("TextInput");
            }
        }

        /// <summary>
        /// The seed which initializes the random number generator (in the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.InputData, "SeedInputCaption", "SeedInputTooltipCaption")]
        public string SeedInput
        {
            get { return this._seedInput; }
            set
            {
                this._seedInput = value;
                OnPropertyChanged("SeedInput");
            }
        }

        /// <summary>
        /// The current key (from the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.InputData, "KeyInputCaption", "KeyInputTooltipCaption", true)]
        public string KeyInput
        {
            get { return this._keyInput; }
            set
            {
                if (value != null && value != this._keyInput)
                {
                    this._keyInput = value;
                    this._newKey = true;
                    OnPropertyChanged("KeyInput");
                }
            }
        }

        /// <summary>
        /// The current plaintext (from the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.InputData, "PlaintextInputCaption", "PlaintextInputTooltipCaption", true)]
        public string PlaintextInput
        {
            get { return this._plaintextInput; }
            set
            {
                if (value != null && value != this._plaintextInput)
                {
                    this._plaintextInput = value;
                    this._newPlaintext = true;
                    OnPropertyChanged("PlaintextInput");
                }
            }
        }

        /// <summary>
        /// The current ciphertext (from the cryptographic component).
        /// </summary>
        [PropertyInfo(Direction.InputData, "CiphertextInputCaption", "CiphertextInputTooltipCaption", true)]
        public string CiphertextInput
        {
            get { return this._ciphertextInput; }
            set
            {
                if (value != null && value != this._ciphertextInput)
                {
                    this._ciphertextInput = value;
                    this._newCiphertext = true;
                    OnPropertyChanged("CiphertextInput");
                }
            }
        }

        /// <summary>
        /// The total number of keys (from the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.InputData, "TotalKeysInputCaption", "TotalKeysInputTooltipCaption", true)]
        public int TotalKeysInput
        {
            get { return this._totalKeysInput; }
            set
            {
                this._totalKeysInput = value;
                OnPropertyChanged("TotalKeysInput");
            }
        }

        /// <summary>
        /// The current best key (from the cryptanalytic component).
        /// </summary>
        [PropertyInfo(Direction.InputData, "BestKeyInputCaption", "BestKeyInputTooltipCaption")]
        public string BestKeyInput
        {
            get { return this._bestKeyInput; }
            set
            {
                if (value != null && value != this._bestKeyInput)
                {
                    this._bestKeyInput = value;
                    this._newBestKey = true;
                    OnPropertyChanged("BestKeyInput");
                }
            }
        }

        /// <summary>
        /// The current best plaintext (from the cryptanalytic component).
        /// </summary>
        [PropertyInfo(Direction.InputData, "BestPlaintextInputCaption", "BestPlaintextInputTooltipCaption")]
        public string BestPlaintextInput
        {
            get { return this._bestPlaintextInput; }
            set
            {
                if (value != null && value != this._bestPlaintextInput)
                {
                    this._bestPlaintextInput = value;
                    this._newBestPlaintext = true;
                    OnPropertyChanged("BestPlaintextInput");
                }
            }
        }

        /// <summary>
        /// The current evaluation container (from the cryptanalytic component).
        /// </summary>
        [PropertyInfo(Direction.InputData, "EvaluationInputCaption", "EvaluationInputTooltipCaption")]
        public EvaluationContainer EvaluationInput
        {
            get { return this._evaluationInput; }
            set
            {
                if (value != null && value != this._evaluationInput)
                {
                    this._evaluationInput = value;
                    OnPropertyChanged("EvaluationInput");

                    if (value.hasValueSet)
                        this._newEvaluation = true;
                }
            }
        }


        /// <summary>
        /// The next key trigger, using the last key (for the TestVectorGenerator).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "TriggerNextKeyCaption", "TriggerNextKeyTooltipCaption")]
        public string TriggerNextKey { get; set; }

        /// <summary>
        /// The current kez output (for the Cipher and CipherAnalyzer components).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "KeyOutputCaption", "KeyOutputTooltipCaption")]
        public string KeyOutput
        {
            get { return this._keyOutput; }
            set
            {
                this._keyOutput = value;
                OnPropertyChanged("KeyOutput");
            }
        }

        /// <summary>
        /// The current plaintext output (for the Cipher and CipherAnalyzer components).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "PlaintextOutputCaption", "PlaintextOutputTooltipCaption")]
        public string PlaintextOutput
        {
            get { return this._plaintextOutput; }
            set
            {
                this._plaintextOutput = value;
                OnPropertyChanged("PlaintextOutput");
            }
        }

        /// <summary>
        /// The minimal correct percentage to match the plaintext for success (for the CipherAnalyzer component).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MinimalCorrectPercentageCaption", "MinimalCorrectPercentageTooltipCaption")]
        public double MinimalCorrectPercentage
        {
            get { return _settings.CorrectPercentage; }
        }

        /// <summary>
        /// The evaluation output (for a text output).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "EvaluationOutputCaption", "EvaluationOutputTooltipCaption")]
        public string EvaluationOutput
        {
            get;
            set;
        }

        /// <summary>
        /// The GnuPlot script output (for a text output).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "GnuPlotScriptOutputCaption", "GnuPlotScriptOutputTooltipCaption")]
        public string GnuPlotScriptOutput
        {
            get;
            set;
        }

        /// <summary>
        /// The GnuPlot data output (for a text output).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "GnuPlotDataOutputCaption", "GnuPlotDataOutputTooltipCaption")]
        public string GnuPlotDataOutput
        {
            get;
            set;
        }

        #endregion

        #region Evaluation

        /// <summary> 
        /// Calculates the correctly decrypted percentage and the success.
        /// Creates the ExtendedEvaluationContainer and adds it to the dictionary.
        /// Resets the evaluation inputs.
        /// </summary>
        public void CollectEvaluationData()
        {
            // calculate the correctly decrypted percentage and the success
            double percentCorrect = _bestPlaintextInput.CalculateSimilarity(_plaintextInput) * 100;
            bool success = percentCorrect >= _settings.CorrectPercentage ? true : false;

            // create the ExtendedEvaluationContainer with the current values
            ExtendedEvaluationContainer testRun = new ExtendedEvaluationContainer(_evaluationInput,
                _seedInput, _keyCount, _keyInput, _plaintextInput, _ciphertextInput, _bestKeyInput, _bestPlaintextInput,
                _settings.CorrectPercentage, percentCorrect, success);

            // add the container to the test run dictionary with the ID as key
            _testRuns.Add(_evaluationInput.GetID(), testRun);

            // increase the evaluation counter
            _evaluationCount++;
        }

        /// <summary> 
        /// Sets or resets the Number Decimal Separator (using a dot for GnuPlot).
        /// </summary>
        private void SetNumberDecimalSeparator(bool reset)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();

            if (!reset)
            {
                // set dot (".") as Number Decimal Separator
                _originalNumberDecimalSeparator = customCulture.NumberFormat.NumberDecimalSeparator;
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
            }
            else if (_originalNumberDecimalSeparator != null)
            {
                customCulture.NumberFormat.NumberDecimalSeparator = _originalNumberDecimalSeparator;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        /// <summary> 
        /// Initializes all the variables for the evaluation.
        /// </summary>
        public void InitializeVariables()
        {
            // set dot (".") as Number Decimal Separator
            SetNumberDecimalSeparator(false);

            // count and helper variables
            _successCount = 0;
            _decryptedCount = 0;
            _decryptionsCount = 0;

            _runtimeCount = 0;
            _noRuntime = !_settings.CalculateRuntime;
            _restarts = 0;
            _noRestarts = false;
            _populationSize = 0;
            _noPopulationSize = false;
            _tabuSetSize = 0;
            _noTabuSetSize = false;
            _testSeriesSeed = "";

            // evaluation key values
            _keyLengths = new Dictionary<int, int>();
            _ciphertextLengths = new Dictionary<int, int>();
            _runtimes = new Dictionary<double, int>();

            // evaluation detailed values
            // key length
            _successPerKeyLength = new Dictionary<int, double>();
            _percentDecryptedPerKeyLength = new Dictionary<int, double>();
            _decryptionsPerKeyLength = new Dictionary<int, double>();
            _restartsPerKeyLength = new Dictionary<int, double>();
            _tabuSizesPerKeyLength = new Dictionary<int, double>();
            _populationSizesPerKeyLength = new Dictionary<int, double>();
            _runtimePerKeyLength = new Dictionary<int, double>();
            // ciphertext length
            _successPerCiphertextLength = new Dictionary<int, double>();
            _percentDecryptedPerCiphertextLength = new Dictionary<int, double>();
            _decryptionsPerCiphertextLength = new Dictionary<int, double>();
            _restartsPerCiphertextLength = new Dictionary<int, double>();
            _tabuSizesPerCiphertextLength = new Dictionary<int, double>();
            _populationSizesPerCiphertextLength = new Dictionary<int, double>();
            _runtimePerCiphertextLength = new Dictionary<int, double>();
            // runtime
            _successPerRuntime = new Dictionary<double, double>();
            _percentDecryptedPerRuntime = new Dictionary<double, double>();
            _decryptionsPerRuntime = new Dictionary<double, double>();
            _restartsPerRuntime = new Dictionary<double, double>();
            _tabuSizesPerRuntime = new Dictionary<double, double>();
            _populationSizesPerRuntime = new Dictionary<double, double>();
        }

        /// <summary> 
        /// Sets the value variables for the GnuPlot output generation
        /// </summary>
        public void SetGnuPlotVariables()
        {
            _val1 = "";
            _val2 = "";
            _val3 = "";

            // set GnuPlot output variables
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted)
            {
                _val1 = Resources.SuccessCaption;
                _val2 = Resources.Percent_Decrypted;
                _ylabel = "%";
                _evalMethod = Resources.Succ_PercDecr_;
            }
            else if (_settings.YAxis == YAxisPlot.success)
            {
                _val1 = Resources.SuccessCaption;
                _ylabel = "%";
                _evalMethod = Resources.Succ_;
            }
            else if (_settings.YAxis == YAxisPlot.percentDecrypted)
            {
                _val1 = Resources.Percent_Decrypted;
                _ylabel = "%";
                _evalMethod = Resources.PercDecr_;
            }

            if (_settings.Y2Axis == Y2AxisPlot.decryptions)
            {
                _val3 = Resources.Decryptions;
                _evalMethod += Resources.NoDecr_;
            }
            else if (_settings.Y2Axis == Y2AxisPlot.restarts)
            {
                _val3 = Resources.Restarts;
                _evalMethod += Resources.Rest_;
            }
            else if (_settings.Y2Axis == Y2AxisPlot.tabuSetSizes)
            {
                _val3 = Resources.Tabu_Set_Sizes;
                _evalMethod += Resources.Tabu_;
            }
            else if (_settings.Y2Axis == Y2AxisPlot.populationSizes)
            {
                _val3 = Resources.Population_Sizes;
                _evalMethod += Resources.Popu_;
            }
            else if (_settings.Y2Axis == Y2AxisPlot.runtime)
            {
                _val3 = Resources.Runtime;
                _evalMethod += Resources.Time_;
            }

            if (_settings.XAxis == XAxisPlot.ciphertextLength)
            {
                _evalMethod += Resources.PerCiphLen;
                _keyValue = Resources.Ciphertext_Length;
            }
            else if (_settings.XAxis == XAxisPlot.keyLength)
            {
                _evalMethod += Resources.PerKeyLen;
                _keyValue = Resources.Key_Length;
            }
            else if (_settings.XAxis == XAxisPlot.runtime)
            {
                _evalMethod += Resources.PerTime;
                _keyValue = Resources.Runtime;
            }

        }

        /// <summary> 
        /// Calculates the average values dependent on the base metrics ciphertext
        /// and key length and runtime. Triggers the evaluation and GnuPlot output generation.
        /// </summary>
        public void Evaluate()
        {
            // Initialize variables
            InitializeVariables();

            bool firstElement = true;
            // counting and sorting the data into the dictionaries
            foreach (KeyValuePair<int, ExtendedEvaluationContainer> entry in _testRuns)
            {
                // current test run values
                ExtendedEvaluationContainer testRun = entry.Value;
                int keyLength = testRun.GetKey().Length;
                int ciphertextLength = testRun.GetCiphertext().Length;
                int currentSuccess = 0;
                if (testRun.GetSuccessfull()) currentSuccess = 1;
                double currentlyDecrypted = testRun.GetPercentDecrypted();
                double decryptions = testRun.GetDecryptions();
                double currentRestarts = 0;
                double currentTabuSize = 0;
                double currentPopulationSize = 0;
                if (!_noRestarts)
                    currentRestarts = testRun.GetRestarts();
                if (!_noTabuSetSize)
                    currentTabuSize = testRun.GetTabuSetSize();
                if (!_noPopulationSize)
                    currentPopulationSize = testRun.GetPopulationSize();

                // get the seed of the whole test series only once
                if (firstElement)
                {
                    _testSeriesSeed = testRun.GetSeed();
                    firstElement = false;
                }

                // count the successful runs
                if (testRun.GetSuccessfull())
                    _successCount++;
                DictionaryExtention.AddOrIncrement<int>(_successPerKeyLength, keyLength, currentSuccess);
                DictionaryExtention.AddOrIncrement<int>(_successPerCiphertextLength, ciphertextLength, currentSuccess);

                // count the overall decryptions and decrypted percentages
                _decryptedCount += currentlyDecrypted;
                _decryptionsCount += decryptions;

                // count the decryptions and decrypted percentages per key and ciphertext lengths
                DictionaryExtention.AddOrIncrement<int>(_percentDecryptedPerKeyLength, keyLength, currentlyDecrypted);
                DictionaryExtention.AddOrIncrement<int>(_decryptionsPerKeyLength, keyLength, decryptions);
                DictionaryExtention.AddOrIncrement<int>(_percentDecryptedPerCiphertextLength, ciphertextLength, currentlyDecrypted);
                DictionaryExtention.AddOrIncrement<int>(_decryptionsPerCiphertextLength, ciphertextLength, decryptions);

                // count the restarts if every run contains a restart value greater zero
                if (!_noRestarts && currentRestarts > 0)
                {
                    _restarts += currentRestarts;
                    DictionaryExtention.AddOrIncrement(_restartsPerKeyLength, keyLength, currentRestarts);
                    DictionaryExtention.AddOrIncrement(_restartsPerCiphertextLength, ciphertextLength, currentRestarts);
                }
                else
                    _noRestarts = true;

                // count the tabu set size if every run contains a size value greater zero
                if (!_noTabuSetSize && currentTabuSize > 0)
                {
                    _tabuSetSize += currentTabuSize;
                    DictionaryExtention.AddOrIncrement(_tabuSizesPerKeyLength, keyLength, currentTabuSize);
                    DictionaryExtention.AddOrIncrement(_tabuSizesPerCiphertextLength, ciphertextLength, currentTabuSize);
                }
                else
                    _noTabuSetSize = true;

                // count the population size if every run contains a size value greater zero
                if (!_noPopulationSize && currentPopulationSize > 0)
                {
                    _populationSize += currentPopulationSize;
                    DictionaryExtention.AddOrIncrement(_populationSizesPerKeyLength, keyLength, currentPopulationSize);
                    DictionaryExtention.AddOrIncrement(_populationSizesPerCiphertextLength, ciphertextLength, currentPopulationSize);
                }
                else
                    _noPopulationSize = true;

                // count all values per runtime and the runtime per key and ciphertext lengths
                TimeSpan timeSpan;
                if (!_noRuntime && testRun.GetRuntime(out timeSpan))
                {
                    double time = timeSpan.TotalMilliseconds;
                    _runtimeCount += time;
                    // update key value dictionary runtimes
                    DictionaryExtention.AddOrIncrement(_runtimes, time, 1);

                    // detailed values
                    DictionaryExtention.AddOrIncrement(_successPerRuntime, time, currentSuccess);
                    DictionaryExtention.AddOrIncrement(_percentDecryptedPerRuntime, time, currentlyDecrypted);
                    DictionaryExtention.AddOrIncrement(_decryptionsPerRuntime, time, decryptions);
                    if (!_noRestarts)
                        DictionaryExtention.AddOrIncrement(_restartsPerRuntime, time, currentRestarts);
                    if (!_noTabuSetSize)
                        DictionaryExtention.AddOrIncrement(_tabuSizesPerRuntime, time, currentTabuSize);
                    if (!_noPopulationSize)
                        DictionaryExtention.AddOrIncrement(_populationSizesPerRuntime, time, currentPopulationSize);

                    DictionaryExtention.AddOrIncrement(_runtimePerKeyLength, keyLength, time);
                    DictionaryExtention.AddOrIncrement(_runtimePerCiphertextLength, ciphertextLength, time);
                }
                else
                {
                    _noRuntime = true;
                }

                // update key value dictionaries keyLengths and ciphertextLengths
                DictionaryExtention.AddOrIncrement(_keyLengths, keyLength, 1);
                DictionaryExtention.AddOrIncrement(_ciphertextLengths, ciphertextLength, 1);
            }

            // after counting all values, we calculate average values here

            // calculate the overall average values
            _successPercentage = Math.Round((double)_successCount / _testRuns.Count * 100, 2);
            _averagePercentDecrypted = Math.Round((double)_decryptedCount / _testRuns.Count, 2);
            _averageDecryptions = Math.Round((double)_decryptionsCount / _testRuns.Count, 2);

            // calculate the average runtime values
            if (!_noRuntime)
            {
                // calculate the overall average values
                _averageRuntime = _runtimeCount / _testRuns.Count;

                // if the current runtime count can be retrieved, calculate the average values
                foreach (var pair in _runtimes)
                {
                    double time = pair.Key;
                    int count = pair.Value;

                    // if the count is greater 1, we have to divide through count to get the average
                    if (count > 0)
                    {
                        // detailed values
                        DictionaryExtention.DivideAndRoundPercent<double>(_successPerRuntime, time, count, 2);
                        DictionaryExtention.DivideAndRound<double>(_percentDecryptedPerRuntime, time, count, 2);
                        DictionaryExtention.Divide<double>(_decryptionsPerRuntime, time, count);
                        if (!_noRestarts)
                            DictionaryExtention.Divide<double>(_restartsPerRuntime, time, count);
                        if (!_noTabuSetSize)
                            DictionaryExtention.Divide<double>(_tabuSizesPerRuntime, time, count);
                        if (!_noPopulationSize)
                            DictionaryExtention.Divide<double>(_populationSizesPerRuntime, time, count);
                    }
                }

                // sort the runtimes to display them in order in the plot
                _sortedRuntimes = from entry in _runtimes orderby entry.Key ascending select entry;
            }

            // calculate the overall average values
            _averageRestarts = 0;
            if (!_noRestarts)
                _averageRestarts = _restarts / _testRuns.Count;
            _averageTabuSetSize = 0;
            if (!_noTabuSetSize)
                _averageTabuSetSize = _tabuSetSize / _testRuns.Count;
            _averagePopulationSize = 0;
            if (!_noPopulationSize)
                _averagePopulationSize = _populationSize / _testRuns.Count;

            // if the current key length count can be retrieved, calculate the average values
            foreach (var pair in _keyLengths)
            {
                int keyLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 0)
                {
                    // calculate the detailed average values
                    DictionaryExtention.DivideAndRoundPercent<int>(_successPerKeyLength, keyLength, count, 2);
                    DictionaryExtention.DivideAndRound<int>(_percentDecryptedPerKeyLength, keyLength, count, 2);
                    DictionaryExtention.Divide<int>(_decryptionsPerKeyLength, keyLength, count);
                    DictionaryExtention.Divide<int>(_runtimePerKeyLength, keyLength, count);

                    if (!_noRestarts)
                        DictionaryExtention.Divide<int>(_restartsPerKeyLength, keyLength, count);

                    if (!_noTabuSetSize)
                        DictionaryExtention.Divide<int>(_tabuSizesPerKeyLength, keyLength, count);

                    if (!_noPopulationSize)
                        DictionaryExtention.Divide<int>(_populationSizesPerKeyLength, keyLength, count);

                }
            }

            // if the current ciphertext length count can be retrieved, calculate the average values
            foreach (var pair in _ciphertextLengths)
            {
                int ciphertextLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 0)
                {
                    // calculate the detailed average values
                    DictionaryExtention.DivideAndRoundPercent<int>(_successPerCiphertextLength, ciphertextLength, count, 2);
                    DictionaryExtention.DivideAndRound<int>(_percentDecryptedPerCiphertextLength, ciphertextLength, count, 2);
                    DictionaryExtention.Divide<int>(_decryptionsPerCiphertextLength, ciphertextLength, count);
                    DictionaryExtention.Divide<int>(_runtimePerCiphertextLength, ciphertextLength, count);

                    if (!_noRestarts)
                        DictionaryExtention.Divide<int>(_restartsPerCiphertextLength, ciphertextLength, count);

                    if (!_noTabuSetSize)
                        DictionaryExtention.Divide<int>(_tabuSizesPerCiphertextLength, ciphertextLength, count);

                    if (!_noPopulationSize)
                        DictionaryExtention.Divide<int>(_populationSizesPerCiphertextLength, ciphertextLength, count);
                }
            }

            // if the testing is done, build the evaluation output string
            if (_keyCount == TotalKeysInput)
                BuildEvaluationOutputString();

            // call GnuPlot generation methods
            SetGnuPlotVariables();
            GenerateGnuPlotDataOutput();
            GenerateGnuPlotScriptOutput();
        }

        /// <summary> 
        /// Generates the evaluation output string and sets the _evaluationOutput variable.
        /// </summary>
        public void BuildEvaluationOutputString()
        {
            // build the average runtime string
            string averageRuntimeString = "";
            if (!_noRuntime)
                averageRuntimeString = new DateTime(TimeSpan.FromMilliseconds(_averageRuntime).Ticks).ToString("HH:mm:ss:FFFF");

            // build the displayed string of occurring ciphertext lengths
            string ciphertextLengthString = "";
            int i = 0;
            foreach (var pair in _ciphertextLengths)
            {
                if (_ciphertextLengths.Count > 6 && i == 3)
                {
                    ciphertextLengthString += " ...";
                }
                else if (_ciphertextLengths.Count <= 6 || i < 3 || i >= _ciphertextLengths.Count - 3)
                {
                    if (ciphertextLengthString != "")
                        ciphertextLengthString += ", ";
                    ciphertextLengthString += pair.Key + " (" + pair.Value + ")";
                }
                i++;
            }

            // build the displayed string of occurring key lengths
            string keyLengthString = "";
            i = 0;
            foreach (var pair in _keyLengths)
            {
                if (_keyLengths.Count > 6 && i == 3)
                {
                    keyLengthString += " ...";
                }
                else if (_keyLengths.Count <= 6 || i < 3 || i >= _keyLengths.Count - 3)
                {
                    if (keyLengthString != "")
                        keyLengthString += ", ";
                    keyLengthString += pair.Key + " (" + pair.Value + ")";
                }
                i++;
            }

            // build the complete displayed evaluation output string
            _evaluationOutput = "";
            if (!string.IsNullOrEmpty(_testSeriesSeed))
                _evaluationOutput = Resources.Test_Series_Seed + _testSeriesSeed + "\r";
            if (!_noRuntime)
                _evaluationOutput += Resources.Average_runtime + averageRuntimeString + "\r";
            _evaluationOutput += Resources.Ciphertext_lengths + ciphertextLengthString + "\r";
            _evaluationOutput += Resources.Key_lengths__ + keyLengthString + "\r";
            _evaluationOutput += Resources.Average_decryptions_necessary__ + _averageDecryptions + "\r";
            if (!_noRestarts)
                _evaluationOutput += Resources.Average_restarts__ + _averageRestarts + "\r";
            if (!_noPopulationSize)
                _evaluationOutput += Resources.Average_population_size__ + _averagePopulationSize + "\r";
            if (!_noTabuSetSize)
                _evaluationOutput += Resources.Average_tabu_set_size__ + _averageTabuSetSize + "\r";
            _evaluationOutput += string.Format(Resources.Averagely_decrypted_of_min, _averagePercentDecrypted, _settings.CorrectPercentage) + "%\r";
            _evaluationOutput += Resources.Average_success__ + _successPercentage + "%\r";
        }

        /// <summary> 
        /// Generates the GnuPlot data output and sets the _gnuPlotDataOutput variable.
        /// </summary>
        public void GenerateGnuPlotDataOutput()
        {
            // generate the GnuPlot data output string
            // build a header to guide the user
            _gnuPlotDataOutput = "###########################################################" + NewLine;
            _gnuPlotDataOutput += "#" + Resources.Gnuplot_script_for_plotting_data_from_output_GnuPlotData + NewLine;
            _gnuPlotDataOutput += "# " + Resources.Save_the_GnuPlotData_output_in_a_file_named_ + NewLine;
            _gnuPlotDataOutput += "#" + NewLine;
            _gnuPlotDataOutput += "# --> '" + _evalMethod + ".dat'" + NewLine;
            _gnuPlotDataOutput += "#" + NewLine;
            _gnuPlotDataOutput += "# " + Resources.Save_the_GnuPlotScript_output_into_a_file_named_ + NewLine;
            _gnuPlotDataOutput += "# '" + _evalMethod + ".p'" + NewLine;
            _gnuPlotDataOutput += "# " + string.Format(Resources.Use__load__0__p__to_plot, _evalMethod) + NewLine;
            _gnuPlotDataOutput += "###########################################################" + NewLine;
            _gnuPlotDataOutput += NewLine;

            // # Data for evaluation method
            _gnuPlotDataOutput += "# " + Resources.Data_for__ + _evalMethod + NewLine;
            _gnuPlotDataOutput += NewLine;
            // write evaluation metrics in the file
            _gnuPlotDataOutput += "# " + _keyValue + "\t\t" + _val1;
            if (!String.IsNullOrEmpty(_val2))
                _gnuPlotDataOutput += "\t\t" + _val2;
            if (!String.IsNullOrEmpty(_val3))
                _gnuPlotDataOutput += "\t\t" + _val3;
            _gnuPlotDataOutput += NewLine;

            // reset the range values
            _lowestXValue = -1;
            _highestXValue = 0;
            _lowestYValue = -1;
            _highestYValue = 0;

            switch (_settings.XAxis)
            {
                case XAxisPlot.ciphertextLength:
                    _yValuesArray = new double[_ciphertextLengths.Count];
                    break;
                case XAxisPlot.keyLength:
                    _yValuesArray = new double[_keyLengths.Count];
                    break;
                case XAxisPlot.runtime:
                    _yValuesArray = new double[_runtimes.Count];
                    _xValuesArray = new double[_runtimes.Count];
                    break;
            }
            _normalizedAverageYValues = 0;

            // add the values for the selected metric
            if (_settings.XAxis == XAxisPlot.ciphertextLength)
                AddCiphertextLengthValues();
            else if (_settings.XAxis == XAxisPlot.keyLength)
                AddKeyLengthValues();
            else if (_settings.XAxis == XAxisPlot.runtime)
                if (!_noRuntime)
                    AddRuntimeValues();
                else {/* TODO: disable runtime in settings*/ }
        }

        /// <summary> 
        /// Generates the GnuPlot data output and sets the _gnuPlotDataOutput variable.
        /// </summary>
        public void AddCiphertextLengthValues()
        {
            int position = 0;
            // iterate over every pair per ciphertext length
            foreach (var pair in _ciphertextLengths)
            {
                int len = pair.Key;
                // Possible use for showing the number of texts per length
                int count = pair.Value;
                _gnuPlotDataOutput += len + "\t\t\t\t\t\t";

                if (_settings.YAxis == YAxisPlot.success ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentSuccess = 0;
                    if (!_successPerCiphertextLength.TryGetValue(len, out currentSuccess))
                    {
                        // Warning! But may be zero
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "successPerCiphertextLength", len, "currentSuccess") + currentSuccess);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentSuccess + "\t\t\t\t";
                }
                if (_settings.YAxis == YAxisPlot.percentDecrypted ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentDecryptedPercentage = 0;
                    if (!_percentDecryptedPerCiphertextLength.TryGetValue(len, out currentDecryptedPercentage))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_percentDecryptedPerCiphertextLength", len, "currentDecryptedPercentage") + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                // set the according values for the second y-axis
                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerCiphertextLength.TryGetValue(len, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_decryptionsPerCiphertextLength", len, "currentDecryptions") + currentDecryptions);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentDecryptions;

                        if (currentDecryptions > 0)
                        {
                            if (_lowestYValue == -1 || currentDecryptions < _lowestYValue)
                                _lowestYValue = currentDecryptions;
                            if (currentDecryptions > _highestYValue)
                                _highestYValue = currentDecryptions;
                            _yValuesArray[position] = currentDecryptions;
                            position++;
                        }
                    }

                }
                else if (_settings.Y2Axis == Y2AxisPlot.runtime && !_noRuntime)
                {
                    double currentRuntime = 0;
                    if (!_runtimePerCiphertextLength.TryGetValue(len, out currentRuntime))
                    {
                        // Warning!
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentRuntime;

                        if (currentRuntime > 0)
                        {
                            if (_lowestYValue == -1 || currentRuntime < _lowestYValue)
                                _lowestYValue = currentRuntime;
                            if (currentRuntime > _highestYValue)
                                _highestYValue = currentRuntime;
                            _yValuesArray[position] = currentRuntime;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.restarts && !_noRestarts)
                {
                    double currentRestarts = 0;
                    if (!_restartsPerCiphertextLength.TryGetValue(len, out currentRestarts))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_restartsPerCiphertextLength", len, "currentRestarts") + currentRestarts);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentRestarts;

                        if (currentRestarts > 0)
                        {
                            if (_lowestYValue == -1 || currentRestarts < _lowestYValue)
                                _lowestYValue = currentRestarts;
                            if (currentRestarts > _highestYValue)
                                _highestYValue = currentRestarts;
                            _yValuesArray[position] = currentRestarts;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.tabuSetSizes && !_noTabuSetSize)
                {
                    double currentTabu = 0;
                    if (!_tabuSizesPerCiphertextLength.TryGetValue(len, out currentTabu))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_tabuSizesPerCiphertextLength", len, "currentTabu") + currentTabu);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentTabu;

                        if (currentTabu > 0)
                        {
                            if (_lowestYValue == -1 || currentTabu < _lowestYValue)
                                _lowestYValue = currentTabu;
                            if (currentTabu > _highestYValue)
                                _highestYValue = currentTabu;
                            _yValuesArray[position] = currentTabu;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.populationSizes && !_noPopulationSize)
                {
                    double currentPopulatioin = 0;
                    if (!_populationSizesPerCiphertextLength.TryGetValue(len, out currentPopulatioin))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_populationSizesPerCiphertextLength", len, "currentPopulatioin") + currentPopulatioin);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentPopulatioin;

                        if (currentPopulatioin > 0)
                        {
                            if (_lowestYValue == -1 || currentPopulatioin < _lowestYValue)
                                _lowestYValue = currentPopulatioin;
                            if (currentPopulatioin > _highestYValue)
                                _highestYValue = currentPopulatioin;
                            _yValuesArray[position] = currentPopulatioin;
                            position++;
                        }
                    }
                }

                // add NewLine
                _gnuPlotDataOutput += NewLine;
            }
        }

        public void AddKeyLengthValues()
        {
            int position = 0;
            // iterate over every pair per key length
            foreach (var pair in _keyLengths)
            {
                int len = pair.Key;
                // Possible use for showing the number of texts per length
                int count = pair.Value;
                _gnuPlotDataOutput += len + "\t\t\t\t\t\t";

                if (_settings.YAxis == YAxisPlot.success ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentSuccess = 0;
                    if (!_successPerKeyLength.TryGetValue(len, out currentSuccess))
                    {
                        // Warning! But may be zero
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_successPerKeyLength", len, "currentSuccess") + currentSuccess);
                        //continue;
                    }
                    else
                        _gnuPlotDataOutput += currentSuccess + "\t\t\t\t";
                }
                if (_settings.YAxis == YAxisPlot.percentDecrypted ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentDecryptedPercentage = 0;
                    if (!_percentDecryptedPerKeyLength.TryGetValue(len, out currentDecryptedPercentage))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_percentDecryptedPerKeyLength", len, "currentDecryptedPercentage") + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                // set the according values for the second y-axis
                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerKeyLength.TryGetValue(len, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_decryptionsPerKeyLength", len, "currentDecryptions") + currentDecryptions);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentDecryptions;

                        if (currentDecryptions > 0)
                        {
                            if (_lowestYValue == -1 || currentDecryptions < _lowestYValue)
                                _lowestYValue = currentDecryptions;
                            if (currentDecryptions > _highestYValue)
                                _highestYValue = currentDecryptions;
                            _yValuesArray[position] = currentDecryptions;
                            position++;
                        }
                    }

                }
                else if (_settings.Y2Axis == Y2AxisPlot.runtime && !_noRuntime)
                {
                    double currentRuntime = 0;
                    if (!_runtimePerKeyLength.TryGetValue(len, out currentRuntime))
                    {
                        // Warning!
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentRuntime;

                        if (currentRuntime > 0)
                        {
                            if (_lowestYValue == -1 || currentRuntime < _lowestYValue)
                                _lowestYValue = currentRuntime;
                            if (currentRuntime > _highestYValue)
                                _highestYValue = currentRuntime;
                            _yValuesArray[position] = currentRuntime;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.restarts && !_noRestarts)
                {
                    double currentRestarts = 0;
                    if (!_restartsPerKeyLength.TryGetValue(len, out currentRestarts))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_restartsPerKeyLength", len, "currentRestarts") + currentRestarts);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentRestarts;

                        if (currentRestarts > 0)
                        {
                            if (_lowestYValue == -1 || currentRestarts < _lowestYValue)
                                _lowestYValue = currentRestarts;
                            if (currentRestarts > _highestYValue)
                                _highestYValue = currentRestarts;
                            _yValuesArray[position] = currentRestarts;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.tabuSetSizes && !_noTabuSetSize)
                {
                    double currentTabu = 0;
                    if (!_tabuSizesPerKeyLength.TryGetValue(len, out currentTabu))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_tabuSizesPerKeyLength", len, "currentTabu") + currentTabu);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentTabu;

                        if (currentTabu > 0)
                        {
                            if (_lowestYValue == -1 || currentTabu < _lowestYValue)
                                _lowestYValue = currentTabu;
                            if (currentTabu > _highestYValue)
                                _highestYValue = currentTabu;
                            _yValuesArray[position] = currentTabu;
                            position++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.populationSizes && !_noPopulationSize)
                {
                    double currentPopulatioin = 0;
                    if (!_populationSizesPerKeyLength.TryGetValue(len, out currentPopulatioin))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_populationSizesPerKeyLength", len, "currentPopulatioin") + currentPopulatioin);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentPopulatioin;

                        if (currentPopulatioin > 0)
                        {
                            if (_lowestYValue == -1 || currentPopulatioin < _lowestYValue)
                                _lowestYValue = currentPopulatioin;
                            if (currentPopulatioin > _highestYValue)
                                _highestYValue = currentPopulatioin;
                            _yValuesArray[position] = currentPopulatioin;
                            position++;
                        }
                    }
                }

                // add NewLine
                _gnuPlotDataOutput += NewLine;
            }
        }

        public void AddRuntimeValues()
        {
            int xPosition = 0;
            int yPosition = 0;
            // iterate over every pair per runtime
            foreach (var pair in _sortedRuntimes)
            {
                double time = pair.Key;
                // Possible use for showing the number of texts per length
                int count = pair.Value;
                _gnuPlotDataOutput += time + "\t\t\t\t\t\t";

                if (_lowestXValue == -1 || time < _lowestXValue)
                    _lowestXValue = time;
                if (time > _highestXValue)
                    _highestXValue = time;
                _xValuesArray[xPosition] = time;
                xPosition++;

                if (_settings.YAxis == YAxisPlot.success ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentSuccess = 0;
                    if (!_successPerRuntime.TryGetValue(time, out currentSuccess))
                    {
                        // Warning! But may be zero
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_successPerRuntime", time, "currentSuccess") + currentSuccess);
                        //continue;
                    }
                    else
                        _gnuPlotDataOutput += currentSuccess + "\t\t\t\t";
                }
                if (_settings.YAxis == YAxisPlot.percentDecrypted ||
                    _settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                {
                    double currentDecryptedPercentage = 0;
                    if (!_percentDecryptedPerRuntime.TryGetValue(time, out currentDecryptedPercentage))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_percentDecryptedPerRuntime", time, "currentDecryptedPercentage") + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                // set the according values for the second y-axis
                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerRuntime.TryGetValue(time, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_decryptionsPerRuntime", time, "currentDecryptions") + currentDecryptions);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentDecryptions;

                        if (currentDecryptions > 0)
                        {
                            if (_lowestYValue == -1 || currentDecryptions < _lowestYValue)
                                _lowestYValue = currentDecryptions;
                            if (currentDecryptions > _highestYValue)
                                _highestYValue = currentDecryptions;
                            _yValuesArray[yPosition] = currentDecryptions;
                            yPosition++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.runtime && !_noRuntime)
                {
                    _gnuPlotDataOutput += time;

                    if (time > 0)
                    {
                        if (_lowestYValue == -1 || time < _lowestYValue)
                            _lowestYValue = time;
                        if (time > _highestYValue)
                            _highestYValue = time;
                        _yValuesArray[yPosition] = time;
                        yPosition++;
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.restarts && !_noRestarts)
                {
                    double currentRestarts = 0;
                    if (!_restartsPerRuntime.TryGetValue(time, out currentRestarts))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_restartsPerRuntime", time, "currentRestarts") + currentRestarts);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentRestarts;

                        if (currentRestarts > 0)
                        {
                            if (_lowestYValue == -1 || currentRestarts < _lowestYValue)
                                _lowestYValue = currentRestarts;
                            if (currentRestarts > _highestYValue)
                                _highestYValue = currentRestarts;
                            _yValuesArray[yPosition] = currentRestarts;
                            yPosition++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.tabuSetSizes && !_noTabuSetSize)
                {
                    double currentTabu = 0;
                    if (!_tabuSizesPerRuntime.TryGetValue(time, out currentTabu))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_tabuSizesPerRuntime", time, "currentTabu") + currentTabu);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentTabu;

                        if (currentTabu > 0)
                        {
                            if (_lowestYValue == -1 || currentTabu < _lowestYValue)
                                _lowestYValue = currentTabu;
                            if (currentTabu > _highestYValue)
                                _highestYValue = currentTabu;
                            _yValuesArray[yPosition] = currentTabu;
                            yPosition++;
                        }
                    }
                }
                else if (_settings.Y2Axis == Y2AxisPlot.populationSizes && !_noPopulationSize)
                {
                    double currentPopulatioin = 0;
                    if (!_populationSizesPerRuntime.TryGetValue(time, out currentPopulatioin))
                    {
                        // Warning!
                        Console.WriteLine(string.Format(Resources.TryGetValue_from__0__failed__ciphertextLength___1____2, "_populationSizesPerRuntime", time, "currentPopulatioin") + currentPopulatioin);
                        //continue;
                    }
                    else
                    {
                        _gnuPlotDataOutput += currentPopulatioin;

                        if (currentPopulatioin > 0)
                        {
                            if (_lowestYValue == -1 || currentPopulatioin < _lowestYValue)
                                _lowestYValue = currentPopulatioin;
                            if (currentPopulatioin > _highestYValue)
                                _highestYValue = currentPopulatioin;
                            _yValuesArray[yPosition] = currentPopulatioin;
                            yPosition++;
                        }
                    }
                }

                // add NewLine
                _gnuPlotDataOutput += NewLine;
            }
        }

        /// <summary> 
        /// Generates the GnuPlot script output and sets the _gnuPlotScriptOutput variable.
        /// </summary>
        public void GenerateGnuPlotScriptOutput()
        {
            // generate the GnuPlot script output string
            // build a header to guide the user
            _gnuPlotScriptOutput = "###########################################################" + NewLine;
            _gnuPlotScriptOutput += "# " + Resources.Gnuplot_script_for_plotting_data_from_output_GnuPlotData + NewLine;
            _gnuPlotScriptOutput += "# " + Resources.Save_the_GnuPlotScript_output_into_a_file_named_ + NewLine;
            _gnuPlotScriptOutput += "#" + NewLine;
            _gnuPlotScriptOutput += "# --> '" + _evalMethod + ".p'" + NewLine;
            _gnuPlotScriptOutput += "#" + NewLine;
            _gnuPlotScriptOutput += "# " + Resources.Save_the_GnuPlotData_output_in_a_file_named_ + NewLine;
            _gnuPlotScriptOutput += "# '" + _evalMethod + ".dat'" + NewLine;
            _gnuPlotScriptOutput += "# " + string.Format(Resources.Use__load__0__p__to_plot, _evalMethod) + NewLine;
            _gnuPlotScriptOutput += "###########################################################" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # General settings
            _gnuPlotScriptOutput += "# " + Resources.General_settings + NewLine;
            _gnuPlotScriptOutput += "set autoscale\t\t\t\t\t# -- scale axes automatically" + NewLine;
            _gnuPlotScriptOutput += "unset log\t\t\t\t\t\t# -- remove any log-scaling" + NewLine;
            _gnuPlotScriptOutput += "unset tics\t\t\t\t\t\t# -- remove any previous tics" + NewLine;
            _gnuPlotScriptOutput += "unset xlabel\t\t\t\t\t# -- remove previous labels" + NewLine;
            _gnuPlotScriptOutput += "unset ylabel" + NewLine;
            _gnuPlotScriptOutput += "unset y2label" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # Style settings
            _gnuPlotScriptOutput += "# " + Resources.Style_settings + NewLine;
            _gnuPlotScriptOutput += "set style line 1 lc rgb '#2ca25f' lt 1 lw 2 pt 1 ps 0.8   # -- green" + NewLine;
            _gnuPlotScriptOutput += "set style line 2 lc rgb '#0060ad' lt 1 lw 2 pt 7 ps 0.8   # -- blue" + NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + NewLine;
            _gnuPlotScriptOutput += "set style line 4 lc rgb '#edb120' lt 1 lw 2               # -- orange" + NewLine;
            _gnuPlotScriptOutput += "set style line 101 lc rgb '#656565' lt 1 lw 1             # -- dark-grey" + NewLine;
            _gnuPlotScriptOutput += "set style line 102 lc rgb '#d6d7d9' lt 0 lw 1             # -- grey" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # Plot settings
            _gnuPlotScriptOutput += "# " + Resources.Plot_settings + NewLine;
            _gnuPlotScriptOutput += "set title \"" + _val1;
            if (!String.IsNullOrEmpty(_val2) && !String.IsNullOrEmpty(_val3))
                _gnuPlotScriptOutput += ", " + _val2 + ", and " + _val3;
            else if (!String.IsNullOrEmpty(_val2))
                _gnuPlotScriptOutput += " and " + _val2;
            else if (!String.IsNullOrEmpty(_val3))
                _gnuPlotScriptOutput += " and " + _val3;
            _gnuPlotScriptOutput += " per " + _keyValue + "\"" + NewLine;
            _gnuPlotScriptOutput += "set size ratio 0.8" + NewLine;
            int border = 3; // 11 = |__|, 3 = |__
            if (_settings.Y2Axis != Y2AxisPlot.none) border = 11;
            _gnuPlotScriptOutput += "set border " + border + " front ls 101" + NewLine;
            _gnuPlotScriptOutput += "set tics nomirror out scale 0.75" + NewLine;
            _gnuPlotScriptOutput += "set format '%g'" + NewLine;
            _gnuPlotScriptOutput += "set grid back ls 102" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # x-Axis settings
            _gnuPlotScriptOutput += "# " + Resources.x_Axis_settings + NewLine;
            // if the x-axis value is runtime and runtime in enabled
            if (_settings.XAxis == XAxisPlot.runtime && !_noRuntime)
            {
                // calculate normalized average y-values
                double normalizedAverageXValues = CalculateNormalizedAverage(_xValuesArray, _settings.NormalizingFactor);

                // calculate new graph ranges
                int min = CalculateMinValue(_lowestXValue, normalizedAverageXValues, "x");
                int max = CalculateMaxValue(_lowestXValue, _highestXValue, normalizedAverageXValues, "x");

                // equality means both values are 0, so skip this line in that case
                if (min != max)
                    _gnuPlotScriptOutput += "set xrange [" + min + ":" + max + "]" + NewLine;
            }
            _gnuPlotScriptOutput += "set xtic auto\t\t\t\t\t# -- set xtics automatically" + NewLine;
            _gnuPlotScriptOutput += "set xlabel \"" + _keyValue + "\"" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # y-axis settings
            _gnuPlotScriptOutput += "# " + Resources.y_Axis_settings + NewLine;
            // if the y-axis value is a value in percent
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted ||
                _settings.YAxis == YAxisPlot.percentDecrypted ||
                _settings.YAxis == YAxisPlot.success)
            {
                // depending on how many graphs and therefore lines in the legend are, we leave
                // some space above the graph to move it beneath the legend
                int percentUpper = 110;
                if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted)
                    percentUpper += 14;
                else
                    percentUpper += 7;

                if (_settings.Y2Axis != Y2AxisPlot.none)
                    percentUpper += 7;
                _gnuPlotScriptOutput += "set yrange [-5:" + percentUpper + "]" + NewLine;  // to gain some space below 0% and above 100%
                _gnuPlotScriptOutput += "set ytics (0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)" + NewLine;
            }
            else
            {
                // not percent, set yrange and ytics (not yet selectable in the UI)
            }
            _gnuPlotScriptOutput += "set ylabel \"" + _ylabel + "\"" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # second y-axis settings
            // make sure the second y-axis is enabled and the value exists
            if (_settings.Y2Axis != Y2AxisPlot.none &&
                !(_settings.Y2Axis == Y2AxisPlot.runtime && _noRuntime) &&
                !(_settings.Y2Axis == Y2AxisPlot.restarts && _noRestarts) &&
                !(_settings.Y2Axis == Y2AxisPlot.tabuSetSizes && _noTabuSetSize) &&
                !(_settings.Y2Axis == Y2AxisPlot.populationSizes && _noPopulationSize))
            {
                _gnuPlotScriptOutput += "# " + Resources.second_y_Axis_settings + NewLine;
                _gnuPlotScriptOutput += "set y2tic scale 0.75" + NewLine;
                _gnuPlotScriptOutput += "set y2label \"" + _val3 + "\"" + NewLine;

                // calculate normalized average y-values
                _normalizedAverageYValues = CalculateNormalizedAverage(_yValuesArray, _settings.NormalizingFactor);

                // calculate new graph ranges
                int min = CalculateMinValue(_lowestYValue, _normalizedAverageYValues, "y2");
                int max = CalculateMaxValue(_lowestYValue, _highestYValue, _normalizedAverageYValues, "y2");

                // format the runtime before plotting
                if (_settings.Y2Axis == Y2AxisPlot.runtime)
                {
                    //TODO: DateTime(_averageRuntime.Ticks).ToString("HH:mm:ss:FFFF");
                }

                // equality means both values are 0, so skip this line in that case
                if (min != max)
                    _gnuPlotScriptOutput += "set y2range [" + min + ":" + max + "]" + NewLine;
                _gnuPlotScriptOutput += NewLine;
            }

            // # plotting
            // set the line style
            int style = 1;
            if (_settings.YAxis == YAxisPlot.percentDecrypted)
                style = 2;
            int column = 3;

            // add the plotting lines to the script
            _gnuPlotScriptOutput += "# " + 
                Resources.plotting + NewLine;
            _gnuPlotScriptOutput += "plot    \"" + _evalMethod + ".dat\" using 1:2 title '" + _val1 + "' with linespoints ls " + style;
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted)
            {
                _gnuPlotScriptOutput += " , \\" + NewLine + "        \"" + _evalMethod + ".dat\" using 1:3 title '" + _val2 + "' with linespoints ls 2" + NewLine;
                column++;
            }

            // if the second y-axis is enabled
            if (_settings.Y2Axis != Y2AxisPlot.none)
            {
                _gnuPlotScriptOutput += "replot  \"" + _evalMethod + ".dat\" using 1:"+column+" title '" + _val3 + "' with linespoints ls 3 axes x1y2";
                
                // if the average of the second y-axis is activated
                if (_settings.ShowY2Average)
                {
                    _gnuPlotScriptOutput += " , \\" + NewLine;
                    _gnuPlotScriptOutput += "        " + Math.Round(_normalizedAverageYValues) + " title 'Average "+ _val3+" = " + Math.Round(_normalizedAverageYValues) + "' with lines ls 4 axes x1y2";

                }
                else
                    _gnuPlotScriptOutput += NewLine;
            }
        }

        /// <summary> 
        /// Calculates the new minimum value of the GnuPlot graph range.
        /// <param name="lowest">The lowest value of the graph</param>
        /// <param name="avg">The average value of the graph</param>
        /// <param name="axis">The axis whose range is to be calculated</param>
        /// <returns>The range minimum of the plot</returns>
        /// </summary>
        public int CalculateMinValue(double lowest, double avg, string axis)
        {
            // initially set the minimum to the lowest value
            int min = (int)lowest;

            // if the axis is the axis is the second y-axis and its value is decryptions
            // calculate the minimum with this formula
            if (_settings.Y2Axis == Y2AxisPlot.decryptions && axis.Equals("y2"))
                if (avg * 0.03 < 10)
                    min -= 10;
                else
                    min -= (int) (avg * 0.03);
            // if the axis is the axis is the second y-axis and its value is runtime OR
            // if the axis is the x-axis, calculate the minimum with this formula
            else if ((_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                (axis.Equals("x")))
                if (avg * 0.03 < 5)
                    min -= 5;
                else
                    min -= (int)(avg * 0.03);
            // if the axis is the axis is the second y-axis and its value is restarts
            // calculate the minimum with this formula
            else if (_settings.Y2Axis == Y2AxisPlot.restarts && axis.Equals("y2"))
            {
                if (avg * 0.03 < 2)
                    min -= 2;
                else
                    min -= (int) (avg * 0.03);
            }

            return min;
        }

        /// <summary> 
        /// Calculates the new maximum value of the GnuPlot graph range.
        /// <param name="lowest">The lowest value of the graph</param>
        /// <param name="highest">The highest value of the graph</param>
        /// <param name="avg">The average value of the graph</param>
        /// <param name="axis">The axis whose range is to be calculated</param>
        /// <returns>The range maximum of the plot</returns>
        /// </summary>
        public int CalculateMaxValue(double lowest, double highest, double avg, string axis)
        {
            // initially set the maximum to the highest value
            int max = (int)highest;

            // if the axis is the axis is the second y-axis and its value is runtime OR
            // if the axis is the y-axis, modify the max value before the actual calculation
            if ((_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                 axis.Equals("x"))
                if (avg * 0.3 < 2)
                    max += 2;
                else
                    max += (int)(avg * 0.3);

            // if the axis is the axis is the second y-axis and its value is runtime or decryptions OR
            // if the axis is the y-axis, calculate the maximum with this formula
            if (_settings.Y2Axis == Y2AxisPlot.decryptions ||
                (_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                axis.Equals("x"))
            {
                // calculate distances to mean (average)
                int lowestToMean = (int)avg - (int)lowest;
                int highestToMean = (int)highest - (int)avg;

                if (highestToMean > lowestToMean * 2)
                    max = (int)(avg + lowestToMean * 2);
            }

            return max;
        }

        /// <summary> 
        /// This method sets the EvaluationOutput and triggers RefreshGnuPlotOutputs().
        /// </summary>
        public void RefreshEvaluationOutputs()
        {
            EvaluationOutput = _evaluationOutput;
            OnPropertyChanged("EvaluationOutput");

            RefreshGnuPlotOutputs();
        }

        /// <summary> 
        /// This method sets the GnuPlot outputs and triggers the Property Changed Listener.
        /// Additionally, the Number Decimal Separator is reset to comma or dot.
        /// </summary>
        public void RefreshGnuPlotOutputs()
        {
            GnuPlotScriptOutput = _gnuPlotScriptOutput;
            OnPropertyChanged("GnuPlotScriptOutput");

            GnuPlotDataOutput = _gnuPlotDataOutput;
            OnPropertyChanged("GnuPlotDataOutput");

            // reset Number Decimal Separator
            SetNumberDecimalSeparator(true);
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
            _evaluationOutput = "";
            _gnuPlotDataOutput = "";
            _gnuPlotScriptOutput = "";
            RefreshEvaluationOutputs();
        }

        /// <summary>
        /// Check if the key and plaintext are set correctly. Warn if the seed is missing.
        /// </summary>
        public bool checkVariables()
        {
            if (String.IsNullOrEmpty(PlaintextInput) &&
                (String.IsNullOrEmpty(KeyInput) || KeyInput.Length == 0))
            {
                if (String.IsNullOrEmpty(KeyInput) || KeyInput.Length == 0)
                {
                    // TESTING!!!
                    //KeyInput = "KEYWORDX";

                    GuiLogMessage(Resources.The_key_input_is_empty_, NotificationLevel.Error);
                    return false;
                }

                if (String.IsNullOrEmpty(PlaintextInput))
                {
                    GuiLogMessage(Resources.The_plaintext_input_is_empty_, NotificationLevel.Error);
                    return false;
                }
            }

            if (String.IsNullOrEmpty(SeedInput))
            {
                GuiLogMessage(Resources.The_seed_input_is_empty__It_is_required_for_logging_purposes_, NotificationLevel.Warning);
            }

            return true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // check if the variables are set
            if (!checkVariables())
            {
                return;
            }

            // If both plaintext and key are new,
            // send them to the output
            if (_newKey && _newPlaintext)
            {
                // consume new values
                _newKey = false;
                _newPlaintext = false;

                if (_keyCount == 0)
                    _testRuns = new Dictionary<int, ExtendedEvaluationContainer>();
                
                // increase key counter
                _keyCount++;

                // update the progress bar
                _progress = (int)Math.Round((double)_keyCount / _totalKeysInput * 100);

                // visualize the evaluation process through the EvaluationOutput
                if (String.IsNullOrEmpty(EvaluationOutput))
                    EvaluationOutput = _keyCount + " / " + _totalKeysInput + NewLine +
                        "0%" + NewLine + NewLine + Resources.Current_key_number__ + _keyCount;
                else
                    EvaluationOutput += NewLine + Resources.Current_key_number__ + _keyCount + " / " + _totalKeysInput;
                    
                OnPropertyChanged("EvaluationOutput");

                // Send the plaintext and key (and min correct percentage) to the encryption method
                OnPropertyChanged("MinimalCorrectPercentage");
                
                // generate intermediate GnuPlot output on plaintext length change
                if (!String.IsNullOrEmpty(PlaintextOutput) &&
                    !String.IsNullOrEmpty(KeyOutput) &&
                    PlaintextOutput.Length != PlaintextInput.Length)
                {
                    Evaluate();

                    RefreshGnuPlotOutputs();
                }

                // sending the input values to the outputs
                PlaintextOutput = PlaintextInput;
                KeyOutput = KeyInput;

                // update the progress bar
                if (_totalKeysInput > 0)
                    ProgressChanged(_keyCount-0.9, _totalKeysInput);
            }
            // Wait for the analysis method to send evaluation data.
            // If the evaluation input is set, together with the best key
            // and best plaintext, do the evaluation for that calculation
            else if (_newEvaluation && _newBestKey && _newBestPlaintext && _newCiphertext &&
                _keyCount <= _totalKeysInput &&
                BestKeyInput != " " &&
                BestPlaintextInput != " ")
            {
                //System.Console.Write("State 2: _newEvaluation & _newBestKey & _newBestPlaintext" + NewLine);
                // consume new values
                _newEvaluation = false;
                _newBestKey = false;
                _newBestPlaintext = false;
                _newCiphertext = false;

                // generate some output infos for the user
                EvaluationOutput = "";
                for (int i = 0; i < (int)_progress / 5; i++)
                    EvaluationOutput += "█";

                EvaluationOutput += " " + _progress + "%" + NewLine + NewLine +
                    Resources.key_number__ + _keyCount + " / " +
                    _totalKeysInput + " - " + Resources.Done + ".";

                EvaluationOutput += NewLine + Resources.ID + ": " + EvaluationInput.GetID() + NewLine;
                if (_settings.CalculateRuntime)
                {
                    TimeSpan time;
                    EvaluationInput.GetRuntime(out time);
                    EvaluationOutput += Resources.Last_runtime + ": " + time.ToString() + NewLine;
                }
                EvaluationOutput += Resources.Last_number_of_restarts + ": " + EvaluationInput.GetRestarts() + NewLine +
                Resources.Last_number_of_decryptions + ": " + _evaluationInput.GetDecryptions();
                // gather all available evaluation data
                CollectEvaluationData();

                // trigger next key if key count less than total keys...
                if (_totalKeysInput > 0 &&
                    _keyCount < _totalKeysInput)
                {
                    OnPropertyChanged("EvaluationOutput");

                    TriggerNextKey = KeyInput;
                    OnPropertyChanged("TriggerNextKey");
                }
                else
                {
                    // ...evaluate if not
                    EvaluationOutput += NewLine + Resources.Started_evaluating + "...";
                    OnPropertyChanged("EvaluationOutput");
                    
                    Evaluate();
                    RefreshEvaluationOutputs();
                }

                // update the progress bar
                if (_totalKeysInput > 0)
                    ProgressChanged(_keyCount, _totalKeysInput);
                else
                    ProgressChanged(1, 1);
            }
            else /*if (!_newKey && !_newPlaintext)*/
            {
                // debug output
                System.Console.Write("_newKey: " + _newKey + ", _newPlaintext: " + _newPlaintext + ", _newEvaluation: " + _newEvaluation + ", _newBestKey: " +
                _newBestKey + ", _newBestPlaintext: " + _newBestPlaintext + NewLine);
                
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// Emptying the evaluation variables.
        /// </summary>
        public void PostExecution()
        {
            _plaintextInput = "";
            _keyInput = "";
            _bestPlaintextInput = "";
            _bestKeyInput = "";
            _evaluationInput = new EvaluationContainer();

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

        #region General Methods

        /// <summary> 
        /// This method is checking the difference between each value in the array
        /// and its direct neighbour. If the difference is bigger than the given
        /// factor, the big value is removed before the new normalized average
        /// value of the array is calculated and returned.
        /// <param name="arr">The array to calculate the normalized average of</param>
        /// <param name="normalizingFactor">The factor for the minimum size difference</param>
        /// <returns>The normalized average value of the given array</returns>
        /// </summary>
        public double CalculateNormalizedAverage(double[] arr, int normalizingFactor)
        {
            // sort the array first
            Array.Sort(arr);

            // calculate the standard average
            double avg = 0;
            foreach (double val in arr)
                avg += val;
            avg /= arr.Length;

            // add all irrepresentable high values to the list
            List<double> irrepresentableValues = new List<double>();
            for (int j = 0; j < arr.Length - 1; j++)
            {
                if (arr[j] < avg)
                    continue;

                // check if one decryptions value is x times bigger than the next value
                // (4 seems to be a good factor for runtime and decryptions)
                if (arr[j] * normalizingFactor < arr[j + 1])
                {
                    for (int i = j + 1; i < arr.Length; i++)
                        irrepresentableValues.Add(arr[i]);
                    break;
                }
            }

            // if there are were no high values found, return the standard average
            if (irrepresentableValues.Count == 0)
                return avg;

            // add up the irrepresentable values
            double IrrepresentableSum = 0;
            foreach (double val in irrepresentableValues)
                IrrepresentableSum += val;

            // calculate and return the normalized average by subtracting the sum of the irrepresentable
            // values from the total sum and subtracting their count from the total count, before
            // calculating the average and return the result
            return (avg * arr.Length - IrrepresentableSum) / (arr.Length - irrepresentableValues.Count);
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

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }

    /// <summary> 
    /// Implements the ExtendedEvaluationContainer as an extension of the
    /// EvaluationContainer, adding the test series seed, the consecutive
    /// key number, the key, the plaintext, the best key, the best plaintext,
    /// the minimal necessary decryption percentage, the actual percentage,
    /// and the success of the test run.
    /// <param name="container">The standard evaluation container</param>
    /// <param name="seed">The initial seed of the whole test series</param>
    /// <param name="keyNumber">The consecutive number of the key</param>
    /// <param name="key">The key from the generator</param>
    /// <param name="plaintext">The plaintext from the generator</param>
    /// <param name="bestKey">The best key from the cryptanalytic component</param>
    /// <param name="bestPlaintext">The best plaintext from the cryptanalytic component</param>
    /// <param name="minimalDecryption">The minimum necessary decryption percentage</param>
    /// <param name="percentDecrypted">The percent decrypted the cryptanalytic component</param>
    /// <param name="successfull">The success of the test run</param>
    /// <returns>ExtendedEvaluationContainer</returns>
    /// </summary>
    public class ExtendedEvaluationContainer : EvaluationContainer
    {
        private string _testSeriesSeed;
        private int _keyNumber;
        private string _key;
        private string _plaintext;
        private string _ciphertext;
        private string _bestKey;
        private string _bestPlaintext;
        private double _minimalDecryption;
        private double _percentDecrypted;
        private bool _successful;

        // constructor to set the standard EvaluationContainer
        public ExtendedEvaluationContainer(EvaluationContainer e)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(e);
        }

        // constructor to set all evaluation values directly
        public ExtendedEvaluationContainer(EvaluationContainer container, string seed,
            int keyNumber, string key, string plaintext, string ciphertext, string bestKey,
            string bestPlaintext, double minimalDecryption, double percentDecrypted,
            bool successfull)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(container);
            this._testSeriesSeed = seed;
            this._keyNumber = keyNumber;
            this._key = key;
            this._plaintext = plaintext;
            this._ciphertext = ciphertext;
            this._bestKey = bestKey;
            this._bestPlaintext = bestPlaintext;
            this._minimalDecryption = minimalDecryption;
            this._percentDecrypted = percentDecrypted;
            this._successful = successfull;
        }

        public bool hasSeed()
        {
            return !string.IsNullOrEmpty(this._testSeriesSeed);
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

        public bool hasCiphertext()
        {
            return !string.IsNullOrEmpty(this._ciphertext);
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

        public string GetSeed()
        {
            if (hasSeed())
                return this._testSeriesSeed;

            return null;
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

        public string GetCiphertext()
        {
            if (hasCiphertext())
                return this._ciphertext;

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

    /// <summary> 
    /// Extends the dictionary by AddOrIncrement and multiple Divide methods.
    /// Updates are done in place.
    /// <param name="dict">The dictionary to increment or divide in</param>
    /// <param name="key">The key of the value to update</param>
    /// <param name="newValue">The value to increment or dicide by</param>
    /// <returns></returns>
    /// </summary>
    public static class DictionaryExtention
    {

        // either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, int> dict, K key, int newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, double> dict, K key, int newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, double> dict, K key, double newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // try to devide
        public static bool Divide<K>(this Dictionary<K, int> dict, K key, int divide)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = dict[key] / divide;
                return true;
            }
            return false;
        }

        // try to devide
        public static bool Divide<K>(this Dictionary<K, double> dict, K key, int divide)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = dict[key] / divide;
                return true;
            }
            return false;
        }

        // try to devide and round
        public static bool DivideAndRound<K>(this Dictionary<K, int> dict, K key, int divide, int round)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = (int)Math.Round((double)dict[key] / divide, round);
                return true;
            }
            return false;
        }

        // try to devide and round
        public static bool DivideAndRound<K>(this Dictionary<K, double> dict, K key, int divide, int round)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = Math.Round((double)dict[key] / divide, round);
                return true;
            }
            return false;
        }

        // try to devide and round
        public static bool DivideAndRoundPercent<K>(this Dictionary<K, double> dict, K key, int divide, int round)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = Math.Round(dict[key] / divide * 100, round);
                return true;
            }
            return false;
        }
    }

    /// <summary> 
    /// Contains methods to calculate the levenshtein distance and the
    /// percentage of similarity of two strings.
    /// </summary>
    public static class SimilarityExtensions
    {
        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        public static int ComputeLevenshteinDistance(this string source, string target)
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
        public static double CalculateSimilarity(this string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 1 : 0;

            if (string.IsNullOrEmpty(target))
                return string.IsNullOrEmpty(source) ? 1 : 0;

            double stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - (stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}

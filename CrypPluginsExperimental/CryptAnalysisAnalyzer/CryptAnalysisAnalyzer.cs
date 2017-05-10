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
using System.Linq;
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

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Applied Information Security - University of Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("CryptAnalysisAnalyzer", "Analyse CryptAnalysis methods", "CryptAnalysisAnalyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class CryptAnalysisAnalyzer : ICrypComponent
    {
        public CryptAnalysisAnalyzer()
        {
            _settings = new CryptAnalysisAnalyzerSettings(this);
        }

        #region Private Variables

        private readonly CryptAnalysisAnalyzerSettings _settings;

        private string _textInput;
        private string _seedInput;
        private string _plaintextInput;
        private string _keyInput;
        private string _bestPlaintextInput;
        private string _bestKeyInput;
        private EvaluationContainer _evaluationInput;

        private string _plaintextOutput;
        private string _keyOutput;
        private string _gnuPlotScriptOutput;
        private string _gnuPlotDataOutput;
        private string _evaluationOutput;

        private int _keyCount = 0;
        private int _evaluationCount = 0;
        private int _totalKeysInput = 0;
        private int _progress;
        private EvaluationContainer _lastEval;
        private Dictionary<int, ExtendedEvaluationContainer> _testRuns;

        private string NewLine = System.Environment.NewLine;

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
            get { return this._seedInput; }
            set
            {
                this._seedInput = value;
                OnPropertyChanged("SeedInput");
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

        [PropertyInfo(Direction.OutputData, "EvaluationOutput", "EvaluationOutput tooltip description")]
        public string EvaluationOutput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "GnuPlotScriptOutput", "GnuPlotScriptOutput tooltip description")]
        public string GnuPlotScriptOutput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "GnuPlotDataOutput", "GnuPlotDataOutput tooltip description")]
        public string GnuPlotDataOutput
        {
            get;
            set;
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
                Console.WriteLine("------- Test Series Seed: " + SeedInput + " -------");

            // Testing output
            if (_evaluationCount < 3)
            {
                Console.WriteLine("----- Key: " + _keyCount + " -----");
                string evaluationString = _evaluationInput.ToString();
                Console.WriteLine(evaluationString);
                Console.WriteLine("Best Key: " + BestKeyInput);
                Console.WriteLine("Best Plaintext: " + BestPlaintextInput.Substring(0,
                    BestPlaintextInput.Length > 50 ? 50 : BestPlaintextInput.Length) +
                    " (" + BestPlaintextInput.Length + ")");
                Console.WriteLine("Plaintext: " + PlaintextInput.Substring(0,
                    PlaintextInput.Length > 50 ? 50 : PlaintextInput.Length) +
                    " (" + PlaintextInput.Length + ")");


                double decryptions = (double)EvaluationInput.GetDecryptions();
                TimeSpan runtime;
                if (_settings.CalculateRuntime && EvaluationInput.GetRuntime(out runtime))
                {
                    double divisor = runtime.TotalMilliseconds / _settings.TimeUnit;
                    double decryptionsPerTimeUnit = Math.Round((double)decryptions / divisor, 4);

                    Console.WriteLine("Decryptions per time unit: " + decryptionsPerTimeUnit);

                }
            }

            double percentCorrect = _bestPlaintextInput.CalculateSimilarity(_plaintextInput) * 100;
            bool success = percentCorrect >= _settings.CorrectPercentage ? true : false;

            if (_evaluationCount < 3)
                Console.WriteLine("percentCorrect: " + percentCorrect);

            ExtendedEvaluationContainer testRun = new ExtendedEvaluationContainer(_evaluationInput,
                _seedInput, _keyCount, _keyInput, _plaintextInput, _bestKeyInput, _bestPlaintextInput,
                _settings.CorrectPercentage, percentCorrect, success);

            _testRuns.Add(_evaluationInput.GetID(), testRun);

            _evaluationCount++;

            BestPlaintextInput = "";
            BestKeyInput = "";
            EvaluationInput = new EvaluationContainer();
        }

        public void InitializeVariables()
        {
            // set dot (".") as Number Decimal Separator
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

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

            // TODO: number of derived keys?

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

        public void SetGnuPlotVariables()
        {
            _val1 = "";
            _val2 = "";
            _val3 = "";

            // set GnuPlot output variables
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted)
            {
                _val1 = "Success";
                _val2 = "Percent Decrypted";
                _ylabel = "%";
                _evalMethod = "Succ_PercDecr_";
            }
            else if (_settings.YAxis == YAxisPlot.success)
            {
                _val1 = "Success";
                _ylabel = "%";
                _evalMethod = "Succ_";
            }
            else if (_settings.YAxis == YAxisPlot.percentDecrypted)
            {
                _val1 = "Percent Decrypted";
                _ylabel = "%";
                _evalMethod = "PercDecr_";
            }
            
            if (_settings.Y2Axis == Y2AxisPlot.decryptions)
            {
                _val3 = "Decryptions";
                _evalMethod += "NoDecr_";
            }
            else if (_settings.Y2Axis == Y2AxisPlot.restarts)
            {
                _val3 = "Restarts";
                _evalMethod += "Rest_";
            }
            else if (_settings.Y2Axis == Y2AxisPlot.tabuSetSizes)
            {
                _val3 = "Tabu Set Sizes";
                _evalMethod += "Tabu_";
            }
            else if (_settings.Y2Axis == Y2AxisPlot.populationSizes)
            {
                _val3 = "Population Sizes";
                _evalMethod += "Popu_";
            }
            else if (_settings.Y2Axis == Y2AxisPlot.runtime)
            {
                _val3 = "Runtime";
                _evalMethod += "Time_";
            }

            if (_settings.XAxis == XAxisPlot.ciphertextLength)
            {
                _evalMethod += "PerCiphLen";
                _keyValue = "Ciphertext Length";
            }
            else if (_settings.XAxis == XAxisPlot.keyLength)
            {
                _evalMethod += "PerKeyLen";
                _keyValue = "Key Length";
            }
            else if (_settings.XAxis == XAxisPlot.runtime)
            {
                _evalMethod += "PerTime";
                _keyValue = "Runtime";
            }

        }

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

                // count the successfull runs
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

            if (_keyCount == TotalKeysInput)
                BuildEvaluationOutputString();

            SetGnuPlotVariables();

            GenerateGnuPlotDataOutput();

            GenerateGnuPlotScriptOutput();
        }

        public void BuildEvaluationOutputString()
        {
            // build the average runtime string
            string averageRuntimeString = "";
            if (!_noRuntime)
                averageRuntimeString = new DateTime(TimeSpan.FromMilliseconds(_averageRuntime).Ticks).ToString("HH:mm:ss:FFFF");

            // build the displayed string of occuring ciphertext lengths
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

            // build the displayed string of occuring key lengths
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
                _evaluationOutput = "Test Series Seed: " + _testSeriesSeed + "\r";
            if (!_noRuntime)
                _evaluationOutput += "Average runtime: " + averageRuntimeString + "\r";
            _evaluationOutput += "Ciphertext lengths: " + ciphertextLengthString + "\r";
            _evaluationOutput += "Key lengths: " + keyLengthString + "\r";
            _evaluationOutput += "Average decryptions necessary: " + _averageDecryptions + "\r";
            if (!_noRestarts)
                _evaluationOutput += "Average restarts: " + _averageRestarts + "\r";
            if (!_noPopulationSize)
                _evaluationOutput += "Average population size: " + _averagePopulationSize + "\r";
            if (!_noTabuSetSize)
                _evaluationOutput += "Average tabu set size: " + _averageTabuSetSize + "\r";
            _evaluationOutput += "Averagely decrypted: " + _averagePercentDecrypted +
                "% of min " + _settings.CorrectPercentage + "%\r";
            _evaluationOutput += "Average success: " + _successPercentage + "%\r";
        }

        public void GenerateGnuPlotDataOutput()
        {
            // generate the GnuPlot data output string
            _gnuPlotDataOutput = "###########################################################" + NewLine;
            _gnuPlotDataOutput += "# Gnuplot script for plotting data from output GnuPlotData" + NewLine;
            _gnuPlotDataOutput += "# Save this GnuPlotData output in a file named " + NewLine;
            _gnuPlotDataOutput += "#" + NewLine;
            _gnuPlotDataOutput += "# --> '" + _evalMethod + ".dat'" + NewLine;
            _gnuPlotDataOutput += "#" + NewLine;
            _gnuPlotDataOutput += "# Save the GnuPlotScript output into a file named " + NewLine;
            _gnuPlotDataOutput += "# '" + _evalMethod + ".p'" + NewLine;
            _gnuPlotDataOutput += "# Use 'load " + _evalMethod + ".p' to plot" + NewLine;
            _gnuPlotDataOutput += "###########################################################" + NewLine;
            _gnuPlotDataOutput += NewLine;

            // # Data for evaluation method
            _gnuPlotDataOutput += "# Data for: " + _evalMethod + NewLine;
            _gnuPlotDataOutput += NewLine;
            _gnuPlotDataOutput += "# " + _keyValue + "\t\t" + _val1;
            if (!String.IsNullOrEmpty(_val2))
                _gnuPlotDataOutput += "\t\t" + _val2;
            if (!String.IsNullOrEmpty(_val3))
                _gnuPlotDataOutput += "\t\t" + _val3;
            _gnuPlotDataOutput += NewLine;

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

            if (_settings.XAxis == XAxisPlot.ciphertextLength)
                AddCiphertextLengthValues();
            else if (_settings.XAxis == XAxisPlot.keyLength)
                AddKeyLengthValues();
            else if (_settings.XAxis == XAxisPlot.runtime)
                if (!_noRuntime)
                    AddRuntimeValues();
                else {/* TODO: disable runtime in settings*/ }
        }

        public void AddCiphertextLengthValues()
        {
            int position = 0;
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
                        Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertextLength: " + len + ", currentSuccess: " + currentSuccess);
                        //continue;
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
                        Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertextLength: " + len + ", currentSuccess: " + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerCiphertextLength.TryGetValue(len, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertextLength: " + len + ", currentSuccess: " + currentDecryptions);
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
                        Console.WriteLine("TryGetValue from restartsPerCiphertextLength failed! ciphertextLength: " + len + ", currentRestarts: " + currentRestarts);
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
                        Console.WriteLine("TryGetValue from tabuSizesPerCiphertextLength failed! ciphertextLength: " + len + ", currentTabu: " + currentTabu);
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
                        Console.WriteLine("TryGetValue from populationSizesPerCiphertextLength failed! ciphertextLength: " + len + ", currentPopulatioin: " + currentPopulatioin);
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
                        Console.WriteLine("TryGetValue from successPerKeyLength failed! keyLength: " + len + ", currentSuccess: " + currentSuccess);
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
                        Console.WriteLine("TryGetValue from successPerKeyLength failed! keyLength: " + len + ", currentSuccess: " + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerKeyLength.TryGetValue(len, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine("TryGetValue from successPerKeyLength failed! keyLength: " + len + ", currentSuccess: " + currentDecryptions);
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
                        Console.WriteLine("TryGetValue from restartsPerKeyLength failed! keyLength: " + len + ", currentRestarts: " + currentRestarts);
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
                        Console.WriteLine("TryGetValue from tabuSizesPerKeyLength failed! keyLength: " + len + ", currentTabu: " + currentTabu);
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
                        Console.WriteLine("TryGetValue from populationSizesPerKeyLength failed! keyLength: " + len + ", currentPopulatioin: " + currentPopulatioin);
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
                        Console.WriteLine("TryGetValue from successPerRuntime failed! runtime: " + time + ", currentSuccess: " + currentSuccess);
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
                        Console.WriteLine("TryGetValue from successPerRuntime failed! runtime: " + time + ", currentSuccess: " + currentDecryptedPercentage);
                        continue;
                    }
                    else
                        _gnuPlotDataOutput += currentDecryptedPercentage + "\t\t\t\t";
                }

                if (_settings.Y2Axis == Y2AxisPlot.decryptions)
                {
                    double currentDecryptions = 0;
                    if (!_decryptionsPerRuntime.TryGetValue(time, out currentDecryptions))
                    {
                        // Warning!
                        Console.WriteLine("TryGetValue from successPerRuntime failed! runtime: " + time + ", currentSuccess: " + currentDecryptions);
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
                        Console.WriteLine("TryGetValue from restartsPerRuntime failed! runtime: " + time + ", currentRestarts: " + currentRestarts);
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
                        Console.WriteLine("TryGetValue from tabuSizesPerRuntime failed! runtime: " + time + ", currentTabu: " + currentTabu);
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
                        Console.WriteLine("TryGetValue from populationSizesPerRuntime failed! runtime: " + time + ", currentPopulatioin: " + currentPopulatioin);
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

        public void GenerateGnuPlotScriptOutput()
        {
            // generate the GnuPlot script output string
            _gnuPlotScriptOutput = "###########################################################" + NewLine;
            _gnuPlotScriptOutput += "# Gnuplot script for plotting data from output GnuPlotData" + NewLine;
            _gnuPlotScriptOutput += "# Save this GnuPlotScript output into a file named" + NewLine;
            _gnuPlotScriptOutput += "#" + NewLine;
            _gnuPlotScriptOutput += "# --> '" + _evalMethod + ".p'" + NewLine;
            _gnuPlotScriptOutput += "#" + NewLine;
            _gnuPlotScriptOutput += "# Save the GnuPlotData output in a file named " + NewLine;
            _gnuPlotScriptOutput += "# '" + _evalMethod + ".dat'" + NewLine;
            _gnuPlotScriptOutput += "# Use 'load " + _evalMethod + ".p' to plot" + NewLine;
            _gnuPlotScriptOutput += "###########################################################" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # General settings
            _gnuPlotScriptOutput += "# General settings" + NewLine;
            _gnuPlotScriptOutput += "set autoscale\t\t\t\t\t# -- scale axes automatically" + NewLine;
            _gnuPlotScriptOutput += "unset log\t\t\t\t\t\t# -- remove any log-scaling" + NewLine;
            _gnuPlotScriptOutput += "unset tics\t\t\t\t\t\t# -- remove any previous tics" + NewLine;
            _gnuPlotScriptOutput += "unset xlabel\t\t\t\t\t# -- remove previous labels" + NewLine;
            _gnuPlotScriptOutput += "unset ylabel" + NewLine;
            _gnuPlotScriptOutput += "unset y2label" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # Style settings
            _gnuPlotScriptOutput += "# Style settings" + NewLine;
            _gnuPlotScriptOutput += "set style line 1 lc rgb '#2ca25f' lt 1 lw 2 pt 1 ps 0.8   # -- green" + NewLine;
            _gnuPlotScriptOutput += "set style line 2 lc rgb '#0060ad' lt 1 lw 2 pt 7 ps 0.8   # -- blue" + NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + NewLine;
            _gnuPlotScriptOutput += "set style line 4 lc rgb '#edb120' lt 1 lw 2               # -- orange" + NewLine;
            _gnuPlotScriptOutput += "set style line 101 lc rgb '#656565' lt 1 lw 1             # -- dark-grey" + NewLine;
            _gnuPlotScriptOutput += "set style line 102 lc rgb '#d6d7d9' lt 0 lw 1             # -- grey" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # Plot settings
            _gnuPlotScriptOutput += "# Plot settings" + NewLine;
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
            _gnuPlotScriptOutput += "# x-Axis settings" + NewLine;
            if (_settings.XAxis == XAxisPlot.runtime && !_noRuntime)
            {
                // calculate normalized average Yvalues
                double normalizedAverageXValues = CalculateNormalizedAverage(_xValuesArray, _settings.NormalizingFactor);

                int min = CalculateMinValue(_lowestXValue, normalizedAverageXValues, "x");
                int max = CalculateMaxValue(_lowestXValue, _highestXValue, normalizedAverageXValues, "x");

                // TODO: DateTime(_averageRuntime.Ticks).ToString("HH:mm:ss:FFFF");

                if (min != max)
                    _gnuPlotScriptOutput += "set xrange [" + min + ":" + max + "]" + NewLine;
            }
            _gnuPlotScriptOutput += "set xtic auto\t\t\t\t\t# -- set xtics automatically" + NewLine;
            _gnuPlotScriptOutput += "set xlabel \"" + _keyValue + "\"" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # y-Axis settings
            _gnuPlotScriptOutput += "# y-Axis settings" + NewLine;
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted ||
                _settings.YAxis == YAxisPlot.percentDecrypted ||
                _settings.YAxis == YAxisPlot.success)
            {
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
                // not percent, set yrange and ytics
            }
            _gnuPlotScriptOutput += "set ylabel \"" + _ylabel + "\"" + NewLine;
            _gnuPlotScriptOutput += NewLine;

            // # second y-Axis settings
            if (_settings.Y2Axis != Y2AxisPlot.none &&
                !(_settings.Y2Axis == Y2AxisPlot.runtime && _noRuntime) &&
                !(_settings.Y2Axis == Y2AxisPlot.restarts && _noRestarts) &&
                !(_settings.Y2Axis == Y2AxisPlot.tabuSetSizes && _noTabuSetSize) &&
                !(_settings.Y2Axis == Y2AxisPlot.populationSizes && _noPopulationSize))
            {
                _gnuPlotScriptOutput += "# second y-Axis settings" + NewLine;
                _gnuPlotScriptOutput += "set y2tic scale 0.75" + NewLine;
                _gnuPlotScriptOutput += "set y2label \"" + _val3 + "\"" + NewLine;

                //    _settings.NormalizingFactor = 4; for runtime and decryptions

                // calculate normalized average Yvalues
                _normalizedAverageYValues = CalculateNormalizedAverage(_yValuesArray, _settings.NormalizingFactor);

                int min = CalculateMinValue(_lowestYValue, _normalizedAverageYValues, "y2");
                int max = CalculateMaxValue(_lowestYValue, _highestYValue, _normalizedAverageYValues, "y2");

                if (_settings.Y2Axis == Y2AxisPlot.runtime)
                {
                    //TODO: DateTime(_averageRuntime.Ticks).ToString("HH:mm:ss:FFFF");
                }

                if (min != max)
                    _gnuPlotScriptOutput += "set y2range [" + min + ":" + max + "]" + NewLine;
                _gnuPlotScriptOutput += NewLine;
            }

            // # plotting
            int style = 1;
            if (_settings.YAxis == YAxisPlot.percentDecrypted)
                style = 2;
            int column = 3;

            _gnuPlotScriptOutput += "# plotting" + NewLine;
            _gnuPlotScriptOutput += "plot    \"" + _evalMethod + ".dat\" using 1:2 title '" + _val1 + "' with linespoints ls " + style;
            if (_settings.YAxis == YAxisPlot.successAndPercentDecrypted)
            {
                _gnuPlotScriptOutput += " , \\" + NewLine + "        \"" + _evalMethod + ".dat\" using 1:3 title '" + _val2 + "' with linespoints ls 2" + NewLine;
                column++;
            }
            if (_settings.Y2Axis != Y2AxisPlot.none)
            {
                _gnuPlotScriptOutput += "replot  \"" + _evalMethod + ".dat\" using 1:"+column+" title '" + _val3 + "' with linespoints ls 3 axes x1y2";
                if (_settings.ShowY2Average)
                {
                    _gnuPlotScriptOutput += " , \\" + NewLine;
                    _gnuPlotScriptOutput += "        " + Math.Round(_normalizedAverageYValues) + " title 'Average "+ _val3+" = " + Math.Round(_normalizedAverageYValues) + "' with lines ls 4 axes x1y2";

                }
                else
                    _gnuPlotScriptOutput += NewLine;
            }
        }

        public int CalculateMinValue(double lowest, double avg, string axis)
        {
            int min = (int)lowest;
            if (_settings.Y2Axis == Y2AxisPlot.decryptions && axis.Equals("y2"))
                if (avg * 0.03 < 10)
                    min -= 10;
                else
                    min -= (int) (avg * 0.03);
            else if ((_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                (axis.Equals("x")))
                if (avg * 0.03 < 5)
                    min -= 5;
                else
                    min -= (int)(avg * 0.03);
            else if (_settings.Y2Axis == Y2AxisPlot.restarts && axis.Equals("y2"))
            {
                if (avg * 0.03 < 2)
                    min -= 2;
                else
                    min -= (int) (avg * 0.03);
            }

            return min;
        }

        public int CalculateMaxValue(double lowest, double highest, double avg, string axis)
        {
            int max = (int)highest;
            if ((_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                 axis.Equals("x"))
                if (avg * 0.3 < 2)
                    max += 2;
                else
                    max += (int)(avg * 0.3);

            if (_settings.Y2Axis == Y2AxisPlot.decryptions ||
                (_settings.Y2Axis == Y2AxisPlot.runtime && axis.Equals("y2")) ||
                axis.Equals("x"))
            {
                // calculate distances to mean (/average)
                int lowestToMean = (int)avg - (int)lowest;
                int highestToMean = (int)highest - (int)avg;

                if (highestToMean > lowestToMean * 2)
                    max = (int)(avg + lowestToMean * 2);
            }

            return max;
        }

        public void RefreshEvaluationOutputs()
        {
            EvaluationOutput = _evaluationOutput;
            OnPropertyChanged("EvaluationOutput");

            RefreshGnuPlotOutputs();
        }

        public void RefreshGnuPlotOutputs()
        {
            GnuPlotScriptOutput = _gnuPlotScriptOutput;
            OnPropertyChanged("GnuPlotScriptOutput");

            GnuPlotDataOutput = _gnuPlotDataOutput;
            OnPropertyChanged("GnuPlotDataOutput");
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
            _testRuns = new Dictionary<int, ExtendedEvaluationContainer>();
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
                _keyCount++;
                _progress = (int)Math.Round((double)_keyCount / _totalKeysInput * 100);

                if (String.IsNullOrEmpty(EvaluationOutput))
                    EvaluationOutput = _keyCount + " / " + _totalKeysInput + NewLine +
                        "0%" + NewLine + NewLine + "Current key number: " +
                        NewLine + _keyCount;
                else
                    EvaluationOutput += NewLine + _keyCount + " / " + _totalKeysInput;
                    
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

                PlaintextOutput = PlaintextInput;
                KeyOutput = KeyInput;

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

                // gather all available evaluation data
                CollectEvaluationData();

                // trigger next key if key count less than total keys...
                if (_totalKeysInput > 0 &&
                    _keyCount < _totalKeysInput)
                {
                    EvaluationOutput = "";
                    for (int i = 0; i < (int)_progress / 5; i++)
                        EvaluationOutput += "█";

                    EvaluationOutput += " " + _progress + "%" + NewLine + NewLine + 
                        "Current key number: " + NewLine + _keyCount + " / " + 
                        _totalKeysInput + " - Done.";

                    OnPropertyChanged("EvaluationOutput");

                    TriggerNextKey = KeyInput;
                    OnPropertyChanged("TriggerNextKey");
                }
                else
                {
                    // ...evaluate if not
                    EvaluationOutput = "";
                    for (int i = 0; i < (int)_progress / 5; i++)
                        EvaluationOutput += "█";

                    EvaluationOutput += " " + _progress + "%" + NewLine + NewLine +
                        "Current key number: " + NewLine + _keyCount + " / " +
                        _totalKeysInput + " - Done." + NewLine +
                        NewLine + "Started Evaluating...";
                    OnPropertyChanged("EvaluationOutput");
                    
                    Evaluate();
                    RefreshEvaluationOutputs();
                }

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

        #region General Methods

        public double CalculateNormalizedAverage(double[] arr, int normalizingFactor)
        {
            Array.Sort(arr);

            double avg = 0;
            foreach (double val in arr)
                avg += val;
            avg /= arr.Length;

            List<double> irrepresentableValues = new List<double>();
            for (int j = 0; j < arr.Length - 1; j++)
            {
                if (arr[j] < avg)
                    continue;

                // check if one decryptions value is x times bigger than the next value
                if (arr[j] * normalizingFactor < arr[j + 1])
                {
                    for (int i = j + 1; i < arr.Length; i++)
                        irrepresentableValues.Add(arr[i]);
                    break;
                }
            }

            if (irrepresentableValues.Count == 0)
                return avg;

            double IrrepresentableSum = 0;
            foreach (double val in irrepresentableValues)
                IrrepresentableSum += val;

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

        private string _testSeriesSeed;
        private int _keyNumber;
        private string _key;
        // private BigInteger _keySpace; ?
        private string _plaintext;
        private string _bestKey;
        private string _bestPlaintext;
        private double _minimalDecryption;
        private double _percentDecrypted;
        private bool _successful;

        public ExtendedEvaluationContainer(EvaluationContainer e)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(e);
        }

        public ExtendedEvaluationContainer(EvaluationContainer e, string seed, 
            int keyNumber, string key, string plaintext, string bestKey,
            string bestPlaintext, double minimalDecryption, double percentDecrypted,
            bool successfull)
        {
            // set EvaluationContainer base class
            base.SetEvaluationContainer(e);
            this._testSeriesSeed = seed;
            this._keyNumber = keyNumber;
            this._key = key;
            this._plaintext = plaintext;
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

    public static class DictionaryExtention
    {

        // Either Add or overwrite
        public static void AddOrUpdate<K, V>(this Dictionary<K, V> dict, K key, V newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = newValue;
            else
                dict.Add(key, newValue);
        }

        // Either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, int> dict, K key, int newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // Either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, BigInteger> dict, K key, int newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // Either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, double> dict, K key, int newValue)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + newValue;
            else
                dict.Add(key, newValue);
        }

        // Either Add or increment
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

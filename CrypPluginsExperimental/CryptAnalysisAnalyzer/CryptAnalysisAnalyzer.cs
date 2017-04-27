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

        #region General Methods

        #endregion

        #region Evaluation

        public void Evaluate()
        {
            // set dot (".") as Number Decimal Separator
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            // count and helper variables
            int successCount = 0;
            double decryptedCount = 0;
            double decryptionsCount = 0;

            TimeSpan runtimeCount = new TimeSpan();
            bool noRuntime = !_settings.CalculateRuntime;
            double restarts = 0;
            bool noRestarts = false;
            double populationSize = 0;
            bool noPopulationSize = false;
            double tabuSetSize = 0;
            bool noTabuSetSize = false;
            string testSeriesSeed = "";

            // TODO: number of derived keys?

            // evaluation key values
            Dictionary<int, int> keyLengths = new Dictionary<int, int>();
            Dictionary<int, int> ciphertextLengths = new Dictionary<int, int>();
            Dictionary<TimeSpan, int> runtimes = new Dictionary<TimeSpan, int>();

            // evaluation detailed values
            // key length
            Dictionary<int, double> successPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, double> decryptedPercentagesPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, double> decryptionsPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, double> restartsPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, double> tabuSizesPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, double> populationSizesPerKeyLength = new Dictionary<int, double>();
            Dictionary<int, TimeSpan> runtimePerKeyLength = new Dictionary<int, TimeSpan>();
            // ciphertext length
            Dictionary<int, double> successPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, double> decryptedPercentagesPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, double> decryptionsPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, double> restartsPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, double> tabuSizesPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, double> populationSizesPerCiphertextLength = new Dictionary<int, double>();
            Dictionary<int, TimeSpan> runtimePerCiphertextLength = new Dictionary<int, TimeSpan>();
            // runtime
            Dictionary<TimeSpan, double> successPerRuntime = new Dictionary<TimeSpan, double>();
            Dictionary<TimeSpan, double> decryptedPercentagesPerRuntime = new Dictionary<TimeSpan, double>();
            Dictionary<TimeSpan, double> decryptionsPerRuntime = new Dictionary<TimeSpan, double>();
            Dictionary<TimeSpan, double> restartsPerRuntime = new Dictionary<TimeSpan, double>();
            Dictionary<TimeSpan, double> tabuSizesPerRuntime = new Dictionary<TimeSpan, double>();
            Dictionary<TimeSpan, double> populationSizesPerRuntime = new Dictionary<TimeSpan, double>();

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
                double percentDecrypted = testRun.GetPercentDecrypted();
                double decryptions = testRun.GetDecryptions();
                double currentRestarts = 0;
                double currentTabuSize = 0;
                double currentPopulationSize = 0;
                if (!noRestarts)
                    currentRestarts = testRun.GetRestarts();
                if (!noTabuSetSize)
                    currentTabuSize = testRun.GetTabuSetSize();
                if (!noPopulationSize)
                    currentPopulationSize = testRun.GetPopulationSize();

                // get the seed of the whole test series only once
                if (firstElement)
                {
                    testSeriesSeed = testRun.GetSeed();
                    firstElement = false;
                }

                // count the successfull runs
                if (testRun.GetSuccessfull())
                    successCount++;
                DictionaryExtention.AddOrIncrement<int>(successPerKeyLength, keyLength, currentSuccess);
                DictionaryExtention.AddOrIncrement<int>(successPerCiphertextLength, ciphertextLength, currentSuccess);

                // count the overall decryptions and decrypted percentages
                decryptedCount += percentDecrypted;
                decryptionsCount += decryptions;

                // count the decryptions and decrypted percentages per key and ciphertext lengths
                DictionaryExtention.AddOrIncrement<int>(decryptedPercentagesPerKeyLength, keyLength, percentDecrypted);
                DictionaryExtention.AddOrIncrement<int>(decryptionsPerKeyLength, keyLength, decryptions);
                DictionaryExtention.AddOrIncrement<int>(decryptedPercentagesPerCiphertextLength, ciphertextLength, percentDecrypted);
                DictionaryExtention.AddOrIncrement<int>(decryptionsPerCiphertextLength, ciphertextLength, decryptions);
                
                // count the restarts if every run contains a restart value greater zero
                if (!noRestarts && currentRestarts > 0)
                {
                    restarts += currentRestarts;
                    DictionaryExtention.AddOrIncrement(restartsPerKeyLength, keyLength, currentRestarts);
                    DictionaryExtention.AddOrIncrement(restartsPerCiphertextLength, ciphertextLength, currentRestarts);
                }
                else
                    noRestarts = true;

                // count the tabu set size if every run contains a size value greater zero
                if (!noTabuSetSize && currentTabuSize > 0)
                {
                    tabuSetSize += currentTabuSize;
                    DictionaryExtention.AddOrIncrement(tabuSizesPerKeyLength, keyLength, currentTabuSize);
                    DictionaryExtention.AddOrIncrement(tabuSizesPerCiphertextLength, ciphertextLength, currentTabuSize);
                }
                else
                    noTabuSetSize = true;

                // count the population size if every run contains a size value greater zero
                if (!noPopulationSize && currentPopulationSize > 0)
                {
                    populationSize += currentPopulationSize;
                    DictionaryExtention.AddOrIncrement(populationSizesPerKeyLength, keyLength, currentPopulationSize);
                    DictionaryExtention.AddOrIncrement(populationSizesPerCiphertextLength, ciphertextLength, currentPopulationSize);
                }
                else
                    noPopulationSize = true;

                // count all values per runtime and the runtime per key and ciphertext lengths
                TimeSpan time;
                if (!noRuntime && testRun.GetRuntime(out time))
                {
                    runtimeCount += time;
                    // update key value dictionary runtimes
                    DictionaryExtention.AddOrIncrement(runtimes, time, 1);

                    // detailed values
                    DictionaryExtention.AddOrIncrement(successPerRuntime, time, currentSuccess);
                    DictionaryExtention.AddOrIncrement(decryptedPercentagesPerRuntime, time, percentDecrypted);
                    DictionaryExtention.AddOrIncrement(decryptionsPerRuntime, time, decryptions);
                    if (!noRestarts)
                        DictionaryExtention.AddOrIncrement(restartsPerRuntime, time, currentRestarts);
                    if (!noTabuSetSize)
                        DictionaryExtention.AddOrIncrement(tabuSizesPerRuntime, time, currentTabuSize);
                    if (!noPopulationSize)
                        DictionaryExtention.AddOrIncrement(populationSizesPerRuntime, time, currentPopulationSize);

                    TimeSpan t = time + time;
                    DictionaryExtention.AddOrIncrement(runtimePerKeyLength, keyLength, time);
                    DictionaryExtention.AddOrIncrement(runtimePerCiphertextLength, ciphertextLength, time);
                }
                else
                {
                    noRuntime = true;
                }

                // update key value dictionaries keyLengths and ciphertextLengths
                DictionaryExtention.AddOrIncrement(keyLengths, keyLength, 1);
                DictionaryExtention.AddOrIncrement(ciphertextLengths, ciphertextLength, 1);
            }

            // after counting all values, we calculate average values here

            // calculate the overall average values
            double successPercentage = Math.Round((double) successCount / _testRuns.Count * 100, 2);
            double averageDecryptedPercentage = Math.Round((double) decryptedCount / _testRuns.Count, 2);
            double averageDecryptions = Math.Round((double)decryptionsCount / _testRuns.Count, 2);

            // calculate the average runtime values
            string averageRuntimeString = "";
            if (!noRuntime)
            {
                // calculate the overall average values
                double ms = runtimeCount.TotalMilliseconds / _testRuns.Count;
                TimeSpan averageRuntime = TimeSpan.FromMilliseconds(ms);
                averageRuntimeString = new DateTime(averageRuntime.Ticks).ToString("HH:mm:ss:FFFF");

                // if the current runtime count can be retrieved, calculate the average values
                foreach (var pair in runtimes)
                {
                    TimeSpan time = pair.Key;
                    int count = pair.Value;

                    // if the count is greater 1, we have to divide through count to get the average
                    if (count > 0)
                    {
                        // detailed values
                        DictionaryExtention.DivideAndRoundPercent<TimeSpan>(successPerRuntime, time, count, 2);
                        DictionaryExtention.DivideAndRound<TimeSpan>(decryptedPercentagesPerRuntime, time, count, 2);
                        DictionaryExtention.Divide<TimeSpan>(decryptionsPerRuntime, time, count);
                        if (!noRestarts)
                            DictionaryExtention.Divide<TimeSpan>(restartsPerRuntime, time, count);
                        if (!noTabuSetSize)
                            DictionaryExtention.Divide<TimeSpan>(tabuSizesPerRuntime, time, count);
                        if (!noPopulationSize)
                            DictionaryExtention.Divide<TimeSpan>(populationSizesPerRuntime, time, count);
                    }
                }
            }

            // calculate the overall average values
            double averageRestarts = 0;
            if (!noRestarts)
                averageRestarts = restarts / _testRuns.Count;
            double averageTabuSetSize = 0;
            if (!noTabuSetSize)
                averageTabuSetSize = tabuSetSize / _testRuns.Count;
            double averagePopulationSize = 0;
            if (!noPopulationSize)
                averagePopulationSize = populationSize / _testRuns.Count;

            // if the current key length count can be retrieved, calculate the average values
            foreach (var pair in keyLengths)
            {
                int keyLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 0)
                {
                    // calculate the detailed average values
                    DictionaryExtention.DivideAndRoundPercent<int>(successPerKeyLength, keyLength, count, 2);
                    DictionaryExtention.DivideAndRound<int>(decryptedPercentagesPerKeyLength, keyLength, count, 2);
                    DictionaryExtention.Divide<int>(decryptionsPerKeyLength, keyLength, count);
                    DictionaryExtention.DivideTimeSpan<int>(runtimePerKeyLength, keyLength, count);

                    if (!noRestarts)
                        DictionaryExtention.Divide<int>(restartsPerKeyLength, keyLength, count);

                    if (!noTabuSetSize)
                        DictionaryExtention.Divide<int>(tabuSizesPerKeyLength, keyLength, count);

                        if (!noPopulationSize)
                            DictionaryExtention.Divide<int>(populationSizesPerKeyLength, keyLength, count);
                    
                }
            }

            // if the current ciphertext length count can be retrieved, calculate the average values
            foreach (var pair in ciphertextLengths)
            {
                int ciphertextLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 0)
                {
                    // calculate the detailed average values
                    //successPerCiphertextLength.AddOrUpdate(ciphertextLength, 0, (length, success) => Math.Round(success / count * 100, 2));
                    //decryptedPercentagesPerCiphertextLength.AddOrUpdate(ciphertextLength, 0, (length, percent) => Math.Round(percent / count, 2));
                    //if (decryptedPercentagesPerCiphertextLength.ContainsKey(ciphertextLength))
                    //    decryptedPercentagesPerCiphertextLength[ciphertextLength] = Math.Round(decryptedPercentagesPerCiphertextLength[ciphertextLength] / count, 2);
                    DictionaryExtention.DivideAndRoundPercent<int>(successPerCiphertextLength, ciphertextLength, count, 2);
                    DictionaryExtention.DivideAndRound<int>(decryptedPercentagesPerCiphertextLength, ciphertextLength, count, 2);
                    DictionaryExtention.Divide<int>(decryptionsPerCiphertextLength, ciphertextLength, count);
                    DictionaryExtention.DivideTimeSpan<int>(runtimePerCiphertextLength, ciphertextLength, count);

                    if (!noRestarts)
                        DictionaryExtention.Divide<int>(restartsPerCiphertextLength, ciphertextLength, count);

                    if (!noTabuSetSize)
                        DictionaryExtention.Divide<int>(tabuSizesPerCiphertextLength, ciphertextLength, count);

                    if (!noPopulationSize)
                        DictionaryExtention.Divide<int>(populationSizesPerCiphertextLength, ciphertextLength, count);
                }
            }

            // build the displayed string of occuring ciphertext lengths
            string ciphertextLengthString = "";
            int i = 1;
            foreach (var pair in ciphertextLengths)
            {
                if (ciphertextLengths.Count > 6 && i == 3)
                {
                    ciphertextLengthString += " ...";
                }
                else if (ciphertextLengths.Count <= 6 || i < 3 || i >= ciphertextLengths.Count - 3)
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
            foreach (var pair in keyLengths)
            {
                if (keyLengths.Count > 6 && i == 3)
                {
                    keyLengthString += " ...";
                }
                else if (keyLengths.Count <= 6 || i < 3 || i >= keyLengths.Count - 3)
                {
                    if (keyLengthString != "")
                        keyLengthString += ", ";
                    keyLengthString += pair.Key + " (" + pair.Value + ")";
                }
                i++;
            }

            // build the complete displayed evaluation output string
            _evaluationOutput = "";
            if (!string.IsNullOrEmpty(testSeriesSeed))
                _evaluationOutput = "Test Series Seed: " + testSeriesSeed + "\r";
            if (!noRuntime)
                _evaluationOutput += "Average runtime: " + averageRuntimeString + "\r";
            _evaluationOutput += "Ciphertext lengths: " + ciphertextLengthString + "\r";
            _evaluationOutput += "Key lengths: " + keyLengthString + "\r";
            _evaluationOutput += "Average decryptions necessary: " + averageDecryptions + "\r";
            if (!noRestarts)
                _evaluationOutput += "Average restarts: " + averageRestarts + "\r";
            if (!noPopulationSize)
                _evaluationOutput += "Average population size: " + averagePopulationSize + "\r";
            if (!noTabuSetSize)
                _evaluationOutput += "Average tabu set size: " + averageTabuSetSize + "\r";
            _evaluationOutput += "Averagely decrypted: " + averageDecryptedPercentage + 
                "% of min " + _settings.CorrectPercentage + "%\r";
            _evaluationOutput += "Average success: " + successPercentage + "%\r";

            // GnuPlot output variables
            string evalMethod = "successDecryptedPercentPerCiphertext";
            string keyValue = "Ciphertext length";
            string val1 = "Success";
            string val2 = "Decrypted Percent";
            string val3 = "Decryptions";
            string xlabel = "Ciphertext Length";
            string ylabel = "%";

            // generate the GnuPlot data output string
            _gnuPlotDataOutput = "###########################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save this GnuPlotData output in a file named " + System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + evalMethod + ".dat" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save the GnuPlotScript output into a file named " + System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + evalMethod + ".p" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# and use 'load " + evalMethod + ".p'" + System.Environment.NewLine;
            _gnuPlotDataOutput += "###########################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += System.Environment.NewLine;

            // # Data for evaluation method
            _gnuPlotDataOutput += "# Data for: " + evalMethod + System.Environment.NewLine;
            _gnuPlotDataOutput += System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + keyValue + "\t\t" + val1 + "\t\t" + val2 + "\t\t" + val3 + System.Environment.NewLine;

            double lowestDecryptions = -1;
            double highestDecryptions = 0;
            double[] decryptionsArray = new double[decryptionsPerCiphertextLength.Count];
            double normalizedAverageDecryptions = 0;
            int position = 0;
            foreach (var pair in ciphertextLengths)
            {
                int ciphertextLength = pair.Key;
                /* // Possible use for showing the number of keys per length
                int count = pair.Value;
                if (count == 0) {
                    // Warning!
                    continue;
                }
                */

                double currentSuccess = 0;
                if (!successPerCiphertextLength.TryGetValue(ciphertextLength, out currentSuccess)) {
                    // Warning! But may be zero
                    Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertestLength: " + ciphertextLength + ", currentSuccess: " + currentSuccess);
                    //continue;
                }

                double currentDecryptedPercentage = 0;
                if (!decryptedPercentagesPerCiphertextLength.TryGetValue(ciphertextLength, out currentDecryptedPercentage))
                {
                    // Warning!
                    Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertestLength: " + ciphertextLength + ", currentSuccess: " + currentDecryptedPercentage);
                    continue;
                }

                double currentDecryptions = 0;
                if (!decryptionsPerCiphertextLength.TryGetValue(ciphertextLength, out currentDecryptions))
                {
                    // Warning!
                    Console.WriteLine("TryGetValue from successPerCiphertextLength failed! ciphertestLength: " + ciphertextLength + ", currentSuccess: " + currentDecryptions);
                    //continue;
                }
                else if (currentDecryptions > 0)
                {
                    if (lowestDecryptions == -1 || currentDecryptions < lowestDecryptions)
                        lowestDecryptions = currentDecryptions;
                    if (currentDecryptions > highestDecryptions)
                        highestDecryptions = currentDecryptions;
                    decryptionsArray[position] = currentDecryptions;
                    position++;
                }

                TimeSpan currentRuntime = new TimeSpan();
                if (!runtimePerCiphertextLength.TryGetValue(ciphertextLength, out currentRuntime))
                {
                    // Warning!
                    //continue;
                }

                /*
                if (!noRestarts)
                    restartsPerKeyLength.AddOrUpdate(keyLength, 1, (length, restarts) => restarts / count;
                
                if (!noTabuSetSize)
                    tabuSizesPerKeyLength.AddOrUpdate(keyLength, 1, (length, tabuSetSizes) => tabuSetSizes / count;

                if (!noPopulationSize)
                    populationSizesPerKeyLength.AddOrUpdate(keyLength, 1, (length, populationSizes) => populationSizes / count;
                */

                // add detailed values to GnuPlot data output string
                _gnuPlotDataOutput += ciphertextLength + "\t\t\t\t\t\t" + currentSuccess + "\t\t\t\t" +
                    currentDecryptedPercentage + "\t\t\t\t" + currentDecryptions + System.Environment.NewLine;
            }

            // generate the GnuPlot script output string
            _gnuPlotScriptOutput = "###########################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save the GnuPlotData output in a file named " + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# " + evalMethod + ".dat" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save this into a file named " + evalMethod + ".p" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# and  use 'load " + evalMethod + ".p'" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "###########################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # General settings
            _gnuPlotScriptOutput += "# General settings" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set autoscale\t\t\t\t\t# -- scale axes automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset log\t\t\t\t\t\t# -- remove any log-scaling" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset tics\t\t\t\t\t\t# -- remove any previous tics" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset xlabel\t\t\t\t\t# -- remove previous labels" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset ylabel" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset y2label" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # Style settings
            _gnuPlotScriptOutput += "# Style settings" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 1 lc rgb '#2ca25f' lt 1 lw 2 pt 1 ps 0.8   # -- green" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 2 lc rgb '#0060ad' lt 1 lw 2 pt 7 ps 0.8   # -- blue" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 3 lc rgb '#e34a33' lt 1 lw 2 pt 2 ps 0.8   # -- red" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 4 lc rgb '#edb120' lt 1 lw 2               # -- orange" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 101 lc rgb '#656565' lt 1 lw 1             # -- dark-grey" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set style line 102 lc rgb '#d6d7d9' lt 0 lw 1             # -- grey" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # Plot settings
            _gnuPlotScriptOutput += "# Plot settings" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set title \"" + val1 + ", " + val2 + ", and " + val3 + " dependent on " + keyValue + "\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set size ratio 0.8" + System.Environment.NewLine;
            int border = 3; // 11 = |__|, 3 = |__
            if (_settings.Y2Axis != Y2AxisPlot.none) border = 11;
            _gnuPlotScriptOutput += "set border " + border + " front ls 101" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set tics nomirror out scale 0.75" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set format '%g'" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set grid back ls 102" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # x-Axis settings
            _gnuPlotScriptOutput += "# x-Axis settings" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xtic auto\t\t\t\t\t# -- set xtics automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xlabel \"" + xlabel + "\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # y-Axis settings
            _gnuPlotScriptOutput += "# y-Axis settings" + System.Environment.NewLine;
            if (_settings.YAxis == YAxisPlot.successAndDecryptedPercent ||
                _settings.YAxis == YAxisPlot.decryptedPercent ||
                _settings.YAxis == YAxisPlot.success)
            {
                int percentUpper = 110;
                if (_settings.YAxis == YAxisPlot.successAndDecryptedPercent)
                    percentUpper += 14;
                else
                    percentUpper += 7;

                if (_settings.Y2Axis != Y2AxisPlot.none)
                    percentUpper += 7;
                _gnuPlotScriptOutput += "set yrange [-5:" + percentUpper + "]" + System.Environment.NewLine;  // to gain some space below 0% and above 100%
                _gnuPlotScriptOutput += "set ytics (0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)" + System.Environment.NewLine;
            }
            else
            {
                // not percent, set yrange and ytics
            }
            _gnuPlotScriptOutput += "set ylabel \"" + ylabel + "\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += System.Environment.NewLine;

            // # second y-Axis settings
            if (_settings.Y2Axis == Y2AxisPlot.decryptions)
            {
                _gnuPlotScriptOutput += "# second y-Axis settings" + System.Environment.NewLine;
                _gnuPlotScriptOutput += "set y2tic scale 0.75" + System.Environment.NewLine;
                _gnuPlotScriptOutput += "set y2label \"" + val3 + "\"" + System.Environment.NewLine;
                // calculate normalized average decryptions
                normalizedAverageDecryptions = CalculateNormalizedAverage(decryptionsArray, 4);
                int min = (int)lowestDecryptions - 100; //1900;
                int max = (int)highestDecryptions; //8000;
                // calculate distances to mean (/average)
                int lowestToMean = (int)normalizedAverageDecryptions - (int)lowestDecryptions;
                int highestToMean = (int)highestDecryptions - (int)normalizedAverageDecryptions;

                if (highestToMean > lowestToMean * 2)
                    max = (int)(normalizedAverageDecryptions + lowestToMean * 2);

                _gnuPlotScriptOutput += "set y2range [" + min + ":" + max + "]" + System.Environment.NewLine;
                _gnuPlotScriptOutput += System.Environment.NewLine;
            }

            // # plotting
            _gnuPlotScriptOutput += "# plotting" + System.Environment.NewLine;
            if (_settings.YAxis == YAxisPlot.successAndDecryptedPercent)
            {
                _gnuPlotScriptOutput += "plot    \"" + evalMethod + ".dat\" using 1:2 title '" + val1 + "' with linespoints ls 1 , \\" + System.Environment.NewLine;
                _gnuPlotScriptOutput += "        \"" + evalMethod + ".dat\" using 1:3 title '" + val2 + "' with linespoints ls 2" + System.Environment.NewLine;
            }
            else if (_settings.YAxis == YAxisPlot.decryptedPercent)
            {
                // TODO: decide: still putting success percentage in .dat file? yes -> using 1:3, no -> using 1:2
                //_gnuPlotScriptOutput += "plot    \"" + evalMethod + ".dat\" using 1:2 title '" + val1 + "' with linespoints ls 1 , \\" + System.Environment.NewLine;
                _gnuPlotScriptOutput += "plot    \"" + evalMethod + ".dat\" using 1:3 title '" + val2 + "' with linespoints ls 2" + System.Environment.NewLine;
            }
            else if (_settings.YAxis == YAxisPlot.success)
            {
                _gnuPlotScriptOutput += "plot    \"" + evalMethod + ".dat\" using 1:2 title '" + val1 + "' with linespoints ls 1" + System.Environment.NewLine;
            }
            if (_settings.Y2Axis == Y2AxisPlot.decryptions)
            {
                _gnuPlotScriptOutput += "replot  \"" + evalMethod + ".dat\" using 1:4 title '" + val3 + "' with linespoints ls 3 axes x1y2";
                if (_settings.ShowY2Average)
                {
                    _gnuPlotScriptOutput += " , \\" + System.Environment.NewLine;
                    _gnuPlotScriptOutput += "        " + Math.Round(normalizedAverageDecryptions) + " title 'Average Decryptions = " + Math.Round(normalizedAverageDecryptions) + "' with lines ls 4 axes x1y2";

                }
                else
                    _gnuPlotScriptOutput += System.Environment.NewLine;
            }



            /*
            // GnuPlot output variables
            string evalMethod = "successDecryptedPercentPerKey";
            string keyValue = "keyLength";
            string val1 = "success";
            string val2 = "decryptedPercent";
            string val3 = "decryptions";
            string xlabel = "Key length";
            string ylabel = "%";

            // generate the GnuPlot script output string
            _gnuPlotScriptOutput = "###################################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save the GnuPlotData output in a file named "+evalMethod+".dat" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save this into a file named "+evalMethod+".p and " + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# use 'load " + evalMethod + ".p'" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "###################################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set   autoscale                        # scale axes automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset log                              # remove any log-scaling" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset label                            # remove any previous labels" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xtic auto                          # set xtics automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set ytic auto                          # set ytics automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set title \""+val1+", "+val2+", and "+val3+" dependent on "+keyValue+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xlabel \""+xlabel+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set ylabel \""+ylabel+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "plot    \""+evalMethod+".dat\" using 1:2 title '"+val1+"' with linespoints , \\" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "        \""+evalMethod+".dat\" using 1:3 title '"+val2+"' with points" + System.Environment.NewLine;

            // generate the GnuPlot data output string
            _gnuPlotDataOutput = "###################################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save this GnuPlotData output in a file named "+evalMethod+".dat" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save the GnuPlotScript output into a file named "+evalMethod+".p and " + System.Environment.NewLine;
            _gnuPlotDataOutput += "# use 'load "+evalMethod+".p'" + System.Environment.NewLine;
            _gnuPlotDataOutput += "###################################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Data for: "+evalMethod + System.Environment.NewLine;
            _gnuPlotDataOutput += "# "+keyValue+"    "+val1+"    "+val2 + System.Environment.NewLine;
            
            foreach (var pair in keyLengths)
            {
                int keyLength = pair.Key;

                int currentSuccess = 0;
                if (!successPerKeyLength.TryGetValue(keyLength, out currentSuccess)) {
                    // Warning!
                    continue;
                }

                double currentDecryptedPercentage = 0;
                if (!decryptedPercentagesPerKeyLength.TryGetValue(keyLength, out currentDecryptedPercentage)) {
                    // Warning!
                    continue;
                }

                // add detailed values to GnuPlot data output string
                _gnuPlotDataOutput += "  " + keyLength + "         " + currentSuccess + "         " +
                    currentDecryptedPercentage + System.Environment.NewLine;

                double currentDecryptions = 0;
                if (!decryptionsPerKeyLength.TryGetValue(keyLength, out currentDecryptions))
                {
                    // Warning!
                    //continue;
                }

                TimeSpan currentRuntime = new TimeSpan();
                if (!runtimePerKeyLength.TryGetValue(keyLength, out currentRuntime))
                {
                    // Warning!
                    //continue;
                }
            }*/
        }

        public double CalculateNormalizedAverage(double[] arr, int normalizingFactor)
        {
            Array.Sort(arr);
            double normalized = 0;
            List<double> irrepresentableValues = new List<double>();
            for (int j = arr.Length - 2; j >= 0; j--)
            {
                // check if one decryptions value is 4 times bigger than the next value
                if (arr[j] * normalizingFactor < arr[j + 1])
                {
                    irrepresentableValues.Add(arr[j + 1]);
                    continue;
                }
                normalized += arr[j + 1];
            }
            normalized += arr[0];
            normalized /= (arr.Length - irrepresentableValues.Count);

            return normalized;
        }

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


                double decryptions = (double) EvaluationInput.GetDecryptions();
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
           
            ExtendedEvaluationContainer testRun = new ExtendedEvaluationContainer(_evaluationInput,
                _seedInput, _keyCount, _keyInput, _plaintextInput, _bestKeyInput, _bestPlaintextInput,
                _settings.CorrectPercentage, percentCorrect, success);

            _testRuns.Add(_evaluationInput.GetID(), testRun);

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

                EvaluationOutput = EvaluationOutput + System.Environment.NewLine + 
                    "Current key number: " + _keyCount;
                OnPropertyChanged("EvaluationOutput");

                // Send the plaintext and key (and min correct percentage) to the encryption method
                OnPropertyChanged("MinimalCorrectPercentage");
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
                    EvaluationOutput = _keyCount + " / " + _totalKeysInput +
                        " - " + Math.Round((double)_keyCount / _totalKeysInput * 100) + "%" +
                        System.Environment.NewLine + "Current key number: " +
                        _keyCount + " - Done.";
                    OnPropertyChanged("EvaluationOutput");

                    TriggerNextKey = KeyInput;
                    OnPropertyChanged("TriggerNextKey");
                }
                else
                {
                    // ...evaluate if not
                    EvaluationOutput = _keyCount + " / " + _totalKeysInput +
                        " - " + Math.Round((double)_keyCount / _totalKeysInput * 100) + "%" +
                        System.Environment.NewLine + "Current key number: " +
                        _keyCount + " - Done." + System.Environment.NewLine +
                        System.Environment.NewLine + "Started Evaluating...";
                    OnPropertyChanged("EvaluationOutput");

                    int i = 9;
                    bool boing = true;
                    while (i < 10 && boing)
                    {
                        Evaluate();
                        i++;
                    }
                    EvaluationOutput = _evaluationOutput;
                    OnPropertyChanged("EvaluationOutput");

                    GnuPlotScriptOutput = _gnuPlotScriptOutput;
                    OnPropertyChanged("GnuPlotScriptOutput");

                    GnuPlotDataOutput = _gnuPlotDataOutput;
                    OnPropertyChanged("GnuPlotDataOutput");
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

        // Either Add or increment
        public static void AddOrIncrement<K>(this Dictionary<K, TimeSpan> dict, K key, TimeSpan newValue)
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

        // try to devide
        public static bool DivideTimeSpan<K>(this Dictionary<K, TimeSpan> dict, K key, int divide)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = System.TimeSpan.FromMilliseconds(Math.Round(dict[key].TotalMilliseconds / divide, 0));
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
}

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
            BigInteger decryptionsCount = 0;

            TimeSpan runtimeCount = new TimeSpan();
            bool noRuntime = !_settings.CalculateRuntime;
            BigInteger restarts = 0;
            bool noRestarts = false;
            BigInteger populationSize = 0;
            bool noPopulationSize = false;
            BigInteger tabuSetSize = 0;
            bool noTabuSetSize = false;
            string testSeriesSeed = "";

            // TODO: number of derived keys?

            // evaluation key values
            ConcurrentDictionary<int, int> keyLengths = new ConcurrentDictionary<int, int>();
            ConcurrentDictionary<int, int> ciphertextLengths = new ConcurrentDictionary<int, int>();
            ConcurrentDictionary<TimeSpan, int> runtimes = new ConcurrentDictionary<TimeSpan, int>();

            // evaluation detailed values
            // key length
            ConcurrentDictionary<int, double> successPerKeyLength = new ConcurrentDictionary<int, double>();
            ConcurrentDictionary<int, double> decryptedPercentagesPerKeyLength = new ConcurrentDictionary<int, double>();
            ConcurrentDictionary<int, BigInteger> decryptionsPerKeyLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> restartsPerKeyLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> tabuSizesPerKeyLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> populationSizesPerKeyLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, TimeSpan> runtimePerKeyLength = new ConcurrentDictionary<int, TimeSpan>();
            // ciphertext length
            ConcurrentDictionary<int, double> successPerCiphertextLength = new ConcurrentDictionary<int, double>();
            ConcurrentDictionary<int, double> decryptedPercentagesPerCiphertextLength = new ConcurrentDictionary<int, double>();
            ConcurrentDictionary<int, BigInteger> decryptionsPerCiphertextLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> restartsPerCiphertextLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> tabuSizesPerCiphertextLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, BigInteger> populationSizesPerCiphertextLength = new ConcurrentDictionary<int, BigInteger>();
            ConcurrentDictionary<int, TimeSpan> runtimePerCiphertextLength = new ConcurrentDictionary<int, TimeSpan>();
            // runtime
            ConcurrentDictionary<TimeSpan, double> successPerRuntime = new ConcurrentDictionary<TimeSpan, double>();
            ConcurrentDictionary<TimeSpan, double> decryptedPercentagesPerRuntime = new ConcurrentDictionary<TimeSpan, double>();
            ConcurrentDictionary<TimeSpan, BigInteger> decryptionsPerRuntime = new ConcurrentDictionary<TimeSpan, BigInteger>();
            ConcurrentDictionary<TimeSpan, BigInteger> restartsPerRuntime = new ConcurrentDictionary<TimeSpan, BigInteger>();
            ConcurrentDictionary<TimeSpan, BigInteger> tabuSizesPerRuntime = new ConcurrentDictionary<TimeSpan, BigInteger>();
            ConcurrentDictionary<TimeSpan, BigInteger> populationSizesPerRuntime = new ConcurrentDictionary<TimeSpan, BigInteger>();

            bool firstElement = true;
            // counting and sorting the data into the dictionaries
            foreach (KeyValuePair<int, ExtendedEvaluationContainer> entry in _testRuns)
            {
                // current test run values
                ExtendedEvaluationContainer testRun = entry.Value;
                int keyLength = testRun.GetKey().Length;
                int ciphertextLength = testRun.GetCiphertext().Length;
                double percentDecrypted = testRun.GetPercentDecrypted();
                BigInteger decryptions = testRun.GetDecryptions();
                BigInteger currentRestarts = 0;
                BigInteger currentTabuSize = 0;
                BigInteger currentPopulationSize = 0;
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
                {
                    successCount++;
                    successPerKeyLength.AddOrUpdate(keyLength, 1, (length, success) => success + 1);
                    successPerCiphertextLength.AddOrUpdate(ciphertextLength, 1, (length, success) => success + 1);
                }

                // count the overall decryptions and decrypted percentages
                decryptedCount += percentDecrypted;
                decryptionsCount += decryptions;

                // count the decryptions and decrypted percentages per key and ciphertext lengths
                decryptedPercentagesPerKeyLength.AddOrUpdate(keyLength, percentDecrypted, (length, percent) => percent + percentDecrypted);
                decryptionsPerKeyLength.AddOrUpdate(keyLength, decryptions, (length, localDecryptions) => localDecryptions + decryptions);
                decryptedPercentagesPerCiphertextLength.AddOrUpdate(ciphertextLength, percentDecrypted, (length, percent) => percent + percentDecrypted);
                decryptionsPerCiphertextLength.AddOrUpdate(ciphertextLength, decryptions, (length, localDecryptions) => localDecryptions + decryptions);
                
                // count the restarts if every run contains a restart value greater zero
                if (!noRestarts && currentRestarts > 0)
                {
                    restarts += currentRestarts;
                    restartsPerKeyLength.AddOrUpdate(keyLength, currentRestarts, (length, localRestarts) => localRestarts + currentRestarts);
                    restartsPerCiphertextLength.AddOrUpdate(ciphertextLength, currentRestarts, (length, localRestarts) => localRestarts + currentRestarts);
                }
                else
                    noRestarts = true;

                // count the tabu set size if every run contains a size value greater zero
                if (!noTabuSetSize && currentTabuSize > 0)
                {
                    tabuSetSize += currentTabuSize;
                    tabuSizesPerKeyLength.AddOrUpdate(keyLength, currentTabuSize, (length, tabuSetSizes) => tabuSetSizes + currentTabuSize);
                    tabuSizesPerCiphertextLength.AddOrUpdate(ciphertextLength, currentTabuSize, (length, tabuSetSizes) => tabuSetSizes + currentTabuSize);
                }
                else
                    noTabuSetSize = true;

                // count the population size if every run contains a size value greater zero
                if (!noPopulationSize && currentPopulationSize > 0)
                {
                    populationSize += currentPopulationSize;
                    populationSizesPerKeyLength.AddOrUpdate(keyLength, currentPopulationSize, (length, populationSizes) => populationSizes + currentPopulationSize);
                    populationSizesPerCiphertextLength.AddOrUpdate(ciphertextLength, currentPopulationSize, (length, populationSizes) => populationSizes + currentPopulationSize);
                }
                else
                    noPopulationSize = true;

                // count all values per runtime and the runtime per key and ciphertext lengths
                TimeSpan time;
                if (!noRuntime && testRun.GetRuntime(out time))
                {
                    runtimeCount += time;
                    // update key value dictionary runtimes
                    runtimes.AddOrUpdate(time, 1, (runtime, count) => count + 1);

                    // detailed values
                    if (testRun.GetSuccessfull())
                        successPerRuntime.AddOrUpdate(time, 1, (runtime, success) => success + 1);
                    decryptedPercentagesPerRuntime.AddOrUpdate(time, percentDecrypted, (runtime, percent) => percent + percentDecrypted);
                    decryptionsPerRuntime.AddOrUpdate(time, decryptions, (runtime, localDecryptions) => localDecryptions + decryptions);
                    if (!noRestarts)
                        restartsPerRuntime.AddOrUpdate(time, currentRestarts, (runtime, localRestarts) => localRestarts + currentRestarts);
                    if (!noTabuSetSize)
                        tabuSizesPerRuntime.AddOrUpdate(time, currentTabuSize, (runtime, tabuSetSizes) => tabuSetSizes + currentTabuSize);
                    if (!noPopulationSize)
                        populationSizesPerRuntime.AddOrUpdate(time, currentPopulationSize, (runtime, populationSizes) => populationSizes + currentPopulationSize);

                    TimeSpan t = time + time;
                    runtimePerKeyLength.AddOrUpdate(keyLength, time, (length, runtime) => runtime + time);
                    runtimePerCiphertextLength.AddOrUpdate(ciphertextLength, time, (length, runtime) => runtime + time);
                }
                else
                {
                    noRuntime = true;
                }

                // update key value dictionaries keyLengths and ciphertextLengths
                keyLengths.AddOrUpdate(keyLength, 1, (length, count) => count + 1);
                ciphertextLengths.AddOrUpdate(ciphertextLength, 1, (length, count) => count + 1);

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
                    if (count > 1)
                    {
                        // detailed values
                        successPerRuntime.AddOrUpdate(time, 0, (runtime, success) => Math.Round(success / count * 100, 2));
                        decryptedPercentagesPerRuntime.AddOrUpdate(time, 0, (runtime, percent) => Math.Round(percent / count, 2));
                        decryptionsPerRuntime.AddOrUpdate(time, 0, (runtime, localDecryptions) => localDecryptions / count);
                        if (!noRestarts)
                            restartsPerRuntime.AddOrUpdate(time, 0, (runtime, localRestarts) => localRestarts / count);
                        if (!noTabuSetSize)
                            tabuSizesPerRuntime.AddOrUpdate(time, 0, (runtime, tabuSetSizes) => tabuSetSizes / count);
                        if (!noPopulationSize)
                            populationSizesPerRuntime.AddOrUpdate(time, 0, (runtime, populationSizes) => populationSizes / count);
                    }
                }
            }

            // calculate the overall average values
            BigInteger averageRestarts = 0;
            if (!noRestarts)
                averageRestarts = restarts / _testRuns.Count;
            BigInteger averageTabuSetSize = 0;
            if (!noTabuSetSize)
                averageTabuSetSize = tabuSetSize / _testRuns.Count;
            BigInteger averagePopulationSize = 0;
            if (!noPopulationSize)
                averagePopulationSize = populationSize / _testRuns.Count;

            // if the current key length count can be retrieved, calculate the average values
            foreach (var pair in keyLengths)
            {
                int keyLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 1)
                {
                    // calculate the detailed average values
                    successPerKeyLength.AddOrUpdate(keyLength, 0, (length, success) => Math.Round(success / count * 100, 2));
                    decryptedPercentagesPerKeyLength.AddOrUpdate(keyLength, 0, (length, percent) => Math.Round(percent / count, 2));
                    decryptionsPerKeyLength.AddOrUpdate(keyLength, 0, (length, localDecryptions) => localDecryptions / count);
                    runtimePerKeyLength.AddOrUpdate(keyLength, new TimeSpan(), (length, runtime) => TimeSpan.FromMilliseconds(Math.Round(runtime.TotalMilliseconds / count, 0)));

                    if (!noRestarts)
                        restartsPerKeyLength.AddOrUpdate(keyLength, 0, (length, localRestarts) => localRestarts / count);
                    
                    if (!noTabuSetSize)
                        tabuSizesPerKeyLength.AddOrUpdate(keyLength, 0, (length, tabuSetSizes) => tabuSetSizes / count);

                    if (!noPopulationSize)
                        populationSizesPerKeyLength.AddOrUpdate(keyLength, 0, (length, populationSizes) => populationSizes / count);
                    
                }
            }

            // if the current ciphertext length count can be retrieved, calculate the average values
            foreach (var pair in ciphertextLengths)
            {
                int ciphertextLength = pair.Key;
                int count = pair.Value;

                // if the count is greater 1, we have to divide through count to get the average
                if (count > 1)
                {
                    // calculate the detailed average values
                    successPerCiphertextLength.AddOrUpdate(ciphertextLength, 0, (length, success) => Math.Round(success / count * 100, 2));
                    decryptedPercentagesPerCiphertextLength.AddOrUpdate(ciphertextLength, 0, (length, percent) => Math.Round(percent / count, 2));
                    decryptionsPerCiphertextLength.AddOrUpdate(ciphertextLength, 0, (length, localDecryptions) => localDecryptions / count);
                    runtimePerCiphertextLength.AddOrUpdate(ciphertextLength, new TimeSpan(), (length, runtime) => TimeSpan.FromMilliseconds(Math.Round(runtime.TotalMilliseconds / count, 0)));

                    if (!noRestarts)
                        restartsPerCiphertextLength.AddOrUpdate(ciphertextLength, 1, (length, localRestarts) => localRestarts / count);
                    
                    if (!noTabuSetSize)
                        tabuSizesPerCiphertextLength.AddOrUpdate(ciphertextLength, 1, (length, tabuSetSizes) => tabuSetSizes / count);

                    if (!noPopulationSize)
                        populationSizesPerCiphertextLength.AddOrUpdate(ciphertextLength, 1, (length, populationSizes) => populationSizes / count); 
                }
            }

            // build the displayed string of occuring ciphertext lengths
            string ciphertextLengthString = "";
            var ciphertextLengthArray = ciphertextLengths.ToArray();
            int i = 1;
            ciphertextLengthString += ciphertextLengthArray[0].Key + " (" + ciphertextLengthArray[0].Value + ")";
            while (i < ciphertextLengthArray.Length)
            {
                ciphertextLengthString += ", " + ciphertextLengthArray[i].Key + " (" + ciphertextLengthArray[i].Value + ")";
                i++;
                if (keyLengths.Count > 6 && i == 3)
                {
                    i = ciphertextLengthArray.Length - 4;
                    ciphertextLengthString += " ...";
                }
            }

            // build the displayed string of occuring key lengths
            string keyLengthString = "";
            var keyLengthArray = keyLengths.ToArray();
            i = 1;
            keyLengthString += keyLengthArray[0].Key + " (" + keyLengthArray[0].Value + ")";
            while (i < keyLengthArray.Length)
            {
                keyLengthString += ", " + keyLengthArray[i].Key + " (" + keyLengthArray[i].Value + ")";
                i++;
                if (keyLengths.Count > 6 && i == 3)
                {
                    i = keyLengthArray.Length - 4;
                    keyLengthString += " ...";
                }
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

            // generate the GnuPlot script output string
            _gnuPlotScriptOutput = "###########################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save the GnuPlotData output in a file named " + System.Environment.NewLine; 
            _gnuPlotScriptOutput += "# " + evalMethod + ".dat" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# Save this into a file named " + evalMethod + ".p" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "# and  use 'load " + evalMethod + ".p'" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "###########################################################" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set   autoscale\t\t\t\t\t# scale axes automatically" + System.Environment.NewLine;

            // if percent
            _gnuPlotScriptOutput += "set yrange [0:115]" + System.Environment.NewLine;  // 115 to gain some space above 100%
            _gnuPlotScriptOutput += "set ytics (10, 20, 30, 40, 50, 60, 70, 80, 90, 100)" + System.Environment.NewLine;

            _gnuPlotScriptOutput += "unset log\t\t\t\t\t\t# remove any log-scaling" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "unset label\t\t\t\t\t\t# remove any previous labels" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xtic auto\t\t\t\t\t# set xtics automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set ytic auto\t\t\t\t\t# set ytics automatically" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set title \""+val1+", "+val2+", and "+val3+" dependent on "+keyValue+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set xlabel \""+xlabel+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "set ylabel \""+ylabel+"\"" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "plot    \""+evalMethod+".dat\" using 1:2 title '"+val1+"' with linespoints , \\" + System.Environment.NewLine;
            _gnuPlotScriptOutput += "        \"" + evalMethod + ".dat\" using 1:3 title '" + val2 + "' with linespoints" + System.Environment.NewLine;

            // generate the GnuPlot data output string
            _gnuPlotDataOutput = "###########################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Gnuplot script for plotting data from output GnuPlotData" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save this GnuPlotData output in a file named " + System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + evalMethod + ".dat" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Save the GnuPlotScript output into a file named " + System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + evalMethod + ".p" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# and use 'load " + evalMethod + ".p'" + System.Environment.NewLine;
            _gnuPlotDataOutput += "###########################################################" + System.Environment.NewLine;
            _gnuPlotDataOutput += "# Data for: "+evalMethod + System.Environment.NewLine;
            _gnuPlotDataOutput += "# " + keyValue + "\t\t" + val1 + "\t\t" + val2 + System.Environment.NewLine;
            
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
                    // Warning!
                    //continue;
                }

                double currentDecryptedPercentage = 0;
                if (!decryptedPercentagesPerCiphertextLength.TryGetValue(ciphertextLength, out currentDecryptedPercentage))
                {
                    // Warning!
                    continue;
                }

                // add detailed values to GnuPlot data output string
                _gnuPlotDataOutput += ciphertextLength + "\t\t\t\t\t\t" + currentSuccess + "\t\t\t\t" +
                    currentDecryptedPercentage + System.Environment.NewLine;

                BigInteger currentDecryptions = 0;
                if (!decryptionsPerCiphertextLength.TryGetValue(ciphertextLength, out currentDecryptions))
                {
                    // Warning!
                    //continue;
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

                BigInteger currentDecryptions = 0;
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


                BigInteger decryptions = EvaluationInput.GetDecryptions();
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
                    TriggerNextKey = KeyInput;
                    OnPropertyChanged("TriggerNextKey");
                }
                else
                {
                    // ...evaluate if not

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
}

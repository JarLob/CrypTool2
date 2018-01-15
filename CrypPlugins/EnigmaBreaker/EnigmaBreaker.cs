/* 
   Copyright 2008-2017, Arno Wacker, University of Kassel

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
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// additional needed libs
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;

//Cryptool 2.0 specific includes
using Cryptool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System.Windows.Threading;
using Cryptool.EnigmaBreaker.Properties;
using Cryptool.PluginBase.Attributes;


namespace Cryptool.EnigmaBreaker
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString);

    [Author("Arno Wacker, Matthäus Wander, Bastian Heuser", "arno.wacker@cryptool.org", "Universität Kassel, Universität Duisburg-Essen", "http://www.ais.uni-kassel.de")]
    [PluginInfo("Cryptool.EnigmaBreaker.Properties.Resources", "PluginCaption", "PluginTooltip", "EnigmaBreaker/DetailedDescription/doc.xml",
      "EnigmaBreaker/Images/Enigma.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class EnigmaBreaker : ICrypComponent
    {
        #region Constants

        internal const int ABSOLUTE = 0;
        internal const int PERCENTAGED = 1;
        internal const int LOG2 = 2;
        internal const int SINKOV = 3;

        #endregion

        #region Private variables

        private EnigmaBreakerSettings _settings;
        private AssignmentPresentation _presentation;
        private EnigmaCore _core;
        private EnigmaAnalyzer _analyzer;
        private string _ciphertextInput;
        private IDictionary<int, IDictionary<string, double[]>> _statistics;
        // FIXME: enable optional statistics input
        //private IDictionary<string, double[]> inputTriGrams;
        private string _bestPlaintext;
        private string _bestKey;
        private string _savedKey;
        private DateTime _startTime;
        private DateTime _endTime;
        public bool _isrunning;
        private bool _newCiphertext = false;
        private bool _newKey = false;

        private bool _running = false;
        private bool _stopped = false;

        // EVALUATION!
        private static int threads = 1;
        private static int currentThread = 0;
        private int _improvements = 0;

        private string _plaintextInput;
        private double _percentageInput;

        private bool _newPlaintext = false;
        private bool _newPercentage = false;

        private TimeSpan _runtime = new TimeSpan();
        private bool _finished = false;
        private int _totalNumberOfRestarts;
        private int _totalNumberOfDecryptions;
        private int[] _numberOfRestarts;
        private int[] _numberOfDecryptions;

        #endregion

        #region Private methods

        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {
            BestPlaintext = plaintextString;
            BestKey = keyString;
        }

        /// <summary>
        /// Set start time in UI
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                ((AssignmentPresentation)Presentation).startTime.Content = "" + _startTime;
                ((AssignmentPresentation)Presentation).endTime.Content = "";
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
            }, null);
        }

        /// <summary>
        /// Set end time in UI
        /// </summary>
        public void UpdateDisplayEnd(string currentlyAnalyzed)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + _endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((AssignmentPresentation)Presentation).currentlyAnalysed.Content = currentlyAnalyzed;

            }, null);
        }

        /// <summary>
        /// Set end time in UI
        /// </summary>
        private void UpdateDisplayEnd()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + _endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;

            }, null);
        }

        #region Formatting stuff

        internal class UnknownToken
        {
            internal string text;
            internal int position;

            internal UnknownToken(char c, int position)
            {
                this.text = char.ToString(c);
                this.position = position;
            }

            public override string ToString()
            {
                return "[" + text + "," + position + "]";
            }
        }

        IList<UnknownToken> unknownList = new List<UnknownToken>();
        IList<UnknownToken> lowerList = new List<UnknownToken>();
        /// <summary>
        /// Format the string to contain only alphabet characters in upper case
        /// </summary>
        /// <param name="text">The string to be prepared</param>
        /// <returns>The properly formated string to be processed direct by the encryption function</returns>
        private string preFormatInput(string text)
        {
            StringBuilder result = new StringBuilder();
            bool newToken = true;
            unknownList.Clear();
            lowerList.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                if (_settings.Alphabet.Contains(char.ToUpper(text[i])))
                {
                    newToken = true;
                    if (text[i] == char.ToLower(text[i])) //Solution for preserve FIXME underconstruction
                    {
                        if (_settings.UnknownSymbolHandling == 1)
                        {
                            lowerList.Add(new UnknownToken(text[i], result.Length));
                        }
                        else
                        {
                            lowerList.Add(new UnknownToken(text[i], i));
                        }
                        
                    }                                      //underconstruction end
                    result.Append(char.ToUpper(text[i])); // FIXME: shall save positions of lowercase letters
                }
                else if (_settings.UnknownSymbolHandling != 1) // 1 := remove
                {
                    // 0 := preserve, 2 := replace by X
                    char symbol = _settings.UnknownSymbolHandling == 0 ? text[i] : 'X';

                    if (newToken)
                    {
                        unknownList.Add(new UnknownToken(symbol, i));
                        newToken = false;
                    }
                    else
                    {
                        unknownList.Last().text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();

        }

        //// legacy code
        //switch (settings.UnknownSymbolHandling)
        //{
        //    case 0: // ignore
        //        result.Append(c);
        //        break;
        //    case 1: // remove
        //        continue;
        //    case 2: // replace by X
        //        result.Append('X');
        //        break;
        //}

        /// <summary>
        /// Formats the string processed by the encryption for presentation according
        /// to the settings given
        /// </summary>
        /// <param name="text">The encrypted text</param>
        /// <returns>The formatted text for output</returns>
        private string postFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in unknownList)
            {
                workstring.Insert(token.position, token.text);
            }

            foreach (UnknownToken token in lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token.position];
                workstring.Remove(token.position, 1);
                workstring.Insert(token.position, char.ToLower(help));
            }                                           //underconstruction end

            switch (_settings.CaseHandling)
            {
                default:
                case 0: // preserve
                    // FIXME: shall restore lowercase letters
                    return workstring.ToString();
                case 1: // upper
                    return workstring.ToString().ToUpper();
                case 2: // lower
                    return workstring.ToString().ToLower();
            }
        }

        #endregion

        #region Analyzer event handler

        /// <summary>
        /// This eventhandler is called, when the analyzer has an intermediate result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void analyzer_OnIntermediateResult(object sender, IntermediateResultEventArgs e)
        {
            // Got an intermidate results from the analyzer, hence display it
            _bestPlaintext = postFormatOutput(e.Result);
            //OnPropertyChanged("BestPlaintext");
        }*/

        #endregion

        #region n-gram frequencies

        private IDictionary<string, double[]> LoadDefaultStatistics(int length)
        {
            Dictionary<string, double[]> grams = new Dictionary<string, double[]>();

            StreamReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, GetStatisticsFilename(length)));

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;

                string[] tokens = WordTokenizer.tokenize(line).ToArray();
                if (tokens.Length == 0)
                    continue;
                Debug.Assert(tokens.Length == 2, "Expected 2 tokens, found " + tokens.Length + " on one line");

                grams.Add(tokens[0], new double[] { Double.Parse(tokens[1]), 0, 0, 0 });
            }

            double sum = grams.Values.Sum(item => item[ABSOLUTE]);
            LogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

            // calculate scaled values
            foreach (double[] g in grams.Values)
            {
                g[PERCENTAGED] = g[ABSOLUTE] / sum;
                g[LOG2] = Math.Log(g[ABSOLUTE], 2);
                g[SINKOV] = Math.Log(g[PERCENTAGED], Math.E);
            }

            return grams;
        }

        /// <summary>
        /// Get file name for default n-gram frequencies.
        /// </summary>
        /// <param name="length"></param>
        /// <exception cref="NotSupportedException">No default n-gram frequencies available</exception>
        /// <returns></returns>
        private string GetStatisticsFilename(int length)
        {
            if (length < 1)
            {
                throw new ArgumentOutOfRangeException("There is no known default statistic for an n-gram length of " + length);
            }

            return "Enigma_" + length + "gram_Frequency.txt";
        }

        #endregion

        #endregion

        #region Constructor

        public EnigmaBreaker()
        {
            this._settings = new EnigmaBreakerSettings();
            this._presentation = new AssignmentPresentation();
            this._core = new EnigmaCore(this);
            this._analyzer = new EnigmaAnalyzer(this);
            //this._analyzer.OnIntermediateResult += new EventHandler<IntermediateResultEventArgs>(analyzer_OnIntermediateResult);
            this._statistics = new Dictionary<int, IDictionary<string, double[]>>();
            _presentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
            
            this._settings.PropertyChanged += settings_OnPropertyChange;
        }

        #endregion

        #region Events

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void newInput(object sender, EventArgs args)
        {
                _running = false;
            
        }

        private void fireLetters(object sender, EventArgs args)  
        {
            Object[] carrier = sender as Object[];

            this._bestPlaintext = (String)carrier[0] ;
            int x = (int)carrier[1];
            int y = (int)carrier[2];
            
            ShowProgress(x,y);
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            EnigmaBreakerSettings dummyset = sender as EnigmaBreakerSettings;
        }

        #endregion

        #region IPlugin properties

        public ISettings Settings
        {
            get { return this._settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        #endregion

        #region Connector properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get { return this._ciphertextInput; }
            set
            {
                if (value != _ciphertextInput)
                {
                    this._ciphertextInput = value;
                    this._newCiphertext = true;
                    OnPropertyChanged("Ciphertext");
                }
            }
        }

        // EVALUATION!
        [CryptoBenchmark()]
        [PropertyInfo(Direction.InputData, "PlaintextInputCaption", "PlaintextInputTooltip", false)]
        public string CorrectPlaintextInput
        {
            get { return this._plaintextInput; }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != this._plaintextInput)
                {
                    this._plaintextInput = value;
                    OnPropertyChanged("CorrectPlaintextInput");
                    this._newPlaintext = true;
                }
            }
        }

        // EVALUATION!
        [CryptoBenchmark()]
        [PropertyInfo(Direction.InputData, "DecryptionPercentageCaption", "DecryptionPercentageTooltip", false)]
        public double MinimalCorrectPercentage
        {
            get { return this._percentageInput; }
            set
            {
                this._percentageInput = value;
                this._newPercentage = true;
                OnPropertyChanged("MinimalCorrectPercentage");
            }
        }

        //[PropertyInfo(Direction.InputData, "InputGramsCaption", "InputGramsTooltip", "", false, false, QuickWatchFormat.Text, "FrequencyTest.QuickWatchDictionary")]
        //public IDictionary<string, double[]> InputGrams
        //{
        //    get { return this.inputTriGrams; }
        //    set
        //    {
        //        if (value != inputTriGrams)
        //        {
        //            this.inputTriGrams = value;
        //            OnPropertyChanged("InputTriGrams");
        //        }
        //    }
        //}

        [PropertyInfo(Direction.OutputData, "BestPlaintextCaption", "BestPlaintextTooltip", false)]
        public string BestPlaintext
        {
            get { return this._bestPlaintext; }
            set
            {
                _bestPlaintext = value;
                OnPropertyChanged("BestPlaintext");
            }
        }

        [PropertyInfo(Direction.OutputData, "BestKeyCaption", "BestKeyTooltip", false)]
        public string BestKey
        {
            get { return this._bestKey; }
            set
            {
                _bestKey = value;
                OnPropertyChanged("BestKey");
            }
        }

        // EVALUATION!
        [CryptoBenchmark()]
        [PropertyInfo(Direction.OutputData, "EvaluationOutputCaption", "EvaluationOutputTooltip", true)]
        public EvaluationContainer EvaluationOutput
        {
            get
            {
                var elapsedtime = DateTime.Now.Subtract(_startTime);
                _runtime = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, elapsedtime.Milliseconds);
                var ID = _ciphertextInput.GetHashCode();

                return new EvaluationContainer(ID, _runtime, _totalNumberOfDecryptions, _totalNumberOfRestarts);
            }
        }


        #endregion

        #region Public methods

        public bool Finished
        {
            get { return this._finished; }
            set { this._finished = value; }
        }

        public bool IncrementDecryptionsCountForThread(int thread)
        {
            if (_numberOfDecryptions.Length - 1 < thread)
                return false;

            _numberOfDecryptions[thread]++;

            return true;
        }

        public bool IncrementRestartsCountForThread(int thread)
        {
            if (_numberOfRestarts.Length - 1 < thread)
                return false;

            _numberOfRestarts[thread]++;

            return true;
        }

        /*
        public void SetBestKey (string key)
        {
            this._bestKey = key;
        }*/

        public void PreExecution()
        {
            _isrunning = true;

            _running = false;
            _stopped = false;

            if (_settings.Model != 3 && _settings.Model != 2)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("This simulator is work in progress. As of right now only Enigma I and Enigma Reichsbahn (Rocket) is supported!!", this, NotificationLevel.Warning));
                return;
            }

            // remember the current key-setting, in order to restore on stop
            _savedKey = _settings.InitialRotorPos;

            //configure the enigma
            _core.setInternalConfig(_settings.Rotor1, _settings.Rotor2, _settings.Rotor3, _settings.Rotor4,
                        _settings.Reflector, _settings.Ring1, _settings.Ring2, _settings.Ring3, _settings.Ring4,
                        _settings.PlugBoard);
        }

        public void Execute()
        {
            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).BestList.Clear();
            }, null);

            _stopped = false;
            ShowProgress(0, 1);

            if (!_newCiphertext)
                return;

            if (_settings.Model != 3 && _settings.Model != 2)
            {
                LogMessage("This simulator is work in progress. As of right now only Enigma I and Enigma Reichsbahn (Rocket) is supported!!", NotificationLevel.Error);
                return;
            }

            //prepare for analysis
            LogMessage("ANALYSIS: Preformatting text...", NotificationLevel.Debug);
            string preformatedText = preFormatInput(_ciphertextInput);

            // EVALUATION! if stopping is active, percentage and plaintext are necessary
            if (_settings.StopIfPercentReached &&
                    (!_newPercentage || !_newPlaintext))
            {
                // wait for new values
                return;
            }

            // consume values
            _newCiphertext = false;
            _newPercentage = false;
            _newPlaintext = false;

            // EVALUATION!
            if (_percentageInput <= 0 ||
                _percentageInput > 100)
            {
                _percentageInput = 95;
            }
            _runtime = new TimeSpan();

            _totalNumberOfRestarts = 0;
            _totalNumberOfDecryptions = 0;
            _numberOfRestarts = new int[1 /*_settings.CoresUsed*/]; // prepared for multi threading
            _numberOfDecryptions = new int[1 /*_settings.CoresUsed*/];
            for (int t = 1 /*_settings.CoresUsed*/ - 1; t >= 0; t--)
            {
                _numberOfRestarts[t] = 0;
                _numberOfDecryptions[t] = 0;
            }

            UpdateDisplayStart();

            // perform the analysis
            foreach (string decrypt in _analyzer.Analyze(preformatedText))
            {
                UpdateDisplayEnd();
                LogMessage(decrypt, NotificationLevel.Debug);

                // fire all best candidates
                _bestPlaintext = postFormatOutput(decrypt);
            }
            UpdateDisplayEnd();

            // We update finally the keys/second of the ui
            _analyzer.FinalUiUpdate();

            // EVALUATION!
            for (var i = 0; i < threads; i++)
            {
                _totalNumberOfRestarts += _numberOfRestarts[i];
                _totalNumberOfDecryptions += _numberOfDecryptions[i];
            }
            // adding 1 for the last decryption
            _totalNumberOfDecryptions++;
            
            if (_presentation.BestList.Count > 0)
            {
                BestPlaintext = _presentation.BestList[0].Text;
                BestKey = _presentation.BestList[0].Key;

                // EVALUATION!
                _finished = false;
                OnPropertyChanged("EvaluationOutput");
            }

            // "fire" the outputs
            //OnPropertyChanged("BestPlaintext");
            //OnPropertyChanged("BestKey");

            ShowProgress(1, 1);
        }

        public void PostExecution()
        {
            _running = false;
            _isrunning = false;
            _ciphertextInput = String.Empty;
            _stopped = false;
            _startTime = new DateTime();
            _endTime = new DateTime();
            _bestKey = String.Empty;
            _bestPlaintext = String.Empty;

            // EVALUATION! reset values
            _plaintextInput = String.Empty;
            _percentageInput = new Double();
            _runtime = new TimeSpan();
            _totalNumberOfRestarts = 0;
            _totalNumberOfDecryptions = 0;
            _improvements = 0;
            _finished = false;
        }

        public void Stop()
        {
            _stopped = true;
            _analyzer.StopAnalysis();
        }

        public void Initialize()
        {
            //LogMessage("Initializing..", NotificationLevel.Debug);
        }

        public void Dispose()
        {
            //LogMessage("Dispose", NotificationLevel.Debug);
        }

        /// <summary>
        /// Logs a message to the Cryptool console
        /// </summary>
        public void LogMessage(string msg, NotificationLevel level)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, level));
        }

        /// <summary>
        /// Sets the progress bar for this plugin
        /// </summary>
        /// <param name="val"></param>
        /// <param name="max"></param>
        public void ShowProgress(double val, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(val, max));
        }

        /// <summary>
        /// Returns a formated string with all plugs from a given substitution string
        /// </summary>
        /// <param name="pb">The substitution string for a plugboard</param>
        /// <returns>A list of plugs</returns>
        public string pB2String(string pb)
        {
            if (pb.Length != _settings.Alphabet.Length)
                return "-- no plugs --";
            
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < _settings.Alphabet.Length; i++)
            {
                if (_settings.Alphabet[i] != pb[i] && !result.ToString().Contains(_settings.Alphabet[i]))
                {
                    if (result.Length > 0)
                        result.Append(' ');

                    result.Append(_settings.Alphabet[i].ToString() + pb[i].ToString());
                }
            }

            if (result.Length == 0)
                result.Append("-- no plugs --");

            return result.ToString();
        }

        public IDictionary<string, double[]> GetStatistics(int gramLength)
        {
            // FIXME: inputTriGrams is not being used!

            // FIXME: implement exception handling

            if (!_statistics.ContainsKey(gramLength))
            {
                LogMessage("Trying to load default statistics for " + gramLength + "-grams", NotificationLevel.Info);
                _statistics[gramLength] = LoadDefaultStatistics(gramLength);
            }

            return _statistics[gramLength];
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }

    public class ResultEntry
    {
        public int Ranking { get; set; }
        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

        public double ExactValue
        {
            get { return Math.Abs(Value); }
        }

        public int KeyLength
        {
            get
            {
                return Key.Length;
            }
        }
    }    
}

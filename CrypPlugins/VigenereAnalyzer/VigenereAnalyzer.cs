/*
   Copyright 2017 Nils Kopal, Applied Information Security, Uni Kassel
   https://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;

namespace Cryptool.VigenereAnalyzer
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString);

    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Uni Kassel", "https://www.ais.uni-kassel.de")]
    [PluginInfo("Cryptool.VigenereAnalyzer.Properties.Resources",
    "PluginCaption", "PluginTooltip", "", "VigenereAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class VigenereAnalyzer : ICrypComponent
    {
        private const int MaxBestListEntries = 100;
        private readonly AssignmentPresentation _presentation = new AssignmentPresentation();
        private string _plaintext;
        private string _key;
        private readonly VigenereAnalyzerSettings _settings = new VigenereAnalyzerSettings();
        private double[,,] _trigrams;
        private double[,,,] _quadgrams;
        private bool _stopped;
        private DateTime _startTime;
        private DateTime _endTime;
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // EVALUATION!
        private static int threads = 1;
        private static int currentThread = 0;
        private int _improvements = 0;
        
        private string _ciphertextInput;
        private string _plaintextInput;
        private double _percentageInput;

        private bool _newCiphertext = false;
        private bool _newPlaintext = false;
        private bool _newPercentage = false;

        private TimeSpan _runtime = new TimeSpan();
        private bool _finished = false;
        private int _totalNumberOfRestarts;
        private int _totalNumberOfDecryptions;
        private int[] _numberOfRestarts;
        private int[] _numberOfDecryptions;

        public VigenereAnalyzer()
        {
            _presentation.UpdateOutputFromUserChoice+=UpdateOutputFromUserChoice;
        }

        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {
            Plaintext = plaintextString;
            Key = keyString;
        }
        
        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext 
        {
            get { return _ciphertextInput; }
            set
            {
                if (!String.IsNullOrEmpty(value) && value != _ciphertextInput)
                {
                    _ciphertextInput = value;
                    _newCiphertext = true;
                    OnPropertyChanged("Ciphertext");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "VigenereAlphabetCaption", "VigenereAlphabetTooltip", false)]
        public string VigenereAlphabet { get; set; }

        // EVALUATION!
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

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public String Plaintext
        {
            get { return _plaintext; }
            set { _plaintext = value; OnPropertyChanged("Plaintext"); }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", true)]
        public String Key
        {
            get { return _key; }
            set { _key = value; OnPropertyChanged("Key"); }
        }

        // EVALUATION!
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
        
        public void PreExecution()
        {
            if (_settings.Language == Language.German)
            {
                Load3Grams("de-3gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                Load4Grams("de-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖß");
            }
            else if (_settings.Language == Language.Englisch)
            {
                Load3Grams("en-3gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                Load4Grams("en-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            else if (_settings.Language == Language.Spanish)
            {
                Load3Grams("es-3gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ");
                Load4Grams("es-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ");
            }
            else
            {
                Load3Grams("en-3gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                Load4Grams("en-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            VigenereAlphabet = Alphabet;
        }

        public void PostExecution()
        {
            Ciphertext = null;
            VigenereAlphabet = null;

            _plaintext = String.Empty;
            _plaintextInput = String.Empty;
            _key = String.Empty;
            _ciphertextInput = String.Empty;
            _percentageInput = new Double();
            _stopped = false;
            _startTime = new DateTime();
            _endTime = new DateTime();

            // EVALUATION! reset values
            _runtime = new TimeSpan();
            _totalNumberOfRestarts = 0;
            _totalNumberOfDecryptions = 0;
            _improvements = 0;
            _finished = false;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void Execute()
        {
            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).BestList.Clear();
            }, null);

            _stopped = false;
            ProgressChanged(0, 1);

            if (!_newCiphertext)
            {
                return;
            }
            if (string.IsNullOrEmpty(Ciphertext))
            {
                GuiLogMessage("No Ciphertext given for analysis!", NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(VigenereAlphabet))
            {
                GuiLogMessage("No Vigenere Alphabet given for analysis!", NotificationLevel.Error);
                return;
            }


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

            var ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(_ciphertextInput.ToUpper(), Alphabet), Alphabet);

            if (_settings.ToKeyLength > ciphertext.Length)
            {
                _settings.ToKeyLength = ciphertext.Length;
                GuiLogMessage("Max tested keylength can not be longer than the plaintext. Set max tested keylength to plaintext length.",NotificationLevel.Warning);
            }
            if (_settings.ToKeyLength < _settings.FromKeylength)
            {
                var temp = _settings.ToKeyLength;
                _settings.ToKeyLength = _settings.FromKeylength;
                _settings.FromKeylength = temp;
            }

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
            for (var keylength = _settings.FromKeylength; keylength <= _settings.ToKeyLength && !_finished; keylength++)
            {
                UpdateDisplayEnd(keylength);
                HillclimbVigenere(ciphertext, keylength, _settings.Restarts);
                if (_stopped)
                {                    
                    return;
                }
            }
            UpdateDisplayEnd(_settings.ToKeyLength);

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
                Plaintext = _presentation.BestList[0].Text;
                Key = _presentation.BestList[0].Key;

                // EVALUATION!
                _finished = false;
                OnPropertyChanged("EvaluationOutput");
            }
            
            ProgressChanged(1, 1);            
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
        private void UpdateDisplayEnd(int keylength)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + _endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((AssignmentPresentation)Presentation).currentAnalysedKeylength.Content = "" + keylength;

            }, null);
        }

        public void Stop()
        {
            _stopped = true;
        }

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Hillclimbs Vigenere or Vigenere Autokey
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keylength"></param>
        /// <param name="restarts"></param>
        private void HillclimbVigenere(int[] ciphertext, int keylength, int restarts = 100)
        {
            var globalbestkeycost = double.MinValue;
            var bestkey = new int[keylength];
            var alphabetlength = Alphabet.Length;
            var numalphabet = MapTextIntoNumberSpace(Alphabet, Alphabet);
            var numvigalphabet = MapTextIntoNumberSpace(VigenereAlphabet, Alphabet);
            var random = new Random(Guid.NewGuid().GetHashCode());
            var totalrestarts = restarts;

            var lasttime = DateTime.Now;
            var keys = 0;

            var runkey = new int[keylength];

            while (restarts > 0)
            {
                // generate random key
                for (var i = 0; i < runkey.Length; i++)
                    runkey[i] = numalphabet[random.Next(alphabetlength)];

                bool foundbetter;
                var bestkeycost = double.MinValue;

                var plaintext = _settings.Mode == Mode.Vigenere 
                    ? DecryptVigenereOwnAlphabet(ciphertext, runkey, numvigalphabet) 
                    : DecryptAutokeyOwnAlphabet(ciphertext, runkey, numvigalphabet);

                do
                {
                    foundbetter = false;
                    // permute key
                    for (var i = 0; i < keylength && !_finished; i++)
                    {
                        for (var j = 0; j < alphabetlength && !_finished; j++)
                        {
                            var oldLetter = runkey[i];
                            runkey[i] = j;
                            plaintext = _settings.Mode == Mode.Vigenere
                                ? DecryptVigenereOwnAlphabetInPlace(plaintext, runkey, numvigalphabet, i, ciphertext)
                                : DecryptAutokeyOwnAlphabetInPlace(plaintext, runkey, numvigalphabet, i, ciphertext);

                            // EVALUATION! Count the number of decryptions per thread
                            _numberOfDecryptions[currentThread]++;

                            keys++;
                            var costvalue = 0.0;
                            switch (_settings.CostFunction)
                            {
                                case CostFunction.Trigrams:
                                    costvalue = CalculateTrigramCost(_trigrams, plaintext) + (_settings.KeyStyle == KeyStyle.NaturalLanguage ? CalculateTrigramCost(_trigrams, runkey) : 0);
                                    break;
                                case CostFunction.Quadgrams:
                                    costvalue = CalculateQuadgramCost(_quadgrams, plaintext) + (_settings.KeyStyle == KeyStyle.NaturalLanguage ? CalculateQuadgramCost(_quadgrams, runkey) : 0);
                                    break;
                                case CostFunction.Both:
                                    var tri = CalculateTrigramCost(_trigrams, plaintext) + (_settings.KeyStyle == KeyStyle.NaturalLanguage ? CalculateTrigramCost(_trigrams, runkey) : 0);
                                    var quad = CalculateQuadgramCost(_quadgrams, plaintext) + (_settings.KeyStyle == KeyStyle.NaturalLanguage ? CalculateQuadgramCost(_quadgrams, runkey) : 0);
                                    costvalue += (0.5 * tri + 0.5 * quad);
                                    break;
                                case CostFunction.IoC:
                                    costvalue = CalculateIoC(plaintext);
                                    break;
                            }                            
                            
                            if (costvalue > bestkeycost)
                            {
                                bestkeycost = costvalue;
                                bestkey = (int[]) runkey.Clone();
                                foundbetter = true;

                                // EVALUATION!
                                if (bestkeycost > globalbestkeycost)
                                {

                                    // EVALUATION! increase the _improvements counter
                                    _improvements++;
                                    // only map the current plaintext into text space and
                                    // compare its correctness, if the stopping option is
                                    // active, a percentage is provided and the _improvements
                                    // have reached the specified minimum (frequency)
                                    if (_settings.StopIfPercentReached &&
                                        _percentageInput != 0 &&
                                        _improvements % _settings.ComparisonFrequency == 0)
                                    {
                                        double progress1 = (keylength - _settings.FromKeylength) * totalrestarts + totalrestarts - restarts;
                                        double progress2 = (_settings.ToKeyLength - _settings.FromKeylength + 1) * totalrestarts;
                                        double progress = progress1 / progress2 * 100.0;

                                        string currentBestPlaintext = MapNumbersIntoTextSpace(_settings.Mode == Mode.Vigenere ? DecryptVigenereOwnAlphabet(ciphertext, bestkey, MapTextIntoNumberSpace(VigenereAlphabet, Alphabet)) :
                                            DecryptAutokeyOwnAlphabet(ciphertext, bestkey, MapTextIntoNumberSpace(VigenereAlphabet, Alphabet)), Alphabet);
                                        
                                        // calculate string similarity between the current
                                        // plaintext and the provided original plaintext
                                        double currentlyCorrect = _plaintextInput.CalculateSimilarity(currentBestPlaintext) * 100;

                                        Console.WriteLine(progress + "% - currentlyCorrect: " + currentlyCorrect + "% - best key cost:" + bestkeycost);

                                        // stop the algorithm if the percentage is high enough
                                        if (currentlyCorrect >= _percentageInput)
                                        {
                                            _finished = true;
                                            restarts = 0;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //reset key
                                runkey[i] = oldLetter;
                                if (j == alphabetlength - 1)
                                {
                                    plaintext = _settings.Mode == Mode.Vigenere
                                        ? DecryptVigenereOwnAlphabetInPlace(plaintext, runkey, numvigalphabet, i, ciphertext)
                                        : DecryptAutokeyOwnAlphabet(ciphertext, runkey, numvigalphabet);
                                }
                            }

                            if (_stopped)
                            {
                                return;
                            }

                            // print keys/sec in the ui
                            if (DateTime.Now >= lasttime.AddMilliseconds(1000))
                            {
                                var keysDispatcher = keys;
                                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    try
                                    {
                                        _presentation.currentSpeed.Content = string.Format("{0:0,0}", keysDispatcher);
                                    }
                                    // ReSharper disable once EmptyGeneralCatchClause
                                    catch (Exception e)
                                    {
                                        //wtf?
                                        Console.WriteLine("e1: " + e);
                                    }
                                }, null);
                                keys = 0;
                                lasttime = DateTime.Now;
                            }
                        }
                    }
                    runkey = (int[])bestkey.Clone();
                } while (foundbetter && !_finished);

                UpdateDisplayEnd(keylength);
                restarts--;

                // EVALUATION! count the number of restarts per thread
                _numberOfRestarts[currentThread]++;

                if (bestkeycost > globalbestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    AddNewBestListEntry(bestkey, globalbestkeycost, ciphertext);
                }
                ProgressChanged((keylength - _settings.FromKeylength) * totalrestarts + totalrestarts - restarts, (_settings.ToKeyLength - _settings.FromKeylength + 1) * totalrestarts);
            
                // EVALUATION!
                if (_finished)
                    return;
            }
            //We update finally the keys/second of the ui
            var keysDispatcher2 = keys;
            var lasttimeDispatcher2 = lasttime;
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.currentSpeed.Content = string.Format("{0:0,0}", Math.Round(keysDispatcher2 * 1000 / (DateTime.Now - lasttimeDispatcher2).TotalMilliseconds, 0));
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception e)
                {
                    //wtf?
                    Console.WriteLine("e2: " + e);
                }
            }, null);

            // EVALUATION!
            return;
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ciphertext"></param>
        private void AddNewBestListEntry(int[] key, double value,int[] ciphertext)
        {            
            var entry = new ResultEntry
            {
                Key = MapNumbersIntoTextSpace(key, Alphabet),
                Text = MapNumbersIntoTextSpace(_settings.Mode == Mode.Vigenere ? DecryptVigenereOwnAlphabet(ciphertext, key,  MapTextIntoNumberSpace(VigenereAlphabet,Alphabet)) :
                    DecryptAutokeyOwnAlphabet(ciphertext, key, MapTextIntoNumberSpace(VigenereAlphabet, Alphabet)), Alphabet),
                Value = value
            };

            if (_presentation.BestList.Count == 0)
            {
                _plaintext = entry.Text;
                _key = entry.Key;
            }
            else if(entry.Value > _presentation.BestList.First().Value)
            {
                _plaintext = entry.Text;
                _key = entry.Key;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0 && entry.Value <= _presentation.BestList.Last().Value)
                    {
                        return;
                    }
                    _presentation.BestList.Add(entry);
                    _presentation.BestList = new ObservableCollection<ResultEntry>(_presentation.BestList.OrderByDescending(i => i.Value));
                    if (_presentation.BestList.Count > MaxBestListEntries)
                    {
                        _presentation.BestList.RemoveAt(MaxBestListEntries);
                    }
                    var ranking = 1;
                    foreach (var e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    _presentation.ListView.DataContext = _presentation.BestList;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception e)
                {
                    //wtf?
                    Console.WriteLine("e3: " + e);
                }
            }, null); 
        }

        /// <summary>
        /// Decrypts the given plaintext using the given key and an own alphabet
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] DecryptVigenereOwnAlphabet(int[] ciphertext, int[] key, int[] alphabet)
        {
            var plaintextlength = ciphertext.Length; // improves the speed because length has not to be calculated in the loop
            var plaintext = new int[plaintextlength];
            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = 0; position < plaintextlength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts the given plaintext using the given key and an own alphabet in place of the given plaintext exchanging only the symbol defined by the offset
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <param name="offset"></param>
        /// <param name="oldciphertext"></param>
        /// <returns></returns>
        public static int[] DecryptVigenereOwnAlphabetInPlace(int[] plaintext, int[] key, int[] alphabet, int offset, int[] oldciphertext)
        {
            var plaintextlength = plaintext.Length; // improves the speed because length has not to be calculated in the loop
            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = offset; position < plaintextlength; position += keylength)
            {
                plaintext[position] = alphabet[(lookup[oldciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts the given plaintext using the given key and an own alphabet
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] DecryptAutokeyOwnAlphabet(int[] ciphertext, int[] key, int[] alphabet)
        {
            var plaintextlength = ciphertext.Length; // improves the speed because length has not to be calculated in the loop
            var plaintext = new int[plaintextlength];
            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = 0; position < keylength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            for (var position = keylength; position < plaintextlength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[plaintext[position - keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts the given plaintext using the given key and an own alphabet in place of the given plaintext exchanging only the symbol defined by the offset
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <param name="offset"></param>
        /// <param name="oldciphertext"></param>
        /// <returns></returns>
        public static int[] DecryptAutokeyOwnAlphabetInPlace(int[] plaintext, int[] key, int[] alphabet, int offset, int[] oldciphertext)
        {
            var plaintextlength = plaintext.Length; // improves the speed because length has not to be calculated in the loop

            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = offset; position < keylength; position+=keylength)
            {
                plaintext[position] = alphabet[(lookup[oldciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            for (var position = keylength + offset; position < plaintextlength; position+=keylength)
            {
                plaintext[position] = alphabet[(lookup[oldciphertext[position]] - lookup[plaintext[position - keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Calculate cost value based on index of coincidence
        /// </summary>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static double CalculateIoC(int[] plaintext)
        {
            Dictionary<int, UInt64> countChars = new Dictionary<int, UInt64>();

            foreach (int c in plaintext)
                if (countChars.ContainsKey(c)) countChars[c]++; else countChars.Add(c, 1);
            
            UInt64 value = 0;

            foreach (UInt64 cnt in countChars.Values)
                value += cnt * (cnt - 1);

            UInt64 N = (UInt64)plaintext.Length;
            return (double)value/(N*(N-1));
        }

        /// <summary>
        /// Calculate cost value based on 3-grams
        /// </summary>
        /// <param name="ngrams3"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static double CalculateTrigramCost(double[,,] ngrams3, int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 2;

            for (var i = 0; i < end; i++)
            {
                value += ngrams3[plaintext[i], plaintext[i + 1], plaintext[i + 2]];
            }
            return value;
        }

        /// <summary>
        /// Calculate cost value based on 4-grams
        /// </summary>
        /// <param name="ngrams4"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static double CalculateQuadgramCost(double[, , ,] ngrams4, int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 3;

            for (var i = 0; i < end; i++)
            {
                value += ngrams4[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];
            }
            return value;
        }

        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var i in numbers)
            {
                builder.Append(alphabet[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (var c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        /// <summary>
        /// Removes all chars from a given string which are not part of the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var c in text)
            {
                if (alphabet.Contains(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Load 3Gram file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="alphabet"></param>
        private void Load3Grams(string filename, string alphabet)
        {
            _trigrams = new double[alphabet.Length, alphabet.Length, alphabet.Length];
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            for (int k = 0; k < alphabet.Length; k++)
                            {                                
                                    var bytes = reader.ReadBytes(8);
                                    _trigrams[i, j, k] = BitConverter.ToDouble(bytes, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load 4Gram file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="alphabet"></param>
        private void Load4Grams(string filename, string alphabet)
        {
            _quadgrams = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            for (int k = 0; k < alphabet.Length; k++)
                            {
                                for (int l = 0; l < alphabet.Length; l++)
                                {
                                    var bytes = reader.ReadBytes(8);
                                    _quadgrams[i, j, k, l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
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

﻿/*
   Copyright 2021 Nils Kopal, Applied Information Security, Uni Kassel
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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase.Utils;
using Cryptool.CrypAnalysisViewControl;

namespace Cryptool.VigenereAnalyzer
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString);
    public delegate int[] DecryptVigenereFunction(int[] plaintext, int[] key, int[] alphabet, int offset, int[] oldciphertext);

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
        private bool _stopped;
        private DateTime _startTime;
        private DateTime _endTime;
        private const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string Alphabet = null;
        private string _ciphertextInput;
        private Grams _grams = null;

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
                if (!string.IsNullOrEmpty(value) && value != _ciphertextInput)
                {
                    _ciphertextInput = value;
                    OnPropertyChanged("Ciphertext");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "VigenereAlphabetCaption", "VigenereAlphabetTooltip", false)]
        public string VigenereAlphabet { get; set; }
             

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
      
        
        public void PreExecution()
        {
            Alphabet = DefaultAlphabet;
            VigenereAlphabet = DefaultAlphabet;
        }

        public void PostExecution()
        {
            Ciphertext = null;
            VigenereAlphabet = null;            
            _key = string.Empty;
            _ciphertextInput = string.Empty;            
            _stopped = false;
            _startTime = new DateTime();
            _endTime = new DateTime();           
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
            
            if (string.IsNullOrEmpty(Ciphertext))
            {
                GuiLogMessage("No Ciphertext given for analysis!", NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(VigenereAlphabet))
            {
                GuiLogMessage("No Vigenère Alphabet given for analysis!", NotificationLevel.Error);
                return;
            }

            //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
            _grams = LanguageStatistics.CreateGrams(_settings.Language, (LanguageStatistics.GramsType)(_settings.GramsType + 1), false);

            //Create a unique alphabet from the given (maybe permuted) alphabet
            if (!string.IsNullOrEmpty(VigenereAlphabet))
            {
                Alphabet = string.Join("", VigenereAlphabet.Distinct().OrderBy(q => q).ToArray());
                _grams.ReduceAlphabet(Alphabet);
            }

            var ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(_ciphertextInput.ToUpper(), Alphabet), Alphabet);

            if (_settings.ToKeyLength > ciphertext.Length)
            {
                _settings.ToKeyLength = ciphertext.Length;
                GuiLogMessage("Max tested keylength cannot be longer than the plaintext. Set max tested keylength to plaintext length.",NotificationLevel.Warning);
            }
            if (_settings.ToKeyLength < _settings.FromKeylength)
            {
                var temp = _settings.ToKeyLength;
                _settings.ToKeyLength = _settings.FromKeylength;
                _settings.FromKeylength = temp;
            }      

            UpdateDisplayStart();
            for (var keylength = _settings.FromKeylength; keylength <= _settings.ToKeyLength; keylength++)
            {
                UpdateDisplayEnd(keylength);
                HillclimbVigenere(ciphertext, keylength, _settings.Restarts);
                if (_stopped)
                {                    
                    return;
                }
            }
            UpdateDisplayEnd(_settings.ToKeyLength);
          

            if (_presentation.BestList.Count > 0)
            {
                Plaintext = _presentation.BestList[0].Text;
                Key = _presentation.BestList[0].Key;            
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
                ((AssignmentPresentation)Presentation).StartTime.Value = "" + _startTime;
                ((AssignmentPresentation)Presentation).EndTime.Value = "";
                ((AssignmentPresentation)Presentation).ElapsedTime.Value = "";
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
                ((AssignmentPresentation)Presentation).EndTime.Value = "" + _endTime;
                ((AssignmentPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                ((AssignmentPresentation)Presentation).CurrentAnalysedKeylength.Value = "" + keylength;

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
                {
                    runkey[i] = numalphabet[random.Next(alphabetlength)];
                }

                bool foundbetter;
                var bestkeycost = double.MinValue;

                var plaintext = _settings.Mode == Mode.Vigenere 
                    ? DecryptVigenereOwnAlphabet(ciphertext, runkey, numvigalphabet) 
                    : DecryptAutokeyOwnAlphabet(ciphertext, runkey, numvigalphabet);

                DecryptVigenereFunction decryptVigenereFunction;
                if (_settings.Mode == Mode.Vigenere)
                {
                    decryptVigenereFunction = DecryptVigenereOwnAlphabetInPlace;
                }
                else
                {
                    decryptVigenereFunction = DecryptAutokeyOwnAlphabetInPlace;
                }

                do
                {
                    foundbetter = false;
                    // permute key
                    for (var i = 0; i < keylength; i++)
                    {
                        for (var j = 0; j < alphabetlength; j++)
                        {
                            var oldLetter = runkey[i];
                            runkey[i] = j;
                            plaintext = decryptVigenereFunction(plaintext, runkey, numvigalphabet, i, ciphertext);                          

                            keys++;
                            var costvalue = _grams.CalculateCost(plaintext);
                            
                            if (costvalue > bestkeycost)
                            {
                                bestkeycost = costvalue;
                                bestkey = (int[]) runkey.Clone();
                                foundbetter = true;                                                               
                            }
                            else
                            {
                                //reset key
                                runkey[i] = oldLetter;
                                if (j == alphabetlength - 1)
                                {
                                    plaintext = decryptVigenereFunction(plaintext, runkey, numvigalphabet, i, ciphertext);
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
                                        _presentation.CurrentSpeed.Value = string.Format("{0:0,0}", keysDispatcher);
                                    }
                                    // ReSharper disable once EmptyGeneralCatchClause
                                    catch (Exception e)
                                    {
                                        //wtf?
                                        //Console.WriteLine("e1: " + e);
                                    }
                                }, null);
                                keys = 0;
                                lasttime = DateTime.Now;
                            }
                        }
                    }
                    runkey = (int[])bestkey.Clone();
                } while (foundbetter);

                UpdateDisplayEnd(keylength);
                restarts--;
               

                if (bestkeycost > globalbestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    AddNewBestListEntry(bestkey, globalbestkeycost, ciphertext);
                }
                ProgressChanged((keylength - _settings.FromKeylength) * totalrestarts + totalrestarts - restarts, (_settings.ToKeyLength - _settings.FromKeylength + 1) * totalrestarts);
                           
            }
            //We update finally the keys/second of the ui
            var keysDispatcher2 = keys;
            var lasttimeDispatcher2 = lasttime;
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.CurrentSpeed.Value = string.Format("{0:0,0}", Math.Round(keysDispatcher2 * 1000 / (DateTime.Now - lasttimeDispatcher2).TotalMilliseconds, 0));
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception e)
                {
                    //do nothing
                }
            }, null);
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
                    //Insert new entry at correct place to sustain order of list:
                    var insertIndex = _presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
                    _presentation.BestList.Insert(insertIndex, entry);

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
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    //wtf?
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

    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => ExactValue.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry => 
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + ExactValue + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "KeyLength: " + KeyLength + Environment.NewLine +
            "Text: " + Text;

        public double ExactValue => Math.Abs(Value);
        
        public int KeyLength => Key.Length;
    }    
}

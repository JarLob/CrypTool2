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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Cryptool.PluginBase.IO;
using System.Windows.Threading;
using System.Threading;
using M138Analyzer;
using System.Linq;
using System.Collections.ObjectModel;


namespace Cryptool.M138Analyzer
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "https://www.ais.uni-kassel.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Cryptool.M138Analyzer.Properties.Resources", "PluginCaption", "PluginTooltoip", "M138Analyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class M138Analyzer : ICrypComponent
    {
        #region Private Variables

        private List<int[]> StripList = new List<int[]>(); //Store used strips
        private int QUADGRAMMULTIPLIER = 3;
        private int TRIGRAMMULTIPLIER = 1;
        private int DIVISOR = 4;
        private List<int[]> BestKeyList = new List<int[]>();
        private List<double> BestKeyValues = new List<double>();
        private List<int> KeyOffsetList = new List<int>();
        private List<string> BestListToVisualize = new List<string>();
        private int BestListLength = 20;
        private double[, ,] Trigrams;
        private double[, , ,] Quadgrams;
        private bool _isStopped = true;
        private DateTime _startTime;
        private DateTime _endTime;

        private readonly M138AnalyzerSettings settings = new M138AnalyzerSettings();
        private int Attack = 0; //What attack whould be used
        private string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int[] CiphertextNumbers;
        private int[] PlaintextNumbers;
        private int KeyLength = 25; //Default key for M138

        private M138AnalyzerPresentation _presentation = new M138AnalyzerPresentation();

        #endregion

        #region Data Properties

        //Inputs
        [PropertyInfo(Direction.InputData, "PlaintextInputDes", "Input tooltip description")]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "CiphertextInputDes", "Input tooltip description")]
        public string Ciphertext
        {
            get;
            set;
        }

        //Outputs
        [PropertyInfo(Direction.OutputData, "ResultTextDes", "Output tooltip description")]
        public string ResultText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyOutputDes", "Output tooltip description")]
        public string CalculatedKey
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
            get { return settings; }
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
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            _isStopped = false;

            //if (Ciphertext != null)
            //{
            //    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext, Alphabet);
            //}
            //if (Plaintext != null)
            //{
            //    PlaintextNumbers = MapTextIntoNumberSpace(Plaintext, Alphabet);
            //}
            Attack = settings.AnalyticMode;


            switch (Attack)
            {
                case 0: //Known Plaintext
                    if (Ciphertext == null || Plaintext == null)
                    {
                        GuiLogMessage("Please provide Ciphertext and Plaintext to perform a known plaintext attack", NotificationLevel.Error);
                        return;
                    }
                    if (Plaintext.Length != Ciphertext.Length)
                    {
                        if (Plaintext.Length > Ciphertext.Length)
                        {
                            Plaintext.Remove(Ciphertext.Length);
                        }
                        else
                        {
                            Ciphertext.Remove(Plaintext.Length);
                        }
                    } //Ciphertext and Plaintext have the same length
                    Ciphertext = Ciphertext.ToUpper();
                    Plaintext = Plaintext.ToUpper();
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext, Alphabet);
                    PlaintextNumbers = MapTextIntoNumberSpace(Plaintext, Alphabet);
                    int _textLength = Plaintext.Length;
                    List<int> _listOfOffsets = new List<int>();
                    List<List<int>> _keysForOffset = new List<List<int>>();
                    List<List<int>> _allKeys = new List<List<int>>();
                    List<string> _allKeysReadable = new List<string>();
                    int _numberOfKeysForThisOffset;
                    //Call Known Plaintext Attack
                    //TextLength should be at least 25
                    for (int i = 0; i < KeyLength; i++)
                    {
                        _keysForOffset = KnownPlaintextAttack(i, _textLength, StripList.Count, StripList[0].Length);
                        if (_keysForOffset != null) //Found a Key for this offset
                        {
                            _numberOfKeysForThisOffset = _keysForOffset.Count;
                            for (int z = 0; z < _numberOfKeysForThisOffset; z++)
                            {
                                _listOfOffsets.Add(i);
                                _allKeys.Add(_keysForOffset[z]);
                                _allKeysReadable.Add("Offset: " + i + ", Strips: " + string.Join(", ", _keysForOffset[z]));
                            }
                        }
                    }
                    CalculatedKey = string.Join("\n", _allKeysReadable);
                    OnPropertyChanged("CalculatedKey");
                    break;

                case 1: //Hill Climbing
                    if (Ciphertext == null)
                    {
                        GuiLogMessage("Please provide a ciphertext to perform Hill Climbing", NotificationLevel.Error);
                        return;
                    }
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext, Alphabet);

                    // Clear presentation
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ((M138AnalyzerPresentation)Presentation).BestList.Clear();
                    }, null);

                    int _restartNtimes;
                    int _tmpTextLength = CiphertextNumbers.Length;
                    for (int _i = 0; _i < BestListLength; _i++)
                    {
                        BestKeyValues.Add(Double.MinValue); //Fill List where Cost Values will be saved with minimum values
                    }

                    if (_tmpTextLength < 100) //0-99, focus on trigrams
                    {
                        _restartNtimes = 35;
                        TRIGRAMMULTIPLIER = 4;
                        QUADGRAMMULTIPLIER = 1;
                        DIVISOR = 5;
                    }
                    else if (_tmpTextLength < 200) //100-199, evenly use tri- and quadgrams
                    {
                        _restartNtimes = 25;
                        TRIGRAMMULTIPLIER = 3;
                        QUADGRAMMULTIPLIER = 3;
                        DIVISOR = 6;
                    }
                    else if (_tmpTextLength < 300) //200-299 focus more on quadgrams
                    {
                        _restartNtimes = 17;
                        TRIGRAMMULTIPLIER = 2;
                        QUADGRAMMULTIPLIER = 4;
                        DIVISOR = 6;
                    }
                    else
                    { // >=300 Use mainly quadgrams
                        _restartNtimes = 10;
                        TRIGRAMMULTIPLIER = 1;
                        QUADGRAMMULTIPLIER = 6;
                        DIVISOR = 7;
                    }

                    int len = Alphabet.Length; //Length of a strip equals Alphabet Length (By Definition)
                    UpdateDisplayStart();
                    for (int i = 1; i < len; i++)
                    {
                        UpdateDisplayEnd(i);
                        HillClimb(CiphertextNumbers, KeyLength, i, StripList, Alphabet, Trigrams, Quadgrams, _restartNtimes);
                        if (_isStopped)
                        {
                            return;
                        }
                    }
                    UpdateDisplayEnd(KeyOffsetList[0]);
                    CalculatedKey = string.Join(", ", BestKeyList[0]);
                    ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, BestKeyList[0], KeyOffsetList[0], StripList), Alphabet);
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;

                case 2:

                    break;

                case 3:
                    break;
                default:
                    break;
            }


            ProgressChanged(1, 1);
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
            _isStopped = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            StripList = LoadStripes(Alphabet);
            Trigrams = Load3Grams(Alphabet);
            Quadgrams = Load4Grams(Alphabet);
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

        #region Helpers

        private bool HillClimb(int[] _cipherText, int _keyLength, int _keyOffset, List<int[]> _stripes, string _alphabet, double[, ,] _ngrams3, double[, , ,] _ngrams4, int _restarts = 10, bool _greedy = false, int[] _startKey = null)
        {

            int _numberOfStrips = _stripes.Count; //Anzahl verfuegbarer Streifen
            double _globalBestKeyCost = double.MinValue; //Kostenwert des globalen Maximums fuer diesen Offset
            int[] _globalBestKey;
            int[] _localBestKey = new int[_keyLength];
            int _keyCount = 0;
            var _startTime = DateTime.Now;
            Random _rand = new Random(Guid.NewGuid().GetHashCode());

            while (_restarts > 0)
            {
                if (_isStopped)
                {
                    return false; ;
                }
                int[] _runkey = new int[_numberOfStrips]; //Runkey that contains every possible Strip. Only the first _keyLength strips will be used though
                List<int> _elements = new List<int>();
                for (int _i = 0; _i < _numberOfStrips; _i++)
                {
                    _elements.Add(_i);
                }
                //Startkey is given, just fill the empty parts of the key with random strips
                if (_startKey != null)
                {
                    int _givenStartkeyLength = _startKey.Length;
                    for (int _i = 0; _i < _givenStartkeyLength; _i++)
                    {
                        if (_elements.Contains(_startKey[_i]))
                        { //Given Startkey value at this position is a valid strip
                            _runkey[_i] = _startKey[_i]; //Use strip in Runkey
                            _elements.Remove(_startKey[_i]); //Remove Strip from list so it's not used twice
                        }
                        else
                        { //Location in startkey is empty / invalid
                            int _number = _rand.Next(0, _elements.Count);
                            _runkey[_i] = _elements[_number]; //Use a random strip
                            _elements.Remove(_elements[_number]);
                        }
                    }
                    for (int _i = _givenStartkeyLength; _i < _numberOfStrips; _i++)
                    {
                        //Fill rest of the runkey
                        int _number = _rand.Next(0, _elements.Count);
                        _runkey[_i] = _elements[_number];
                        _elements.Remove(_elements[_number]);
                    }
                }
                //No Startkey is given, generate a random startkey
                else
                {
                    for (int _i = 0; _i < _numberOfStrips; _i++)
                    {
                        int _number = _rand.Next(0, _elements.Count);
                        _runkey[_i] = _elements[_number];
                        _elements.Remove(_elements[_number]);
                    }
                }

                //Do the actual Hill Climbing, Do Permutations of key,...

                bool _foundBetterKey;
                double _localBestKeyCost = double.MinValue;

                do
                {
                    _foundBetterKey = false;

                    //Iterate over the first 25 elements of the key (The actual key)
                    for (int i = 0; i < _keyLength; i++)
                    {
                        //Iterate over the complete Key (100 elements) and swap one of the first 25 elements with one element of the key
                        //TODO: Might it be better to swap >1 elements at one time?
                        for (int j = 0; j < _numberOfStrips; j++)
                        {
                            if (i == j)
                            {
                                continue; //Don't swap an element with itself
                            }
                            int[] _copykey = new int[_numberOfStrips]; //Copy of Runkey
                            for (int k = 0; k < _numberOfStrips; k++)
                            {
                                _copykey[k] = _runkey[k];
                            }
                            //Swap 2 Elements in copykey
                            int _tmpElement = _copykey[i];
                            _copykey[i] = _copykey[j];
                            _copykey[j] = _tmpElement;
                            //TODO: Swap more elements at one time?

                            int[] _trimKey = new int[_keyLength]; //First n (25) Elements of runkey that will actually be used for en/decryption
                            for (int k = 0; k < _keyLength; k++)
                            {
                                _trimKey[k] = _copykey[k];
                            }

                            int[] _plainText = Decrypt(_cipherText, _trimKey, _keyOffset, _stripes);
                            double _costValue = ((TRIGRAMMULTIPLIER * CalculateTrigramCost(_ngrams3, _plainText)) + (QUADGRAMMULTIPLIER * CalculateQuadgramCost(_ngrams4, _plainText))) / DIVISOR;
                            //TODO: Improve cost Function

                            _keyCount++;

                            if (_costValue > _localBestKeyCost) //Tested key is better then the best local key so far
                            {
                                _localBestKeyCost = _costValue;
                                _localBestKey = _copykey;

                                /*
                                if (BestKeyList.Count == 0) //BestList is empty, only happens once
                                {
                                    BestKeyList.Add(_trimKey); //Add Trimkey because that has been used to decrpt
                                    BestKeyValues.Insert(0, _localBestKeyCost); //Store corresponding cost value
                                    KeyOffsetList.Add(_keyOffset); //Corresponding offset for the key
                                    BestKeyValues.RemoveAt(BestListLength);
                                    break;
                                }
                                else
                                { //There are already elements in the bestlist
                                    int _tmpBestlistCount = BestKeyList.Count;
                                    for (int k = 0; k < _tmpBestlistCount; k++)
                                    { //go through bestlist until end of bestlist is reached or bestlist is full
                                        if (ArraysEqual(BestKeyList[k], _trimKey))
                                        {
                                            break; //Key is already in bestlist
                                        }
                                        if (_localBestKeyCost > BestKeyValues[k])
                                        { //Current Key is better than key on position k
                                            BestKeyList.Insert(k, _trimKey);
                                            BestKeyValues.Insert(k, _localBestKeyCost);
                                            KeyOffsetList.Insert(k, _keyOffset);
                                            BestKeyValues.RemoveAt(BestListLength);
                                            break;
                                        }
                                    }
                                }
                                // Cut bestlist if it becomes too long
                                if (BestKeyList.Count > BestListLength)
                                {
                                    BestKeyList.RemoveAt(BestListLength);
                                    KeyOffsetList.RemoveAt(BestListLength);
                                }
                                 */

                                //Fill Bestlist if necessary (Could be that all Keys found in a different run already ahve been better and there's no need to save this crap
                                _foundBetterKey = true;
                                if (_greedy)
                                {
                                    _runkey = _localBestKey;
                                }
                            }
                        }
                    }
                    _runkey = _localBestKey;
                } while (_foundBetterKey);
                UpdateDisplayEnd(_keyOffset);
                var _endTime = DateTime.Now;
                Console.WriteLine("Keys per second: " + (_keyCount / (_endTime - _startTime).TotalSeconds));
                _restarts--;

                if (_localBestKeyCost > _globalBestKeyCost) //Found a better Key then the best key found so far
                {
                    //New best global Key found
                    _globalBestKeyCost = _localBestKeyCost;
                    _globalBestKey = _localBestKey;
                    int[] _trimKey = new int[KeyLength];
                    for (int z = 0; z < KeyLength; z++)
                    {
                        _trimKey[z] = _globalBestKey[z];
                    }
                    /*
                    if (BestKeyList.Count == 0) //BestList is empty, only happens once
                    {
                        BestKeyList.Add(_trimKey); //Add Trimkey because that has been used to decrpt
                        BestKeyValues.Insert(0, _localBestKeyCost); //Store corresponding cost value
                        KeyOffsetList.Add(_keyOffset); //Corresponding offset for the key
                        BestKeyValues.RemoveAt(BestListLength);
                        break;
                    }
                    else
                    { //There are already elements in the bestlist
                        int _tmpBestlistCount = BestKeyList.Count;
                        for (int k = 0; k < _tmpBestlistCount; k++)
                        { //go through bestlist until end of bestlist is reached or bestlist is full
                            if (ArraysEqual(BestKeyList[k], _trimKey))
                            {
                                break; //Key is already in bestlist
                            }
                            if (_localBestKeyCost > BestKeyValues[k])
                            { //Current Key is better than key on position k
                                BestKeyList.Insert(k, _trimKey);
                                BestKeyValues.Insert(k, _localBestKeyCost);
                                KeyOffsetList.Insert(k, _keyOffset);
                                BestKeyValues.RemoveAt(BestListLength);
                                break;
                            }
                        }
                    }
                    // Cut bestlist if it becomes too long
                    if (BestKeyList.Count > BestListLength)
                    {
                        BestKeyList.RemoveAt(BestListLength);
                        KeyOffsetList.RemoveAt(BestListLength);
                    }
                     */
                    //AddNewBestListEntry(_globalBestKey, _globalBestKeyCost, CiphertextNumbers, _keyOffset);
                    AddNewBestListEntry(_trimKey, _globalBestKeyCost, CiphertextNumbers, _keyOffset);
                }
                //ProgressChanged((KeyLength - settings.FromKeylength) * totalrestarts + totalrestarts - restarts, (_settings.ToKeyLength - _settings.FromKeylength + 1) * totalrestarts);
            }

            return true;
        }

        private List<List<int>> KnownPlaintextAttack(int _offset, int _textLength, int _availableStrips, int _stripLength)
        {
            int[] _currentStrip;
            int p; //Plaintext Character
            int c; //Ciphertext Character
            int isAt;
            List<List<int>> _workingStrips = new List<List<int>>();
            List<List<int>> _possibleStrips = new List<List<int>>();

            for (int location = 0; location < _textLength; location++)
            {
                List<int> _possibleStripsForThisLocation = new List<int>();
                p = PlaintextNumbers[location];
                c = CiphertextNumbers[location];
                for (int testStripNumber = 0; testStripNumber < _availableStrips; testStripNumber++)
                {
                    //Test each strip for this offset
                    _currentStrip = StripList[testStripNumber];
                    isAt = Array.IndexOf(_currentStrip, p);
                    if (_currentStrip[(isAt + _offset) % _stripLength] == c)
                    {
                        _possibleStripsForThisLocation.Add(testStripNumber);
                    }
                }
                if (_possibleStripsForThisLocation.Count == 0) //No strips work for this location, so the whole offset will not have a valid key. Break here
                {
                    return null;
                }
                _possibleStrips.Add(_possibleStripsForThisLocation);
            }
            //Now there should be a non-empty list of Strips with working offsets for each position
            for (int location = 0; location < KeyLength; location++)
            {
                //Make advantage of the period and check which strips still work
                int tmp = location + KeyLength;
                List<int> _possibleStripsForThisLocation = new List<int>();
                _possibleStripsForThisLocation = _possibleStrips[location];
                while (tmp < _textLength) //Go through the whole text again
                {
                    _possibleStripsForThisLocation = _possibleStripsForThisLocation.Intersect(_possibleStrips[tmp]).ToList<int>();
                    if (_possibleStripsForThisLocation.Count == 0)
                    {
                        return null;
                    }
                    tmp += KeyLength; //Check location 1 period later
                }
                _workingStrips.Add(_possibleStripsForThisLocation); //In Working Strips we should now have KeyLength Elements of Lists that each hold possible strips for their location
                //Now make this to a list of Lists that holds all possible keys
            }
            List<List<int>> _allPossibleKeysForThisOffst = new List<List<int>>();
            int _numberOfPossibleKeys = 1;
            for (int i = 0; i < KeyLength; i++)
            {
                _numberOfPossibleKeys = _numberOfPossibleKeys * _workingStrips[i].Count;
            }
            if (_numberOfPossibleKeys == 1)
            {
                List<int> _tmpList = new List<int>();
                for (int z = 0; z < KeyLength; z++)
                {
                    _tmpList.Add(_workingStrips[z][0]);
                }
                _allPossibleKeysForThisOffst.Add(_tmpList);
            }
            else
            {
                IEnumerable<IEnumerable<int>> _enumerableStriplist = _workingStrips;
                _enumerableStriplist = PermuteAllKeys(_enumerableStriplist);
                _allPossibleKeysForThisOffst = _enumerableStriplist as List<List<int>>;
            }
            
            return _allPossibleKeysForThisOffst;
        }

        IEnumerable<IEnumerable<int>> PermuteAllKeys(IEnumerable<IEnumerable<int>> sequences)
        {
            IEnumerable<IEnumerable<int>> result = new[] { Enumerable.Empty<int>() };

            return sequences.Aggregate(result, (accumulator, sequence) => from accseq in accumulator from item in sequence select accseq.Concat(new[] { item }));
        }
        private bool ArraysEqual(int[] a, int[] b)
        {
            int _tmpLength = a.Length;
            for (int i = 0; i < _tmpLength; i++)
            {
                if (a[i] == b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private double CalculateTrigramCost(double[, ,] ngrams3, int[] plaintext)
        {
            double value = 0;
            int end = plaintext.Length - 2;

            for (int i = 0; i < end; i++)
            {
                value += ngrams3[plaintext[i], plaintext[i + 1], plaintext[i + 2]];
            }
            return value;
        }

        private double CalculateQuadgramCost(double[, , ,] ngrams4, int[] plaintext)
        {
            double value = 0;
            int end = plaintext.Length - 3;

            for (int i = 0; i < end; i++)
            {
                value += ngrams4[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];
            }
            return value;
        }

        private int[] Decrypt(int[] cipherText, int[] key, int keyoffset, List<int[]> stripes)
        {
            int length = cipherText.Length;
            int keylength = key.Length;
            int stripeslength = stripes[0].Length;
            int[] selectedStrip;
            int[] plainText = new int[length];

            for (int i = 0; i < length; i++)
            {
                selectedStrip = stripes[key[Mod(i, keylength)]];
                plainText[i] = selectedStrip[Mod(IndexOf(selectedStrip, cipherText[i]) - keyoffset, stripeslength)];
            }
            return plainText;
        }

        private int Mod(int a, int n)
        {
            int result = a % n;
            if (a < 0)
            {
                result += n;
            }
            return result;
        }

        private int IndexOf(int[] array, int value)
        {
            int length = array.Length;
            for (int i = 0; i < length; i++)
            {
                if (array[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        private int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            int[] numbers = new int[text.Length];
            int position = 0;
            foreach (char c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in numbers)
            {
                sb.Append(alphabet[i]);
            }
            return sb.ToString();
        }

        private double[, ,] Load3Grams(string _a)
        {
            int _tmpAlphabetLength = Alphabet.Length;
            double[, ,] Trigrams = new double[_tmpAlphabetLength, _tmpAlphabetLength, _tmpAlphabetLength];
            using (FileStream fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "en-3gram-nocs.bin"), FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < _tmpAlphabetLength; i++)
                    {
                        for (int j = 0; j < _tmpAlphabetLength; j++)
                        {
                            for (int k = 0; k < _tmpAlphabetLength; k++)
                            {
                                byte[] bytes = reader.ReadBytes(8);
                                Trigrams[i, j, k] = BitConverter.ToDouble(bytes, 0);
                            }
                        }
                    }
                }
            }
            return Trigrams;
        }

        private double[, , ,] Load4Grams(string _a)
        {
            int _tmpAlphabetLength = Alphabet.Length;
            double[, , ,] Quadgrams = new double[_tmpAlphabetLength, _tmpAlphabetLength, _tmpAlphabetLength, _tmpAlphabetLength];
            using (FileStream fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "en-4gram-nocs.bin"), FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < _tmpAlphabetLength; i++)
                    {
                        for (int j = 0; j < _tmpAlphabetLength; j++)
                        {
                            for (int k = 0; k < _tmpAlphabetLength; k++)
                            {
                                for (int l = 0; l < _tmpAlphabetLength; l++)
                                {
                                    byte[] bytes = reader.ReadBytes(8);
                                    Quadgrams[i, j, k, l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
                return Quadgrams;
            }
        }

        private List<int[]> LoadStripes(string alphabet)
        {
            List<int[]> _tmpStripes = new List<int[]>();
            StringBuilder sb = new StringBuilder();
            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"), FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        _tmpStripes.Add(MapTextIntoNumberSpace(line, alphabet));
                    }
                }
            }
            return _tmpStripes;
        }


        private string PrintNumbers(IEnumerable<int> numbers)
        {
            StringBuilder builder = new StringBuilder();
            foreach (int i in numbers)
            {
                if (i < 10)
                {
                    builder.Append("0");
                }
                builder.Append(i + " ");
            }
            return builder.ToString();
        }

        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                ((M138AnalyzerPresentation)Presentation).startTime.Content = "" + _startTime;
                ((M138AnalyzerPresentation)Presentation).endTime.Content = "";
                ((M138AnalyzerPresentation)Presentation).elapsedTime.Content = "";
            }, null);
        }

        private void UpdateDisplayEnd(int _offset)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((M138AnalyzerPresentation)Presentation).endTime.Content = "" + _endTime;
                ((M138AnalyzerPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((M138AnalyzerPresentation)Presentation).currentAnalysedKeylength.Content = "" + _offset;

            }, null);
        }

        private void AddNewBestListEntry(int[] key, double value, int[] ciphertext, int offset)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = string.Join(", ", key),
                Text = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, key, offset, StripList), Alphabet),
                Value = value,
                Offset = offset
            };

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0 && value <= _presentation.BestList.Last().Value)
                    {
                        return; //All Entries in Bestlist are better than this one
                    }
                    _presentation.BestList.Add(entry);
                    _presentation.BestList = new ObservableCollection<ResultEntry>(_presentation.BestList.OrderByDescending(i => i.Value));
                    if (_presentation.BestList.Count > BestListLength)
                    {
                        _presentation.BestList.RemoveAt(BestListLength);
                    }
                    int z = 0;
                    foreach (ResultEntry r in _presentation.BestList)
                    {
                        r.Ranking = z;
                        z++;
                    }
                    _presentation.ListView.DataContext = _presentation.BestList;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

            }, null);

        }
        #endregion
    }

    public class ResultEntry
    {
        public int Ranking { get; set; }
        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public int Offset { get; set; }

        public double ExactValue
        {
            get { return Math.Abs(Value); }
        }
    }
}

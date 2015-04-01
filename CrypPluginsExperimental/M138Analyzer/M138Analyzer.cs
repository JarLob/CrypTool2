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
        private int _selectedLanguage = 0;
        private int MinOffsetUserSelect;
        private int MaxOffsetUserSelect;
        private double KeysPerSecondCurrent = 0;
        private double KeysPerSecondAverage = 0;
        private double AverageTimePerRestart = 0;
        private bool fastConverge = false;

        private readonly M138AnalyzerSettings settings = new M138AnalyzerSettings();
        private int Attack = 0; //What attack whould be used
        private string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int[] CiphertextNumbers;
        private int[] PlaintextNumbers;
        private int KeyLength = 25; //Default key for M138
        private int RetryCounter;
        private int MaxRetriesNecesary;
        private double BestCostValueOfAllKeys = double.MinValue;

        private M138AnalyzerPresentation _presentation = new M138AnalyzerPresentation();
        #endregion
        #region Constructor
        public M138Analyzer()
        {
            settings = new M138AnalyzerSettings();
            settings.UpdateTaskPaneVisibility();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }
        #endregion

        #region Data Properties

        //Inputs
        [PropertyInfo(Direction.InputData, "PlaintextInputDes", "PlaintextInputDescription", "Input tooltip description")]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "CiphertextInputDes", "CiphertextInputDescription", "Input tooltip description")]
        public string Ciphertext
        {
            get;
            set;
        }

        //Outputs
        [PropertyInfo(Direction.OutputData, "ResultTextDes", "ResultTextDescription", "Output tooltip description")]
        public string ResultText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyOutputDes", "KeyOutputDescription", "Output tooltip description")]
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

            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((M138AnalyzerPresentation)Presentation).BestList.Clear();
            }, null);
            // Get Settings
            getUserSelections();


            switch (Attack)
            {
                case 0: //Known Plaintext
                    if (String.IsNullOrEmpty(Ciphertext) || String.IsNullOrEmpty(Plaintext))
                    {
                        GuiLogMessage("Please provide Ciphertext and Plaintext to perform a known plaintext attack", NotificationLevel.Error);
                        return;
                    }

                    Plaintext = RemoveInvalidChars(Plaintext.ToUpper(), Alphabet);
                    Ciphertext = RemoveInvalidChars(Ciphertext.ToUpper(), Alphabet);
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
                    ResultText = Plaintext;
                    OnPropertyChanged("ResultText");
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
                    StringBuilder AllPossibleKeysAsString = new StringBuilder();
                    var _estimatedEndTime = DateTime.Now;
                    for (int i = MinOffsetUserSelect; i < MaxOffsetUserSelect+1; i++) //Go Over Keylength (Try all possible offsets)
                    {
                        var _startTime = DateTime.Now;
                        ProgressChanged(i, MaxOffsetUserSelect+1);
                        _keysForOffset = KnownPlaintextAttack(i, _textLength, StripList.Count, StripList[0].Length);
                        if (_keysForOffset != null) //Found a Key for this offset
                        {
                            StringBuilder sb = new StringBuilder();
                            //sb.Append("Key for Offset " + i + ": ");
                            int _cachedKeyLength = _keysForOffset.Count;
                            for (int _keyLocation = 0; _keyLocation < _cachedKeyLength; _keyLocation++)
                            {

                                sb.Append("[");
                                sb.Append(string.Join("|", _keysForOffset[_keyLocation].ToArray()));
                                sb.Append("]");
                                if (_keyLocation != _cachedKeyLength - 1)
                                {
                                    sb.Append(",");
                                }
                                else
                                {
                                    sb.Append(" / " + i);
                                    sb.Append("\n");
                                }
                            }
                            AddNewBestListEntryKnownPlaintext(sb.ToString().Split('/')[0], i);
                            AllPossibleKeysAsString.Append(sb);
                        }
                        var _endTime = DateTime.Now;
                        var _elapsedTime = _endTime - _startTime;
                        _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect + 1 - i));
                        UpdateDisplayEnd(i, _estimatedEndTime);
                    }
                    CalculatedKey = AllPossibleKeysAsString.ToString();
                    OnPropertyChanged("CalculatedKey");
                    break;

                case 1: //Hill Climbing
                    if (String.IsNullOrEmpty(Ciphertext))
                    {
                        GuiLogMessage("Please provide a ciphertext to perform Hill Climbing", NotificationLevel.Error);
                        return;
                    }
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext, Alphabet);
                    RetryCounter = 0;
                    _selectedLanguage = settings.LanguageSelection;

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
                    switch (_selectedLanguage)
                    {
                        case 0: //English
                            Trigrams = Load3Grams(Alphabet);
                            Quadgrams = Load4Grams(Alphabet);
                            break;
                        case 1: //German
                            Quadgrams = LoadGerman4Grams(Alphabet);
                            TRIGRAMMULTIPLIER = 0;
                            QUADGRAMMULTIPLIER = 1;
                            DIVISOR = 1;
                            break;
                    }

                    if (!String.IsNullOrEmpty(settings.HillClimbRestarts))
                    {
                        _restartNtimes = Convert.ToInt32(settings.HillClimbRestarts);
                    }
                    int len = Alphabet.Length; //Length of a strip equals Alphabet Length (By Definition)
                    MaxRetriesNecesary = _restartNtimes * len;

                    UpdateDisplayStart();

                    _estimatedEndTime = DateTime.Now;
                    for (int i = MinOffsetUserSelect; i < MaxOffsetUserSelect+1; i++)
                    {
                        var _startTime = DateTime.Now;
                        UpdateDisplayEnd(i, _estimatedEndTime);
                        HillClimb(CiphertextNumbers, KeyLength, i, StripList, Alphabet, Trigrams, Quadgrams, _restartNtimes, fastConverge);
                        if (_isStopped)
                        {
                            return;
                        }
                        var _endTime = DateTime.Now;
                        var _elapsedTime = _endTime - _startTime;
                        _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect - i));
                        UpdateDisplayEnd(i, _estimatedEndTime);
                    }
                    ResultEntry re = _presentation.BestList.First();
                    UpdateDisplayEnd(re.Offset, DateTime.Now);
                    string _tmpKeyStrips = re.Key.Split('/')[0];
                    int[] _tmpKey = _tmpKeyStrips.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    CalculatedKey = re.Key;
                    ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _tmpKey, re.Offset, StripList), Alphabet);
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;

                case 2: //Partially Known Plaintext
                    if (String.IsNullOrEmpty(Plaintext))
                    {
                        GuiLogMessage("Please provide a Plaintext for a Partially Known Plaintext attack", NotificationLevel.Error);
                        return;
                    }
                    else
                    {
                        PlaintextNumbers = MapTextIntoNumberSpace(Plaintext, Alphabet);
                    }
                    int _lengthOfPlaintext = PlaintextNumbers.Length;
                    if (String.IsNullOrEmpty(Ciphertext))
                    {
                        GuiLogMessage("Please provide a Ciphertext for a Partially Known Plaintext attack", NotificationLevel.Error);
                        return;
                    }
                    else
                    {
                        if (Ciphertext.Length < _lengthOfPlaintext)
                        {
                            GuiLogMessage("For a Partially Known Plaintext attack, the length of the known Ciphertext needs to be larger than the length of the known Plaintext. Otherwise, please perform a known Plaintext attack", NotificationLevel.Error);
                            return;
                        }
                        CiphertextNumbers= new int[_lengthOfPlaintext];
                        int[] _tmpCipherText = MapTextIntoNumberSpace(Ciphertext, Alphabet);
                        for (int i = 0; i < _lengthOfPlaintext; i++)
                        {
                            CiphertextNumbers[i] = _tmpCipherText[i];
                        }
                        
                    }
                    

                    for (int i = MinOffsetUserSelect; i < MaxOffsetUserSelect + 1; i++) //Do a known Plaintext on the known Plaintext and afterwards do a Hill Climbing on the complete Ciphertext
                    {
                        var _startTime = DateTime.Now;
                        ProgressChanged(i, MaxOffsetUserSelect + 1);
                        _keysForOffset = KnownPlaintextAttack(i, _lengthOfPlaintext, StripList.Count, StripList[0].Length);
                        if (_keysForOffset != null) //Found a Key for this offset, do Hill Climbing on complete Ciphertext
                        {
                            int _numPosKeys = 1;
                            foreach (List<int> l in _keysForOffset) {
                                _numPosKeys = _numPosKeys * l.Count;
                            }
                            if (_numPosKeys > 1000)
                            {
                                //too much to do hill climbing on every possible key
                            }
                            else
                            {
                                //Do Hill Climbing with given Startkey
                                int listLength = _keysForOffset.Count;
                                int[] counters = new int[listLength];
                                for (int _tmpCount = 0; _tmpCount < listLength; _tmpCount++)
                                {
                                    counters[_tmpCount] = 1;
                                }
                                for(int _tmpCount = 0; _tmpCount < _numPosKeys; _tmpCount++)
                                {
                                    int[] _tempKey = new int[KeyLength];

                                }

                            }
                        }
                        var _endTime = DateTime.Now;
                        var _elapsedTime = _endTime - _startTime;
                        _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect + 1 - i));
                        UpdateDisplayEnd(i, _estimatedEndTime);
                    }
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
            KeysPerSecondCurrent = 0;
            KeysPerSecondAverage = 0;
            AverageTimePerRestart = 0;
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

        private bool HillClimb(int[] _cipherText, int _keyLength, int _keyOffset, List<int[]> _stripes, string _alphabet, double[, ,] _ngrams3, double[, , ,] _ngrams4, int _restarts = 10, bool _fastConverge = false, int[] _startKey = null)
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

                            //TEST
                            _tmpElement = _copykey[Mod(i + 7, _keyLength)];
                            _copykey[Mod(i + 7, _keyLength)] = _copykey[Mod(j + 3, _numberOfStrips)];
                            _copykey[Mod(j + 3, _numberOfStrips)] = _tmpElement;
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

                                //Fill Bestlist if necessary (Could be that all Keys found in a different run already ahve been better and there's no need to save this crap
                                _foundBetterKey = true;
                                if (_fastConverge)
                                {
                                    _runkey = _localBestKey;
                                }
                            }
                        }
                    }
                    _runkey = _localBestKey;
                } while (_foundBetterKey);
                var _endTime = DateTime.Now;
                var _timeForOneRestart = (_endTime - _startTime);
                KeysPerSecondCurrent = _keyCount / _timeForOneRestart.TotalSeconds;
                KeysPerSecondAverage = (RetryCounter * KeysPerSecondAverage + KeysPerSecondCurrent) / (RetryCounter + 1);
                UpdateKeysPerSecond((int)KeysPerSecondCurrent, (int)KeysPerSecondAverage);
                _restarts--;
                RetryCounter++;
                //UpdateDisplayEnd(_keyOffset, _calcualtedEndTime);
                ProgressChanged(RetryCounter, MaxRetriesNecesary);

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
                    AddNewBestListEntry(_trimKey, _globalBestKeyCost, CiphertextNumbers, _keyOffset);
                    if (_globalBestKeyCost > BestCostValueOfAllKeys)
                    {
                        //New Best key over all offsetz found, update output
                        ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _trimKey, _keyOffset, StripList), Alphabet);
                        OnPropertyChanged("ResultText");
                        BestCostValueOfAllKeys = _globalBestKeyCost;
                    }
                }
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

            for (int location = 0; location < _textLength; location++) //Go over the whole text
            {
                List<int> _possibleStripsForThisLocation = new List<int>();
                p = PlaintextNumbers[location];
                c = CiphertextNumbers[location];
                for (int testStripNumber = 0; testStripNumber < _availableStrips; testStripNumber++)
                {
                    //Test each strip for this offset
                    _currentStrip = StripList[testStripNumber];
                    isAt = Array.IndexOf(_currentStrip, p);
                    int _tmpTestBla = (isAt + _offset) % _stripLength;
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
                if (location >= _textLength)
                {
                    break;
                }
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
            //Prevent strips from appearing twice in a key
            int _tmpCount = _workingStrips.Count; //Size of the list
            bool _stripWasKicked = true;
            while (_stripWasKicked)
            {
                _stripWasKicked = false;
                for (int i = 0; i < _tmpCount; i++) //Iterate over the List
                {
                    if (_workingStrips[i].Count == 1)
                    {
                        int _analyzedStrip = _workingStrips[i][0];
                        foreach (List<int> l in _workingStrips)
                        {
                            if (l != _workingStrips[i]) //Don't compare Location with itself
                            {
                                if(l.Contains(_analyzedStrip)) 
                                {
                                    l.Remove(_analyzedStrip);
                                    if (l.Count == 0)
                                    {
                                        return null;
                                    }
                                    _stripWasKicked = true;
                                }
                            }
                        }
                    }
                }
            }
            return _workingStrips;
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


        private double[, , ,] LoadGerman4Grams(string _a)
        {
            int _tmpAlphabetLength = Alphabet.Length;
            double[, , ,] Quadgrams = new double[_tmpAlphabetLength, _tmpAlphabetLength, _tmpAlphabetLength, _tmpAlphabetLength];
            using (FileStream fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "de-4gram-nocs.bin"), FileMode.Open, FileAccess.Read))
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

        private void UpdateDisplayEnd(int _offset, DateTime _estimatedEnd)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((M138AnalyzerPresentation)Presentation).endTime.Content = "" + _estimatedEnd;
                ((M138AnalyzerPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((M138AnalyzerPresentation)Presentation).currentAnalysedKeylength.Content = "" + _offset;
            }, null);
        }

        private void UpdateKeysPerSecond(int _current, int _average)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((M138AnalyzerPresentation)Presentation).keysPerSecondAverageLabel.Content = _average;
                ((M138AnalyzerPresentation)Presentation).keysPerSecondCurrentLabel.Content = _current;
            }, null);
        }

        private void AddNewBestListEntry(int[] key, double value, int[] ciphertext, int offset)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = string.Join(", ", key) + " / " + offset,
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

        private void AddNewBestListEntryKnownPlaintext(string key, int offset)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = key,
                Text = Plaintext,
                Value = offset,
            };

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.BestList.Add(entry);
                    _presentation.BestList = new ObservableCollection<ResultEntry>(_presentation.BestList);
                    //if (_presentation.BestList.Count > BestListLength)
                    //{
                    //    _presentation.BestList.RemoveAt(BestListLength);
                    //}
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

        private void getUserSelections()
        {
            KeyLength = settings.KeyLengthUserSelection;
            MaxOffsetUserSelect = settings.MaxOffsetUserSelection;
            MinOffsetUserSelect = settings.MinOffsetUserSelection;
            Attack = settings.Method;
            fastConverge = settings.FastConverge;
        }

        private string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "Method":
                        settings.UpdateTaskPaneVisibility();
                        break;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during settings_PropertyChanged: {0}", ex), NotificationLevel.Error);
            }
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

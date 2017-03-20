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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Fare;
using System.Security.Cryptography;
using System.Text;

namespace Cryptool.Plugins.TestVectorGenerator
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("TestVectorGenerator", "Subtract one number from another", "TestVectorGenerator/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class TestVectorGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly TestVectorGeneratorSettings _settings = new TestVectorGeneratorSettings();
        private string _textInput;
        private int _seedInput;
        private string _regexInput;
        private string _plaintextOutput;
        private string _textOutput2;
        private string _textOutput3;
        private string _singleKeyOutput;
        private string[] _keyOutput;

        private int _progress = 0;
        private string[] _inputArray;
        private System.Random _rand;
        private int _startSentence;
        private List<String> _keyList = new List<string>();
        private List<String> _plaintextList = new List<string>();
        private int keysToGenerate = -1;
        private int lastKeyLengthIndex = -1;
        private bool _notFound = false;
        ConcurrentDictionary<int, int> occurences;
        

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description", true)]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                if (_textInput == null || !_textInput.Equals(value))
                {
                    this._textInput = value;
                    OnPropertyChanged("TextInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "SeedInput", "SeedInput tooltip description", true)]
        public string SeedInput
        {
            get { return this._seedInput.ToString(); }
            set
            {
                int seed = SHA1AsInt32(value);
                if (_seedInput != seed)
                {
                    this._seedInput = seed;
                    OnPropertyChanged("SeedInput");
                }

            }
        }

        [PropertyInfo(Direction.InputData, "RegexInput", "RegexInput tooltip description")]
        public string RegexInput
        {
            get { return this._regexInput; }
            set
            {
                if (_regexInput == null || !_regexInput.Equals(value))
                {
                    this._regexInput = value;
                    OnPropertyChanged("RegexInput");
                }

            }
        }

        [PropertyInfo(Direction.OutputData, "SingleKeyOutput", "KeyOutput tooltip description")]
        public string SingleKeyOutput
        {
            get { return this._singleKeyOutput; }
            set
            {
                if (_singleKeyOutput == null || !_singleKeyOutput.Equals(value))
                {
                    this._singleKeyOutput = value;
                    OnPropertyChanged("SingleKeyOutput");
                    //Console.WriteLine("OnPropertyChanges SingleKeyOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutput tooltip description")]
        public string[] KeyOutput
        {
            get { return this._keyOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_keyOutput != value)
                {
                    this._keyOutput = value;
                    OnPropertyChanged("KeyOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get { return this._plaintextOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_plaintextOutput == null || !_plaintextOutput.Equals(value))
                {
                    this._plaintextOutput = value;
                    OnPropertyChanged("PlaintextOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutput2", "textOutput tooltip description")]
        public string TextOutput2
        {
            get { return this._textOutput2; }
            set
            {
                // TODO: check if test works and is necessary
                if (_textOutput2 != value)
                {
                    this._textOutput2 = value;
                    OnPropertyChanged("TextOutput2");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutput3", "textOutput tooltip description")]
        public string TextOutput3
        {
            get { return this._textOutput3; }
            set
            {
                // TODO: check if test works and is necessary
                if (_textOutput3 != value)
                {
                    this._textOutput3 = value;
                    OnPropertyChanged("TextOutput3");
                }
            }
        }

        #endregion

        #region Generate Plaintext

        public void generatePlaintext()
        {
            // check if plaintext list contains all elements, break if so
            if (_plaintextList.Count == (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength)
                return;

            _startSentence = _rand.Next(0, _inputArray.Length);
            while (_plaintextList.Exists(s => s.StartsWith(_inputArray[_startSentence])))
            {
                //Console.WriteLine("_seedInput: " + _seedInput + ", _rand: " + _rand +
                //    ", Length: " + _inputArray.Length + ", StartSentence: " + _startSentence);
                _startSentence = _rand.Next(0, _inputArray.Length);
            }

            _plaintextOutput = "";
            //Console.WriteLine("_seedInput: " + _seedInput + ", _rand: " + _rand +
            //    ", Length: " + _inputArray.Length + ", StartSentence: " + _startSentence);
            for (int i = _startSentence; i != _startSentence - 1; i = i == _inputArray.Length - 1 ? 0 : i + 1)
            {
                _plaintextOutput = _plaintextOutput + _inputArray[i] + ". ";
                if (_plaintextOutput.Length >= _settings.TextLength)
                {
                    String finalPlaintext = _plaintextOutput.Substring(0, _settings.TextLength);
                    _plaintextList.Add(finalPlaintext);
                    PlaintextOutput = finalPlaintext;
                    break;
                }
            }
        }

        #endregion

        #region Generate Keys

        public void generateNaturalSpeechKeys()
        {
            _startSentence = _rand.Next(0, _inputArray.Length);
            //GuiLogMessage("_seedInput: " + _seedInput + ", StartSentence: " + _startSentence, NotificationLevel.Debug);

            if (occurences == null)
            {
                occurences = new ConcurrentDictionary<int, int>();

                for (int i = _settings.MinKeyLength; i <= _settings.MaxKeyLength; i++)
                {
                    occurences.AddOrUpdate(i, 0, (id, count) => 0);
                    //GuiLogMessage("Initialize: " + i, NotificationLevel.Debug);
                }
            }

            string sentence = _inputArray[_startSentence].ToUpper().Replace(" ", "");
            int originalStartSentence = _startSentence;
            //GuiLogMessage("Checking sentence: \"" + sentence + "\"", NotificationLevel.Debug);

            int sentenceLength = sentence.Length;
            int smallestMissingLength = -1;

            while (true)
            {
                // check if key list contains all elements, break if so
                if (_keyList.Count == (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength)
                    return;

                int lengthOccurences = 0;

                // if no complete sentence could be found, search for a longer one and cut it
                if (_notFound && sentenceLength > _settings.MaxKeyLength)
                {
                    if (smallestMissingLength == -1)
                        smallestMissingLength = _settings.MinKeyLength - 1;

                    lengthOccurences = _settings.KeyAmountPerLength;

                    // search for the smallest sentence/key length that is missing
                    while (smallestMissingLength <= _settings.MaxKeyLength &&
                        lengthOccurences == _settings.KeyAmountPerLength)
                    {
                        smallestMissingLength++;
                        occurences.TryGetValue(smallestMissingLength, out lengthOccurences);
                    }

                    // double check, should not happen
                    if (smallestMissingLength <= _settings.MaxKeyLength &&
                        lengthOccurences > _settings.KeyAmountPerLength)
                    {
                        GuiLogMessage("Too many sentences added for length: " +
                            smallestMissingLength, NotificationLevel.Debug);
                        continue;
                    }

                    // cutting of the sentence
                    sentence = sentence.Substring(0, smallestMissingLength);
                    sentenceLength = sentence.Length;
                }
                else
                {
                    occurences.TryGetValue(sentenceLength, out lengthOccurences);
                }
                if (sentenceLength >= _settings.MinKeyLength &&
                        sentenceLength <= _settings.MaxKeyLength &&
                        lengthOccurences < _settings.KeyAmountPerLength &&
                        !_keyList.Contains(sentence))
                {
                    _keyList.Add(sentence);
                    occurences.AddOrUpdate(sentenceLength, 1, (id, count) => count + 1);

                    // !!! TESTING ONLY !!!
                    //sentence = sentence + " (" + sentence.Length + ")";

                    //int length = 15 < sentence.Length ? 15 : sentence.Length;
                    //Console.WriteLine("sentence: "+ sentence.Substring(0,length) +" - " + _startSentence + "/" + originalStartSentence);

                    // if the letters should be replaced by numbers, do so
                    if (_settings.KeyFormatNaturalSpeech == FormatType.numbers)
                    {
                        sentence = convertToNumericKey(sentence);
                        //replaceLettersByNumbersWithSpaces();
                    }

                    SingleKeyOutput = sentence;

                    return;
                }

                _startSentence = _startSentence == _inputArray.Length - 1 ? 0 : _startSentence + 1;
                sentence = _inputArray[_startSentence].ToUpper().Replace(" ", "");
                sentenceLength = sentence.Length;

                // if the start sentence reaches the original one, the whole imput array is computed
                if (_startSentence == originalStartSentence)
                {
                    // check if also no longer sentence to cut could be found, break if so
                    if (_notFound)
                    {
                        return;
                    }
                    _notFound = true;
                }

                // set progress
                _progress++;
                ProgressChanged(_progress / (_inputArray.Length - 1) * 2, 1);
            }
        }

        public void generateRandomKeys()
        {
            if (keysToGenerate == -1 || lastKeyLengthIndex == -1)
            {
                keysToGenerate = (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength;
                lastKeyLengthIndex = 0;
            }
            else if (lastKeyLengthIndex < keysToGenerate - 1)
            {
                lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            if (_settings.KeyFormatRandom == FormatType.lettersOnly)
            {
                //GuiLogMessage("generate random key with letters only", NotificationLevel.Info);
                string randomKey = "";
                for (int j = 0; j < (_settings.MinKeyLength + lastKeyLengthIndex / _settings.KeyAmountPerLength); j++)
                {
                    char ch = Convert.ToChar(_rand.Next(0, 26) + Convert.ToInt32('A'));
                    randomKey = randomKey + ch;
                }
                //GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), lastKeyLengthIndex: " + lastKeyLengthIndex, NotificationLevel.Info);

                SingleKeyOutput = randomKey;
            }
            else
            {
                int upperLimit = 0;
                if (_settings.KeyFormatRandom == FormatType.binaryOnly)
                {
                    // 0 to 1 means binary
                    upperLimit = 1;
                }
                else if (_settings.KeyFormatRandom == FormatType.digitsOnly)
                {
                    // 0 to 9 are all digits
                    upperLimit = 9;
                }
                else if (_settings.KeyFormatRandom == FormatType.numbers)
                {
                    // from 0 to 25 for the 26 letters of the alphabet
                    upperLimit = 25;
                }
                //GuiLogMessage("generate random key with 0 - "+upperLimit+" only", NotificationLevel.Info);
                string randomKey = "";
                for (int j = 0; j < (_settings.MinKeyLength + lastKeyLengthIndex / _settings.KeyAmountPerLength); j++)
                {
                    string space = "";
                    if (!String.IsNullOrEmpty(randomKey))
                        space = " ";
                    int randomInt = (_rand.Next(0, upperLimit + 1));
                    randomKey = randomKey + space + randomInt;
                }
                //GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), lastKeyLengthIndex: " + lastKeyLengthIndex, NotificationLevel.Info);

                SingleKeyOutput = randomKey;
            }
        }

        public void generateRandomKeysWithRegex()
        {
            if (keysToGenerate == -1 || lastKeyLengthIndex == -1)
            {
                if (_regexInput.Contains("$amount"))
                {
                    keysToGenerate = (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength;
                }
                else
                {
                    keysToGenerate = _settings.KeyAmountPerLength;
                }
                lastKeyLengthIndex = 0;
            }
            else if (lastKeyLengthIndex < keysToGenerate - 1)
            {
                lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            var str = _regexInput;
            if (_regexInput.Contains("$amount"))
            {
                int length = _settings.MinKeyLength + (lastKeyLengthIndex / _settings.KeyAmountPerLength);
                //GuiLogMessage("length: " + length, NotificationLevel.Warning);
                //var str = "[a-zA-Z]{" + length + "}";
                str = str.Replace("$amount", length.ToString());
            }
            var regex = @str;
            var xeger = new Fare.Xeger(regex, _rand);
            var regexString = xeger.Generate();

            while (_keyList.Contains(regexString))
            {
                regexString = xeger.Generate();
            }

            _keyList.Add(regexString);

            // TESTING ONLY!
            regexString = regexString + " (" + regexString.Length + ")";
            //GuiLogMessage("regexString: " + regexString, NotificationLevel.Warning);

            if (!Regex.IsMatch(regexString, regex))
            {
                GuiLogMessage("regexString \"" + regexString + "\" does not match regex \"" + regex + "\"!", NotificationLevel.Error);

            }

            SingleKeyOutput = regexString;
        }

        #endregion

        #region General Methods

        public bool checkVariables()
        {
            if (_settings.MinKeyLength < 1)
            {
                GuiLogMessage("KeyLength has to be at least 1", NotificationLevel.Warning);
                _settings.MinKeyLength = 1;
            }

            if (_textInput.Length < _settings.MinKeyLength)
            {
                GuiLogMessage("The input text is too small!", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public void preProcessTextInput()
        {
            // replace double minus and newlines by space and ? and ! by full stops
            TextInput = TextInput.Replace("--", " ");
            TextInput = TextInput.Replace("?", ".");
            TextInput = TextInput.Replace("!", ".");
            TextInput = TextInput.Replace("..", ".");
            TextInput = TextInput.Replace(System.Environment.NewLine, " ");

            // delete all characters appart from letters, spaces and full stops
            TextInput = Regex.Replace(TextInput, @"[^A-Za-z. ]+", "");

            // replace double space by single space
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            TextInput = regex.Replace(TextInput, " ");
            int substringLength = 1000 < TextInput.Length ? 1000 : TextInput.Length;
            //TextOutput2 = TextInput.Substring(0, substringLength);

            // replace newlines by space
            TextInput = TextInput.Replace(". ", ".");
            if (TextInput.EndsWith("."))
                TextInput = TextInput.Substring(0, TextInput.Length - 1);

            substringLength = 1000 < TextInput.Length ? 1000 : TextInput.Length;
            TextOutput3 = TextInput.Substring(0, substringLength);

            // split input text into sentences
            _inputArray = TextInput.Split('.');
        }

        public String convertToNumericKey(String key)
        {
            return convertToNumericKey(key, true);
        }

        public String convertToNumericKey(String key, bool addSpaces)
        {
            char[] chars = key.ToCharArray();
            int[] numbers = new int[chars.Length];
            string space = "";
            if (addSpaces)
                space = " ";

            int index = 0;

            for (int i = 0; index < chars.Length; i = i == chars.Length - 1 ? 0 : i + 1)
            {
                char ch1 = chars[i];
                if (ch1 > 'Z')
                {
                    continue;
                }

                int smallestLetter = 0;

                for (int j = 0; j < chars.Length; j++)
                {
                    ch1 = chars[smallestLetter];
                    char ch2 = chars[j];

                    if (smallestLetter == j)
                        continue;

                    if (ch2 < ch1)
                        smallestLetter = j;
                }

                numbers[smallestLetter] = index;
                chars[smallestLetter] = Convert.ToChar('Z' + 1);
                index++;
            }
            String numericKey = numbers[0].ToString();
            for (int i = 1; i < numbers.Length; i++)
                numericKey = numericKey + space + numbers[i];
            //Console.WriteLine("numericKey: " + numericKey);

            return numericKey;
        }

        public void replaceLettersByNumbersWithSpaces()
        {
            char[] chars = SingleKeyOutput.ToCharArray();
            string transpositionKey = "";
            foreach (char ch in chars)
            {
                try
                {
                    string space = "";
                    if (!String.IsNullOrEmpty(transpositionKey))
                        space = " ";
                    transpositionKey = transpositionKey + space + (Convert.ToInt32(ch) - Convert.ToInt32('A'));
                }
                catch (OverflowException)
                {
                    GuiLogMessage("Unable to convert " + ((int)ch).ToString("X4") + " to an Int32.", NotificationLevel.Info);
                }
            }

            SingleKeyOutput = transpositionKey;
        }

        public static int SHA1AsInt32(string stringToHash)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToInt32(sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToHash)), 0);
            }
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
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (!checkVariables())
                return;

            if (_inputArray == null)
                preProcessTextInput();
            
            ProgressChanged(0, 1);

            _rand = new System.Random(_seedInput);
            GuiLogMessage("_seedInput: " + _seedInput, NotificationLevel.Info);

            if (_settings.TextLength > 0)
                generatePlaintext();

            if (_settings.KeyGeneration == GenerationType.naturalSpeech)
            {
                generateNaturalSpeechKeys();
            }
            else if (_settings.KeyGeneration == GenerationType.regex)
            {
                //generateSingleRandomKeysWithRegex();
                generateRandomKeysWithRegex();
            }
            else
            {
                generateRandomKeys();
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            keysToGenerate = -1;
            lastKeyLengthIndex = -1;
            _inputArray = null;
            occurences = null;
            _keyList = new List<string>();
            _plaintextList = new List<string>();
            _progress = 0;
            _notFound = false;
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
}

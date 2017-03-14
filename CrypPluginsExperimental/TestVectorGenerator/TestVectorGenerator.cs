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
        private string[] _keyOutput;

        private int _progress;
        private string[] _inputArray;
        private System.Random _rand;
        private int _startSentence;
        

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description", true)]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                if (_textInput != value)
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
                int seed = value.GetHashCode();
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
                if (_regexInput != value)
                {
                    this._regexInput = value;
                    OnPropertyChanged("RegexInput");
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
                if (_plaintextOutput != value)
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
            if (TextInput.Length > 10000)
                TextOutput2 = TextInput.Substring(0, 10000);

            // replace newlines by space
            TextInput = TextInput.Replace(". ", ".");
            if (TextInput.Length > 10000)
                TextOutput3 = TextInput.Substring(0, 10000);

            // split input text into sentences
            _inputArray = TextInput.Split('.');
        }

        public void generatePlaintext()
        {
            _plaintextOutput = "";
            GuiLogMessage("_seedInput: " + _seedInput + ", _rand: " + _rand +
                ", Length: " + _inputArray.Length + ", StartSentence: " + _startSentence, NotificationLevel.Info);
            for (int i = _startSentence; i != _startSentence - 1; i = i == _inputArray.Length - 1 ? 0 : i + 1)
            {
                _plaintextOutput = _plaintextOutput + _inputArray[i] + ". ";
                if (_plaintextOutput.Length >= _settings.TextLength)
                {
                    PlaintextOutput = _plaintextOutput.Substring(0, _settings.TextLength);
                    break;
                }
            }
        }

        public void generateNaturalSpeechKeys()
        {
            List<string> outputList = new List<string>();
            ConcurrentDictionary<int, int> occurences = new ConcurrentDictionary<int, int>();
            _startSentence = _rand.Next(0, _inputArray.Length);
            GuiLogMessage("_seedInput: " + _seedInput + ", StartSentence: " + _startSentence, NotificationLevel.Info);

            for (int i = _settings.MinKeyLength; i <= _settings.MaxKeyLength; i++)
            {
                occurences.AddOrUpdate(i, 0, (id, count) => 0);
                //GuiLogMessage("Initialize: " + i, NotificationLevel.Debug);
            }

            // for loop for sentence searching by length
            for (int i = _startSentence; i != _startSentence - 1; i = i == _inputArray.Length - 1 ? 0 : i + 1)
            {
                string sentence = _inputArray[i].ToUpper().Replace(" ", "");
                //GuiLogMessage("Checking sentence: \"" + sentence + "\"", NotificationLevel.Debug);
                int sentenceLength = sentence.Length;
                if (sentenceLength >= _settings.MinKeyLength &&
                    sentenceLength <= _settings.MaxKeyLength)
                {
                    int sentenceOccurences = 0;
                    occurences.TryGetValue(sentenceLength, out sentenceOccurences);

                    if (sentenceOccurences < _settings.KeyAmountPerLength)
                    {
                        outputList.Add(sentence);
                        occurences.AddOrUpdate(sentenceLength, 1, (id, count) => count + 1);

                        GuiLogMessage("i: " + i + " (" + (i >= _startSentence ? i - _startSentence : i + _inputArray.Length - _startSentence - 1) +
                            "/" + (_inputArray.Length - 1) + ") - Adding sentence: \"" + sentence +
                            "\", occurences[" + sentenceLength + "]: " + occurences[sentenceLength],
                            NotificationLevel.Info);



                        _progress++;

                        ProgressChanged(_progress / (_settings.MaxKeyLength - _settings.MinKeyLength + 1) *
                            _settings.KeyAmountPerLength, 1);
                    }
                }

                if (allKeysFound(occurences))
                    break;

            }

            if (!allKeysFound(occurences))
            {
                // for loop for sentence searching by longer length and trimming down
                for (int i = _startSentence; i != _startSentence - 1; i = i == _inputArray.Length - 1 ? 0 : i + 1)
                {
                    string sentence = _inputArray[i].ToUpper().Replace(" ", "");
                    //GuiLogMessage("Checking sentence: \"" + sentence + "\"", NotificationLevel.Debug);
                    int sentenceLength = sentence.Length;
                    int smallestMissingLength = _settings.MinKeyLength - 1;

                    if (sentenceLength > _settings.MaxKeyLength)
                    {

                        int sentenceOccurences = _settings.KeyAmountPerLength;

                        while (smallestMissingLength <= _settings.MaxKeyLength &&
                            sentenceOccurences == _settings.KeyAmountPerLength)
                        {
                            smallestMissingLength++;
                            occurences.TryGetValue(smallestMissingLength, out sentenceOccurences);
                        }

                        // double check, should not happen
                        if (smallestMissingLength <= _settings.MaxKeyLength &&
                            sentenceOccurences > _settings.KeyAmountPerLength)
                        {
                            GuiLogMessage("Too many sentences added for length: " +
                                smallestMissingLength, NotificationLevel.Debug);
                        }

                        if (smallestMissingLength > _settings.MaxKeyLength)
                        {
                            if (allKeysFound(occurences))
                            {
                                break;
                            }
                            else
                            {
                                GuiLogMessage("Something went wrong! Some sentences are missing " +
                                    "(not correctly added?!)", NotificationLevel.Warning);
                                break;
                            }
                        }


                        string cutSentence = sentence.Substring(0, smallestMissingLength);
                        outputList.Add(cutSentence);
                        occurences.AddOrUpdate(smallestMissingLength, 1, (id, count) => count + 1);

                        GuiLogMessage("i: " + i + " (" + (i >= _startSentence ? i - _startSentence : i +
                            _inputArray.Length - _startSentence - 1) + ") - Adding cut sentence: \"" +
                            cutSentence + "\", occurences[" + smallestMissingLength + "]: " +
                            occurences[smallestMissingLength], NotificationLevel.Info);

                        _progress++;

                        ProgressChanged(_progress / (_settings.MaxKeyLength - _settings.MinKeyLength + 1) *
                            _settings.KeyAmountPerLength, 1);
                    }


                }

                if (!allKeysFound(occurences))
                {
                    GuiLogMessage("Somethings wrong! Not all key lengths found!", NotificationLevel.Warning);

                }
            }

            KeyOutput = outputList.ToArray();

            Array.Sort(KeyOutput, (x, y) => x.Length.CompareTo(y.Length));

            if (_settings.KeyFormatRandom == FormatType.numbers)
            {
                replaceLettersByNumbersWithSpaces();
            }
        }

        public void replaceLettersByNumbersWithSpaces()
        {
            List<string> transpositionList = new List<string>();
            foreach (string str in KeyOutput)
            {
                char[] chars = str.ToCharArray();
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

                transpositionList.Add(transpositionKey);
            }

            KeyOutput = transpositionList.ToArray();
        }

        public void generateRandomKeys()
        {
            string[] outputArray = new string[(_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength];

            if (_settings.KeyFormatRandom == FormatType.lettersOnly)
            {
                GuiLogMessage("generate random key with letters only", NotificationLevel.Info);

                for (int i = 0; i < outputArray.Length; i++)
                {
                    string randomKey = "";
                    for (int j = 0; j < (_settings.MinKeyLength + i / _settings.KeyAmountPerLength); j++)
                    {
                        char ch = Convert.ToChar(_rand.Next(0, 26) + Convert.ToInt32('A'));
                        randomKey = randomKey + ch;
                    }
                    GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), i: " + i, NotificationLevel.Info);

                    outputArray[i] = randomKey;
                }

                KeyOutput = outputArray;
            }
            else
            {
                int upperLimit = 0;
                if (_settings.KeyFormatRandom == FormatType.binaryOnly)
                {
                    // 0 to 1 means binary
                    upperLimit = 1;
                } else if (_settings.KeyFormatRandom == FormatType.digitsOnly)
                {
                    // 0 to 9 are all digits
                    upperLimit = 9;
                } else if (_settings.KeyFormatRandom == FormatType.numbers)
                {
                    // from 0 to 25 for the 26 letters of the alphabet
                    upperLimit = 25;
                }
                GuiLogMessage("generate random key with 0 - "+upperLimit+" only", NotificationLevel.Info);

                for (int i = 0; i < outputArray.Length; i++)
                {
                    string randomKey = "";
                    for (int j = 0; j < (_settings.MinKeyLength + i / _settings.KeyAmountPerLength); j++)
                    {
                        string space = "";
                        if (!String.IsNullOrEmpty(randomKey))
                            space = " ";
                        int randomInt = (_rand.Next(0, upperLimit+1));
                        randomKey = randomKey + space + randomInt;
                    }
                    GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), i: " + i, NotificationLevel.Info);

                    outputArray[i] = randomKey;
                }

                KeyOutput = outputArray;
            }
        }

        public void generateRandomKeysWithRegex()
        {
            if (_regexInput.Contains("$amount"))
            {
                string[] outputArray = new string[(_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength];

                for (int i = 0; i < outputArray.Length; i++)
                {
                    int length = _settings.MinKeyLength + (i / _settings.KeyAmountPerLength);
                    //GuiLogMessage("length: " + length, NotificationLevel.Warning);

                    //var str = "[a-zA-Z]{" + length + "}";
                    var str = _regexInput.Replace("$amount", length.ToString());
                    var regex = @str;
                    var xeger = new Fare.Xeger(regex, _rand);
                    var regexString = xeger.Generate();

                    // TESTING ONLY!
                    regexString = regexString + " (" + regexString.Length + ", " + length.ToString() + ")";
                    //GuiLogMessage("regexString: " + regexString, NotificationLevel.Warning);

                    if (!Regex.IsMatch(regexString, regex))
                    {
                        GuiLogMessage("regexString \"" + regexString + "\" does not match regex \"" + regex + "\"!", NotificationLevel.Error);

                    }

                    outputArray[i] = regexString;
                }

                KeyOutput = outputArray;
            }
            else
            {
                string[] outputArray = new string[_settings.KeyAmountPerLength];

                for (int i = 0; i < outputArray.Length; i++)
                {
                    var regex = @_regexInput;
                    var xeger = new Fare.Xeger(regex);
                    var regexString = xeger.Generate();

                    // TESTING ONLY!
                    regexString = regexString + " (" + regexString.Length.ToString() + ")";
                    //GuiLogMessage("regexString: " + regexString, NotificationLevel.Warning);

                    if (!Regex.IsMatch(regexString, regex))
                    {
                        GuiLogMessage("regexString \"" + regexString + "\" does not match regex \"" + regex + "\"!", NotificationLevel.Error);
                    }

                    outputArray[i] = regexString;
                }

                KeyOutput = outputArray;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (!checkVariables())
                return;

            preProcessTextInput();
            
            ProgressChanged(0, 1);

            _rand = new System.Random(_seedInput);
            GuiLogMessage("_seedInput: " + _seedInput, NotificationLevel.Info);

            generatePlaintext();

            if (_settings.KeyGeneration == GenerationType.naturalSpeech)
            {
                generateNaturalSpeechKeys();
            }
            else if (_settings.KeyGeneration == GenerationType.regex)
            {
                generateRandomKeysWithRegex();
            }
            else
            {
                generateRandomKeys();
            }

            ProgressChanged(1, 1);
        }

        private bool allKeysFound(ConcurrentDictionary<int, int> dict) {
            foreach (int key in dict.Keys)
            {
                //GuiLogMessage("dict[" + key + "]: " + dict[key], NotificationLevel.Info);
                if (dict[key] < _settings.KeyAmountPerLength)
                    return false;
            }

            return true;
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

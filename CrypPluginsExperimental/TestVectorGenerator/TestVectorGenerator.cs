/*
   Copyright 2017 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Linq;
using System.Numerics;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.TestVectorGenerator
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Applied Information Security - University of Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("TestVectorGenerator", "Generate keys and plaintexts as test vectors", "TestVectorGenerator/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class TestVectorGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly TestVectorGeneratorSettings _settings = new TestVectorGeneratorSettings();
        private string _textInput;
        private int _seedInput;
        private string _regexInput;
        private string _alphabetInput;
        private string _plaintextOutput;
        private string _debugOutput;
        private string _keyOutput;

        private int _testRunCount = 0;
        private string[] _inputArray;
        private System.Random _rand;
        private int _startSentence;
        private List<String> _keyList = new List<string>();
        private List<String> _plaintextList = new List<string>();
        private int _currentTextLength = 0;
        private int _calculatedTextsCurrentLength = 0;
        private int _lastKeyLengthIndex = -1;
        private bool _notFound = false;
        ConcurrentDictionary<int, int> _occurrences;
        
        #endregion

        #region Data Properties

        /// <summary>
        /// The input text from which the plaintexts are taken.
        /// </summary>
        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description", true)]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                this._textInput = value;
                OnPropertyChanged("TextInput");
            }
        }

        /// <summary>
        /// The seed which initializes the random number generator.
        /// </summary>
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
                }
                OnPropertyChanged("SeedInput");

            }
        }

        /// <summary>
        /// The regex pattern as string (optional).
        /// </summary>
        [PropertyInfo(Direction.InputData, "RegexInput", "RegexInput tooltip description")]
        public string RegexInput
        {
            get { return this._regexInput; }
            set
            {
                this._regexInput = value;
                OnPropertyChanged("RegexInput");
            }
        }

        /// <summary>
        /// The additional alphabet input (optional).
        /// </summary>
        [PropertyInfo(Direction.InputData, "AlphabetInput", "AlphabetInput tooltip description")]
        public string AlphabetInput
        {
            get { return this._alphabetInput; }
            set
            {
                this._alphabetInput = value;
                OnPropertyChanged("AlphabetInput");
            }
        }

        /// <summary>
        /// The current key (for the CryptAnalysisAnalyzer).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutput tooltip description")]
        public string KeyOutput
        {
            get;
            set;
        }

        /// <summary>
        /// The current plaintext (for the CryptAnalysisAnalyzer).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get;
            set;
        }

        /// <summary>
        /// The total number of keys (for the CryptAnalysisAnalyzer).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "TotalKeys", "TotalKeys tooltip description")]
        public int TotalKeys
        {
            get;
            set;
        }

        /// <summary>
        /// The debug output for additional information (usable with the string output component).
        /// </summary>
        [PropertyInfo(Direction.OutputData, "DebugOutput", "DebugOutput tooltip description")]
        public string DebugOutput
        {
            get { return this._debugOutput; }
            set
            {
                this._debugOutput = value;
                OnPropertyChanged("DebugOutput");
            }
        }

        #endregion

        #region Generate Plaintext

        /// <summary>
        /// Generates the plaintext according to the settings.
        /// </summary>
        public void generatePlaintext()
        {
            // check if plaintext list contains all elements, break if so
            if (_plaintextList.Count == _settings.NumberOfTestRuns)
                return;

            // if the current text length is 0, set it to the specified minimum
            if (_currentTextLength == 0)
                _currentTextLength = _settings.MinTextLength;

            // generate the first starting sentence index of the input array
            _startSentence = _rand.Next(0, _inputArray.Length);
            int count = 0;

            // generate a new starting sentence as long as it has the same beginning as a preceding one
            while (_plaintextList.Exists(s => s.StartsWith(_inputArray[_startSentence])))
            {
                _startSentence = _rand.Next(0, _inputArray.Length);
                count++;

                // break the loop after going through the array 3 times and return
                if (count > _inputArray.Length * 3)
                {
                    GuiLogMessage("Text input seems to be too short for the entered amount of plaintexts!", NotificationLevel.Error);
                    return;
                }
            }

            _plaintextOutput = "";
            // iterate over the input array, starting at the start sentence
            for (int i = _startSentence; i != _startSentence - 1; i = i == _inputArray.Length - 1 ? 0 : i + 1)
            {
                // append next sentence as long as current text length reached
                // replace spaces and dots as specified in the settings
                _plaintextOutput = _plaintextOutput + replaceSpaces(replaceDots(_inputArray[i]));
                if (_plaintextOutput.Length >= _currentTextLength)
                {
                    // cut the final plaintext to the exact specified length
                    string finalPlaintext = _plaintextOutput.Substring(0, _currentTextLength);

                    // add the final plaintext to the list, set the output and increment counter
                    _plaintextList.Add(finalPlaintext);
                    _plaintextOutput = finalPlaintext;
                    _calculatedTextsCurrentLength++;

                    // increase current text length if number of texts per length is reached and reset counter
                    if (_settings.PlaintextsPerLength != 0 &&
                        _calculatedTextsCurrentLength >= _settings.PlaintextsPerLength)
                    {
                        _calculatedTextsCurrentLength = 0;
                        _currentTextLength += _settings.TextLengthIncrease;
                    }
                    break;
                }
            }
        }

        #endregion

        #region Generate Keys

        /// <summary>
        /// Generates the natural speech key according to the settings.
        /// </summary>
        public void generateNaturalSpeechKeys()
        {
            // generate the first starting sentence index of the input array
            _startSentence = _rand.Next(0, _inputArray.Length);
            
            // if the occurrences dictionary is not defined yet, initialize it with
            // all lengths to generate and 0 occurrences per length
            if (_occurrences == null)
            {
                _occurrences = new ConcurrentDictionary<int, int>();

                for (int i = _settings.MinKeyLength; i <= _settings.MaxKeyLength; i++)
                {
                    _occurrences.AddOrUpdate(i, 0, (id, count) => 0);
                }
            }

            // replace the spaces in the current sentence if specified in the settings
            string sentence = replaceSpaces(_inputArray[_startSentence]);
            int originalStartSentence = _startSentence;

            int sentenceLength = sentence.Length;
            // the smallest missing length will be set in the loop below
            int smallestMissingLength = -1;

            while (true)
            {
                // check if key list contains all elements, break if so
                if (_keyList.Count == _settings.NumberOfTestRuns)
                    return;

                int lengthOccurrences = 0;

                // if no complete sentence could be found, search for a longer one and cut it
                // #2 This is part two of the search
                if (_notFound && sentenceLength > _settings.MaxKeyLength)
                {
                    // if the smallest missing length is -1, set it to the minimum key
                    // length minus 1 (to balance out the first ++ below in the while loop)
                    if (smallestMissingLength == -1)
                        smallestMissingLength = _settings.MinKeyLength - 1;

                    // set the occurrences to the maximum to enter the while loop below
                    lengthOccurrences = _settings.KeysPerLength;

                    // search for the smallest sentence/key length that is missing
                    while (smallestMissingLength <= _settings.MaxKeyLength &&
                        lengthOccurrences == _settings.KeysPerLength)
                    {
                        smallestMissingLength++;
                        // get the occurrences of the current smallest length
                        _occurrences.TryGetValue(smallestMissingLength, out lengthOccurrences);
                    }

                    // double check, should not happen
                    if (smallestMissingLength <= _settings.MaxKeyLength &&
                        lengthOccurrences > _settings.KeysPerLength)
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
                    _occurrences.TryGetValue(sentenceLength, out lengthOccurrences);
                }

                // #1 this is part one of the search
                if (sentenceLength >= _settings.MinKeyLength &&
                        sentenceLength <= _settings.MaxKeyLength &&
                        lengthOccurrences < _settings.KeysPerLength &&
                        !_keyList.Contains(sentence))
                {
                    _keyList.Add(sentence);
                    _occurrences.AddOrUpdate(sentenceLength, 1, (id, count) => count + 1);

                    // if the letters should be replaced by numbers, do so
                    if (_settings.KeyFormatNaturalSpeech == FormatType.numbers)
                    {
                        if (_settings.UniqueSymbolUsage)
                            sentence = ConvertToUniqueNumericKey(sentence);
                        else
                            sentence = ConvertToNumericKey(sentence);
                    }
                    else
                    {
                        // add separator
                        sentence = AddSeparator(sentence);
                    }

                    _keyOutput = sentence;

                    return;
                }

                // set the sentence to the next one in the array (returning to zero after maximum)
                _startSentence = _startSentence == _inputArray.Length - 1 ? 0 : _startSentence + 1;
                // replace the spaces and get the current length
                sentence = replaceSpaces(_inputArray[_startSentence]);
                sentenceLength = sentence.Length;

                // if the start sentence reaches the original one, the whole input array has been computed once
                if (_startSentence == originalStartSentence)
                {
                    // check if also no longer sentence to cut could be found, break if so
                    if (_notFound)
                    {
                        return;
                    }

                    // at this point, we switch from #1 to #2, starting to search longer
                    // sentences than the actual necessary lengths (and cut them)
                    _notFound = true;
                }
            }
        }

        /// <summary>
        /// Adds the specified separator between the key symbols.
        /// </summary>
        private string AddSeparator(string str)
        {
            char[] arr = str.ToArray();

            string result = arr[0].ToString();
            for (int i = 1; i < arr.Length; i++)
            {
                result = result + _settings.Separator + arr[i];
            }

            return result;
        }

        /// <summary>
        /// Selects the key alphabet and triggers the random key generation.
        /// </summary>
        public void generateRandomKeys()
        {
            // only generate keys if the number of test runs was not completed
            if (_lastKeyLengthIndex == -1)
            {
                _lastKeyLengthIndex = 0;
            }
            else if (_lastKeyLengthIndex < _settings.NumberOfTestRuns - 1)
            {
                _lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            // find the current alphabet and put it into the alphabet list
            var alphabet = new List<string>();
            if (_settings.KeyFormatRandom == FormatType.letters)
            {
                if (_settings.UppercaseOnly)
                    alphabet = FindAlphabet("A-Z")
                else
                    alphabet = FindAlphabet("a-zA-Z");
            }
            else if (_settings.KeyFormatRandom == FormatType.inputAlphabet)
            {
                if (_alphabetInput == null || String.IsNullOrEmpty(_alphabetInput))
                {
                    GuiLogMessage("Alphabet input is empty!", NotificationLevel.Error);
                    return;
                }
                alphabet = _alphabetInput.Split(' ').ToList();
            }
            else if (_settings.KeyFormatRandom == FormatType.numbers)
            {
                alphabet = FindAlphabet("0-9");
            }
            else if (_settings.KeyFormatRandom == FormatType.binary)
            {
                alphabet = "0 1".Split(' ').ToList();
            }

            // generate the random key 
            string randomKey = GenerateRandomKeyWithAlphabet(alphabet, 
                _settings.MinKeyLength + _lastKeyLengthIndex / _settings.KeysPerLength);

            if (randomKey == null)
                return;
            
            _keyOutput = randomKey;
        }

        /// <summary>
        /// Triggers the random key generation with a separator repetition of 1.
        /// <param name="alphabet">The alphabet list containing the alphabet symbols</param>
        /// <param name="length">The requested length of the key</param>
        /// <returns>The generated random key</returns>
        /// </summary>
        public string GenerateRandomKeyWithAlphabet(List<string> alphabet, int length)
        {
            return GenerateRandomKeyWithAlphabet(alphabet, length, 1);
        }

        /// <summary>
        /// Generates the random key from the given input alphabet of the given length.
        /// <param name="alphabet">The alphabet list containing the alphabet symbols</param>
        /// <param name="length">The requested length of the key</param>
        /// <param name="separatorRepeat">The number of key symbols after which the separator is inserted</param>
        /// <returns>The generated random key</returns>
        /// </summary>
        public string GenerateRandomKeyWithAlphabet(List<string> alphabet, int length, int separatorRepeat)
        {
            // throw error if the alphabet does not contain enough letters for a unique key generation
            if (length > alphabet.Count && _settings.UniqueSymbolUsage)
            {
                GuiLogMessage("Alphabet length (" + alphabet.Count + ") is too short to generate a string of length " + length + " of unique letters!", NotificationLevel.Error);
                return null;
            }

            string randomKey = "";

            // run the loop body once for each key symbol
            for (int j = 0; j < length; j++)
            {
                // generate the next index of the alphabet list
                int i = _rand.Next(0, alphabet.Count - 1);

                // take the element at the random index and remove it if the key should be unique
                string symbol = alphabet.ElementAt(i);
                if (_settings.UniqueSymbolUsage)
                    alphabet.RemoveAt(i);

                // append the random key by the taken symbol 
                // (separated by the separator in the specified frequency)
                if (randomKey == "")
                    randomKey = symbol;
                else if (j % separatorRepeat == 0)
                    randomKey = randomKey + _settings.Separator + symbol;
                else
                    randomKey = randomKey + symbol;
            }

            return randomKey;
        }

        /// <summary>
        /// Searches the short version of an alphabet in the given string or splits the given symbols.
        /// <param name="alphabetString">The alphabet string containing the alphabet (e.g. in short version)</param>
        /// <returns>The alphabet as a list of strings</returns>
        /// </summary>
        public List<string> FindAlphabet(string alphabetString)
        {
            if (String.IsNullOrEmpty(_alphabetInput))
                return null;

            List<string> alphabet = null;

            // if the string contains a dash, search for abbreviated alphabet representations
            if (alphabetString.Contains("-"))
            {
                if (Regex.IsMatch(alphabetString, @"a-zA-Z"))
                {
                    alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"a-z"))
                {
                    alphabet = "abcdefghijklmnopqrstuvwxyz".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"A-Z"))
                {
                    alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"0-9"))
                {
                    alphabet = "0123456789".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"a-zA-Z0-9"))
                {
                    alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"A-Z0-9"))
                {
                    alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(c => c.ToString()).ToList();
                }
                else if (Regex.IsMatch(alphabetString, @"a-z0-9"))
                {
                    alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".Select(c => c.ToString()).ToList();
                }
                else if (Regex.Matches(alphabetString, @"-").Count > 1)
                {
                    GuiLogMessage("Alphabet with multiple '-' not recognized!", NotificationLevel.Error);
                    return null;
                }
                else
                {
                    // if no abbreviated representation could be found, split at the dash
                    // and check whether the upper and lower symbols are numeric or letters
                    string[] alphabetBounds = alphabetString.Split('-');
                    bool upperIsNumeric = Regex.IsMatch(alphabetBounds[1], @"[0-9]");
                    bool upperIsLetter = Regex.IsMatch(alphabetBounds[1], @"[a-zA-Z]");

                    bool lowerIsNumeric = Regex.IsMatch(alphabetBounds[0], @"[0-9]");
                    bool lowerIsLetter = Regex.IsMatch(alphabetBounds[0], @"[a-zA-Z]");

                    // if both are different or not numeric or a letter, return null
                    if (!upperIsNumeric && !upperIsLetter ||
                        !lowerIsNumeric && !lowerIsLetter ||
                        !upperIsNumeric && lowerIsNumeric ||
                        !upperIsLetter && lowerIsLetter)
                    {
                        GuiLogMessage("Alphabet with single '-' not recognized!", NotificationLevel.Error);
                        return null;
                    }

                    // for numeric symbols try to parse the upper and lower bounds to get the alphabet
                    if (upperIsNumeric)
                    {
                        int lower;
                        if (!int.TryParse(alphabetBounds[0], out lower))
                        {
                            GuiLogMessage("Numeric alphabet not recognized!", NotificationLevel.Error);
                            return null;
                        }
                        int upper;
                        if (!int.TryParse(alphabetBounds[1], out upper))
                        {
                            GuiLogMessage("Numeric alphabet not recognized!", NotificationLevel.Error);
                            return null;
                        }
                        alphabet = new List<string>();
                        for (int i = lower; i <= upper; i++)
                        {
                            alphabet.Add(i.ToString());
                        }
                    }
                    // for letter symbols try to parse the upper and lower bounds to get the alphabet
                    else if (upperIsLetter)
                    {
                        string lowerUpperAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        int lower = lowerUpperAlphabet.IndexOf(char.Parse(alphabetBounds[0]));
                        int upper = lowerUpperAlphabet.IndexOf(char.Parse(alphabetBounds[1]));
                        if (upper < lower)
                        {
                            GuiLogMessage("Latin alphabet not recognized!", NotificationLevel.Error);
                            return null;
                        }

                        string latinAlphabetString = lowerUpperAlphabet.Substring(lower, upper - lower);
                        alphabet = latinAlphabetString.Select(c => c.ToString()).ToList();
                    }
                }
            }
            // if the string contains pipes or spaces, split at those and 
            // return the result as alphabet list
            else if (alphabetString.Contains("|"))
            {
                alphabet = alphabetString.Split('|').ToList();
            }
            else if (_alphabetInput.Contains(' '))
            {
                alphabet = _alphabetInput.Split(' ').ToList();
            }
            // if nothing else matched, split each symbol and return the result as alphabet list
            else
            {
                alphabet = _alphabetInput.Select(c => c.ToString()).ToList();
            }

            return alphabet;
        }

        /// <summary>
        /// Generates the reverse regex random keys according to the settings.
        /// </summary>
        public void generateRandomKeysWithRegex()
        {
            // only generate keys if the number of test runs was not completed
            if (_lastKeyLengthIndex == -1)
            {
                _lastKeyLengthIndex = 0;
            }
            else if (_lastKeyLengthIndex < _settings.NumberOfTestRuns - 1)
            {
                _lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            // check for $length and $unique variables
            var str = _regexInput;
            if (str.Contains("$length"))
            {
                // replace $length with the current key length
                int length = _settings.MinKeyLength + _lastKeyLengthIndex / _settings.KeysPerLength;
                str = str.Replace("$length", length.ToString());
            }
            while (str.Contains("$unique"))
            {
                // parse the $unique string using this format: $unique([A-Z](1){3})
                int uniqueIndex = str.IndexOf("$unique");
                string beforeUnique = str.Substring(0, uniqueIndex);
                string uniqueString = str.Substring(uniqueIndex, str.Length - uniqueIndex);

                // check for multiple opening brackets
                int firstClosingBracketIndex = uniqueString.IndexOf(")");
                int uniqueEndIndex = firstClosingBracketIndex;
                int firstOpeningBracketIndex = uniqueString.IndexOf("(");
                firstOpeningBracketIndex = uniqueString.IndexOf("(", firstOpeningBracketIndex+1);
                // handle multiple opening brackets
                if (firstOpeningBracketIndex != -1 &&
                    firstOpeningBracketIndex < firstClosingBracketIndex)
                {
                    int openingBracketCount = 1;
                    int nextBracketIndex = firstOpeningBracketIndex;
                    int nextClosingBracketIndex = firstClosingBracketIndex;
                    int nextOpeningBracketIndex = firstOpeningBracketIndex;

                    while (openingBracketCount > 0)
                    {
                        nextClosingBracketIndex = uniqueString.IndexOf(")", nextBracketIndex+1);
                        nextOpeningBracketIndex = uniqueString.IndexOf("(", nextBracketIndex+1);

                        // multiple round opening brackets are not supported yet
                        if (nextOpeningBracketIndex != -1 &&
                            nextOpeningBracketIndex < nextClosingBracketIndex)
                        {
                            // TODO: check content between other brackets
                            //openingBracketCount++;
                            //nextBracketIndex = nextOpeningBracketIndex;

                            GuiLogMessage("Multiple round brackets not supported yet!", NotificationLevel.Error);
                            return;
                        }
                        else if (nextOpeningBracketIndex == -1 ||
                            nextOpeningBracketIndex > nextClosingBracketIndex)
                        {
                            openingBracketCount--;
                            nextBracketIndex = nextClosingBracketIndex;
                        }
                        else
                        {
                            GuiLogMessage("Error involving multiple brackets!", NotificationLevel.Error);
                            return;
                        }
                    }

                    // retrieve definitive unique string end index
                    uniqueEndIndex = uniqueString.IndexOf(")", nextClosingBracketIndex+1);
                }

                string afterUnique = "";
                if (uniqueEndIndex < uniqueString.Length - 1)
                    afterUnique = uniqueString.Substring(uniqueEndIndex + 1, uniqueString.Length - (uniqueEndIndex + 1));

                // remove the $unique surrounding
                uniqueString = uniqueString.Substring(0, uniqueEndIndex);
                uniqueString = uniqueString.Replace("$unique(", "");
                
                // find alphabet and length (repetition) for manual random key generation in unique string
                string alphabetString = uniqueString.Between("[", "]");
                string lengthString = uniqueString.Between("{", "}");
                int length;
                if (!int.TryParse(lengthString, out length))
                {
                    GuiLogMessage("Error parsing length string!", NotificationLevel.Error);
                    return;
                }

                // get the alphabet as list
                List<string> alphabet = FindAlphabet(alphabetString);
                if (alphabet == null)
                    return;

                string randomKey = null;

                if (firstOpeningBracketIndex < firstClosingBracketIndex)
                {
                    // find the separator repeat frequency and generate random key
                    string separatorRepeatString = uniqueString.Between("(", ")");
                    int separatorRepeat;
                    if (int.TryParse(separatorRepeatString, out separatorRepeat))
                        // if the separator repeat frequency can be retrieved use it
                        randomKey = GenerateRandomKeyWithAlphabet(alphabet, length, separatorRepeat);
                    else
                        // otherwise use 1 as frequency and generate
                        randomKey = GenerateRandomKeyWithAlphabet(alphabet, length);
                }
                else
                {
                    // if no repetition separator is specified, use 1
                    randomKey = GenerateRandomKeyWithAlphabet(alphabet, length);
                }
                if (randomKey == null)
                    return;

                // replace complete $unique variable in regex pattern with new random key
                str = beforeUnique + randomKey + afterUnique;
            }

            // begin reverse regex generation through Xeger (wrapped for C# by Fare)
            var regex = @str;
            var xeger = new Fare.Xeger(regex, _rand);
            var regexString = xeger.Generate();

            // repeat generation until the key is a new one
            while (_keyList.Contains(regexString))
            {
                regexString = xeger.Generate();
            }

            // add the key to the list
            _keyList.Add(regexString);

            // double check if the key matches the regex pattern
            if (!Regex.IsMatch(regexString, regex))
            {
                GuiLogMessage("regexString \"" + regexString + "\" does not match regex \"" + regex + "\"!", NotificationLevel.Error);

            }

            // set the key output
            _keyOutput = regexString;
        }

        #endregion

        #region General Methods

        /// <summary>
        /// Check if the current input variables and correct some.
        /// </summary>
        public bool checkVariables()
        {
            // set the min and max text lengths equal and the increase 
            // to 0 for disabled extended settings
            if (!_settings.ShowExtendedSettings)
            {
                _settings.MaxTextLength = _settings.MinTextLength;
                _settings.TextLengthIncrease = 0;
            }

            // check for an input text and seed
            if (String.IsNullOrEmpty(_textInput))
            {
                GuiLogMessage("The input text is missing!", NotificationLevel.Error);
                return false;
            }

            if (String.IsNullOrEmpty(SeedInput))
            {
                GuiLogMessage("The input seed is missing!", NotificationLevel.Error);
                return false;
            }

            // check for a regex input if regex is selected
            if (String.IsNullOrEmpty(_regexInput) &&
                _settings.KeyGeneration == GenerationType.regex)
            {
                GuiLogMessage("The input regex is missing!", NotificationLevel.Error);
                return false;
            }

            // check if the min key length if higher than the max and throw an error if
            if (_settings.MinKeyLength > _settings.MaxKeyLength)
            {
                GuiLogMessage("Maximum key length has to be at least minimum key length!", NotificationLevel.Warning);
                _settings.MaxKeyLength = _settings.MinKeyLength;
            }

            // check if the min text length if higher than the max and throw an error if
            if (_settings.MinTextLength > _settings.MaxTextLength)
            {
                GuiLogMessage("Maximum text length has to be at least minimum text length!", NotificationLevel.Warning);
                _settings.MaxTextLength = _settings.MinTextLength;
            }

            // check if the text length increase is to big and throw an error if
            if (_settings.TextLengthIncrease > _settings.MaxTextLength - _settings.MinTextLength)
            {
                GuiLogMessage("The text length increase has to be at most the difference between minimum and maximum text length!", NotificationLevel.Warning);
                _settings.TextLengthIncrease = _settings.MaxTextLength - _settings.MinTextLength;
            }

            // check if the input text is big enough, through an error if not
            if (_textInput.Length < _settings.MinKeyLength ||
                _textInput.Length < _settings.MinTextLength)
            {
                GuiLogMessage("The input text is too small!", NotificationLevel.Error);
                return false;
            }

            // if all variables are set correctly, return true
            return true;
        }
        
        /// <summary> 
        /// Replaces or deletes the dots (full stops) in the given string
        /// according to the settings.
        /// <param name="text">The text to modify</param>
        /// <returns>The modified text</returns>
        /// </summary>
        public string replaceDots(string text)
        {
            // text modifications according to user settings
            text = text + ". ";
            if (_settings.DotSymbolHandling == DotSymbolHandlingMode.Remove)
                text = text.Replace(".", String.Empty);
            else if (_settings.DotSymbolHandling == DotSymbolHandlingMode.Replace)
                text = text.Replace(".", _settings.DotReplacer);
            return text;
        }

        /// <summary> 
        /// Deletes the spaces in the given string according to the settings.
        /// <param name="text">The text to modify</param>
        /// <returns>The modified text</returns>
        /// </summary>
        public string replaceSpaces(string text)
        {
            // text modifications according to user settings
            if (_settings.DeleteSpaces)
                text = text.Replace(" ", String.Empty);
            return text;
        }

        /// <summary> 
        /// Processes the text input array according to the settings.
        /// </summary>
        public void processTextSettings()
        {
            // text modifications according to user settings
            if (_settings.NumbersHandling == NumbersHandlingMode.Remove)
                _textInput = Regex.Replace(_textInput, @"[0-9]", String.Empty);
            else if (_settings.NumbersHandling == NumbersHandlingMode.ReplaceEnglish)
            {
                _textInput = _textInput.Replace("0", "NULL");
                _textInput = _textInput.Replace("1", "ONE");
                _textInput = _textInput.Replace("2", "TWO");
                _textInput = _textInput.Replace("3", "THREE");
                _textInput = _textInput.Replace("4", "FOUR");
                _textInput = _textInput.Replace("5", "FIVE");
                _textInput = _textInput.Replace("6", "SIX");
                _textInput = _textInput.Replace("7", "SEVEN");
                _textInput = _textInput.Replace("8", "EIGHT");
                _textInput = _textInput.Replace("9", "NINE");
            }
            else if (_settings.NumbersHandling == NumbersHandlingMode.ReplaceGerman)
            {
                _textInput = _textInput.Replace("0", "NULL");
                _textInput = _textInput.Replace("1", "EINS");
                _textInput = _textInput.Replace("2", "ZWEI");
                _textInput = _textInput.Replace("3", "DREI");
                _textInput = _textInput.Replace("4", "VIER");
                _textInput = _textInput.Replace("5", "FÜNF");
                _textInput = _textInput.Replace("6", "SECHS");
                _textInput = _textInput.Replace("7", "SIEBEN");
                _textInput = _textInput.Replace("8", "ACHT");
                _textInput = _textInput.Replace("9", "NEUN");
            }
            if (_settings.ReplaceSZ)
                _textInput = _textInput.Replace("ß", "sz");
            if (_settings.ReplaceUmlauts)
            {
                _textInput = _textInput.Replace("ä", "ae");
                _textInput = _textInput.Replace("Ä", "AE");
                _textInput = _textInput.Replace("ö", "oe");
                _textInput = _textInput.Replace("Ö", "OE");
                _textInput = _textInput.Replace("ü", "ue");
                _textInput = _textInput.Replace("Ü", "UE");
            }
            if (_settings.UppercaseOnly)
                _textInput = _textInput.ToUpper();
        }

        /// <summary> 
        /// Preprocesses the input text and split it into the text array.
        /// </summary>
        public void preProcessTextInput()
        {
            // replace double minus and newlines by space and ? and ! by full stops
            TextInput = TextInput.Replace("?", ".");
            TextInput = TextInput.Replace("!", ".");
            // replace newlines by space
            TextInput = TextInput.Replace(System.Environment.NewLine, " ");

            // delete all characters appart from letters, spaces and full stops
            TextInput = Regex.Replace(TextInput, @"[^A-Za-z0-9äöüÄÖÜß. ]+", String.Empty);

            // replace double space by single space
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            TextInput = regex.Replace(TextInput, " ");

            // replace spaces after full stops and delete very last full stop
            TextInput = TextInput.Replace(". ", ".");
            if (TextInput.EndsWith("."))
                TextInput = TextInput.Substring(0, TextInput.Length - 1);

            // process all text modification settings
            processTextSettings();

            // split input text into sentences
            _inputArray = TextInput.Split('.');
        }

        /// <summary> 
        /// Convert a letter key to a numeric key with unique consecutive numbers.
        /// </summary>
        public String ConvertToUniqueNumericKey(String key)
        {
            char[] chars = key.ToCharArray();
            int[] numbers = new int[chars.Length];

            int index = 0;

            for (int i = 0; index < chars.Length; i = i == chars.Length - 1 ? 0 : i + 1)
            {
                char ch1 = chars[i];
                if (ch1 > 'Z' || ch1 < 'a')
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
                numericKey += _settings.Separator + numbers[i];

            return numericKey;
        }

        /// <summary> 
        /// Convert a letter key to a numeric key with the numbers 1-26.
        /// </summary>
        public void ConvertToNumericKey()
        {
            char[] chars = _keyOutput.ToCharArray();
            string numericKey = "";
            foreach (char ch in chars)
            {
                List<string> alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Select(c => c.ToString()).ToList();
                numericKey += alphabet.indexOf(i);
                /*try
                {
                    numericKey += (Convert.ToInt32(ch) - Convert.ToInt32('A'));
                }
                catch (OverflowException)
                {
                    GuiLogMessage("Unable to convert " + ((int)ch).ToString("X4") + " to an Int32.", NotificationLevel.Info);
                }*/
            }

            // add the separator
            numericKey = AddSeparator(numericKey);

            return numericKey;
        }
        
        /// <summary> 
        /// Checks the stringToHash for UTF8 encoding, hashes it with SHA1,
        /// and converts the hash to a 32-bit integer.
        /// <param name="stringToHash">The string to hash and convert</param>
        /// <returns>The integer representation of the SHA1 hash</returns>
        /// </summary>
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
            ProgressChanged(0, 1);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // check if the number of test runs has already been processed
            if (_testRunCount >= _settings.NumberOfTestRuns)
            {
                GuiLogMessage("Number of keys to generate already reached! Skipping generation...", NotificationLevel.Warning);
                return;
            }

            // update the progress bar
            if (_testRunCount > 0)
                ProgressChanged(_testRunCount - 1, _settings.NumberOfTestRuns);
            else
                ProgressChanged(0, _settings.NumberOfTestRuns);

            // check the input variables
            if (!checkVariables())
                return;

            // preprocess the input text in the first execution
            if (_inputArray == null)
                preProcessTextInput();

            // initialize the pseudo-random number generator with the current seed
            _rand = new System.Random(_seedInput);

            // generate the plaintext
            if (_settings.MinTextLength > 0)
                generatePlaintext();

            // update progress bar
            ProgressChanged(_testRunCount - 0.5, _settings.NumberOfTestRuns);

            // generate key
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

            // fill the outputs
            if (!string.IsNullOrEmpty(_keyOutput) &&
                !string.IsNullOrEmpty(_plaintextOutput))
            {
                KeyOutput = _keyOutput;
                PlaintextOutput = _plaintextOutput;
                OnPropertyChanged("KeyOutput");
                OnPropertyChanged("PlaintextOutput");
            }

            // increment the test run counter and set the total keys output
            _testRunCount++;
            if (_testRunCount <= 1) {
                TotalKeys = _settings.NumberOfTestRuns;
                OnPropertyChanged("TotalKeys");
            }

            ProgressChanged(_testRunCount, _settings.NumberOfTestRuns);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            // reset the variables
            _textInput = null;
            _seedInput = new int();
            _regexInput = null;
            _alphabetInput = null;
            _plaintextOutput = null;
            _debugOutput = null;
            _keyOutput = null;

            _testRunCount = 0;
            _inputArray = null;
            _rand = null;
            _keyList = new List<string>();
            _plaintextList = new List<string>();
            _currentTextLength = 0;
            _calculatedTextsCurrentLength = 0;
            _lastKeyLengthIndex = -1;
            _notFound = false;
            _occurrences = null;

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

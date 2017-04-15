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
using System.Linq;
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
        private string _singleKeyOutput;

        private int _keyCount = 0;
        private string[] _inputArray;
        private System.Random _rand;
        private int _startSentence;
        private List<String> _keyList = new List<string>();
        private List<String> _plaintextList = new List<string>();
        private int _keysToGenerate = -1;
        private int _lastKeyLengthIndex = -1;
        private bool _notFound = false;
        ConcurrentDictionary<int, int> _occurences;
        

        #endregion

        #region Data Properties

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

        [PropertyInfo(Direction.OutputData, "SingleKeyOutput", "SingleKeyOutput tooltip description")]
        public string SingleKeyOutput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TotalKeys", "TotalKeys tooltip description")]
        public int TotalKeys
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "DebugOutput", "textOutput tooltip description")]
        public string DebugOutput
        {
            get { return this._debugOutput; }
            set
            {
                this._debugOutput = value;
                OnPropertyChanged("DebugOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "EmptyEvaluationContainer", "EmptyEvaluationContainer tooltip description")]
        public EvaluationContainer EmptyEvaluationContainer
        { get; set; }

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
                    _plaintextOutput = finalPlaintext;
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

            if (_occurences == null)
            {
                _occurences = new ConcurrentDictionary<int, int>();

                for (int i = _settings.MinKeyLength; i <= _settings.MaxKeyLength; i++)
                {
                    _occurences.AddOrUpdate(i, 0, (id, count) => 0);
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
                        _occurences.TryGetValue(smallestMissingLength, out lengthOccurences);
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
                    _occurences.TryGetValue(sentenceLength, out lengthOccurences);
                }
                if (sentenceLength >= _settings.MinKeyLength &&
                        sentenceLength <= _settings.MaxKeyLength &&
                        lengthOccurences < _settings.KeyAmountPerLength &&
                        !_keyList.Contains(sentence))
                {
                    _keyList.Add(sentence);
                    _occurences.AddOrUpdate(sentenceLength, 1, (id, count) => count + 1);

                    // !!! TESTING ONLY !!!
                    //sentence = sentence + " (" + sentence.Length + ")";

                    //int length = 15 < sentence.Length ? 15 : sentence.Length;
                    //Console.WriteLine("sentence: "+ sentence.Substring(0,length) +" - " + _startSentence + "/" + originalStartSentence);

                    // if the letters should be replaced by numbers, do so
                    if (_settings.KeyFormatNaturalSpeech == FormatType.uniqueNumbers)
                    {
                        sentence = convertToNumericKey(sentence);
                        //replaceLettersByNumbersWithSpaces();
                    }
                    else
                    {
                        // add separator
                        sentence = AddSeparator(sentence);
                    }

                    _singleKeyOutput = sentence;

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
            }
        }

        private string AddSeparator(string str)
        {
            char[] arr = str.ToArray();

            string result = arr[0].ToString();
            foreach (char c in arr)
            {
                result = result + _settings.Separator + c;
            }

            return result;
        }

        public void generateRandomKeys()
        {
            if (_keysToGenerate == -1 || _lastKeyLengthIndex == -1)
            {
                _keysToGenerate = (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength;
                _lastKeyLengthIndex = 0;
            }
            else if (_lastKeyLengthIndex < _keysToGenerate - 1)
            {
                _lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            if (_settings.KeyFormatRandom == FormatType.lettersOnly)
            {
                //GuiLogMessage("generate random key with letters only", NotificationLevel.Info);
                string randomKey = "";
                for (int j = 0; j < (_settings.MinKeyLength + _lastKeyLengthIndex / _settings.KeyAmountPerLength); j++)
                {
                    char ch = Convert.ToChar(_rand.Next(0, 26) + Convert.ToInt32('A'));

                    if (randomKey == "")
                        randomKey = ch.ToString();
                    else
                        randomKey = randomKey + _settings.Separator + ch;
                }
                //GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), lastKeyLengthIndex: " + lastKeyLengthIndex, NotificationLevel.Info);

                _singleKeyOutput = randomKey;
            }
            else if (_settings.KeyFormatRandom == FormatType.uniqueLetters ||
                _settings.KeyFormatRandom == FormatType.uniqueNumbers)
            {
                var alphabet = new List<string>();
                if (_alphabetInput == null || String.IsNullOrEmpty(_alphabetInput))
                {
                    if (_settings.KeyFormatRandom == FormatType.uniqueLetters)
                    {
                        alphabet = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z".Split(' ').ToList();
                    }
                    else if (_settings.KeyFormatRandom == FormatType.uniqueNumbers)
                    {
                        alphabet = "0 1 2 3 4 5 6 7 8 9".Split(' ').ToList();
                        //alphabet = "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18".Split(' ').ToList();
                    }
                }
                else
                {
                    alphabet = _alphabetInput.Split(' ').ToList();
                }

                string randomKey = GenerateRandomKeyWithAlphabet(alphabet, 
                    _settings.MinKeyLength + _lastKeyLengthIndex / _settings.KeyAmountPerLength);

                if (randomKey == null)
                    return;
                //GuiLogMessage("randomKey: " + randomKey + "(" + randomKey.Length + "), lastKeyLengthIndex: " + lastKeyLengthIndex, NotificationLevel.Info);

                _singleKeyOutput = randomKey;
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

                string randomKey = "";
                for (int j = 0; j < (_settings.MinKeyLength + _lastKeyLengthIndex / _settings.KeyAmountPerLength); j++)
                {
                    int randomInt = (_rand.Next(0, upperLimit + 1));

                    if (randomKey == "")
                        randomKey = randomInt.ToString();
                    else
                        randomKey = randomKey + _settings.Separator + randomInt;
                }
                _singleKeyOutput = randomKey;
            }
        }

        public string GenerateRandomKeyWithAlphabet(List<string> alphabet, int length)
        {
            return GenerateRandomKeyWithAlphabet(alphabet, length, 1);
        }

        public string GenerateRandomKeyWithAlphabet(List<string> alphabet, int length, int separatorRepeat)
        {
            if (length > alphabet.Count)
            {
                GuiLogMessage("Alphabet length is too short to generate a string of unique letters!", NotificationLevel.Error);
                return null;
            }

            string randomKey = "";

            for (int j = 0; j < length; j++)
            {
                int i = _rand.Next(0, alphabet.Count - 1);

                string symbol = alphabet.ElementAt(i);
                alphabet.RemoveAt(i);

                if (randomKey == "")
                    randomKey = symbol;
                else if (j % separatorRepeat == 0)
                    randomKey = randomKey + _settings.Separator + symbol;
                else
                    randomKey = randomKey + symbol;
            }

            return randomKey;
        }

        public List<string> FindAlphabet(string alphabetString)
        {
            List<string> alphabet = null;
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
                    string[] alphabetBounds = alphabetString.Split('-');
                    bool upperIsNumeric = Regex.IsMatch(alphabetBounds[1], @"[0-9]");
                    bool upperIsLetter = Regex.IsMatch(alphabetBounds[1], @"[a-zA-Z]");

                    bool lowerIsNumeric = Regex.IsMatch(alphabetBounds[0], @"[0-9]");
                    bool lowerIsLetter = Regex.IsMatch(alphabetBounds[0], @"[a-zA-Z]");

                    if (!upperIsNumeric && !upperIsLetter ||
                        !lowerIsNumeric && !lowerIsLetter ||
                        !upperIsNumeric && lowerIsNumeric ||
                        !upperIsLetter && lowerIsLetter)
                    {
                        GuiLogMessage("Alphabet with single '-' not recognized!", NotificationLevel.Error);
                        return null;
                    }

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
            else if (alphabetString.Contains("|"))
            {
                alphabet = alphabetString.Split('|').ToList();

            }
            else if (!String.IsNullOrEmpty(_alphabetInput))
            {
                if (_alphabetInput.Contains(' '))
                {
                    alphabet = _alphabetInput.Split(' ').ToList();
                }
                else
                {
                    alphabet = _alphabetInput.Select(c => c.ToString()).ToList();
                }
            }
            else
            {
                GuiLogMessage("Alphabet not recognized!", NotificationLevel.Error);
                return null;
            }

            return alphabet;
        }

        public void generateRandomKeysWithRegex()
        {
            if (_keysToGenerate == -1 || _lastKeyLengthIndex == -1)
            {
                if (_regexInput.Contains("$amount"))
                {
                    _keysToGenerate = (_settings.MaxKeyLength - _settings.MinKeyLength + 1) * _settings.KeyAmountPerLength;
                }
                else
                {
                    _keysToGenerate = _settings.KeyAmountPerLength;
                }
                _lastKeyLengthIndex = 0;
            }
            else if (_lastKeyLengthIndex < _keysToGenerate - 1)
            {
                _lastKeyLengthIndex++;
            }
            else
            {
                return;
            }

            var str = _regexInput;
            if (str.Contains("$amount"))
            {
                int length = _settings.MinKeyLength + (_lastKeyLengthIndex / _settings.KeyAmountPerLength);
                //GuiLogMessage("length: " + length, NotificationLevel.Warning);
                //var str = "[a-zA-Z]{" + length + "}";
                str = str.Replace("$amount", length.ToString());
            }
            while (str.Contains("$unique"))
            {
                //$unique([0-24]{25})
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

                    uniqueEndIndex = uniqueString.IndexOf(")", nextClosingBracketIndex+1);
                }

                string afterUnique = "";
                if (uniqueEndIndex < uniqueString.Length - 1)
                    afterUnique = uniqueString.Substring(uniqueEndIndex + 1, uniqueString.Length - (uniqueEndIndex + 1));

                uniqueString = uniqueString.Substring(0, uniqueEndIndex);
                uniqueString = uniqueString.Replace("$unique(", "");
                
                
                // find values for manual generation in unique string
                string alphabetString = uniqueString.Between("[", "]");
                string repeatString = uniqueString.Between("{", "}");
                int repeatInt;
                if (!int.TryParse(repeatString, out repeatInt))
                {
                    GuiLogMessage("Error parsing repetition string!", NotificationLevel.Error);
                    return;
                }

                List<string> alphabet = FindAlphabet(alphabetString);
                if (alphabet == null)
                    return;

                string randomKey = null;

                if (firstOpeningBracketIndex < firstClosingBracketIndex)
                {
                    string separatorRepeatString = uniqueString.Between("(", ")");
                    int separatorRepeat;
                    if (int.TryParse(separatorRepeatString, out separatorRepeat))
                        randomKey = GenerateRandomKeyWithAlphabet(alphabet, repeatInt, separatorRepeat);
                    else
                        randomKey = GenerateRandomKeyWithAlphabet(alphabet, repeatInt);
                }
                else
                {
                    randomKey = GenerateRandomKeyWithAlphabet(alphabet, repeatInt);
                }
                if (randomKey == null)
                    return;

                str = beforeUnique + randomKey + afterUnique;
            }

            // beginn reverse regex generation
            var regex = @str;
            var xeger = new Fare.Xeger(regex, _rand);
            var regexString = xeger.Generate();

            /*
            var regex1 = "^.*(.).*\\1.*$";
            var Regex1 = @regex1;

            //((00|01|02|03|04){4}
            //((?:([0-9])(?!.*\2)){10})*
            //((?:(00|01|02|03|04)(?!.*\2)){4})*
            var regex2 = "((?:(05|01|02|03|04)(?!.*\\2)){8})*"; //"^.*(.).*\\2.*$";
            var Regex2 = @regex2;
            //regexString = "01020304";
            */

            while (_keyList.Contains(regexString))
            {
                regexString = xeger.Generate();
                //regexString = "01020304";
            }

            _keyList.Add(regexString);

            // TESTING ONLY!
            //regexString = regexString + " (" + regexString.Length + ")";
            //GuiLogMessage("regexString: " + regexString, NotificationLevel.Warning);

            if (!Regex.IsMatch(regexString, regex))
            {
                GuiLogMessage("regexString \"" + regexString + "\" does not match regex \"" + regex + "\"!", NotificationLevel.Error);

            }

            _singleKeyOutput = regexString;
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
            //DebugOutput = TextInput.Substring(0, substringLength);

            // replace newlines by space
            TextInput = TextInput.Replace(". ", ".");
            if (TextInput.EndsWith("."))
                TextInput = TextInput.Substring(0, TextInput.Length - 1);

            substringLength = 1000 < TextInput.Length ? 1000 : TextInput.Length;
            DebugOutput = TextInput.Substring(0, substringLength);

            // split input text into sentences
            _inputArray = TextInput.Split('.');
        }

        public String convertToNumericKey(String key)
        {
            char[] chars = key.ToCharArray();
            int[] numbers = new int[chars.Length];

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
                numericKey = numericKey + _settings.Separator + numbers[i];
            //Console.WriteLine("numericKey: " + numericKey);

            return numericKey;
        }

        public void replaceLettersByNumbersWithSpaces()
        {
            char[] chars = _singleKeyOutput.ToCharArray();
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

            _singleKeyOutput = transpositionKey;
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
            ProgressChanged(0, 1);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (_keysToGenerate > 0 && _keyCount >= _keysToGenerate)
            {
                Console.WriteLine("Number of keys to generate already reached! Skipping generation...");
                GuiLogMessage("Number of keys to generate already reached! Skipping generation...", NotificationLevel.Warning);
                return;
            }

            if (_keyCount > 0)
                ProgressChanged(_keyCount - 1, _keysToGenerate);
            else
                ProgressChanged(0, _keysToGenerate);

            if (!checkVariables())
                return;

            if (_inputArray == null)
                preProcessTextInput();

            _rand = new System.Random(_seedInput);
            //GuiLogMessage("_seedInput: " + _seedInput, NotificationLevel.Info);

            if (_settings.TextLength > 0)
                generatePlaintext();

            ProgressChanged(_keyCount-0.5, _keysToGenerate);

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

            if (!string.IsNullOrEmpty(_singleKeyOutput) &&
                !string.IsNullOrEmpty(_plaintextOutput))
            {
                SingleKeyOutput = _singleKeyOutput;
                PlaintextOutput = _plaintextOutput;
                OnPropertyChanged("SingleKeyOutput");
                OnPropertyChanged("PlaintextOutput");
            }

            EmptyEvaluationContainer = new EvaluationContainer();
            OnPropertyChanged("EmptyEvaluationContainer");
            _keyCount++;
            if (_keysToGenerate > 0 && _keyCount <= 1) {
                TotalKeys = _keysToGenerate;
                OnPropertyChanged("TotalKeys");
            }

            ProgressChanged(_keyCount, _keysToGenerate);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _textInput = null;
            _seedInput = new int();
            _regexInput = null;
            _alphabetInput = null;
            _plaintextOutput = null;
            _debugOutput = null;
            _singleKeyOutput = null;

            _keyCount = 0;
            _inputArray = null;
            _rand = null;
            _keyList = new List<string>();
            _plaintextList = new List<string>();
            _keysToGenerate = -1;
            _lastKeyLengthIndex = -1;
            _notFound = false;
            _occurences = null;

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

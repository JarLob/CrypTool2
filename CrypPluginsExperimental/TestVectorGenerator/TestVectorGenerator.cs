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

        private readonly TestVectorGeneratorSettings settings = new TestVectorGeneratorSettings();
        private string textInput;
        private int seedInput;
        private string plaintextOutput;
        private string textOutput2;
        private string textOutput3;
        private string[] keyOutput;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description")]
        public string TextInput
        {
            get { return this.textInput; }
            set
            {
                if (textInput != value)
                {
                    this.textInput = value;
                    OnPropertyChanged("TextInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "SeedInput", "SeedInput tooltip description")]
        public string SeedInput
        {
            get { return this.seedInput.ToString(); }
            set
            {
                try
                {
                    int seed = System.Int32.Parse(value);
                    if (seedInput != seed)
                    {
                        this.seedInput = seed;
                        OnPropertyChanged("SeedInput");
                    }
                }
                catch (System.FormatException)
                {
                    GuiLogMessage(value+": Bad Format", NotificationLevel.Error);
                }
                catch (System.OverflowException)
                {
                    GuiLogMessage(value + ": Overflow", NotificationLevel.Error);
                }  
                
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutput tooltip description")]
        public string[] KeyOutput
        {
            get { return this.keyOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (keyOutput != value)
                {
                    this.keyOutput = value;
                    OnPropertyChanged("KeyOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get { return this.plaintextOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (plaintextOutput != value)
                {
                    this.plaintextOutput = value;
                    OnPropertyChanged("PlaintextOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutput2", "textOutput tooltip description")]
        public string TextOutput2
        {
            get { return this.textOutput2; }
            set
            {
                // TODO: check if test works and is necessary
                if (textOutput2 != value)
                {
                    this.textOutput2 = value;
                    OnPropertyChanged("TextOutput2");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "TextOutput3", "textOutput tooltip description")]
        public string TextOutput3
        {
            get { return this.textOutput3; }
            set
            {
                // TODO: check if test works and is necessary
                if (textOutput3 != value)
                {
                    this.textOutput3 = value;
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
            get { return settings; }
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
            plaintextOutput = "";
            textOutput2 = "";
            textOutput3 = "";
            keyOutput = null;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (textInput.Length < settings.MinKeyLength)
            {
                GuiLogMessage("The input text is too small!", NotificationLevel.Error);
                return;
            }

            if (settings.MinKeyLength < 1)
            {
                GuiLogMessage("KeyLength has to be at least 1", NotificationLevel.Warning);
                settings.MinKeyLength = 1;
            }
            
            ProgressChanged(0, 1);
            int progress = 0;

            // ##### PRE-PROCESSING of the text input #####
            TextInput = TextInput.Replace("--", " ");
            TextInput = TextInput.Replace("?", ".");
            TextInput = TextInput.Replace("!", ".");
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
            string[] inputArray = TextInput.Split('.');
            // ############################################

            // ##### Plaintext generation #################

            var rand = new System.Random(seedInput);
            int startSentence = rand.Next(0, inputArray.Length);
            GuiLogMessage("seedInput: " + seedInput + ", rand: " + rand + 
                ", Length: " + inputArray.Length + ", StartSentence: " + startSentence, NotificationLevel.Info);
            for (int i = startSentence; i != startSentence -1; i = i == inputArray.Length-1 ? 0 : i+1)
            {
                plaintextOutput = plaintextOutput + inputArray[i] + ". ";
                if (plaintextOutput.Length >= settings.TextLength)
                {
                    PlaintextOutput = plaintextOutput.Substring(0, settings.TextLength);
                    break;
                }
            }

            // ############################################

            // ##### Key generation #######################
            List<string> outputList = new List<string>();
            ConcurrentDictionary<int, int> occurences = new ConcurrentDictionary<int, int>();
            startSentence = rand.Next(0, inputArray.Length);
            GuiLogMessage("seedInput: " + seedInput + ", StartSentence: " + startSentence, NotificationLevel.Info);

            for (int i = settings.MinKeyLength; i <= settings.MaxKeyLength; i++)
            {
                    occurences.AddOrUpdate(i, 0, (id, count) => 0);
                    //GuiLogMessage("Initialize: " + i, NotificationLevel.Debug);
            }

            for (int i = startSentence; i != startSentence - 1; i = i == inputArray.Length-1 ? 0 : i + 1)
            {
                string sentence = inputArray[i].ToUpper().Replace(" ", "");
                //GuiLogMessage("Checking sentence: \"" + sentence + "\"", NotificationLevel.Debug);
                int sentenceLength = sentence.Length;
                if (sentenceLength >= settings.MinKeyLength &&
                    sentenceLength <= settings.MaxKeyLength)
                {
                    int sentenceOccurences = 0;
                    occurences.TryGetValue(sentenceLength, out sentenceOccurences);

                    if (sentenceOccurences < settings.KeyAmountPerLength)
                    {
                        outputList.Add(sentence);
                        occurences.AddOrUpdate(sentenceLength, 1, (id, count) => count + 1);

                        GuiLogMessage("i: " + i + " ("+ (i >= startSentence ? i-startSentence : i+inputArray.Length-startSentence-1) + 
                            "/" + (inputArray.Length-1) + ") - Adding sentence: \"" + sentence +
                            "\", occurences[" + sentenceLength + "]: " + occurences[sentenceLength], 
                            NotificationLevel.Info);



                        progress++;

                        ProgressChanged(progress / (settings.MaxKeyLength - settings.MinKeyLength + 1) *
                            settings.KeyAmountPerLength, 1);
                    }
                }

                if (allKeysFound(occurences))
                    break;

            }

            if (!allKeysFound(occurences))
            {
                for (int i = startSentence; i != startSentence - 1; i = i == inputArray.Length-1 ? 0 : i + 1)
                {
                    string sentence = inputArray[i];
                    //GuiLogMessage("Checking sentence: \"" + sentence + "\"", NotificationLevel.Debug);
                    int sentenceLength = sentence.Length;
                    int smallestMissingLength = settings.MinKeyLength-1;

                    if (sentenceLength > settings.MaxKeyLength)
                    {

                        int sentenceOccurences = settings.KeyAmountPerLength;

                        while (smallestMissingLength <= settings.MaxKeyLength &&
                            sentenceOccurences == settings.KeyAmountPerLength)
                        {
                            smallestMissingLength++;
                            occurences.TryGetValue(smallestMissingLength, out sentenceOccurences);
                        }

                        // double check, should not happen
                        if (smallestMissingLength <= settings.MaxKeyLength &&
                            sentenceOccurences > settings.KeyAmountPerLength)
                        {
                            GuiLogMessage("Too many sentences added for length: " + 
                                smallestMissingLength, NotificationLevel.Debug);
                        }

                        if (smallestMissingLength > settings.MaxKeyLength)
                        {
                            if (allKeysFound(occurences))
                            {
                                break;
                            }
                            else
                            {
                                GuiLogMessage("Something went wrong! Some sentences are missing "+
                                    "(not correctly added?!)", NotificationLevel.Warning);
                                break;
                            }
                        }


                        string cutSentence = sentence.Substring(0, smallestMissingLength);
                        outputList.Add(cutSentence);
                        occurences.AddOrUpdate(smallestMissingLength, 1, (id, count) => count + 1);

                        GuiLogMessage("i: " + i + " (" + (i >= startSentence ? i - startSentence : i + 
                            inputArray.Length - startSentence - 1) + ") - Adding cut sentence: \"" + 
                            cutSentence + "\", occurences[" + smallestMissingLength + "]: " + 
                            occurences[smallestMissingLength], NotificationLevel.Info);

                        progress++;

                        ProgressChanged(progress / (settings.MaxKeyLength - settings.MinKeyLength + 1) *
                            settings.KeyAmountPerLength, 1);
                    }


                }

                if (!allKeysFound(occurences))
                {
                    GuiLogMessage("Somethings wrong! Not all key lengths found!", NotificationLevel.Warning);

                }
            }

            outputList.Sort();

            KeyOutput = outputList.ToArray();

            if (settings.KeyFormat == FormatType.transformation)
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
                            transpositionKey = transpositionKey + space + (Convert.ToInt32(ch) - Convert.ToInt32('A') + 1);
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

            ProgressChanged(1, 1);
        }

        private bool allKeysFound(ConcurrentDictionary<int, int> dict) {
            foreach (int key in dict.Keys)
            {
                //GuiLogMessage("dict[" + key + "]: " + dict[key], NotificationLevel.Info);
                if (dict[key] < settings.KeyAmountPerLength)
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

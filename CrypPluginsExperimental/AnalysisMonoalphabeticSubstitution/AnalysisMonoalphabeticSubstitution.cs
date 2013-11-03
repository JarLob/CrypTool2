﻿/*
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
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Diagnostics;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutputCiphertext(List<LetterPair> lp);
    public delegate void RestartSearch();
    delegate double CalculateFitness(Text plaintext);

    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("AnalysisMonoalphabeticSubstitution.Properties.Resources","PluginCaption", "PluginTooltip", null, "CrypWin/images/default.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Frequencies langFreq = null;
        private LanguageDictionary langDic = null;
        private Text cText = null;
        private Text refText = null;
        private Boolean caseSensitive = false;
        private String wordSeparator;

        // Statistics
        private TimeSpan total_time = new TimeSpan();
        private TimeSpan currun_time;

        // Input property variables
        private ICryptoolStream ciphertext;
        private String ciphertext_alphabet;
        private String plaintext_alphabet;
        private ICryptoolStream reference_text;
        private ICryptoolStream language_dictionary;

        // Output property variables
        private String plaintext;
        private String plaintext_alphabet_output;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();

        // Alphabet constants
        private const String smallEng = "abcdefghijklmnopqrstuvwxyz";
        private const String capEng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const String smallGer = "abcdefghijklmnopqrstuvwxyzöäüß";
        private const String capGer = "ABCDEFGHIJKLMNOPQRSTUVWXYZÖÄÜ";

        // Key for presentation
        private Analyzer analyzer = new Analyzer();
        private List<LetterPair> pairs = new List<LetterPair>();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PropCiphertextCaption", "PropCiphertextTooltip", true)]
        public ICryptoolStream Ciphertext
        {
            get { return this.ciphertext; }
            set {
                this.ciphertext = value;
                //this.ciphertext_has_changed = true;
            }
        }
        /*
        [PropertyInfo(Direction.InputData, "PropCiphertextalphabetCaption", "PropCiphertextalphabetTooltip", false)]
        public String Ciphertext_Alphabet
        {
            get { return this.ciphertext_alphabet; }
            set {
                this.ciphertext_alphabet = value;
                //this.ciphertext_alphabet_has_changed = true;
            }
        }

        [PropertyInfo(Direction.InputData, "PropPlaintextalphabetCaption", "PropPlaintextalphabetTooltip", false)]
        public String Plaintext_Alphabet
        {
            get { return this.plaintext_alphabet; }
            set{
                this.plaintext_alphabet = value;
                //this.plaintext_alphabet_has_changed = true;
            }
        }

        [PropertyInfo(Direction.InputData, "PropReferencetextCaption", "PropReferencetextTooltip", false)]
        public ICryptoolStream Reference_Text
        {
            get { return this.reference_text; }
            set {
                this.reference_text = value;
                //this.reference_text_has_changed = true;
            }
        }
        */
        [PropertyInfo(Direction.InputData, "PropDictionaryCaption", "PropDictionaryTooltip", false)]
        public ICryptoolStream Language_Dictionary
        {
            get { return this.language_dictionary; }
            set {
                this.language_dictionary = value;
                //this.language_dictionary_has_changed = true;
            }
        }

        [PropertyInfo(Direction.OutputData, "PropPlaintextCaption", "PropPlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
            set { }
            //set { this.plaintext = value; }
        }

        [PropertyInfo(Direction.OutputData, "PropPlaintextalphabetoutputCaption", "PropPlaintextalphabetoutputTooltip", true)]
        public String Plaintext_Alphabet_Output
        {
            get { return this.plaintext_alphabet_output; }
            set { }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return this.masPresentation; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            String alpha = "";
            Boolean inputOK = true;

            // Prepare the cryptanalysis of the ciphertext

            // Set ciphertext alphabet, plaintext alphabet 
            if (settings.SeparateAlphabets == false)
            {
                // Set ciphertext and plaintext alphabet
                if (settings.boAlphabet == 0 || settings.boAlphabet == 1)
                {
                    alpha = detAlphabet(settings.boAlphabet, settings.boCaseSensitive);
                    this.ptAlphabet = new Alphabet(alpha, 1);
                    this.ctAlphabet = new Alphabet(alpha, 1);
                    this.caseSensitive = settings.boCaseSensitive;
                }
                else if (settings.boAlphabet == 2)
                {
                    //this.PTAlphabet = this.plainAlphabet;
                    //this.CTAlphabet = this.cipherAlphabet;
                }
            }
            else if (settings.SeparateAlphabets == true)
            {
                // Set plaintext alphabet
                if (settings.ptAlphabet == 0 || settings.ptAlphabet == 1)
                {
                    alpha = detAlphabet(settings.ptAlphabet, settings.ptCaseSensitive);
                    this.ptAlphabet = new Alphabet(alpha, 1);

                }
                else if (settings.ptAlphabet == 2)
                {
                    //this.PTAlphabet = this.plainAlphabet;  
                }

                // Set ciphertext alphabet
                if (settings.ctAlphabet == 0 || settings.ctAlphabet == 1)
                {
                    alpha = detAlphabet(settings.ctAlphabet, settings.ctCaseSensitive);
                    this.ctAlphabet = new Alphabet(alpha, 1);
                    this.caseSensitive = settings.ctCaseSensitive;
                }
                else if (settings.ctAlphabet == 2)
                {
                    // this.CTAlphabet = this.cipherAlphabet;
                }
            }

            // N-gram probabilities 
            if (settings.SeparateAlphabets == false)
            {
                if (settings.boAlphabet == 0 || settings.boAlphabet == 1)
                {
                    String helper = IdentifyNGramFile(settings.boAlphabet, this.caseSensitive);
                    if (helper != null)
                    {
                        this.langFreq = new Frequencies(this.ptAlphabet);
                        this.langFreq.ReadProbabilitiesFromNGramFile(helper);
                    }
                    else
                    {
                        GuiLogMessage("No ngram file available.", NotificationLevel.Error);
                    }
                }
                else if (settings.boAlphabet == 2)
                {
                    String helper = null;
                    try
                    {
                        helper = returnStreamContent(this.reference_text);
                    }
                    catch
                    {
                        GuiLogMessage("No reference text as input.", NotificationLevel.Error);
                        this.refText = null;
                    }
                    if (helper != null)
                    {
                        this.refText = new Text(helper, this.ptAlphabet, this.caseSensitive);
                        this.langFreq = new Frequencies(this.ptAlphabet);
                        this.langFreq.CreateProbabilitiesFromReferenceText(this.refText);
                    }
                }
            }
            else if (settings.SeparateAlphabets == true)
            {
                if (settings.ptAlphabet == 0 || settings.ptAlphabet == 1)
                {
                    String helper = IdentifyNGramFile(settings.ptAlphabet, this.caseSensitive);
                    if (helper != null)
                    {
                        this.langFreq = new Frequencies(this.ptAlphabet);
                        this.langFreq.ReadProbabilitiesFromNGramFile(helper);
                    }
                    else
                    {
                        GuiLogMessage("No ngram file available.", NotificationLevel.Error);
                    }
                }
                else if (settings.ptAlphabet == 2)
                {
                    String helper = null;
                    try
                    {
                        helper = returnStreamContent(this.reference_text);
                    }
                    catch
                    {
                        GuiLogMessage("No reference text as input.", NotificationLevel.Error);
                        this.refText = null;
                    }
                    if (helper != null)
                    {
                        this.refText = new Text(helper, this.ptAlphabet, this.caseSensitive);
                        this.langFreq = new Frequencies(this.ptAlphabet);
                        this.langFreq.CreateProbabilitiesFromReferenceText(this.refText);
                    }
                }
            }


            // Dictionary
            if (settings.SeparateAlphabets == false)
            {
                if (settings.boAlphabet == 0)
                {
                    try
                    {
                        this.langDic = new LanguageDictionary("dictionary_english.txt", ' ');
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                    }
                }
                else if (settings.boAlphabet == 1)
                {
                    try
                    {
                        this.langDic = new LanguageDictionary("dictionary_german.txt", ' ');
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining German language dictionary file.", NotificationLevel.Error);
                    }
                }
                else if (settings.boAlphabet == 2)
                {
                    String helper = null;
                    try
                    {
                        helper = returnStreamContent(this.language_dictionary);
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining language dictionary stream.", NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        this.langDic = new LanguageDictionary(helper, ' ');
                    }
                }
            }
            else if (settings.SeparateAlphabets == true)
            {
                if (settings.ptAlphabet == 0)
                {
                    try
                    {
                        this.langDic = new LanguageDictionary("dictionary_english.txt", ' ');
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                    }
                }
                else if (settings.ptAlphabet == 1)
                {
                    try
                    {
                        this.langDic = new LanguageDictionary("dictionary_german.txt", ' ');
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining English language dictionary file", NotificationLevel.Error);
                    }
                }
                else if (settings.ptAlphabet == 2)
                {
                    String helper = null;
                    try
                    {
                        helper = returnStreamContent(this.language_dictionary);
                    }
                    catch
                    {
                        GuiLogMessage("Error while obtaining language dictionary stream.", NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        this.langDic = new LanguageDictionary(helper, ' ');
                    }
                }
            }

            // Set ciphertext
            String helper1 = null;
            try
            {
                helper1 = returnStreamContent(this.ciphertext);
            }
            catch
            {
                GuiLogMessage("Error while obtaining ciphertext.", NotificationLevel.Error);
            }
            if (helper1 != null)
            {
                this.cText = new Text(helper1, this.ctAlphabet, this.caseSensitive);
            }
            else
            {
                this.cText = null;
            }

            // Check word separator
            if (settings.UseDefaultWordSeparator == true)
            {
                this.wordSeparator = " ";
            }
            else
            {
                if (settings.DefaultWordSeparator == "")
                {
                    this.wordSeparator = " ";
                }
                else
                {
                    this.wordSeparator = settings.DefaultWordSeparator;
                }
            }


            // PTAlphabet correct?
            if (this.ptAlphabet == null)
            {
                GuiLogMessage("No plaintext alphabet is set", NotificationLevel.Error);
                inputOK = false;
            }
            // CTAlphabet correct?
            if (this.ctAlphabet == null)
            {
                GuiLogMessage("No ciphertext alphabet is set", NotificationLevel.Error);
                inputOK = false;
            }
            // Ciphertext correct?
            if (this.ciphertext == null)
            {
                GuiLogMessage("No ciphertext is set", NotificationLevel.Error);
                inputOK = false;
            }
            // Dictionary correct?
            if (this.langDic == null)
            {
                GuiLogMessage("No language dictionary is set", NotificationLevel.Warning);
            }
            // Language frequencies
            if (this.langFreq == null)
            {
                GuiLogMessage("No language frequencies available.", NotificationLevel.Error);
                inputOK = false;
            }
            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length != this.ptAlphabet.Length)
            {
                GuiLogMessage("Length of ciphertext alphabet and plaintext alphabet is different.", NotificationLevel.Error);
                inputOK = false;
            }


            // If input incorrect return
            if (inputOK == false)
            {
                inputOK = true;
                return;
            }

            // Create new analyzer
            this.analyzer = new Analyzer();
            this.pairs.Clear();
            
            // Initialize analyzer
            this.analyzer.Ciphertext = this.cText;
            this.analyzer.Ciphertext_Alphabet = this.ctAlphabet;
            this.analyzer.Plaintext_Alphabet = this.ptAlphabet;
            this.analyzer.Language_Frequencies = this.langFreq;
            this.analyzer.Language_Dictionary = null; // this.langDic;
            this.analyzer.WordSeparator = this.wordSeparator;
            this.analyzer.SetPluginProgressCallback(ProgressChanged);
            
            // Conduct analysis
            Stopwatch watch = new Stopwatch();
            watch.Start();
            this.analyzer.Analyze();
            watch.Stop();
            this.total_time = watch.Elapsed;
            this.currun_time = watch.Elapsed;

            // Show result
            this.plaintext = this.analyzer.Plaintext.ToString(this.ptAlphabet);
            OnPropertyChanged("Plaintext");

            // Set letter assignment in user interface
            int[] key = this.analyzer.Key;
            String alpha_output = "";
            for (int i = 0; i < this.ctAlphabet.Length; i++)
            {
                LetterPair lp = new LetterPair
                {
                    Ciphertext_letter = this.ctAlphabet.GetLetterFromPosition(i),
                    Plaintext_letter = this.ptAlphabet.GetLetterFromPosition(key[i])
                };
                pairs.Add(lp);
                alpha_output += lp.Plaintext_letter + ";";
                
            }

            // Show plaintext alphabet
            this.plaintext_alphabet_output = alpha_output;
            OnPropertyChanged("Plaintext_Alphabet_Output");

            // Refresh GUI
            this.masPresentation.RefreshGUI();
            this.masPresentation.EnableGUI();

            string totalTime = String.Format("{0:00}:{1:00}:{2:00}", this.total_time.Minutes, this.total_time.Seconds, this.total_time.Milliseconds / 10);
            string curTime = String.Format("{0:00}:{1:00}:{2:00}", this.currun_time.Minutes, this.currun_time.Seconds, this.currun_time.Milliseconds / 10);

            GuiLogMessage("Current analysis time: " + curTime + "   Total analysis time: " + totalTime, NotificationLevel.Info);
            GuiLogMessage("Current number of tested keys: " + this.analyzer.Currun_Keys + "   Total number of tested keys: " + this.analyzer.Total_Keys, NotificationLevel.Info);
        }

        public void PostExecution()
        {
            this.masPresentation.DisableGUI();
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            this.settings.Initialize();
            this.masPresentation.SetRestartSearch(RestartSearch);
            this.masPresentation.SetUpdateOutputCiphertext(UpdateCiphertext);
            this.masPresentation.DisableGUI();
            this.masPresentation.Data = this.pairs;
            this.masPresentation.ConnectDataSource();
            this.masPresentation.RefreshGUI();
        }

        public void Dispose()
        {
        }

        public void UpdateCiphertext(List<LetterPair> lp)
        {
            int[] key = new int[this.ctAlphabet.Length];
            String alpha_output = "";

            for (int i = 0; i < key.Length; i++)
            {
                key[i] = this.ptAlphabet.GetPositionOfLetter(lp[i].Plaintext_letter);
                alpha_output += lp[i].Plaintext_letter+";";
            }
            
            this.plaintext = this.analyzer.DecryptCiphertext(key);
            OnPropertyChanged("Plaintext");
            this.plaintext_alphabet_output = alpha_output;
            OnPropertyChanged("Plaintext_Alphabet_Output");
        }

        public void RestartSearch()
        {
            // Conduct analysis
            this.analyzer.Language_Dictionary = null;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            this.analyzer.Analyze();
            watch.Stop();
            this.total_time.Add(watch.Elapsed);
            this.currun_time = watch.Elapsed;

            // Show result
            this.plaintext = this.analyzer.Plaintext.ToString(this.ptAlphabet);
            OnPropertyChanged("Plaintext");

            // Set letter assignment in user interface
            this.pairs.Clear();
            int[] key = this.analyzer.Key;
            String alpha_output = "";
            for (int i = 0; i < this.ctAlphabet.Length; i++)
            {
                LetterPair lp = new LetterPair
                {
                    Ciphertext_letter = this.ctAlphabet.GetLetterFromPosition(i),
                    Plaintext_letter = this.ptAlphabet.GetLetterFromPosition(key[i])
                };
                pairs.Add(lp);
                alpha_output += lp.Plaintext_letter + ";";
            }
            // Show plaintext alphabet
            this.plaintext_alphabet_output = alpha_output;
            OnPropertyChanged("Plaintext_Alphabet_Output");

            // Refresh GUI
            this.masPresentation.RefreshGUI();
            this.masPresentation.EnableGUI();

            ProgressChanged(1, 1);

            string totalTime = String.Format("{0:00}:{1:00}:{2:00}", this.total_time.Minutes, this.total_time.Seconds, this.total_time.Milliseconds / 10);
            string curTime = String.Format("{0:00}:{1:00}:{2:00}", this.currun_time.Minutes, this.currun_time.Seconds, this.currun_time.Milliseconds / 10);

            GuiLogMessage("Current analysis time: " + curTime + "   Total analysis time: " + totalTime, NotificationLevel.Info);
            GuiLogMessage("Current number of tested keys: " + this.analyzer.Currun_Keys + "   Total number of tested keys: " + this.analyzer.Total_Keys, NotificationLevel.Info);
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

        #region Helper Functions

        private string returnFileContent(String filename)
        {
            string res = "";

            using (TextReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename)))
            {
                res = reader.ReadToEnd();
            }
            return res;
        }

        private string returnStreamContent(ICryptoolStream stream)
        {
            string res = "";

            if (stream == null)
            {
                return "";
            }

            using (CStreamReader reader = stream.CreateReader())
            {
                res = Encoding.Unicode.GetString(reader.ReadFully());
            }

            return res;
        }

        private string detAlphabet(int lang, bool caseSensitive)
        {
            String alpha = "";
            if (lang == 0)
            {
                if (caseSensitive == true)
                {
                    alpha = AnalysisMonoalphabeticSubstitution.smallEng + AnalysisMonoalphabeticSubstitution.capEng;
                }
                else
                {
                    alpha = AnalysisMonoalphabeticSubstitution.smallEng;
                }
            }
            else if ( lang == 1)
            {
                if (caseSensitive == true)
                {
                    alpha = AnalysisMonoalphabeticSubstitution.smallGer + AnalysisMonoalphabeticSubstitution.capGer;
                }
                else
                {
                    alpha = AnalysisMonoalphabeticSubstitution.smallGer;
                }
            }

            return alpha;
        }

        private string IdentifyNGramFile(int alpha_nr, bool cs)
        {
            string name = "";
            string lang = "";
            string casesen = "";
            if (alpha_nr == 0)
            {
                lang = "en";
            } 
            else if (alpha_nr == 1)
            {
                lang = "de";
            }
            if (cs == false)
            {
                casesen = "nocs";
            } else
            {
                casesen = "cs";
            }

            for (int i = 7; i > 0; i--)
            {
                name = lang + "-" + i.ToString() + "gram-" + casesen + ".lm";
                if (File.Exists(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, name)))
                {
                    return name;
                }
            }
            return null;
        }

        #endregion
    }

    public class LetterPair
    {
        public string Ciphertext_letter { get; set; }
        public string Plaintext_letter { get; set; }
    }
}

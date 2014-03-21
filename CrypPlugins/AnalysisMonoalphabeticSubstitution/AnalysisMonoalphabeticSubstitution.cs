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
using System.Windows.Threading;
using System.Threading;
using AnalysisMonoalphabeticSubstitution.Properties;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String key_string, String plaintext_string);
    delegate double CalculateFitness(Text plaintext);
    delegate void UpdateKeyDisplay(KeyCandidate keyCan);

    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("AnalysisMonoalphabeticSubstitution.Properties.Resources", "PluginCaption", "PluginTooltip", "AnalysisMonoalphabeticSubstitution/Documentation/doc.xml", "AnalysisMonoalphabeticSubstitution/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // StopFlag
        StopFlag stopFlag = new StopFlag();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Frequencies langFreq = null;
        private Dictionary langDic = null;
        private Text cText = null;
        private Text refText = null;
        private Boolean caseSensitive = false;
        private String wordSeparator;
        private List<KeyCandidate> keyCandidates;

        // Statistics
        private TimeSpan total_time = new TimeSpan();
        private TimeSpan currun_time;

        // Input property variables
        private String ciphertext;
        private String ciphertext_alphabet;
        private String plaintext_alphabet;
        private ICryptoolStream reference_text;
        private ICryptoolStream language_dictionary;

        // Output property variables
        private String plaintext;
        private String plaintext_alphabet_output;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();
        private DateTime startTime;
        private DateTime endTime;

        // Alphabet constants
        private const String smallEng = "abcdefghijklmnopqrstuvwxyz";
        private const String capEng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const String smallGer = "abcdefghijklmnopqrstuvwxyz";
        private const String capGer = "ABCDEFGHIJKLMNOPQRSTUVWXYZÖÄÜ";

        // Attackers
        private DictionaryAttacker dicAttacker;
        private GeneticAttacker genAttacker;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PropCiphertextCaption", "PropCiphertextTooltip", true)]
        public String Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
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
        
        [PropertyInfo(Direction.InputData, "PropDictionaryCaption", "PropDictionaryTooltip", false)]
        public ICryptoolStream Language_Dictionary
        {
            get { return this.language_dictionary; }
            set {
                this.language_dictionary = value;
                //this.language_dictionary_has_changed = true;
            }
        }*/

        [PropertyInfo(Direction.OutputData, "PropPlaintextCaption", "PropPlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
            set { }
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
            this.genAttacker = new GeneticAttacker();
            this.dicAttacker = new DictionaryAttacker();

            String alpha = "";
            Boolean inputOK = true;

            // Clear presentation
            ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).entries.Clear();
            }, null);

            // Prepare the cryptanalysis of the ciphertext

            // Set ciphertext alphabet, plaintext alphabet 
            if (settings.SeparateAlphabets == false)
            {
                // Set ciphertext and plaintext alphabet
                if (settings.boAlphabet == 0 || settings.boAlphabet == 1)
                {
                    alpha = detAlphabet(settings.boAlphabet, settings.bo_caseSensitive);
                    this.ptAlphabet = new Alphabet(alpha, 1, settings.boAlphabet);
                    this.ctAlphabet = new Alphabet(alpha, 1, settings.boAlphabet);
                    this.caseSensitive = settings.bo_caseSensitive;
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
                    alpha = detAlphabet(settings.ptAlphabet, settings.pt_caseSensitive);
                    this.ptAlphabet = new Alphabet(alpha, 1, settings.ptAlphabet);

                }
                else if (settings.ptAlphabet == 2)
                {
                    //this.PTAlphabet = this.plainAlphabet;  
                }

                // Set ciphertext alphabet
                if (settings.ctAlphabet == 0 || settings.ctAlphabet == 1)
                {
                    alpha = detAlphabet(settings.ctAlphabet, settings.ct_caseSensitive);
                    this.ctAlphabet = new Alphabet(alpha, 1, settings.ptAlphabet);
                    this.caseSensitive = settings.ct_caseSensitive;
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
                        GuiLogMessage(Resources.no_ngram_file, NotificationLevel.Error);
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
                        GuiLogMessage(Resources.no_reference_text, NotificationLevel.Error);
                        this.refText = null;
                    }
                    if (helper != null)
                    {
                        this.refText = new Text(helper, this.ptAlphabet, settings.TreatmentInvalidChars);
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
                        GuiLogMessage(Resources.no_ngram_file, NotificationLevel.Error);
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
                        GuiLogMessage(Resources.no_reference_text, NotificationLevel.Error);
                        this.refText = null;
                    }
                    if (helper != null)
                    {
                        this.refText = new Text(helper, this.ptAlphabet, settings.TreatmentInvalidChars);
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
                        this.langDic = new Dictionary("en-small.dic");
                    }
                    catch
                    {
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
                    }
                }
                else if (settings.boAlphabet == 1)
                {
                    try
                    {
                        this.langDic = new Dictionary("de-small.dic");
                    }
                    catch
                    {
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
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
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        this.langDic = new Dictionary(helper);
                    }
                }
            }
            else if (settings.SeparateAlphabets == true)
            {
                if (settings.ptAlphabet == 0)
                {
                    try
                    {
                        this.langDic = new Dictionary("en-small.dic");
                    }
                    catch
                    {
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
                    }
                }
                else if (settings.ptAlphabet == 1)
                {
                    try
                    {
                        this.langDic = new Dictionary("de-small.dic");
                    }
                    catch
                    {
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
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
                        GuiLogMessage(Resources.error_dictionary, NotificationLevel.Error);
                    }
                    if (helper != null)
                    {
                        this.langDic = new Dictionary(helper);
                    }
                }
            }

            // Set ciphertext
            String helper1 = null;
            try
            {
                //helper1 = returnStreamContent(this.ciphertext);
                if (this.ciphertext.Length != 0)
                {
                    helper1 = this.ciphertext;
                }
            }
            catch
            {
                GuiLogMessage(Resources.error_ciphertext, NotificationLevel.Error);
            }
            if (helper1 != null)
            {
                this.cText = new Text(helper1, this.ctAlphabet, settings.TreatmentInvalidChars);
            }
            else
            {
                this.cText = null;
            }



            // PTAlphabet correct?
            if (this.ptAlphabet == null)
            {
                GuiLogMessage(Resources.no_plaintext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }
            // CTAlphabet correct?
            if (this.ctAlphabet == null)
            {
                GuiLogMessage(Resources.no_ciphertext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }
            // Ciphertext correct?
            if (this.cText == null)
            {
                GuiLogMessage(Resources.no_ciphertext, NotificationLevel.Error);
                inputOK = false;
            }
            // Dictionary correct?
            if (this.langDic == null)
            {
                GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
            }
            // Language frequencies
            if (this.langFreq == null)
            {
                GuiLogMessage(Resources.no_lang_freq, NotificationLevel.Error);
                inputOK = false;
            }
            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length != this.ptAlphabet.Length)
            {
                GuiLogMessage(Resources.error_alphabet_length, NotificationLevel.Error);
                inputOK = false;
            }

            // If input incorrect return otherwise execute analysis
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            
            if (inputOK == false)
            {
                inputOK = true;
                return;
            }
            else
            {
                this.UpdateDisplayStart();
                //this.masPresentation.DisableGUI();
                this.masPresentation.UpdateOutputFromUserChoice = this.UpdateOutput;
                this.keyCandidates = new List<KeyCandidate>();
                if (this.langDic == null)
                {
                    AnalyzeGenetic();
                }
                else
                {
                    AnalyzeDictionary();
                    AnalyzeGenetic();
                }
                //this.masPresentation.EnableGUI();
                this.UpdateDisplayEnd();
            }
        }

        public void PostExecution()
        {
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = false;
            }
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            this.dicAttacker.StopFlag = true;
            this.genAttacker.StopFlag = true;
            this.langDic.StopFlag = true;
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = true;
            }
        }

        public void Initialize()
        {
            this.settings.Initialize();
        }

        public void Dispose()
        {
        }

        private void AnalyzeDictionary()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ////////////////////// Create keys with dictionary attacker
            // Initialize dictionary attacker
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
                //this.dicAttacker = new DictionaryAttacker();
            this.dicAttacker.ciphertext = this.cText;
            this.dicAttacker.languageDictionary = this.langDic;
            this.dicAttacker.frequencies = this.langFreq;
            this.dicAttacker.ciphertext_alphabet = this.ctAlphabet;
            this.dicAttacker.plaintext_alphabet = this.ptAlphabet;
            this.dicAttacker.PluginProgressCallback = this.ProgressChanged;
            this.dicAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            

            // Prepare text
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            this.dicAttacker.PrepareAttack();
            

            // Deterministic search
            // Try to find full solution with all words enabled
     
                this.dicAttacker.SolveDeterministicFull();

                // Try to find solution with disabled words
                if (!this.dicAttacker.CompleteKey)
                {
                    this.dicAttacker.SolveDeterministicWithDisabledWords();

                    // Randomized search;
                    if (!this.dicAttacker.PartialKey)
                    {
                        this.dicAttacker.SolveRandomized();
                    }
                }
                  

            watch.Stop();

            string curTime = String.Format("{0:00}:{1:00}:{2:00}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
            GuiLogMessage(Resources.dic_attack_finished + curTime, NotificationLevel.Info);
        }

        private void AnalyzeGenetic()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            ////////////////// Create keys with genetic attacker
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }

            // Initialize analyzer
            this.genAttacker.Ciphertext = this.cText;
            this.genAttacker.Ciphertext_Alphabet = this.ctAlphabet;
            this.genAttacker.Plaintext_Alphabet = this.ptAlphabet;
            this.genAttacker.Language_Frequencies = this.langFreq;
            this.genAttacker.PluginProgressCallback = this.ProgressChanged;
            this.genAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            

            // Start attack
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop == true)
                {
                    return;
                }
            }
            
            this.genAttacker.Analyze();
            
            
            watch.Stop();

            string curTime = String.Format("{0:00}:{1:00}:{2:00}", watch.Elapsed.Hours ,watch.Elapsed.Minutes, watch.Elapsed.Seconds);
            GuiLogMessage(Resources.gen_attack_finished + curTime, NotificationLevel.Info);
            GuiLogMessage(Resources.gen_attack_testedkeys + genAttacker.Currun_Keys.ToString("#,##0"), NotificationLevel.Info);
        }

        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            // Add key if key does not already exists
            if (!this.keyCandidates.Contains(keyCan))
            {
                this.keyCandidates.Add(keyCan);
                this.keyCandidates.Sort(new KeyCandidateComparer());
                //this.masPresentation.RefreshGUI();

                if (this.keyCandidates.Count > 20)
                {
                    this.keyCandidates.RemoveAt(this.keyCandidates.Count-1);
                }

                // Display output
                this.plaintext = this.keyCandidates[0].Plaintext;
                OnPropertyChanged("Plaintext");

                this.plaintext_alphabet_output = CreateAlphabetOutput(this.keyCandidates[0].Key, this.ctAlphabet);
                OnPropertyChanged("Plaintext_Alphabet_Output");

                ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((AssignmentPresentation)Presentation).entries.Clear();

                    for (int i = 0; i < this.keyCandidates.Count; i++)
                    {
                        KeyCandidate keyCandidate = this.keyCandidates[i];

                        ResultEntry entry = new ResultEntry();
                        entry.Ranking = i.ToString();
                        entry.Text = keyCandidate.Plaintext;
                        entry.Key = keyCandidate.Key_string;
                        double f = Math.Log10(Math.Abs(keyCandidate.Fitness));
                        entry.Value = string.Format("{0:0.00000} ",f);
                        ((AssignmentPresentation)Presentation).entries.Add(entry);

                    }
                }, null);
            }  
        }  
        

        private void UpdateDisplayStart()
        {
             ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
                 this.startTime = DateTime.Now;
                 ((AssignmentPresentation)Presentation).startTime.Content = "" + startTime;
                 ((AssignmentPresentation)Presentation).endTime.Content = "";
                 ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
              }, null);
        }

        private void UpdateDisplayEnd()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.endTime = DateTime.Now;
                TimeSpan elapsedtime = this.endTime.Subtract(this.startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + this.endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;

            }, null);
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
                return null;
            }

            using (CStreamReader reader = stream.CreateReader())
            {
                res = Encoding.Default.GetString(reader.ReadFully());
               
               
                if (res.Length == 0)
                {
                    return null;
                }
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

            for (int i = 4; i > 2; i--)
            {
                name = lang + "-" + i.ToString() + "gram-" + casesen + ".lm";
                if (File.Exists(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, name)))
                {
                    return name;
                }
            }
            return null;
        }

        private void UpdateOutput(String key_string, String plaintext_string)
        {
            // Plaintext
            this.plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");

            // Alphabet
            this.plaintext_alphabet_output = key_string;
            OnPropertyChanged("Plaintext_Alphabet_Output");
        }

        private String CreateAlphabetOutput(int[] key, Alphabet ciphertext_alphabet)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < key.Length; i++)
            {
                sb.Append(ciphertext_alphabet.GetLetterFromPosition(key[i]));
            }

            return sb.ToString();
        }

        #endregion
    }

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

    }

    public class StopFlag
    {
        public Boolean Stop { get; set; }
    }
}
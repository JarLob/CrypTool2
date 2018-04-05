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
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using Cryptool.AnalysisMonoalphabeticSubstitution.Properties;
using Cryptool.PluginBase.Utils;
using ManagedCuda;

namespace Cryptool.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String key_string, String plaintext_string);
    delegate double CalculateFitness(Text plaintext);
    delegate void UpdateKeyDisplay(KeyCandidate keyCan);
    delegate double CalculateCostDelegate(int[] plaintext);

    [Author("Andreas Grüner, Cordian Henkel", "Andreas.Gruener@web.de, Cordian.Henkel@yahoo.de", "Humboldt University Berlin, Universität Kassel", "http://www.hu-berlin.de, https://www.ais.uni-kassel.de")]
    [PluginInfo("Cryptool.AnalysisMonoalphabeticSubstitution.Properties.Resources", "PluginCaption", "PluginTooltip", "AnalysisMonoalphabeticSubstitution/Documentation/doc.xml", "AnalysisMonoalphabeticSubstitution/icon.png")]
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
        private List<KeyCandidate> keyCandidates;
        private string ciphertextalphabet;
        private string plaintextalphabet;
        private string keyoutput;
        private QuadGrams quadgrams;

        // Input property variables
        private String ciphertext;
      
        // Output property variables
        private String plaintext;
        private String plaintextalphabetoutput;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();
        private DateTime startTime;
        private DateTime endTime;
        private long totalKeys;
        private double keysPerSecond;

        // Attackers
        private DictionaryAttacker dicAttacker;
        private GeneticAttacker genAttacker;
        private HillclimbingAttacker hillAttacker;

        CalculateCostDelegate CalculateCost;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public String Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
        }

        [PropertyInfo(Direction.InputData, "CiphertextAlphabetCaption", "CiphertextAlphabetTooltip", false)]
        public String CiphertextAlphabet
        {
            get { return this.ciphertextalphabet; }
            set { this.ciphertextalphabet = value; }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextAlphabetOutputCaption", "PlaintextAlphabetOutputTooltip", true)]
        public String PlaintextAlphabetOutput
        {
            get { return this.plaintextalphabetoutput; }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutputCaption", "KeyOutputTooltip", true)]
        public String KeyOutput
        {
            get { return this.keyoutput; }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }
        
        public UserControl Presentation
        {
            get { return this.masPresentation; }
        }
        
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Ciphertext = null;
            CiphertextAlphabet = null;
        }

        public void Execute()
        {
            this.genAttacker = new GeneticAttacker();
            this.dicAttacker = new DictionaryAttacker();
            this.hillAttacker = new HillclimbingAttacker();
            
            Boolean inputOK = true;

            // Clear presentation
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).entries.Clear();
            }, null);

            // Prepare the cryptanalysis of the ciphertext
            
            ciphertext = ciphertext.ToLower();   // Todo: add case handling

            if (settings.ChooseAlgorithm < 2)
            {
                // Set alphabets
                string lang = LanguageStatistics.LanguageCode(settings.Language);
                quadgrams = new QuadGrams(lang, settings.UseSpaces);
                //pentagrams = new PentaGrams(lang, settings.UseSpaces);
                CalculateCost = quadgrams.CalculateCost;

                plaintextalphabet = quadgrams.Alphabet;
                ciphertextalphabet = String.IsNullOrEmpty(CiphertextAlphabet)
                    ? new string(Ciphertext.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "")
                    : new string(CiphertextAlphabet.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "");
                //            ciphertextalphabet = String.IsNullOrEmpty(CiphertextAlphabet) ? plaintextalphabet : new string(Ciphertext.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "");

                this.ptAlphabet = new Alphabet(plaintextalphabet);
                this.ctAlphabet = new Alphabet(ciphertextalphabet);
            }
            else
            {
                // Dictionary

                try
                {
                    this.langDic = null;

                    switch (settings.Language2)
                    {
                        case 1: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜß"; break;  // German
                        case 2: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÑ"; break;  // spanish
                        case 3: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Latin 
                        case 4: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // French 
                        case 5: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Hungarian 
                        case 6: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ"; break; // Swedish 
                        case 7: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Italian 
                        case 8: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Dutch 
                        case 9: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Portuguese 
                        case 10: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break; // Czech 
                        case 11: plaintextalphabet = "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ"; break; // Greek
                        case 12: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break;  // Latin
                        default: plaintextalphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; break;  // English
                    }

                    switch (settings.Language2)
                    {
                        case 1: this.langDic = new Dictionary("de-small.dic", plaintextalphabet.Length); break;
                        case 2: this.langDic = new Dictionary("es-small.dic", plaintextalphabet.Length); break;
                        case 3: this.langDic = new Dictionary("la-small.dic", plaintextalphabet.Length); break;
                        case 4: this.langDic = new Dictionary("fr-small.dic", plaintextalphabet.Length); break;
                        case 5: this.langDic = new Dictionary("hu-small.dic", plaintextalphabet.Length); break;
                        case 6: this.langDic = new Dictionary("sv-small.dic", plaintextalphabet.Length); break;
                        case 7: this.langDic = new Dictionary("it-small.dic", plaintextalphabet.Length); break;
                        case 8: this.langDic = new Dictionary("nl-small.dic", plaintextalphabet.Length); break;
                        case 9: this.langDic = new Dictionary("pt-small.dic", plaintextalphabet.Length); break;
                        case 10: this.langDic = new Dictionary("cs-small.dic", plaintextalphabet.Length); break;
                        case 11: this.langDic = new Dictionary("el-small.dic", plaintextalphabet.Length); break;
                        default: this.langDic = new Dictionary("en-small.dic", plaintextalphabet.Length); break;
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + ": " + ex.Message, NotificationLevel.Error);
                }
                // Dictionary correct?
                if (this.langDic == null)
                {
                    GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
                }

                String helper = IdentifyNGramFile(settings.Language2);
                if (helper == null)
                {
                    GuiLogMessage(Resources.no_ngram_file, NotificationLevel.Error);
                    return;
                }

                //ciphertextalphabet = String.IsNullOrEmpty(CiphertextAlphabet)
                //    ? Regex.Replace(new string(Ciphertext.ToLower().Distinct().OrderBy(c => c).ToArray()), @"\s", "")
                //    : Regex.Replace(new string(CiphertextAlphabet.ToLower().Distinct().OrderBy(c => c).ToArray()), @"\s", "");
                ciphertext = ciphertext.ToLower();
                plaintextalphabet = plaintextalphabet.ToLower();
                ciphertextalphabet = plaintextalphabet;
                
                this.ptAlphabet = new Alphabet(plaintextalphabet);
                this.ctAlphabet = new Alphabet(ciphertextalphabet);
                this.langFreq = new Frequencies(this.ptAlphabet);
                this.langFreq.ReadProbabilitiesFromNGramFile(helper);
            }

            // Plaintext Alphabet
            this.plaintextalphabetoutput = plaintextalphabet;
            OnPropertyChanged("PlaintextAlphabetOutput");

            if (ciphertext != null)
            {
                this.cText = new Text(ciphertext.ToLower(), this.ctAlphabet, settings.TreatmentInvalidChars);
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
            
            // Language frequencies
            //if (this.langFreq == null)
            //{
            //    GuiLogMessage(Resources.no_lang_freq, NotificationLevel.Error);
            //    //inputOK = false;
            //}

            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length > this.ptAlphabet.Length)
            {
                GuiLogMessage(String.Format(Resources.error_alphabet_length, ciphertextalphabet, ciphertextalphabet.Length, plaintextalphabet, plaintextalphabet.Length), NotificationLevel.Error);
                inputOK = false;
            }
            
            // If input incorrect return otherwise execute analysis
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop)
                    return;
            }
            
            if (!inputOK)
            {
                inputOK = true;
                return;
            }

            this.UpdateDisplayStart();

            //this.masPresentation.DisableGUI();
            this.masPresentation.UpdateOutputFromUserChoice = this.UpdateOutput;
            this.keyCandidates = new List<KeyCandidate>();

            /* Algorithm:
             * 0 = Hillclimbing CPU
             * 1 = Hillclimbing GPU
             * 2 = Genetic & Dictionary */
            if (settings.ChooseAlgorithm == 0)
            {
                AnalyzeHillclimbing(false);
                totalKeys = hillAttacker.TotalKeys;
            }
            else if (settings.ChooseAlgorithm == 1)
            {
                if (!isCudaAvailable())
                {
                    GuiLogMessage(Resources.no_cuda, NotificationLevel.Error);
                    return;
                }

                AnalyzeHillclimbing(true);
                totalKeys = hillAttacker.TotalKeys;
            }
            else if (settings.ChooseAlgorithm == 2)
            {
                if (this.langDic != null)
                    AnalyzeDictionary();
                AnalyzeGenetic();
            }

            this.UpdateDisplayEnd();
            
            //set final plugin progress to 100%:
            OnPluginProgressChanged(this, new PluginProgressEventArgs(1.0, 1.0));
        }

        public void PostExecution()
        {
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = false;
            }
            this.ciphertextalphabet = null;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (this.dicAttacker != null) this.dicAttacker.StopFlag = true;
            if (this.genAttacker != null) this.genAttacker.StopFlag = true;
            if (this.hillAttacker != null) this.hillAttacker.StopFlag = true;
            if (this.langDic != null) this.langDic.StopFlag = true;
            lock (this.stopFlag)
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

        private bool isCudaAvailable()
        {
            //Check if Cuda is Available (Device & Driver).
            bool cudaAvailable = true;

            try
            {
                CudaContext chtxt = new CudaContext();

                if (CudaContext.GetDeviceCount() != 0 && CudaContext.GetDriverVersion() != null)
                {
                    Console.WriteLine("" + CudaContext.GetDriverVersion());
                    cudaAvailable = true;
                }
                else
                {
                    cudaAvailable = false;
                }
                chtxt.Dispose();
            }
            catch (Exception ex)
            {
                //CUDA does not work; Exception is also thrown when there is no CUDA installed
                cudaAvailable = false;
            }

            return cudaAvailable;
        }

        public void AnalyzeHillclimbing(bool GPU = false)
        {
            // Initialize analyzer
            //this.hillAttacker.Ciphertext = this.cText.ToString(this.ctAlphabet);
            this.hillAttacker.Ciphertext = ciphertext;
            this.hillAttacker.Restarts = settings.Restarts;
            this.hillAttacker.PlaintextAlphabet = plaintextalphabet;
            this.hillAttacker.CiphertextAlphabet = ciphertextalphabet;
            this.hillAttacker.CalculateCost = CalculateCost;
            this.hillAttacker.quadgrams = quadgrams;

            this.hillAttacker.PluginProgressCallback = this.ProgressChanged;
            this.hillAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;

            // Start attack
            if(GPU)
                this.hillAttacker.ExecuteOnGPU();
            else
                this.hillAttacker.ExecuteOnCPU();
        }

        private void AnalyzeDictionary()
        {
            ////////////////////// Create keys with dictionary attacker
            // Initialize dictionary attacker
            
            //this.dicAttacker = new DictionaryAttacker();
            this.dicAttacker.ciphertext = this.cText;
            this.dicAttacker.languageDictionary = this.langDic;
            this.dicAttacker.frequencies = this.langFreq;
            this.dicAttacker.ciphertext_alphabet = this.ctAlphabet;
            this.dicAttacker.plaintext_alphabet = this.ptAlphabet;

            this.dicAttacker.PluginProgressCallback = this.ProgressChanged;
            this.dicAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            
            // Prepare text

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
                    this.dicAttacker.SolveRandomized();
            }
        }

        private void AnalyzeGenetic()
        {
            ////////////////// Create keys with genetic attacker

            // Initialize analyzer
            this.genAttacker.Ciphertext = this.cText;
            this.genAttacker.Ciphertext_Alphabet = this.ctAlphabet;
            this.genAttacker.Plaintext_Alphabet = this.ptAlphabet;
            this.genAttacker.Language_Frequencies = this.langFreq;

            this.genAttacker.PluginProgressCallback = this.ProgressChanged;
            this.genAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            
            // Start attack
            
            this.genAttacker.Analyze();
        }

        private string IdentifyNGramFile(int alpha_nr)
        {
            string name = "";
            string lang = "";

            switch(alpha_nr)
            {
                case 1: lang = "de"; break;
                case 2: lang = "es"; break;
                case 3: lang = "la"; break;
                case 4: lang = "fr"; break;
                case 5: lang = "hu"; break;
                case 6: lang = "sv"; break;
                case 7: lang = "it"; break;
                case 8: lang = "nl"; break;
                case 9: lang = "pt"; break;
                case 10: lang = "cs"; break;
                case 11: lang = "el"; break;
                default: lang = "en"; break;
            }

            // It is always looked for a 4-gram file at first. If the 4-gram file is not found the 3-gram file is choosen
            for (int i = 4; i > 2; i--)
            {
                name = lang + "-" + i.ToString() + "gram-nocs.bin";
                if (File.Exists(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, name)))
                    return name;
            }

            return null;
        }

        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            try
            {
                bool update = false;

                // Add key if key does not already exist
                if (!this.keyCandidates.Contains(keyCan))
                {
                    this.keyCandidates.Add(keyCan);
                    this.keyCandidates.Sort(new KeyCandidateComparer());

                    if (this.keyCandidates.Count > 20)
                        this.keyCandidates.RemoveAt(this.keyCandidates.Count - 1);

                    update = true;
                }
                else
                {
                    int index = this.keyCandidates.IndexOf(keyCan);
                    KeyCandidate keyCanAlreadyInList = this.keyCandidates[index];

                    if (keyCan.DicAttack)
                    {
                        if (!keyCanAlreadyInList.DicAttack)
                        {
                            keyCanAlreadyInList.DicAttack = true;
                            update = true;
                        }
                    }
                    if (keyCan.GenAttack)
                    {
                        if (!keyCanAlreadyInList.GenAttack)
                        {
                            keyCanAlreadyInList.GenAttack = true;
                            update = true;
                        }
                    }
                    if (keyCan.HillAttack)
                    {
                        if (!keyCanAlreadyInList.HillAttack)
                        {
                            keyCanAlreadyInList.HillAttack = true;
                            update = true;
                        }
                    }
                }

                // Display output
                if (update)
                {
                    //this.plaintext = this.keyCandidates[0].Plaintext;
                    //OnPropertyChanged("Plaintext");

                    //this.plaintextalphabetoutput = CreateKeyOutput(this.keyCandidates[0].Key, this.ptAlphabet, this.ctAlphabet);
                    //OnPropertyChanged("PlaintextAlphabetOutput");
                    UpdateOutput(this.keyCandidates[0].Key_string, this.keyCandidates[0].Plaintext);

                    ((AssignmentPresentation) Presentation).Dispatcher.Invoke(DispatcherPriority.Normal,
                        (SendOrPostCallback) delegate
                        {
                            try
                            {
                                ((AssignmentPresentation) Presentation).entries.Clear();

                                for (int i = 0; i < this.keyCandidates.Count; i++)
                                {
                                    KeyCandidate keyCandidate = this.keyCandidates[i];

                                    ResultEntry entry = new ResultEntry();
                                    entry.Ranking = (i+1).ToString();
                                    entry.Text = keyCandidate.Plaintext;
                                    entry.Key = keyCandidate.Key_string;

                                    if (keyCandidate.GenAttack && !keyCandidate.DicAttack)
                                    {
                                        entry.Attack = Resources.GenAttackDisplay;
                                    }
                                    else if (keyCandidate.DicAttack && !keyCandidate.GenAttack)
                                    {
                                        entry.Attack = Resources.DicAttackDisplay;
                                    }
                                    else if (keyCandidate.GenAttack && keyCandidate.DicAttack)
                                    {
                                        entry.Attack = Resources.GenAttackDisplay + ", " + Resources.DicAttackDisplay;
                                    }
                                    else if (keyCandidate.HillAttack)
                                    {
                                        entry.Attack = Resources.HillAttackDisplay;
                                    }

                                    double f = keyCandidate.Fitness;
                                    //double f = Math.Log10(Math.Abs(keyCandidate.Fitness));
                                    entry.Value = string.Format("{0:0.00000} ", f);
                                    ((AssignmentPresentation) Presentation).entries.Add(entry);
                                }
                            }
                            catch (Exception ex)
                            {
                                GuiLogMessage("Exception during UpdateKeyDisplay Presentation.Dispatcher: " + ex.Message, NotificationLevel.Error);
                            }
                        }, null);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during UpdateKeyDisplay: " +ex.Message,NotificationLevel.Error);
            }
        }  
        
        private void UpdateDisplayStart()
        {
             ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
                 startTime = DateTime.Now;
                 ((AssignmentPresentation)Presentation).startTime.Content = "" + startTime;
                 ((AssignmentPresentation)Presentation).endTime.Content = "";
                 ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
                 ((AssignmentPresentation)Presentation).totalKeys.Content = "";
                 ((AssignmentPresentation)Presentation).keysPerSecond.Content = "";
             }, null);
        }

        private void UpdateDisplayEnd()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                endTime = DateTime.Now;
                TimeSpan elapsedtime = endTime.Subtract(startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                
                double totalSeconds = elapsedtime.TotalSeconds;
                if (totalSeconds == 0) totalSeconds = 0.001;
                keysPerSecond = totalKeys / totalSeconds;

                ((AssignmentPresentation)Presentation).endTime.Content = "" + endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((AssignmentPresentation)Presentation).totalKeys.Content = String.Format(culture, "{0:##,#}", totalKeys);
                ((AssignmentPresentation)Presentation).keysPerSecond.Content = String.Format(culture, "{0:##,#}", (ulong)keysPerSecond);
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
        
        //private string detAlphabet(int n)
        //{
        //    var langs = "en de es fr it hu ru cs".Split(new char[] { ' ' });
        //    string lang = langs[n % langs.Length];
        //    quadgrams = new QuadGrams(lang, settings.UseSpaces);
        //    return quadgrams.Alphabet.ToLower();

        //    //string alphabet;
        //    //var quadgrams = LanguageStatistics.Load4Grams(lang, out alphabet, settings.UseSpaces);
        //    //this.langFreq = new Frequencies(new Alphabet(alphabet, 1, 0));
        //    //this.langFreq.prob4gram = quadgrams;
        //    //this.langFreq.ngram = 4;
        //    //this.langFreq.CalculateFitnessOfKey = this.langFreq.CalculateFitness4gram2;

        //    //return alphabet.ToLower();
        //}

        private void UpdateOutput(string key_string, String plaintext_string)
        {
            this.plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");
            
            this.keyoutput = key_string;
            OnPropertyChanged("KeyOutput");
        }

        private String CreateKeyOutput(int[] key, Alphabet plaintextalphabet, Alphabet ciphertextalphabet)
        {
            char[] k = new char[plaintextalphabet.Length];

            for (int i = 0; i < k.Length; i++) k[i] = ' ';

            for (int i = 0; i < ciphertextalphabet.Length; i++)
                k[key[i]] = ciphertextalphabet.GetLetterFromPosition(i)[0];

            return new string(k);
        }

        private String CreateAlphabetOutput2(int[] key, Alphabet ciphertext_alphabet)
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
        public string Attack { get; set; }
    }

    public class StopFlag
    {
        public Boolean Stop { get; set; }
    }
}
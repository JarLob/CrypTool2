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

/*You want to do a lot as the RSAKeyGeneratorSettings*/
using System;
using System.Windows;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.NFSFactorizer
{
    // HOWTO: rename class (click name, press F2)
    public class NFSFactorizerSettings : ISettings
    {
        private const int BRUTEFORCEMIN = 100;
        private const int BRUTEFORCEMAX = (1 << 30);
        private const int NFSTMIN = 100;
        private const int NFSTMAX = (1 << 30);
        private const int SIQSTMIN = 100;
        private const int SIQSTMAX = (1 << 30);

        public static int trialdiv = 7;
        public int maxit;
        private int nfsT;
        private int siqsT;
        private int bruteforcelimit = 10000;
        private int threads = 0;
        private int algs = 0;
        private string options;
        private string bits;
        private string trialExpl = "Trial division will go through all primes until limit. Once it reaches that point, it will stop factorizing. Maybe output aswell the missing factor? Doesn't sound bad. ";
        private bool trialButton = false;
        private bool noecm = false;
        private bool doc = false;
        private int extra = 0;
        private bool action = false; // 0 = factorize, 1 = find smallest factor

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        [TaskPane("Algorithms", "Select the algorithm to use", "Algorithms", 0, false, ControlType.ComboBox, new string[] { "Quadratic Sieve", "Smallmpqs (optimized for small inputs)", "Number Field Sieve", "Shank's method", "p minus 1", "p plus 1", "Pollard's rho", "Trial", "ECM", "Fermat's method", "Special Number Field Sieve", "General Factorization" })]
        public int Algs
        {
            get { return this.algs; }
            set
            {
                if (((int)value) != algs)
                {
                    this.algs = (int)value;

                    UpdateTaskPaneVisibility();

                    FirePropertyChangedEvent("Algs");
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;
            switch (algs)
            {
                case 0: //Quadratic Sieve
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 1: //Smallmpqs
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 2: //NFS
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 3: //Shank's method
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 4: // p minus 1
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 5: // p plus 1
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 6: //Pollard Rho
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 7:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 8: //ECM
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 9: //Fermat
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Visible)));
                    break;
                case 10: //SNFS
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
                case 11: //General Factorization
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BruteForceLimit", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NfsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("SiqsT", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TrialExpl", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoECM", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxIt", Visibility.Collapsed)));
                    break;
            }

            var tba = new TaskPaneAttribteContainer("Options", TrialButton ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));

            tba = new TaskPaneAttribteContainer("Details", TrialButton ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tba));
            
        }
        [TaskPane("Details", "Press to Open Doc.txt", "General Settings", 0, false, ControlType.Button)]
        public void Details()
        {
            FirePropertyChangedEvent("Details");
        }

        [TaskPane("Generate a hard to factor number", "Check to enter the number of bits", "Generate Number", 1, false, ControlType.Button)]
        public void NmbrGen()
        {
            FirePropertyChangedEvent("NmbrGen");
        }
        [TaskPane("Number of bits", "Select the number of bits you want your number to be", "Generate Number", 2, false, ControlType.TextBox)]
        public string Bits
        {
            get { return this.bits; }
            set
            {
                this.bits = value;
                FirePropertyChangedEvent("Bits Selected");
            }
        }

        [TaskPane("Options:", "Add the options for the running algorithm", "General Settings", 7, false, ControlType.TextBox)]
        public string Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
                FirePropertyChangedEvent("Extra options chosen");
            }
        }
        [TaskPane("Threads", "Select the number of threads you want to use", "General Settings", 3, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4" })]
        public int Threads
        {
            get { return this.threads; }
            set
            {
                this.threads = (int)value;
                FirePropertyChangedEvent("Threads");
            }
        }
        
        [TaskPane("Personalize settings", "Tick in case you want to add a flag", "General Settings", 8, false, ControlType.CheckBox)]
        public bool TrialButton
        {
            get { return trialButton; }
            set
            {
                if (value != trialButton)
                {
                    trialButton = value;
                    FirePropertyChangedEvent("TrialButton");
                    UpdateTaskPaneVisibility();
                }
            }
        }
        [TaskPane("Don't run ECM", "Factorize withouth using EC Method", "Algorithm Properties", 3, false, ControlType.CheckBox)]
        public bool NoECM
        {
            get { return noecm; }
            set
            {
                if (value != noecm)
                {
                    noecm = value;
                    FirePropertyChangedEvent("NoECM");
                }
            }
        }
        [TaskPane("Idle Priority", "Running at idle priority will slow down other programs", "General Settings", 9, false, ControlType.CheckBox)]
        public bool Action
        {
            get { return action; }
            set
            {
                if (value != action)
                {
                    action = value;
                    FirePropertyChangedEvent("IdlePriority");
                }
            }
        }
        [TaskPane("", "Trial factorization will go through all primes until the limit is reached.", "Algorithm Properties", 3, false, ControlType.TextBoxReadOnly)]
        public string TrialExpl
        {
            get { return trialExpl; }
            set
            {
                if (value != trialExpl)
                {
                    trialExpl = value;
                    FirePropertyChangedEvent("TrialExpl");
                }
            }
        }
        [TaskPane("Brute Force Limit", "Select the limit of primes for Trial division", "Algorithm Properties", 9, false, ControlType.NumericUpDown, ValidationType.RangeInteger, BRUTEFORCEMIN, BRUTEFORCEMAX)]
        public int BruteForceLimit
        {
            get { return bruteforcelimit; }
            set
            {
                if (value != bruteforcelimit)
                {
                    bruteforcelimit = Math.Max(BRUTEFORCEMIN, value);
                    bruteforcelimit = Math.Min(BRUTEFORCEMAX, value);
                    FirePropertyChangedEvent("BruteForceLimit");
                }
            }
        }
        [TaskPane("Limit of iterations","Determine the bound of the iterations.", "Algorithm Properties", 1, false, ControlType.TextBox)]
        public int MaxIt
        {
            get { return maxit; }
            set
            {
                if (value != maxit)
                {
                    maxit = value;
                }
            }
        }
        [TaskPane("Max time", "Give an upperbound for the time spent factorizing", "Algorithm Properties", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, NFSTMIN, NFSTMAX)]
        public int NfsT
        {
            get { return nfsT; }
            set
            {
                if (value != nfsT)
                {
                    nfsT = Math.Max(NFSTMIN, value);
                    nfsT = Math.Min(NFSTMAX, value);
                    FirePropertyChangedEvent("NfsT");
                }
            }
        }
        [TaskPane("Max time", "Give an upperbound for the time spent factorizing", "Algorithm Properties", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, SIQSTMIN, SIQSTMAX)]
        public int SiqsT
        {
            get { return siqsT; }
            set
            {
                if (value != siqsT)
                {
                    siqsT = Math.Max(NFSTMIN, value);
                    siqsT = Math.Min(NFSTMAX, value);
                    FirePropertyChangedEvent("SiqsT");
                }
            }
        }
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

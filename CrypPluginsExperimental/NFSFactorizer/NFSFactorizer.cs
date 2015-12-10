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
using System.IO.Compression;
using Ionic.Zip;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using Cryptool.PluginBase.Properties;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;
using System.Reflection;



namespace Cryptool.Plugins.NFSFactorizer
{
    [Author("Queremendi", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("NFS Factorizer", "Subtract one number from another", "NFSFactorizer/userdoc.xml", new[] { "NFSFactorizer/images.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]

    public class NFSFactorizer : ICrypComponent
    {
        private readonly string _directoryName;
        private BigInteger inputNumber;
        private BigInteger[] FactorArray;
        private string CoFactArray;
        private BigInteger InputNumber1 = 1;

        private NFSFactorizerSettings settings = new NFSFactorizerSettings();

        #region INotifyPropertyChanged Members
        public NFSFactorizer()
        {
            this.settings = new NFSFactorizerSettings();
            settings.PropertyChanged += settings_PropertyChanged;
            Presentation = new NFSFactorizerPresentation();
            _directoryName = DirectoryHelper.DirectoryLocalTemp;
        }


        private NFSFactorizerPresentation nfsFactQuickWatchPresentation
        {
            get { return Presentation as NFSFactorizerPresentation; }
        }

        private void ExtractYAFU()
        {
            var mainDir = "..\\..\\CrypPluginsExperimental\\NFSFactorizer";
            if (Directory.Exists(mainDir + "\\ggnfs-bin"))
            {
                Directory.Delete(mainDir + "\\ggnfs-bin", true);
            }
            if (Directory.Exists(mainDir + "\\yafu-1.34"))
            {
                Directory.Delete(mainDir + "\\yafu-1.34", true);
            }
            string zipPath = mainDir + "\\yafu-1.34.zip";
            string zipPath1 = mainDir + "\\ggnfs-bin.zip";
            ZipFile zipFile = new ZipFile(zipPath);
            ZipFile zipFile1 = new ZipFile(zipPath1);
            zipFile.ExtractAll(mainDir);
            zipFile1.ExtractAll(mainDir);
        }


        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Details")
            {
                Process.Start("notepad.exe", "docfile.txt");
            }
            if (e.PropertyName == "NmbrGen")
            {
                SortOutputRedirection pr = new SortOutputRedirection();
                pr.cmndLine = String.Format("/c ..\\..\\CrypPluginsExperimental\\NFSFactorizer\\yafu-1.34\\yafu-x64.exe \"rsa(150)\" ");

                pr.ConvertToMPEG();
                while (!pr.redirectInfo.Contains("ans = ")) { }
                string genNumb = pr.redirectInfo;
                InputNumber1 = BigInteger.Parse(genNumb.Between("ans = ", "\n"));

                nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    nfsFactQuickWatchPresentation.ComplementaryInfo.Content = "The generated number will appear when you start factorization.";
                }
                , null);
            }
            else
            {
                nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    nfsFactQuickWatchPresentation.ComplementaryInfo.Content = "Input number";
                }
                , null);
            }
        }



        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        #region Data Properties

        [PropertyInfo(Direction.InputData, "Input Number", "Enter the Number you want to Factorize")]
        public BigInteger InputNumber
        {
            get
            {
                return inputNumber;
            }
            set
            {
                if (InputNumber1 == 1)
                {
                    this.inputNumber = value;
                    FirePropertyChangedEvent("InputNumber");
                }
                else
                {
                    this.inputNumber = InputNumber1;
                    FirePropertyChangedEvent("InputNumber");
                }
                

            }
        }

        [PropertyInfo(Direction.OutputData, "Output Factors", "Oh Geoff", true)]
        public BigInteger[] Factors
        {
            get { return FactorArray; }
            set
            {
                FactorArray = value;
                FirePropertyChangedEvent("Factors");
            }
        }

        [PropertyInfo(Direction.OutputData, "Co-factor", "Non factored remainder", false)]
        public string CoFact
        {
            get { return CoFactArray; }
            set
            {
                CoFactArray = value;
                FirePropertyChangedEvent("CoFact");
            }
        }
        
        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            ExtractYAFU();
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            if (InputNumber <= 0)
            {
                FireOnGuiLogNotificationOccuredEventError("Input must be a natural number > 0");
                return;
            }
            string method = "";
            string Choice = "";
            string option = "";
            string idle = "";
            string SecondArg = "";
            switch (settings.Algs)
            {
                case 0:
                    method = "siqs";
                    Choice = "SIQS";
                    break;
                case 1:
                    method = "smallmpqs";
                    Choice = "Multy Polynomial QS";
                    break;
                case 2:
                    method = "nfs";
                    Choice = "Number Field Sieve";
                    break;
                case 3:
                    method = "squfof";
                    Choice = "SqufOf";
                    break;
                case 4:
                    method = "pm1";
                    Choice = "Pollards p minus 1";
                    break;
                case 5:
                    method = "pp1";
                    Choice = "Williams p plus one";
                    break;
                case 6:
                    method = "rho";
                    Choice = "Pollards Rho method";
                    break;
                case 7:
                    method = "trial";
                    Choice = "Trial Division";
                    SecondArg = "," + settings.BruteForceLimit;
                    break;
                case 8:
                    method = "ecm";
                    Choice = "Elliptic curve method";
                    break;
                case 9:
                    method = "fermat";
                    Choice = "Fermats Factorization";
                    SecondArg = "," + settings.MaxIt;
                    break;
                case 10:
                    method = "snfs";
                    Choice = "Special number field Sieve";
                    break;
                case 11:
                    Choice = "Multy polynomial factorization";
                    method = "factor";
                    if (!settings.NoECM)
                        option = "-noecm -R";
                    else
                        option = "-R";
                    break;
            }

            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                nfsFactQuickWatchPresentation.NmbrToFactor.Content = InputNumber;
                nfsFactQuickWatchPresentation.SelMethod.Content = "You are using the following algorithm: " + Choice + ".";
                // nfsFactQuickWatchPresentation.SelMethod.Content = DirectoryHelper.DirectoryLocalTemp;
            }
            , null);

            if (settings.Action)
                idle = "-p";
            
            SortOutputRedirection pr = new SortOutputRedirection();
            pr.cmndLine = String.Format("/c ..\\..\\CrypPluginsExperimental\\NFSFactorizer\\yafu-1.34\\yafu-x64.exe \"{0}( {1}{2} )\" -threads {3} {4} {5}", method, InputNumber, SecondArg, settings.Threads + 1, option, idle);

            pr.ConvertToMPEG();
            
            while (!pr.redirectInfo.Contains("ans = ")) { }
            {
                if (pr.redirectInfo != null)
                {
                    nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        nfsFactQuickWatchPresentation.Details.Content += pr.redirectInfo;
                    }
                    , null);
                }

            }
            string tmp = pr.redirectInfo;
            string primality = "";

            string facts = tmp.Between("***factors found***", "ans = ");
            string GUIfactors = "";
            string cofact;
            string coFact = "1";
            string response = " ";
            List<BigInteger> Facts = new List<BigInteger>();
            if (facts.Contains("***co-factor***"))
            {
                facts = tmp.Between("***factors found***", "***co-factor***");
                cofact = tmp.Between("***co-factor***", "ans = ");
                foreach (var myString in cofact.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    coFact = myString.After("= ");
                if (BigInteger.Parse(coFact).IsProbablePrime())
                {
                    response = "co-Factor is prime, the number has been factored.";
                }
                else
                {
                    response = "co-Factor is not prime, please use another algorithm or augment the boundaries.";
                }
                CoFact = coFact + "\n" + response;
            }
            else
            {
                CoFact = "1";
            }
            foreach (var myString in facts.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                Facts.Add(BigInteger.Parse(myString.After("= ")));
                primality += BigInteger.Parse(myString.After("= ")).IsProbablePrime() + "\n";
            }
                
                
            Facts.Sort();
            Factors = Facts.ToArray();
            foreach (BigInteger l in Facts)
                GUIfactors = GUIfactors + l.ToString() + "\n";
            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
            nfsFactQuickWatchPresentation.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                nfsFactQuickWatchPresentation.factorInfo.Content = GUIfactors;
                nfsFactQuickWatchPresentation.primality.Content = primality;
                if (nfsFactQuickWatchPresentation.ShowDetailsButton.IsChecked==true)
                {
                    Process.Start("notepad.exe", "D:\\Documents\\CrypTool2\\trunk\\CrypBuild\\Debug\\factor.log");
                }
            }
            , null);
        }

        public void PostExecution()
        {
            Directory.Delete("..\\..\\CrypPluginsExperimental\\NFSFactorizer\\ggnfs-bin", true);
            Directory.Delete("..\\..\\CrypPluginsExperimental\\NFSFactorizer\\yafu-1.34", true); 
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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
        private void FireOnGuiLogNotificationOccuredEvent(string message, NotificationLevel lvl)
        {
            if (OnGuiLogNotificationOccured != null) OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, lvl));
        }
        private void FireOnGuiLogNotificationOccuredEventError(string message)
        {
            FireOnGuiLogNotificationOccuredEvent(message, NotificationLevel.Error);
        }

        #endregion
    }
    class SortOutputRedirection
    {
        public string cmndLine = "";
        public string redirectInfo = "";

        public void ConvertToMPEG()
        {
            //NFSFactorizer n = new NFSFactorizer();
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = cmndLine;

            cmd.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);

            cmd.Start();
            cmd.BeginOutputReadLine();
        }


        public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                redirectInfo += e.Data.ToString() + "\n";
                
            }
        }
    }

}

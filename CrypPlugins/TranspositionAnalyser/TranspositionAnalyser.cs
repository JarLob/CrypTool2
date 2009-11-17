﻿using System;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;


namespace TranspositionAnalyser
{


    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "Transposition Analyser", "Bruteforces the columnar transposition.", null, "TranspositionAnalyser/Images/icon.png")]
    public class TranspositionAnalyser : IAnalysisMisc
    {
        TranspositionAnalyserSettings settings;

        /// <summary>
        /// Constructor
        /// </summary>
        public TranspositionAnalyser()
        {
            settings = new TranspositionAnalyserSettings();
        }

        private IControlEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                value.OnStatusChanged += onStatusChanged;
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }

        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "Cost Master", "Used for cost calculation", "", DisplayLevel.Beginner)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
            }
        }

        private byte[] output;
        [PropertyInfo(Direction.OutputData, "Output", "output", "", DisplayLevel.Beginner)]
        public byte[] Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
        }

        #region IPlugin Member

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {

        }

        public void Execute()
        {

        }

        public void PostExecution()
        {

        }

        public void Pause()
        {

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

        private void onStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
            {
                this.process((IControlEncryption)sender);
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void process(IControlEncryption sender)
        {
            switch (settings.Action)
            {
                case ((int)TranspositionAnalyserSettings.ActionMode.costfunction):
                    Output = costfunction_bruteforce(sender);
                    break;
                case ((int)TranspositionAnalyserSettings.ActionMode.crib):
                    Output = crib_bruteforce(sender);
                    break;
            }


        }

        private byte[] crib_bruteforce(IControlEncryption sender)
        {
            if (sender != null)
            {
                String empty_string = "";
                if ((!settings.Crib.Equals(empty_string)) && !(settings.Crib == null))
                {
                    PermutationGenerator per = new PermutationGenerator(2);
                    String crib = settings.Crib;
                    byte[] found = null;
                    int max = 0;
                    max = settings.MaxLength;
                    
                    if (max > 1)
                    {
                        long size = 0;
                        for (int i = 2; i <= max; i++)
                        {
                            size = size + per.getFactorial(i);
                        }
                        long sum = 0;
                        for (int i = 0; i <= max; i++)
                        {
                            per = new PermutationGenerator(i);
                            
                            while (per.hasMore())
                            {
                                int[] key = per.getNext();
                                byte[] b = new byte[key.Length];
                                for (int j = 0; j < b.Length; j++)
                                {
                                    b[j] = Convert.ToByte(key[j]);
                                }
                                byte[] dec = sender.Decrypt(b, b.Length);
                                if (dec != null)
                                {
                                    String tmp = System.Text.Encoding.ASCII.GetString(dec);
                                    if (tmp.Contains(crib))
                                    {
                                        found = dec;
                                    }
                                }
                                
                                sum++;
                                ProgressChanged(sum, size);
                            }
                        }
                        return found;
                    }
                    else
                    {
                        GuiLogMessage("Error: Check transposition bruteforce length.", NotificationLevel.Error);
                        return null;
                    }
                }
                else
                {
                    GuiLogMessage("Invalide Crib", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                GuiLogMessage("Sender Error", NotificationLevel.Error);
                return null;
            }
        }

        private byte[] costfunction_bruteforce(IControlEncryption sender)
        {
            if (sender != null && costMaster != null)
            {
                GuiLogMessage("start", NotificationLevel.Info);
                double best = Double.MinValue;
                if (costMaster.getRelationOperator() == RelationOperator.LessThen)
                {
                    best = Double.MaxValue;
                }
                String best_text = "";
                byte[] best_bytes = null;

                //Just for fractional-calculation:
                PermutationGenerator per = new PermutationGenerator(2);
                int max = 0;
                max = settings.MaxLength;
                GuiLogMessage("Max: " + max, NotificationLevel.Info);
                if (max > 1)
                {
                    long size = 0;
                    for (int i = 2; i <= max; i++)
                    {
                        size = size + per.getFactorial(i);
                    }
                    long sum = 0;
                    for (int i = 0; i <= max; i++)
                    {
                        per = new PermutationGenerator(i);

                        while (per.hasMore())
                        {
                            int[] key = per.getNext();
                            byte[] b = new byte[key.Length];
                            for (int j = 0; j < b.Length; j++)
                            {
                                b[j] = Convert.ToByte(key[j]);
                            }
                            byte[] dec = sender.Decrypt(b, b.Length);
                            if (dec != null)
                            {
                                double val = costMaster.calculateCost(dec);
                                if (costMaster.getRelationOperator() == RelationOperator.LessThen)
                                {
                                    if (val < best)
                                    {
                                        best = val;
                                        best_text = System.Text.Encoding.ASCII.GetString(dec);
                                        best_bytes = dec;
                                    }
                                }
                                else
                                {
                                    if (val > best)
                                    {
                                        best = val;
                                        best_text = System.Text.Encoding.ASCII.GetString(dec);
                                        best_bytes = dec;
                                    }
                                }
                            }
                            else
                            {
                            }
                            sum++;
                            ProgressChanged(sum, size);
                        }
                    }
                    GuiLogMessage("ENDE " + best + ": " + best_text, NotificationLevel.Info);
                    return best_bytes;
                }
                else
                {
                    GuiLogMessage("Error: Check transposition bruteforce length.", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                GuiLogMessage("Error: No costfunction applied.", NotificationLevel.Error);
                return null;
            }
        }



        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));

            }
        }
    }
}

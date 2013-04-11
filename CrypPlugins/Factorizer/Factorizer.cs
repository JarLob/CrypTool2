/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Factorizer
{
    [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Factorizer.Properties.Resources", "PluginCaption", "PluginTooltip", "Factorizer/DetailedDescription/doc.xml", "Factorizer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class Factorizer : ICrypComponent
    {
        #region IPlugin Members

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        private void FireOnPluginStatusChangedEvent()
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, new StatusEventArgs(0));
        }

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void FireOnGuiLogNotificationOccuredEvent(string message, NotificationLevel lvl)
        {
            if (OnGuiLogNotificationOccured != null) OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, lvl));
        }
        private void FireOnGuiLogNotificationOccuredEventError(string message)
        {
            FireOnGuiLogNotificationOccuredEvent(message, NotificationLevel.Error);
        }

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void FireOnPluginProgressChangedEvent(string message, NotificationLevel lvl)
        {
            if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(0, 0));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private FactorizerSettings m_Settings = new FactorizerSettings();
        public Cryptool.PluginBase.ISettings Settings
        {
            get { return m_Settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0,1);

            if (InputNumber <= 0)
            {
                FireOnGuiLogNotificationOccuredEventError("Input must be a natural number > 0");
                return;
            }

            if (m_Settings.Action == 0) // find all prime factors
            {
                Dictionary<BigInteger, long> factors;

                if (m_Settings.BruteForceLimitEnabled)
                {
                    bool isFactorized = false;
                    factors = InputNumber.Factorize(m_Settings.BruteForceLimit, out isFactorized);
                    if(!isFactorized)
                        FireOnGuiLogNotificationOccuredEvent(string.Format("Brute force limit of {0} reached, the last factor is still composite.", m_Settings.BruteForceLimit), NotificationLevel.Warning);
                }
                else
                {
                    factors = InputNumber.Factorize();
                }

                List<BigInteger> l = new List<BigInteger>();
                foreach (var f in factors.Keys)
                    for (int i = 0; i < factors[f]; i++)
                        l.Add(f);
                l.Sort();
                Factors = l.ToArray();
            }
            else  // find the smallest prime factor
            {
                if (InputNumber==1)
                {
                    // do nothing
                }
                else if (InputNumber.IsProbablePrime())
                {
                    Factor = InputNumber;
                    Remainder = 1;
                }
                else
                {
                    BigInteger sqrt = InputNumber.Sqrt();
                    BigInteger limit = sqrt;
                    if(m_Settings.BruteForceLimitEnabled && limit > m_Settings.BruteForceLimit) limit = m_Settings.BruteForceLimit;
                    int progressdisplay = 0;

                    for (BigInteger factor = 2; factor <= sqrt; factor = (factor + 1).NextProbablePrime())
                    {
                        if (m_Settings.BruteForceLimitEnabled && factor > m_Settings.BruteForceLimit)
                        {
                            FireOnGuiLogNotificationOccuredEvent(string.Format("Brute force limit of {0} reached, no factors found.", m_Settings.BruteForceLimit), NotificationLevel.Warning);
                            break;
                        }

                        if (InputNumber % factor == 0)
                        {
                            // Factor found, exit gracefully
                            Factor = factor;
                            Remainder = InputNumber / factor;
                            break;
                        }

                        if (++progressdisplay >= 100)
                        {
                            progressdisplay = 0;
                            ProgressChanged((int)((factor * 100) / limit), 100);
                        }
                    }
                }
            }

            ProgressChanged(0, 1);
        }

        public void PostExecution()
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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        private BigInteger m_inputNumber;

        [PropertyInfo(Direction.InputData, "InputNumberCaption", "InputNumberTooltip", true)]
        public BigInteger InputNumber
        {
            get
            {
                return m_inputNumber;
            }
            set
            {
                this.m_inputNumber = value;
                FirePropertyChangedEvent("InputNumber");
            }
        }

        private BigInteger[] m_FactorArray;

        [PropertyInfo(Direction.OutputData, "FactorsCaption", "FactorsTooltip", true)]
        public BigInteger[] Factors
        {
            get { return m_FactorArray; }
            set
            {
                m_FactorArray = value;
                FirePropertyChangedEvent("Factors");
            }
        }

        private BigInteger m_Factor;

        [PropertyInfo(Direction.OutputData, "FactorCaption", "FactorTooltip", true)]
        public BigInteger Factor
        {
            get { return m_Factor; }
            set
            {
                m_Factor = value;
                FirePropertyChangedEvent("Factor");
            }
        }

        private BigInteger m_Remainder;

        [PropertyInfo(Direction.OutputData, "RemainderCaption", "RemainderTooltip", true)]
        public BigInteger Remainder
        {
            get { return m_Remainder; }
            set
            {
                m_Remainder = value;
                FirePropertyChangedEvent("Remainder");
            }
        }

        #endregion
    }
}

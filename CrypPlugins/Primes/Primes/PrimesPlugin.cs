﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Primes.WpfVisualization;

namespace Primes
{
    [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Primes.Properties.Resources", "PluginCaption", "PluginTooltip", "Primes/DetailedDescription/doc.xml", "Primes/PrimesPlugin.png")]
    [FunctionList("FL_F_factorization", "FL_P_bruteforce")]
    [FunctionList("FL_F_factorization", "FL_P_qs")]
    [FunctionList("FL_F_primalitytest", "FL_P_eratosthenes")]
    [FunctionList("FL_F_primalitytest", "FL_P_millerrabin")]
    [FunctionList("FL_F_primalitytest", "FL_P_atkin")]
    [FunctionList("FL_F_primegeneration", "FL_P_primegeneration")]
    [FunctionList("FL_F_primedistribution", "FL_P_numberline")]
    [FunctionList("FL_F_primedistribution", "FL_P_numbergrid")]
    [FunctionList("FL_F_primedistribution", "FL_P_numberofprimes")]
    [FunctionList("FL_F_primedistribution", "FL_P_ulam")]
    [FunctionList("FL_F_numbertheory", "FL_P_powering")]
    [FunctionList("FL_F_numbertheory", "FL_P_numbertheoryfunctions")]
    [FunctionList("FL_F_numbertheory", "FL_P_primitiveroots")]
    [FunctionList("FL_F_numbertheory", "FL_P_goldbach")]
    public class PrimesPlugin : ICrypTutorial
    {
        #region IPlugin Members

        private PrimesControl m_PrimesPlugin = null;
        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get
            {
                if (m_PrimesPlugin == null) m_PrimesPlugin = new PrimesControl();
                return m_PrimesPlugin;
            }
        }

        public void Execute()
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
            if (m_PrimesPlugin != null)
                m_PrimesPlugin.Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
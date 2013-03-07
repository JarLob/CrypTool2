﻿/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Numerics;
using Cryptool.PluginBase.Miscellaneous;
using Primes.Bignum;
using Primes.WpfControls.Validation.Validator;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.Library;
using rsc = Primes.Resources.lang.WpfControls.Primetest;

namespace Primes.WpfControls.Primetest.MillerRabin
{
    /// <summary>
    /// Interaction logic for MillerRabinControl.xaml
    /// </summary>
    public partial class MillerRabinControl : UserControl, IPrimeTest
    {
        #region Initialization

        public MillerRabinControl()
        {
            InitializeComponent();
            SetInputValidators();
            SetDefaults();
            iscBaseRandom.Execute += new ExecuteSingleDelegate(iscBaseRandom_Execute);
            iscRounds.Execute += new ExecuteSingleDelegate(iscRounds_Execute);
            ircSystematic.Execute += new ExecuteDelegate(ircSystematic_Execute);
            ircSystematic.IntervalSizeCanBeZero = true;
            rnd = new Random(DateTime.Today.Millisecond);
        }

        void ircSystematic_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
        }

        void iscRounds_Execute(PrimesBigInteger value)
        {
            if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
        }

        void iscBaseRandom_Execute(PrimesBigInteger value)
        {
            if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
        }

        private void SetInputValidators()
        {
            InputValidator<PrimesBigInteger> ivRndRounds = new InputValidator<PrimesBigInteger>();
            ivRndRounds.DefaultValue = "2";
            ivRndRounds.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One);
            iscRounds.AddInputValidator(InputSingleControl.Free, ivRndRounds);

            InputValidator<PrimesBigInteger> ivRndBase = new InputValidator<PrimesBigInteger>();
            ivRndBase.DefaultValue = "3";
            ivRndBase.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Three);
            iscBaseRandom.AddInputValidator(InputSingleControl.Free, ivRndBase);

            InputValidator<PrimesBigInteger> ivSysBaseFrom = new InputValidator<PrimesBigInteger>();
            ivSysBaseFrom.DefaultValue = "2";
            ivSysBaseFrom.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Two);
            InputValidator<PrimesBigInteger> ivSysBaseTo = new InputValidator<PrimesBigInteger>();
            ivSysBaseTo.DefaultValue = "3";
            ivSysBaseTo.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Three);
            ircSystematic.AddInputValidator(InputRangeControl.FreeFrom, ivSysBaseFrom);
            ircSystematic.AddInputValidator(InputRangeControl.FreeTo, ivSysBaseTo);
        }

        private void SetDefaults()
        {
            iscBaseRandom.FreeText = "100";
            iscRounds.FreeText = "10";
            ircSystematic.SetText(InputRangeControl.FreeFrom, "2");
            ircSystematic.SetText(InputRangeControl.FreeTo, "100");
        }

        #endregion

        #region Properties

        private PrimesBigInteger m_Value;
        private PrimesBigInteger m_Rounds;
        private PrimesBigInteger m_RandomBaseTo;
        private PrimesBigInteger m_SystematicBaseFrom;
        private PrimesBigInteger m_SystematicBaseTo;
        private PrimesBigInteger m_d;
        private int m_shift;

        private Thread m_TestThread;
        Random rnd;

        #endregion

        //private bool MillerRabin(PrimesBigInteger rounds, PrimesBigInteger n)
        //{
        //  PrimesBigInteger i = PrimesBigInteger.One;

        //  while (i.CompareTo(rounds) <= 0)
        //  {
        //    if (Witness(PrimesBigInteger.RandomM(n.Subtract(PrimesBigInteger.Two)).Add(PrimesBigInteger.Two), n))
        //      return false;
        //    i = i.Add(PrimesBigInteger.One);
        //  }
        //  return true;
        //}

        private void GetShift(PrimesBigInteger a)
        {
            m_d = m_Value - 1;
            m_shift = 0;

            while (m_d.Mod(2).CompareTo(0) == 0)
            {
                log.Info(string.Format(rsc.Primetest.mr_shiftright, m_d, m_d.ShiftRight(1)));
                m_d = m_d.ShiftRight(1);
                m_shift++;
            }
        }

        private bool Witness(PrimesBigInteger a)
        {
            // a mustn't be a multiple of n
            if (a.Mod(m_Value).CompareTo(0) == 0)
                return false;

            PrimesBigInteger n_1 = m_Value - 1;
            log.Info(string.Format("n-1 = {0} = 2^{1} * {2}", n_1, m_shift, m_d));

            PrimesBigInteger former = a.ModPow(m_d, m_Value);
            log.Info(string.Format(rsc.Primetest.mr_calculating1, a, m_d, m_Value, former));
            if (former.CompareTo(1) == 0)
            {
                log.Info(string.Format(rsc.Primetest.mr_isprime, a, m_Value));
                return false;
            }

            PrimesBigInteger square = 1;
            for (int i = 1; i <= m_shift; i++)
            {
                square = former.ModPow(2, m_Value);
                log.Info(string.Format(rsc.Primetest.mr_calculating2, a, m_d, 1 << i, m_Value, former, square));
                if (square.CompareTo(1) == 0)
                {
                    bool trivialroot = former.CompareTo(1) == 0 || former.CompareTo(n_1) == 0;
                    if (trivialroot)
                    {
                        log.Info(string.Format(rsc.Primetest.mr_isprime, a, m_Value));
                        return false;
                    }
                    else
                    {
                        log.Info(string.Format(rsc.Primetest.mr_isnotprime1, former, m_Value));
                        return true;
                    }
                }
                former = square;
            }

            log.Info(string.Format(rsc.Primetest.mr_isnotprime2, m_Value, a, n_1, m_Value, square));
            return true;
        }

        #region IPrimeTest Members

        public void Execute(PrimesBigInteger value)
        {
            CancelTestThread();
            log.Clear();

            m_Value = value;

            if (m_Value.Mod(2).CompareTo(0) == 0)
            {
                log.Info(string.Format(rsc.Primetest.mr_iseven, value));
                FireEventCancelTest();
            }
            else
            {
                log.Info(string.Format(rsc.Primetest.mr_shiftcalc, m_Value - 1));
                GetShift(value);
                log.Info(string.Format((m_shift == 1) ? rsc.Primetest.mr_shiftedright : rsc.Primetest.mr_shiftedright2, m_shift, m_Value - 1, m_d));
                log.Info("");

                switch (KindOfTest)
                {
                    case KOD.Random:
                        ExecuteRandom();
                        break;
                    case KOD.Systematic:
                        ExecuteSystematic();
                        break;
                }
            }
        }

        private void ExecuteRandom()
        {
            m_Rounds = iscRounds.GetValue();
            m_RandomBaseTo = iscBaseRandom.GetValue();

            if (m_Rounds != null && m_RandomBaseTo != null)
            {
                m_TestThread = new Thread(new ThreadStart(new VoidDelegate(ExecuteRandomThread)));
                m_TestThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                m_TestThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                m_TestThread.Start();
            }
            else
            {
                FireEventCancelTest();
            }
        }

        private void ExecuteRandomThread()
        {
            FireEventExecuteTest();

            int i = 1;
            for (; i <= m_Rounds; i++)
            {
                BigInteger k = BigIntegerHelper.Max(2, BigIntegerHelper.RandomIntLimit(BigInteger.Parse(m_RandomBaseTo.ToString())));
                if (ExecuteWitness(i, new PrimesBigInteger(k.ToString()))) break;
            }

            if (i <= m_Rounds)
                log.Info(string.Format((i == 1) ? rsc.Primetest.mr_witnessfound1 : rsc.Primetest.mr_witnessfound2, i, m_Value));
            else
                log.Info(string.Format((m_Rounds.IntValue == 1) ? rsc.Primetest.mr_witnessnotfound1 : rsc.Primetest.mr_witnessnotfound2, m_Rounds.IntValue, m_Value));

            FireEventCancelTest();
        }

        private bool ExecuteWitness(int round, PrimesBigInteger a)
        {
            log.Info(string.Format(rsc.Primetest.mr_round, round, a));
            bool result = Witness(a);
            log.Info("");
            return result;
        }

        private void ExecuteSystematic()
        {
            if (ircSystematic.GetValue(ref m_SystematicBaseFrom, ref m_SystematicBaseTo))
            {
                m_TestThread = new Thread(new ThreadStart(new VoidDelegate(ExecuteSystematicThread)));
                m_TestThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                m_TestThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                m_TestThread.Start();
            }
            else
            {
                FireEventCancelTest();
            }
        }

        private void ExecuteSystematicThread()
        {
            FireEventExecuteTest();

            bool foundwitness = false;

            int i = 1;
            for (; m_SystematicBaseFrom <= m_SystematicBaseTo; i++)
            {
                if (ExecuteWitness(i, m_SystematicBaseFrom)) { foundwitness = true; break; }
                m_SystematicBaseFrom = m_SystematicBaseFrom + 1;
            }

            if (foundwitness)
                log.Info(string.Format((i == 1) ? rsc.Primetest.mr_witnessfound1 : rsc.Primetest.mr_witnessfound2, i, m_Value));
            else
                log.Info(string.Format((m_Rounds.IntValue == 1) ? rsc.Primetest.mr_witnessnotfound1 : rsc.Primetest.mr_witnessnotfound2, m_Rounds.IntValue, m_Value));

            FireEventCancelTest();
        }

        public void CancelExecute()
        {
            CancelTestThread();
            FireEventCancelTest();
        }

        private void CancelTestThread()
        {
            if (m_TestThread != null)
            {
                m_TestThread.Abort();
                m_TestThread = null;
            }
        }

        public bool IsRunning()
        {
            return (m_TestThread != null) && m_TestThread.IsAlive;
        }

        public event VoidDelegate Start;

        public event VoidDelegate Stop;

        private void FireEventExecuteTest()
        {
            if (Start != null) Start();
        }

        private void FireEventCancelTest()
        {
            if (Stop != null) Stop();
        }

        #endregion

        #region IPrimeTest Members

        public Primes.WpfControls.Validation.IValidator<PrimesBigInteger> Validator
        {
            get { return new BigIntegerMinValueValidator(null, PrimesBigInteger.Three); }
        }

        #endregion

        private void rbClick(object sender, RoutedEventArgs e)
        {
            pnlRandom.Visibility = (rbRandom.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
            pnlSystematic.Visibility = (rbSystematic.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
        }

        private KOD KindOfTest
        {
            get
            {
                if (rbRandom.IsChecked.Value) return KOD.Random;
                else return KOD.Systematic;
            }
        }

        private enum KOD { Random, Systematic }

        #region IPrimeTest Members

        #endregion

        #region IPrimeVisualization Members

        public event VoidDelegate Cancel;

        private void FireCancelEvent()
        {
            if (Cancel != null) Cancel();
        }

        public event CallbackDelegateGetInteger ForceGetInteger;

        private void FireForceGetInteger()
        {
            ForceGetInteger(null);
        }

        public event CallbackDelegateGetInteger ForceGetIntegerInterval;

        private void FireForceGetIntegerInterval()
        {
            ForceGetIntegerInterval(null);
        }

        public void Execute(PrimesBigInteger from, PrimesBigInteger to)
        {
        }

        #endregion
    }
}

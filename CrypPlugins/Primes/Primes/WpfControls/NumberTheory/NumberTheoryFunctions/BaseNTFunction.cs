/*
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
using System.Threading;
using Primes.Bignum;
using System.Windows.Controls;
using Primes.WpfControls.Components;
using Primes.Library;
using System.Diagnostics;
using System.Resources;
using System.Reflection;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
    public abstract class BaseNTFunction : INTFunction
    {
        #region Resources

        protected const string groupbox_choosefunctions = "groupbox_choosefunctions";
        protected const string eulerphi = "eulerphi";
        protected const string eulerphisum = "eulerphisum";
        protected const string eulerphivalues = "eulerphivalues";
        protected const string pix = "pix";
        protected const string rho = "sigma";
        protected const string tau = "tau";
        protected const string tauvalues = "tauvalues";
        protected const string gcd = "gcd";
        protected const string lcm = "lcm";
        protected const string modinv = "modinv";
        protected const string exteuclid = "exteuclid";

        protected static ResourceManager m_ResourceManager;

        static BaseNTFunction()
        {
            m_ResourceManager = new ResourceManager("Primes.Resources.lang.Numbertheory.Numbertheory", typeof(Primes.Resources.lang.Numbertheory.Numbertheory).Assembly);
        }

        #endregion

        public BaseNTFunction()
        {
        }

        #region Properties

        protected Thread m_Thread;
        protected PrimesBigInteger m_From;
        protected PrimesBigInteger m_To;
        protected PrimesBigInteger m_SecondParameter;

        #endregion

        #region INTFunction Members

        public virtual void Start(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
        {
            Stop();
            m_From = from;
            m_To = to;
            m_SecondParameter = second;
            m_Thread = new Thread(new ThreadStart(DoExecute));
            m_Thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            m_Thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            m_Thread.Start();
            m_IsRunning = true;
        }

        public virtual void Stop()
        {
            m_IsRunning = false;
            CancelThread();
        }

        protected void CancelThread()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        protected abstract void DoExecute();
        public event VoidDelegate OnStart;
        public event VoidDelegate OnStop;

        protected void FireOnStart()
        {
            if (OnStart != null) OnStart();
        }

        protected void FireOnStop()
        {
            if (OnStop != null)
            {
                m_IsRunning = false;
                OnStop();
            }
        }

        public event NumberTheoryMessageDelegate Message;

        protected void FireOnMessage(INTFunction function, PrimesBigInteger value, string message)
        {
            if (Message != null) Message(function, value, message);
        }

        #endregion

        #region INTFunction Members

        public virtual string Description
        {
            get
            {
                return "Base";
            }
        }

        #endregion

        #region INTFunction Members

        bool m_IsRunning;

        public bool IsRunnung
        {
            get { return m_IsRunning; }
        }

        public virtual bool NeedsSecondParameter
        {
            get { return false; }
        }

        #endregion
    }
}
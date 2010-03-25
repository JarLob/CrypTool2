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
using Primes.WpfControls.Threads;
using Primes.Bignum;
using Primes.Library;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;

namespace Primes.WpfControls.Primetest.SieveOfEratosthenes
{
  /// <summary>
  /// Interaction logic for SieveOfEratosthenes.xaml
  /// </summary>
  public partial class SieveOfEratosthenes : UserControl, IPrimeTest
  {
    private enum Method { Manual, Automatic }

    #region Constructors
    private PrimesBigInteger m_Value;
    private Step s;
    private Method m_Method;
    public SieveOfEratosthenes()
    {
      InitializeComponent();
      sievegrid.NumberButtonClick += new Primes.WpfControls.Primetest.Numbergrid.NumberButtonClickDelegate(sievegrid_NumberButtonClick);
      m_PrimesThreadFinished = new ManualResetEvent(false);
    }
    public void Init(PrimesBigInteger value)
    {

    }
    #endregion

    #region Events
    public event VoidDelegate Start;
    public event VoidDelegate Stop;

    private void FireOnCancel()
    {
      if (Stop != null) Stop();
    }
    private void FireOnExecute()
    {
      if (Start != null) Start();
    }
    #endregion
    #region Properties
    Thread m_ThreadAutomatic;
    FindPrimesThread m_FPThread;
    EventWaitHandle m_PrimesThreadFinished;
    #endregion

    #region Steps
    void sievegrid_NumberButtonClick(Primes.WpfControls.Components.NumberButton value)
    {
      if (m_Method == Method.Manual)
      {
        if (s.Expected.IntValue == 7)
        {
          btnForceAutomatic.Visibility = Visibility.Visible;
        }
        StepResult result = s.DoStep(value.BINumber);

        switch (result)
        {

          case StepResult.SUCCESS:
            log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_removemultiple, s.Current.ToString()));
            break;
          case StepResult.FAILED:
            log.Error(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_multiplenotexpected, value.BINumber.ToString()));
            break;
          case StepResult.END:
            log.Info(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_done);
            PrimesStatus();
            break;
        }
      }

    }

    void StepAutomatic()
    {
      StepResult result = StepResult.SUCCESS;
      PrimesBigInteger value = s.Expected;
      while (result != StepResult.END)
      {
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_removemultipleof, value.ToString()));
        result = s.DoStep(value);
        
        value = value.NextProbablePrime();
      }
      PrimesStatus();
      sievegrid.MarkNumbers(Brushes.LightSkyBlue);
      FireOnCancel();
      m_ThreadAutomatic.Abort();
    }

    #endregion

    #region Begin/Cancel
    public void Execute(PrimesBigInteger integer)
    {
      try
      {
        log.Info("Start!");
      }
      catch (Exception ex) { MessageBox.Show(ex.Message + "\n" + ex.StackTrace); return; }
      m_Value = integer;
      sievegrid.Limit = m_Value;
      this.Reset();
      ControlHandler.SetPropertyValue(gridpanel, "IsEnabled", true);
      if (rbAutomatic.IsChecked.Value) m_Method = Method.Automatic;
      else m_Method = Method.Manual;
      s = new Step(this.sievegrid, m_Value);
      if (m_Method == Method.Manual)
      {
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_infostepwise1, m_Value.ToString()));
        log.Info(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_infostepwise2);

      }
      else
      {
        m_ThreadAutomatic = new Thread(new ThreadStart(StepAutomatic));
        FireOnExecute();
        sievegrid.SetButtonStatus(false, false);
        m_ThreadAutomatic.Start();
      }
      
    }

    private void PrimesStatus()
    {
      ControlHandler.SetPropertyValue(btnForceAutomatic, "Visibility", Visibility.Hidden);

      //m_PrimesThreadFinished.WaitOne();
      //IList<PrimesBigInteger> remainders = m_FPThread.Primes;
      StringBuilder sbResult = new StringBuilder(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_doneautomatic);
      //  new StringBuilder(string.Format("Fertig. Es wurden {0} Primzahlen gefunden.", remainders.Count));
      if (m_Value.IsPrime(10))
      {
        sbResult.Append(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_isprime, m_Value.ToString()));
      }
      {
        sbResult.Append(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_isnotprime, m_Value.ToString()));
      }
      log.Info(sbResult.ToString());


    }
    private void btnForceAutomatic_Click(object sender, RoutedEventArgs e)
    {
      ControlHandler.DisableButton(btnForceAutomatic);
      m_ThreadAutomatic = new Thread(new ThreadStart(StepAutomatic));
      FireOnExecute();
      sievegrid.SetButtonStatus(false, false);
      m_ThreadAutomatic.Start();
    }

    #region Resetting
    private void Reset()
    {
      ResetControl();
      //ResetSieve();
    }
    private void ResetControl()
    {
      if (m_FPThread != null) { m_FPThread.Terminate(); m_FPThread = null; }
      m_FPThread = new FindPrimesThread(m_Value, m_PrimesThreadFinished);
      if (m_ThreadAutomatic != null) { m_ThreadAutomatic.Abort(); m_ThreadAutomatic = null; }
      FireOnCancel();
      log.Clear();
      btnForceAutomatic.Visibility = Visibility.Hidden;
      btnForceAutomatic.IsEnabled = true;
      if(s!=null)
        s.Reset();
    }

    private void ResetSieve()
    {
      sievegrid.Reset();
      sievegrid.Limit = m_Value;
      sievegrid.RemoveNumber(PrimesBigInteger.ValueOf(1));
    }

    public void CancelExecute()
    {
      Reset();
    }
    #endregion
    #endregion

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      //log.Width = Math.Max(sizeInfo.NewSize.Width- 20,200);
      //log.Height = Math.Max(sizeInfo.NewSize.Height- 70,200);

    }

    #region Free Control
    public void CleanUp()
    {
      ResetControl();
    }
    #endregion

    private IValidator<PrimesBigInteger> m_Validator;
    public IValidator<PrimesBigInteger> Validator
    {
      get {
        if (m_Validator == null)
        {
          m_Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Two, PrimesBigInteger.ValueOf(100000));
          m_Validator.Message = Primes.Resources.lang.WpfControls.Primetest.Primetest.soe_validatormessage;
        }

        return m_Validator;
      }
    }

    #region IPrimeTest Members



    #endregion

    private void rbModeClick(object sender, RoutedEventArgs e)
    {
      if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
    }

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
      throw new NotImplementedException();
    }

    #endregion
  }
}

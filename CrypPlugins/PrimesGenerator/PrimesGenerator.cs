﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;
using System.Security.Cryptography;

namespace Cryptool.PrimesGenerator
{

  [PluginInfo("PrimesGenerator.Properties.Resources", "PluginCaption", "PluginTooltip", "PrimesGenerator/DetailedDescription/doc.xml", "PrimesGenerator/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
  public class PrimesGenerator : ICrypComponent
  {
    public PrimesGenerator()
    {
      m_Settings = new PrimesGeneratorSettings();
      //m_Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Settings_PropertyChanged);
    }


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


    private BigInteger m_OutputString;
    [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
    public BigInteger OutputString
    {
      get { return this.m_OutputString; }
      set
      {
        m_OutputString = value;
        FirePropertyChangedEvent("OutputString");
      }
    }

    [PropertyInfo(Direction.InputData, "nCaption", "nTooltip")]
    public BigInteger n
    {
        get;
        set;
    }

    private PrimesGeneratorSettings m_Settings = new PrimesGeneratorSettings();
    public ISettings Settings
    {
      get { return m_Settings; }
    }

    bool checkParameters()
    {
        try
        {
            if (n == 0) n = BigInteger.Parse(m_Settings.Input);

            switch ( m_Settings.Mode )
            {
                case 0:
                case 1:
                    if (n < 2)
                    {
                        FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1.");
                        return false;
                    }
                    if (n >= 1024)
                        FireOnGuiLogNotificationOccuredEvent("Please note that the generation of prime numbers with " + n + " bits may take some time...", NotificationLevel.Warning);
                    break;
                case 2:
                    if (n <= 1)
                    {
                        FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1");
                        return false;
                    }
                    break;
                case 3: // search previous prime
                    if (n <= 2)
                    {
                        FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 2");
                        return false;
                    }
                    break;
                case 4: // search next prime
                    break;
            }
        }
        catch
        {
            FireOnGuiLogNotificationOccuredEventError("Please enter an Integer value for n.");
            return false;
        }

        return true;
    }

    public System.Windows.Controls.UserControl Presentation
    {
      get { return null; }
    }

    public void PreExecution()
    {
        n = 0;
    }

    public void Execute()
    {
        if (!checkParameters()) return;

        ProgressChanged(0, 100);

        switch (m_Settings.Mode)
        {
            case 0:   // create prime with m_Input bits
                OutputString = this.RandomPrimeBits((int)n);
                break;
            case 1:   // create prime with m_Input bits, MSB set
                OutputString = this.RandomPrimeMSBSet((int)n);
                break;
            case 2:   // create prime <= m_Input
                OutputString = BigIntegerHelper.RandomPrimeLimit(n + 1);
                break;
            case 3:   // search biggest prime < m_Input
                OutputString = this.PreviousProbablePrime(n-1);
                break;
            case 4:   // search smallest prime > m_Input
                OutputString = this.NextProbablePrime(n+1);
                break;
        }

        ProgressChanged(100, 100);
    }        
      
    private BigInteger RandomPrimeBits(int bits)
    {
        if (bits < 0) throw new ArithmeticException("Enter a positive bitcount");
        BigInteger limit = ((BigInteger)1) << bits;
        if (limit <= 2) throw new ArithmeticException("No primes below this limit");

        while (true)
        {
            var p = this.NextProbablePrime(limit.RandomIntLimit());
            if (p < limit) return p;
        }
    }

    private BigInteger RandomPrimeMSBSet(int bits)
    {
        if (bits <= 1) throw new ArithmeticException("No primes with this bitcount");

        BigInteger limit = ((BigInteger)1) << bits;

        while (true)
        {
            var p = this.NextProbablePrime(BigIntegerHelper.SetBit(BigIntegerHelper.RandomIntBits(bits - 1), bits - 1));
            if (p < limit) return p;
        }
    }

    private BigInteger NextProbablePrime(BigInteger n)
    {
        if (n < 0) throw new ArithmeticException("NextProbablePrime cannot be called on value < 0");
        if (n <= 2) return 2;
        if (n.IsEven) n++;
        if (n == 3) return 3;
        BigInteger r = n % 6;
        if (r == 3) n += 2;
        if (r == 1) { if (n.IsProbablePrime()) return n; else n += 4; }

        // at this point n mod 6 = 5

        int expectedtries = (int)(BigInteger.Log(n) / 6);
        int tries = 0;          // number of actual tries

        while (true)
        {
            ProgressChanged((int)(tries * 100.0 / expectedtries), 100);
            if (tries + 1 < expectedtries) tries++;

            if (n.IsProbablePrime()) return n;
            n += 2;
            if (n.IsProbablePrime()) return n;
            n += 4;
        }
    }

    private BigInteger PreviousProbablePrime(BigInteger n)
    {
        if (n < 2) throw new ArithmeticException("PreviousProbablePrime cannot be called on value < 2");
        if (n == 2) return 2;
        if (n.IsEven) n--;
        if (n == 3) return 3;
        BigInteger r = n % 6;
        if (r == 3) n -= 2;
        if (r == 1) { if (n.IsProbablePrime()) return n; else n -= 4; }

        // at this point n mod 6 = 1

        int expectedtries = (int)(BigInteger.Log(n) / 6);
        int tries = 0;          // number of actual tries

        while (true)
        {
            ProgressChanged((int)(tries * 100.0 / expectedtries), 100);
            if (tries + 1 < expectedtries) tries++;

            if (n.IsProbablePrime()) return n;
            n -= 2;
            if (n.IsProbablePrime()) return n;
            n -= 4;
        }
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
  }
}

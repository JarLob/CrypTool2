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
using Primes.WpfControls.Components;

using Primes.Bignum;
using Primes.Library;
using System.Threading;
using System.Diagnostics;

namespace Primes.WpfControls.Primegeneration.SieveOfAtkin
{
  /// <summary>
  /// Interaction logic for Numbergrid.xaml
  /// </summary>
  public delegate void NumberButtonClickDelegate(NumberButton value);
  public partial class Numbergrid : UserControl
  {
    private const short MAX = 20;
    private const short MIN = 2;
    //private IDictionary<long, bool> m_Sieved;
    private bool[] m_Sieved;

    private IDictionary<PrimesBigInteger, Brush> m_MarkedNumbers;
    #region Constructors
    public Numbergrid()
    {
       
      InitializeComponent();
      this.Rows = 20;
      this.Columns = 20;
      this.numbergrid.Children.Clear();
      this.border.BorderThickness = new Thickness(0);
      m_RemovedNumbers = new List<PrimesBigInteger>();
      m_RemovedMods = new List<PrimesBigInteger>();
      //m_Sieved = new Dictionary<long, bool>();
      if (m_Limit != null)
      {
        DrawGrid();
      }
      m_MarkedNumbers = new Dictionary<PrimesBigInteger, Brush>();
    }
    #endregion

    #region Properties
    private int m_Rows;

    public int Rows
    {
      get { return m_Rows; }
      set { m_Rows = Math.Min(Math.Max(value, MIN), MAX); }
    }
    private int m_Columns;

    public int Columns
    {
      get { return m_Columns; }
      set { m_Columns = Math.Min(Math.Max(value, MIN), MAX); }
    }

    private PrimesBigInteger m_Limit;

    //public PrimesBigInteger Limit
    //{
    //  get { return m_Limit; }
    //  set {
    //    m_Limit = value;
    //    m_Sieved = new long[m_Limit.LongValue];
    //    PrimesBigInteger i = PrimesBigInteger.Zero;
    //    m_Sieved.Clear();
    //    while (i.CompareTo(m_Limit) <= 0)
    //    {
    //      m_Sieved.Add(i.LongValue, false);
    //      i = i.Add(PrimesBigInteger.One);
    //    }
    //    m_ButtonColor = Brushes.White; 
    //    m_RemovedMods.Clear(); 
    //    InitButtons(); 
    //    RemoveNumber(PrimesBigInteger.One);
    //  }
    //}
    public PrimesBigInteger Limit
    {
      get { return m_Limit; }
      set
      {
        m_Limit = value;
        m_Sieved = new bool[m_Limit.LongValue+1];
        for (long i = 0; i <= m_Limit.LongValue; i++)
        {
          m_Sieved[i] = true;
        }
        m_ButtonColor = Brushes.White;
        m_RemovedMods.Clear();
        m_MarkedNumbers.Clear();
        InitButtons();
        
      }
    }

    public bool[] Sieved
    {
      get { return m_Sieved; }
      set { 
        m_Sieved = value;
        ScrollGrid(PrimesBigInteger.Zero, false);
      }
    }

    #endregion

    #region DeleteNumbers
    private IList<PrimesBigInteger> m_RemovedNumbers;
    private IList<PrimesBigInteger> m_RemovedMods;


    public void RemoveNumber(PrimesBigInteger value)
    {
      m_RemovedNumbers.Add(value);
      if(m_Limit!=null)
        RedrawButtons();
    }

    public void RemoveMulipleOf(PrimesBigInteger value)
    {
      DateTime start = DateTime.Now;

      PrimesBigInteger i = value.Multiply(PrimesBigInteger.Two);
      while (i.CompareTo(m_Limit) <= 0)
      {
        m_Sieved[i.LongValue] = false;
        i = i.Add(value);
      }
      //PrimesBigInteger counter = PrimesBigInteger.Two;
      //while (counter.Multiply(value).CompareTo(m_Limit)<=0)
      //{
      //  m_RemovedNumbers.Add(counter.Multiply(value));
      //  counter = counter.Add(PrimesBigInteger.One);
      //}
      m_RemovedMods.Add(value);
      if(value.Pow(2).CompareTo(GetMaxVisibleValue())<=0)
        RedrawButtons();
      TimeSpan diff = DateTime.Now - start;

    }
    private void RedrawButtons()
    {
      ScrollGrid(PrimesBigInteger.Zero,false);
    }

    public void ClearRemovedNumbers(bool redraw)
    {
      this.m_RemovedNumbers.Clear();
      this.m_RemovedMods.Clear();

      if (redraw) RedrawButtons();
    }

    public IList<PrimesBigInteger> Remainders
    {
      get
      {
        List<PrimesBigInteger> result = new List<PrimesBigInteger>();
        //BigInteger bi = BigInteger.Two;
        //while (bi.CompareTo(m_Limit) <= 0)
        //{
        //  if (!m_RemovedNumbers.Contains(bi))
        //  {
        //    result.Add(bi);
        //  }
        //  bi = bi.Add(BigInteger.One);
        //}
        return result;
      }
    }

    public void Reset()
    {
      if (m_Limit != null)
      {
        this.m_ButtonColor = null;
        this.ClearRemovedNumbers(true);
        this.SetButtonStatus();
      }
    }

    #endregion


    #region Events
    public event NumberButtonClickDelegate NumberButtonClick;
    #endregion

    #region Drawing
    private void DrawGrid()
    {
      this.border.BorderThickness = new Thickness(1);
      this.numbergrid.RowDefinitions.Clear();
      this.numbergrid.ColumnDefinitions.Clear();
      for (int i = 0; i < this.Rows + this.Rows - 1; i++)
      {
        RowDefinition rd = new RowDefinition();
        if (i % 2 == 0)
        {
          rd.Height = new GridLength(0.1, GridUnitType.Star);
        }
        else
        {
          rd.Height = new GridLength(1, GridUnitType.Pixel);
          Rectangle rect = new Rectangle();
          rect.Height = 1.0;
          rect.Fill = Brushes.Black;
          Grid.SetColumnSpan(rect, this.Columns + this.Columns - 1);
          Grid.SetRow(rect, i);
          this.numbergrid.Children.Add(rect);
        }
        this.numbergrid.RowDefinitions.Add(rd);
      }

      for (int i = 0; i < this.Columns + this.Columns - 1; i++)
      {
        ColumnDefinition cd = new ColumnDefinition();
        if (i % 2 == 0)
        {
          cd.Width = new GridLength(0.1, GridUnitType.Star);
        }
        else
        {
          cd.Width = new GridLength(1, GridUnitType.Pixel);
          Rectangle rect = new Rectangle();
          rect.Width = 1.0;
          rect.Fill = Brushes.Black;
          Grid.SetRowSpan(rect, this.Rows + this.Rows - 1);
          Grid.SetColumn(rect, i);
          this.numbergrid.Children.Add(rect);
        }
        this.numbergrid.ColumnDefinitions.Add(cd);
      }
    }
    private Brush m_ButtonColor = null;
    public void MarkNumbers(Brush color)
    {
      m_ButtonColor = color;
      ScrollGrid(PrimesBigInteger.Zero);
    }

    
    public void MarkNumber(PrimesBigInteger number, Brush color)
    {
      if (!m_MarkedNumbers.ContainsKey(number))
        m_MarkedNumbers.Add(number, color);
      else
        m_MarkedNumbers[number] = color;

      UIElementCollection buttons =
        ControlHandler.GetPropertyValue(numbergrid, "Children") as UIElementCollection;
      int i = (int)ControlHandler.GetPropertyValue(buttons, "Count");

      NumberButton first = (buttons[Columns+Rows-2] as NumberButton);
      if (first.BINumber.Add(number).CompareTo(PrimesBigInteger.ValueOf(Columns * Rows)) < 0)
      {
        int index = number.Subtract(first.BINumber).IntValue + Columns + Rows - 2;
        NumberButton btn = buttons[index] as NumberButton;
        ControlHandler.SetPropertyValue(btn, "Background", color);
      }
    }

    #endregion 

    #region Buttons
    private void InitButtons()
    {
      btnBack.Visibility = btnCompleteBack.Visibility = Visibility.Visible;
      btnForward.Visibility = btnCompleteForward.Visibility = Visibility.Visible;

      if (numbergrid.RowDefinitions.Count >= MIN)
      {
        this.numbergrid.Children.Clear();
        DrawGrid();
      }

      PrimesBigInteger counter = PrimesBigInteger.ValueOf(1);

      for (int i = 0; i < this.Rows; i++)
      {
        for (int j = 0; j < this.Columns; j++)
        {
          NumberButton btn = new NumberButton();
          btn.NumberButtonStyle = Primes.WpfControls.Components.NumberButtonStyle.Button.ToString();
          btn.ShowContent = true;
          btn.Number = (counter).ToString();
          btn.Click += new RoutedEventHandler(NumberButton_Click);
          Grid.SetColumn(btn, 2 * j);
          Grid.SetRow(btn, 2 * i);

          btn.Background = Brushes.White;

          this.numbergrid.Children.Add(btn);
          if (counter.CompareTo(this.m_Limit) > 0)
            btn.Visibility = Visibility.Hidden;
          if(m_RemovedNumbers.Contains(btn.BINumber))
            btn.Visibility = Visibility.Hidden;

          counter = counter.Add(PrimesBigInteger.ValueOf(1));
        }
      }
      SetButtonStatus();
    }

    void NumberButton_Click(object sender, RoutedEventArgs e)
    {
      if (sender != null && sender.GetType() == typeof(NumberButton))
      {
        if (NumberButtonClick != null)
          NumberButtonClick(sender as NumberButton);
      }
    }

    
    #endregion 

    #region Forward/Back Button
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
      PrimesBigInteger amount = PrimesBigInteger.ValueOf(this.Rows * -1);
      if (sender == btnCompleteBack)
      {
        amount = PrimesBigInteger.One.Subtract((numbergrid.Children[this.Rows + this.Columns - 2] as NumberButton).BINumber);
      }

      ScrollGrid(amount, false);
    }

    private void btnForward_Click(object sender, RoutedEventArgs e)
    {
      PrimesBigInteger rows = PrimesBigInteger.ValueOf(this.Rows);
      PrimesBigInteger amount = rows;
      if (sender == btnCompleteForward)
      {
        amount = 
          m_Limit.Subtract((numbergrid.Children[this.Rows + this.Columns - 2 + (this.Columns * this.Rows) - 1] as NumberButton).BINumber);
        amount = amount.Add(rows.Subtract(amount.Mod(rows)));
      }
      ScrollGrid(amount, false);
    }

    delegate void ParameterDelegate(object o);
    private void ScrollGrid(PrimesBigInteger amount, bool AsThread)
    {
      if (AsThread)
      {
        Thread t = new Thread(new ParameterizedThreadStart(new ParameterDelegate(ScrollGrid)));
        t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
        t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
        t.Start(amount);
      }
      else
      {
        ScrollGrid(amount);
      }
    }

    private void DoScrollGrid(PrimesBigInteger amount)
    {
      bool keepColor = true;
      int counter = 0;
      for (int i = 0; i < this.Rows; i++)
      {
        for (int j = 0; j < this.Columns; j++)
        {
          NumberButton btn = GetNumberButton(this.Rows + this.Columns - 2 + counter);
          if (btn != null)
          {
            PrimesBigInteger newVal = btn.BINumber.Add(amount);
            btn.BINumber = newVal;
            if (newVal.CompareTo(m_Limit) > 0 || newVal.CompareTo(PrimesBigInteger.One) < 0)
            {
              if (m_Limit.CompareTo(PrimesBigInteger.ValueOf(Rows * Columns)) < 0) return;
              ControlHandler.SetButtonVisibility(btn, Visibility.Hidden);
            }
            else
            {
              // Color the Buttons
              if (m_ButtonColor != null)
              {
                ControlHandler.SetPropertyValue(btn, "Background", m_ButtonColor);
              }
              else
              {
                if (!keepColor)
                  btn.Background = Brushes.White;
              }
              bool isMultipleOfRemovedNumbers = !m_Sieved[btn.BINumber.LongValue];
              if (
                isMultipleOfRemovedNumbers
                || m_RemovedNumbers.Contains(btn.BINumber))
              {
                ControlHandler.SetButtonVisibility(btn, Visibility.Hidden);
              }
              else
              {
                if (m_MarkedNumbers.ContainsKey(newVal))
                {
                  ControlHandler.SetPropertyValue(btn, "Background", m_MarkedNumbers[newVal]);

                }
                ControlHandler.SetButtonVisibility(btn, Visibility.Visible);
              }
            }
          }

          counter++;
        }
      }
      SetButtonStatus();
    }

    private void ScrollGrid(object o)
    {
      if (o != null && o.GetType() == typeof(PrimesBigInteger))
      {
        DoScrollGrid(o as PrimesBigInteger);
      }
    }
    
    private NumberButton GetNumberButton(int index)
    {
      UIElementCollection buttons =
        ControlHandler.GetPropertyValue(numbergrid, "Children") as UIElementCollection;
      return buttons[index] as NumberButton;
    }

    private PrimesBigInteger GetMaxVisibleValue()
    {
      NumberButton nb = GetNumberButton(this.Rows + this.Columns - 2 + (this.Columns * this.Rows) - 1);
      if (nb != null)
        return nb.BINumber;
      else
        return PrimesBigInteger.Zero;
    }
    private PrimesBigInteger GetMinVisibleValue()
    {
      NumberButton nb = GetNumberButton(this.Rows + this.Columns - 2);
      if (nb != null)
        return nb.BINumber;
      else
        return PrimesBigInteger.Zero;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      if ((e.Key == Key.Down || e.Key==Key.PageDown) && ButtonForwardEnabled)
        ScrollGrid(BiRows, true);
      else if ((e.Key == Key.Up || e.Key == Key.PageUp) && ButtonBackEnabled)
        ScrollGrid(BiRows.Multiply(PrimesBigInteger.ValueOf(-1)), true);

    }
    private void SetButtonStatus()
    {
      SetButtonStatus(ButtonBackEnabled,ButtonForwardEnabled);
    }

    public void SetButtonStatus(bool backEnabled, bool forwardEnabled)
    {
      ControlHandler.SetButtonEnabled(btnForward, forwardEnabled);
      ControlHandler.SetButtonEnabled(btnCompleteForward, forwardEnabled);
      ControlHandler.SetButtonEnabled(btnBack, backEnabled);
      ControlHandler.SetButtonEnabled(btnCompleteBack, backEnabled);
    }
    private bool ButtonBackEnabled
    {
      get {
        NumberButton nb = GetNumberButton(this.Rows + this.Columns - 2);;
        if (nb != null)
          return nb.BINumber.CompareTo(PrimesBigInteger.One) >= 1; 
        else
          return false;
      }
    }
    private bool ButtonForwardEnabled
    {
      get {
        NumberButton nb = GetNumberButton(this.Rows + this.Columns - 2 + (this.Columns * this.Rows) - 1);
        if (nb != null)
          return nb.BINumber.CompareTo(m_Limit) <= 0; 
        else
          return false;

      
      }
    }

    private PrimesBigInteger BiRows
    {
      get { return PrimesBigInteger.ValueOf(this.Rows); }
    }
    private void numbergrid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      int scrollfactor = (e.Delta > 0) ? -2 : 2;
      if (scrollfactor>0 && ButtonForwardEnabled)
        ScrollGrid(PrimesBigInteger.ValueOf(this.Rows * scrollfactor), false);
      else if (scrollfactor <0 && ButtonBackEnabled)
        ScrollGrid(PrimesBigInteger.ValueOf(this.Rows * scrollfactor), false);
    }

    #endregion

  }
}

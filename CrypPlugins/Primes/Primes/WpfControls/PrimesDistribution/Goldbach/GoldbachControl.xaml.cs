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
using System.Linq;
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
using Primes.Bignum;
using Primes.Library;
using Primes.WpfControls.Components.Arrows;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using Cryptool.PluginBase.Miscellaneous;
using Primes.Resources.lang.WpfControls.Distribution;

namespace Primes.WpfControls.PrimesDistribution.Goldbach
{
  /// <summary>
  /// Interaction logic for GoldbachControl.xaml
  /// </summary>
  public partial class GoldbachControl : UserControl, IPrimeDistribution
  {
    private bool m_Initialized;

    private Thread m_Thread;

    private PrimesBigInteger m_From = 2;
    private PrimesBigInteger m_To = 1000;
    private PrimesBigInteger m_Min = 0;
    private PrimesBigInteger m_Max = 52;

    private const double PADDING_AXISLEFT = 20;
    private const double PADDING_AXISBOTTOM = 20;
    private const double PADDING_AXISTOP = 10;
    private const double PADDING_AXISRIGHT = 10;

    private const double PADDING_LEFT = PADDING_AXISLEFT + 20;
    private const double PADDING_BOTTOM = PADDING_AXISBOTTOM + 20;
    private const double PADDING_TOP = PADDING_AXISTOP + 20;
    private const double PADDING_RIGHT = PADDING_AXISRIGHT + 20;

    private const int MAXSUMSINTOOLTIP = 10;

    public struct GoldbachInfo
    {
        public int n;
        public int sums;
        public String tooltip;
    }

    private int maxindex = 0;

    public GoldbachControl()
    {
      InitializeComponent();
      Init();

      this.Start += new VoidDelegate(GoldbachControl_Start);
      this.Stop += new VoidDelegate(GoldbachControl_Stop);
      this.Cancel += new VoidDelegate(GoldbachControl_Cancel);

      PaintCoordinateAxis();
    }

    void GoldbachControl_Cancel()
    {
      ircGoldbach.UnLockControls();
      CancelThread();
    }

    void GoldbachControl_Stop()
    {
      ircGoldbach.UnLockControls();
    }

    void GoldbachControl_Start()
    {
      ControlHandler.ClearChildren(PaintArea);
      ircGoldbach.LockControls();
    }

    void ircGoldbach_Cancel()
    {      
      FireCancelEvent();
    }

    void ircGoldbach_Execute(PrimesBigInteger from, PrimesBigInteger to, PrimesBigInteger second)
    {
      m_From = from;
      m_To = to;

      StartThread();
    }

    #region Events

    private event VoidDelegate Start;
    private event VoidDelegate Stop;
    private event VoidDelegate Cancel;

    private void FireStartEvent()
    {
      if (Start != null) Start();
    }

    private void FireStopEvent()
    {
      if (Stop != null) Stop();
    }

    private void FireCancelEvent()
    {
      if (Cancel != null) Cancel();
    }

    #endregion

    #region Thread

    private void StartThread()
    {
      m_Thread = new Thread(new ThreadStart(DoExecute));
      m_Thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
      m_Thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
      m_Thread.Start();
    }

    private void CancelThread()
    {
      if (m_Thread != null)
      {
        m_Thread.Abort();
        m_Thread = null;
      }
    }

    #endregion
    
    #region Properties

    private PrimesBigInteger Range
    {
      get { return m_Max - m_Min; }
    }

    #endregion

    private PrimesBigInteger From
    {
      get 
      {
        PrimesBigInteger value = m_From;
        if (value.Mod(2).Equals(1)) value = value + 1;
        return value;
      }
    }

    private void DoExecute()
    {
      FireStartEvent();

      m_Max = 0;
      m_Min = 1000000000;

      List<GoldbachInfo> points = new List<GoldbachInfo> {};
      PrimesBigInteger goldbachResult = null;
      ControlHandler.ExecuteMethod(this, "PaintCoordinateAxis");
      bool repaint = false;

      for (PrimesBigInteger value = From; value <= m_To; value = value + 2)
      {
          GoldbachInfo info;
          String text = "";
          goldbachResult = Calculate(value, ref text);
          info.n = value.IntValue;
          info.sums = goldbachResult.IntValue;
          String fmt = (info.sums == 1) ? Distribution.numberline_goldbachsum : Distribution.numberline_goldbachsums;
          if(info.sums>0) text = "\n" + text;
          info.tooltip = String.Format("{0}: {1} " + fmt + "{2}", info.n, info.sums, text);
          if (info.sums > MAXSUMSINTOOLTIP) info.tooltip += "\n...";

          points.Add(info);

          if (m_Min > goldbachResult) m_Min = goldbachResult;

          if (m_Max < goldbachResult)
          {
              m_Max = goldbachResult;
              repaint = true;
          }

          if (points.Count >= 100)
          {
              ControlHandler.ExecuteMethod(this, "PaintGoldbachResult", new object[] { points });
              points.Clear();
              if (repaint)
                  ControlHandler.ExecuteMethod(this, "RepaintGoldbach");
          }
      }

      if (points.Count > 0)
      {
        ControlHandler.ExecuteMethod(this, "PaintGoldbachResult", new object[] { points });
        points.Clear();
        if (repaint)
          ControlHandler.ExecuteMethod(this, "RepaintGoldbach");
      }

      FireStopEvent();
    }

    #region Painting

    private double UnitHeight
    {
      get 
      {
        return (PaintArea.ActualHeight - PADDING_BOTTOM - PADDING_TOP) / Range.DoubleValue;
      }
    }

    private double UnitWidth
    {
      get
      {
          if (m_To.Equals(From)) return 0;
          return (PaintArea.ActualWidth - PADDING_LEFT - PADDING_RIGHT) / (m_To-From).DoubleValue;
      }
    }

    public void PaintGoldbachResult(List<GoldbachInfo> points)
    {
        foreach (var point in points)
        {
            Ellipse el = new Ellipse();
            el.Tag = point;
            el.StrokeThickness = 0.0;
            el.ToolTip = new ToolTip { Content = point.tooltip };
            SetEllipse(el);

            el.MouseEnter += new MouseEventHandler(el_MouseEnter);
            el.MouseLeave += new MouseEventHandler(el_MouseLeave);

            PaintArea.Children.Add(el);
        }
    }

    double MapX(double x)
    {
        return (x - From.DoubleValue) * UnitWidth + PADDING_LEFT;
    }

    double MapY(double y)
    {
        return PaintArea.ActualHeight - ((y-m_Min.DoubleValue) * UnitHeight + PADDING_BOTTOM);
    }

    void SetEllipse(Ellipse el, bool normal = true)
    {
        double size;

        if (normal)
        {
            el.Fill = Brushes.Red;
            size = 4;
        }
        else
        {
            el.Fill = Brushes.Green;
            size = 8;
        }

        el.Width = size;
        el.Height = size;

        var point = (GoldbachInfo)el.Tag;
        Canvas.SetTop(el, MapY(point.sums) - size / 2);
        Canvas.SetLeft(el, MapX(point.n) - size / 2);
    }

    void el_MouseLeave(object sender, MouseEventArgs e)
    {
        SetEllipse((Ellipse)sender);
        (((Ellipse)sender).ToolTip as ToolTip).IsOpen = false;
    }

    void el_MouseEnter(object sender, MouseEventArgs e)
    {
        SetEllipse((Ellipse)sender, false);
        (((Ellipse)sender).ToolTip as ToolTip).IsOpen = true;
        //Canvas.SetZIndex((Ellipse)sender, ++maxindex); // Problem: really slow when many points are present
    }

    public void RepaintGoldbach()
    {
      IList<UIElement> toremove = new List<UIElement>();

      foreach (UIElement element in PaintArea.Children)
      {
        if (element.GetType() == typeof(Ellipse))
        {
            SetEllipse((Ellipse)element);
        }
        else if (element.GetType() == typeof(Line))
        {
          if (((Line)element).Name.StartsWith("YAXSIS"))
              toremove.Add(element);
        }
        else if (element.GetType() == typeof(TextBlock))
        {
          if (((TextBlock)element).Name.StartsWith("YAXSIS"))
              toremove.Add(element);
        }
      }

      foreach (UIElement e in toremove)
        PaintArea.Children.Remove(e);

      // Koordinaten neu zeichnen
      double inc = Math.Max(Range.DoubleValue / 10, 2);

      for (double yk = m_Min.DoubleValue; yk <= m_Max.DoubleValue+0.001; yk += inc)
      {
          string s = ((int)(yk+0.5)).ToString();

          Line l = new Line();
          l.Name = "YAXSIS" + s;

          l.X1 = PADDING_AXISLEFT - 2;
          l.X2 = PADDING_AXISLEFT + 2;

          l.Y1 = l.Y2 = MapY(yk);

          l.Stroke = Brushes.Black;
          l.StrokeThickness = 1.0;
          PaintArea.Children.Add(l);

          TextBlock lbl = new TextBlock();
          lbl.Name = "YAXSISlbl" + s;
          lbl.Text = s;
          PaintArea.Children.Add(lbl);

          lbl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
          Rect measureRect = new Rect(lbl.DesiredSize);
          lbl.Arrange(measureRect);
          Canvas.SetLeft(lbl, PADDING_AXISLEFT - lbl.ActualWidth - 5);
          Canvas.SetTop(lbl, l.Y1 - lbl.ActualHeight / 2);
      }
    }

    public void ClearCoordinateAxis()
    {
      IList<UIElement> toremove = new List<UIElement>();

      foreach (UIElement element in PaintArea.Children)
        if (element.GetType() == typeof(Line) || element.GetType() == typeof(ArrowLine) || element.GetType() == typeof(TextBlock))
          toremove.Add(element);

      foreach (UIElement e in toremove)
        PaintArea.Children.Remove(e);
    }

    public void PaintCoordinateAxis()
    {
      ArrowLine y = new ArrowLine();
      y.X1 = y.X2 = PADDING_AXISLEFT;
      y.Y1 = PaintArea.ActualHeight - PADDING_AXISBOTTOM;
      y.Y2 = PADDING_AXISTOP;
      y.Stroke = Brushes.Black;
      y.StrokeThickness = 1.0;

      ArrowLine x = new ArrowLine();
      x.X1 = PADDING_AXISLEFT;
      x.X2 = PaintArea.ActualWidth - PADDING_AXISRIGHT;
      x.Y1 = x.Y2 = PaintArea.ActualHeight - PADDING_AXISBOTTOM;
      x.Stroke = Brushes.Black;
      x.StrokeThickness = 1.0;

      PaintArea.Children.Add(x);
      PaintArea.Children.Add(y);

      TextBlock lblX = new TextBlock();
      lblX.Text = "n";
      lblX.Name = "AxisLabelX";
      Canvas.SetTop(lblX, x.Y2-8);
      Canvas.SetLeft(lblX, x.X2+8);
      PaintArea.Children.Add(lblX);

      TextBlock lblY = new TextBlock();
      lblY.Text = "y(n)";
      lblY.Name = "AxisLabelY";
      Canvas.SetTop(lblY, y.Y2-8);
      Canvas.SetLeft(lblY, y.X2-25);
      PaintArea.Children.Add(lblY);

      double inc = Math.Max((m_To.DoubleValue - From.DoubleValue) / 10, 2);

      for (double xk = From.DoubleValue; xk <= m_To.DoubleValue+0.001; xk += inc)
      {
          Line l = new Line();

          l.X1 = l.X2 = MapX(xk);
          l.Y1 = x.Y1 - 2;
          l.Y2 = x.Y1 + 2;

          l.Stroke = Brushes.Black;
          l.StrokeThickness = 1.0;
          PaintArea.Children.Add(l);

          TextBlock lbl = new TextBlock();
          lbl.Text = ((int)(xk+0.5)).ToString();
          PaintArea.Children.Add(lbl);

          lbl.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
          Rect measureRect = new Rect(lbl.DesiredSize);
          lbl.Arrange(measureRect);
          Canvas.SetLeft(lbl, l.X1 - lbl.ActualWidth / 2);
          Canvas.SetTop(lbl, x.Y1 + 4);
      }
    }

    #endregion

    #region Calculating

    private PrimesBigInteger Calculate(PrimesBigInteger value, ref string info)
    {
        long x = value.LongValue;

        int count = 0;

        if (x % 2 == 0)
            for (int i = 0; i < PrimeNumbers.primes.Length && PrimeNumbers.primes[i] <= x / 2; i++)
            {
                long p = PrimeNumbers.primes[i];
                if (PrimeNumbers.isprime.Contains(x - p))
                    if (++count <= MAXSUMSINTOOLTIP)
                        info += "\n" + p + " + " + (x - p) + " = " + x;
            }
        
        return count;
    }

    //private PrimesBigInteger Calculate(PrimesBigInteger value)
    //{
    //    BigInteger x = BigInteger.Parse(value.ToString());
    //    if (x % 2 == 1) return PrimesBigInteger.Zero;

    //    int count = 0;
    //    for (long p = 2; p <= x / 2; p = (long)BigIntegerHelper.NextProbablePrime(p + 1))
    //        if (BigIntegerHelper.IsProbablePrime(x - p))
    //            count++;

    //    return PrimesBigInteger.ValueOf(count);
    //}

    //private PrimesBigInteger Calculate(PrimesBigInteger value)
    //{
    //    PrimesBigInteger result = PrimesBigInteger.Zero;
    //    if (value != null && value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero))
    //    {
    //        PrimesBigInteger sum1 = PrimesBigInteger.Two;
    //        while (sum1.CompareTo(value.Divide(PrimesBigInteger.Two)) <= 0)
    //        {
    //            if (value.Subtract(sum1).IsProbablePrime(10))
    //                result = result.Add(PrimesBigInteger.One);
    //            sum1 = sum1.NextProbablePrime();
    //        }
    //    }

    //    return result;
    //}

    #endregion

    #region IPrimeDistribution Members

    public void Init()
    {
        m_Initialized = true;
        ircGoldbach.IntervalSizeCanBeZero = true;
        ircGoldbach.SetButtonCancelButtonEnabled(false);
        InitInput();
    }

    private void InitInput()
    {
      ircGoldbach.Execute += new Primes.WpfControls.Components.ExecuteDelegate(ircGoldbach_Execute);
      ircGoldbach.Cancel += new VoidDelegate(ircGoldbach_Cancel);

      ircGoldbach.SetText(InputRangeControl.FreeFrom, m_From.ToString());
      ircGoldbach.SetText(InputRangeControl.FreeTo, m_To.ToString());

      InputValidator<PrimesBigInteger> ivFrom = new InputValidator<PrimesBigInteger>();
      ivFrom.Validator = new BigIntegerMinValueMaxValueValidator(null, 2, 100000);
      ircGoldbach.AddInputValidator(InputRangeControl.FreeFrom, ivFrom);
      InputValidator<PrimesBigInteger> ivTo = new InputValidator<PrimesBigInteger>();
      ivTo.Validator = new BigIntegerMinValueMaxValueValidator(null, 2, 100000);
      ircGoldbach.AddInputValidator(InputRangeControl.FreeTo, ivTo);
    }

    public void Dispose()
    {
    }

    #endregion

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);

      if (m_Initialized)
      {
        PaintArea.Width = gridContent.ActualWidth;
        PaintArea.Height = gridContent.ActualHeight - ircGoldbach.ActualHeight - spSlider.ActualHeight;
      }
    }

    private void PaintArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (m_From != null && m_To != null)
      {
        ClearCoordinateAxis();
        PaintCoordinateAxis();
        RepaintGoldbach();
      }
    }

  }
}
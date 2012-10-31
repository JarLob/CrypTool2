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
using Primes.Library;
using Primes.Library.Function;
using System.Threading;
using Primes.WpfControls.Threads;
using Primes.WpfControls.Components;
using Primes.Bignum;

namespace Primes.WpfControls.PrimesDistribution.Graph
{
  public enum FunctionType { CONSTANT, STAIR }
  /// <summary>
  /// Interaction logic for CartesianPlane.xaml
  /// </summary>
  /// 
  public delegate void AddChild(UIElement element);
  public delegate BoolWrapper DrawLine(LineParameters lineparams);
  public delegate void FunctionEvent(IFunction function);
  
  public partial class CartesianPlane : System.Windows.Controls.UserControl
  {
      //private int paddingleft = 50;
      //private int paddingbottom = 50;
      private int paddingleft = 0;
      private int paddingbottom = 0;
    private int m_Padding = 0;
    double m_UnitSizeX = 0.0;
    double m_UnitSizeY = 0.0;
    private IList<UIElement> m_CooardinateAxisElements;
    private IDictionary<Line, IFunction> m_FunctionLines;


    private FunctionEvent m_OnFunctionStart;

    public FunctionEvent OnFunctionStart
    {
      get { return m_OnFunctionStart; }
      set { m_OnFunctionStart = value; }
    }
    private FunctionEvent m_OnFunctionStop;

    public FunctionEvent OnFunctionStop
    {
      get { return m_OnFunctionStop; }
      set { m_OnFunctionStop = value; }
    }

    private AddChild ac;
    private DrawLine dl;
    private object m_AddChildLockObject;
    private IDictionary<IFunction, SuspendableThread> m_Threads = null;

    private Range m_RangeX;

    public Range RangeX
    {
      get { return m_RangeX; }
      set { m_RangeX = value; }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      ScaleFunction();
      RefreshCoordinateAxis();
      //ExecuteFunctions();
    }

    private void ScaleFunction()
    {
      if (!double.IsNaN(this.Width) && !double.IsNaN(this.Height) && RangeX != null && RangeY != null && RangeX.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0 && RangeY.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0)
      {

        double xSize = (this.Width - paddingleft) / RangeX.RangeAmount.DoubleValue;
        double ySize = (this.Height - paddingbottom) / RangeY.RangeAmount.DoubleValue;

        foreach (UIElement element in this.PaintArea.Children)
        {
          if (element.GetType() == typeof(Line))
          {
            if (element != Abscissa && element != Ordinate)
            {
              Line line = element as Line;
              double x1 = (line.X1 - paddingleft) / m_UnitSizeX;
              double x2 = (line.X2 - paddingleft) / m_UnitSizeX;
              double y1 = (line.Y1) / m_UnitSizeY;
              double y2 = (line.Y2) / m_UnitSizeY;

              line.X1 = (x1* xSize) + paddingleft;
              line.X2 = (x2 * xSize) + paddingleft;
              line.Y1 = (y1 * ySize) ;
              line.Y2 = (y2 * ySize) ;
              //if (l.Y1 > Ordinate.Y2) l.Y1 = Ordinate.Y2;
              //if (l.Y2 > Ordinate.Y2) l.Y2 = Ordinate.Y2;

            }
          }
        }
      }

    }
    //private void ScaleFunction()
    //{
    //  if (!double.IsNaN(this.Width) && !double.IsNaN(this.Height) && RangeX != null && RangeY != null && RangeX.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0 && RangeY.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0)
    //  {
    //    double xSize = ((this.Width) / double.Parse(RangeX.RangeAmount.ToString()));
    //    double ySize = this.Height / double.Parse(RangeY.RangeAmount.ToString());

    //    foreach (UIElement element in this.PaintArea.Children)
    //    {
    //      if (element.GetType() == typeof(Line))
    //      {
    //        if (element != Abscissa && element != Ordinate)
    //        {
    //          Line line = element as Line;

    //          double x1 = (line.X1 - paddingleft ) / m_UnitSizeX;
    //          double x2 = (line.X2 - paddingleft) / m_UnitSizeX;
    //          double y1 = ((line.Y1 - (double.Parse(RangeY.GetZeroPosition().ToString()) * m_UnitSizeY)) / m_UnitSizeY) + double.Parse(this.RangeY.From.ToString()); ;
    //          double y2 = ((line.Y2 - (double.Parse(RangeY.GetZeroPosition().ToString()) * m_UnitSizeY)) / m_UnitSizeY) + double.Parse(this.RangeY.From.ToString()); ;

    //          line.X1 = (x1 * xSize) + m_Padding;
    //          line.X2 = (x2 * xSize) + m_Padding;
    //          line.Y1 = (((y1 - double.Parse(this.RangeY.From.ToString())) * ySize) ) + (double.Parse(RangeY.GetZeroPosition().ToString()) * ySize);
    //          line.Y2 = (((y2 - double.Parse(this.RangeY.From.ToString())) * ySize) ) + (double.Parse(RangeY.GetZeroPosition().ToString()) * ySize);
    //          //if (l.Y1 > Ordinate.Y2) l.Y1 = Ordinate.Y2;
    //          //if (l.Y2 > Ordinate.Y2) l.Y2 = Ordinate.Y2;

    //        }
    //      }
    //    }
    //  }
    
    //}
    private Range m_RangeY;

    public Range RangeY
    {
      get { return m_RangeY; }
      set { m_RangeY = value; m_Padding = m_RangeY.To.ToString().Length * 10; }
    }

    private IList<FunctionExecute> m_FunctionExecutes;

    public CartesianPlane()
    {
      InitializeComponent();
      m_FunctionExecutes = new List<FunctionExecute>();
      ac = new AddChild(this.AddChild);
      m_AddChildLockObject = new object();
      m_Threads = new Dictionary<IFunction, SuspendableThread >();
      dl = new DrawLine(this.DrawLine);
      this.m_CooardinateAxisElements =  new List<UIElement>();
      m_FunctionLines = new Dictionary<Line, IFunction>();

    }

    public void ClearPaintArea()
    {
      while(this.PaintArea.Children.Count>0)
        this.PaintArea.Children.Clear();

      AddLabel(lblAbscissaText);
      AddLabel(lblOrdinateText);
      AddLine(Ordinate);
      AddLine(Abscissa);

      if (RangeX == null && RangeY == null)
      {
        lblAbscissaText.Visibility = Visibility.Hidden;
        lblOrdinateText.Visibility = Visibility.Hidden;
        Ordinate.Visibility = Visibility.Hidden;
        Abscissa.Visibility = Visibility.Hidden;
      }
    }

    public void ClearFunctions()
    {
      this.m_FunctionExecutes.Clear();
    }

    public void AddFunctionExecute(IFunction func, PrimesBigInteger from, PrimesBigInteger to, FunctionType type, Brush color)
    {
      if (!ContainsFunction(func))
      {
        FunctionExecute fe = new FunctionExecute();
        fe.Function = func;
        fe.Range.From = from;
        fe.Range.To = to;
        fe.FunctionType = type;
        fe.Color = color;
        this.m_FunctionExecutes.Add(fe);
      }
    }

    private bool ContainsFunction(IFunction function)
    {
      foreach (FunctionExecute fe in this.m_FunctionExecutes)
      {
        if (fe.Function.GetType() == function.GetType())
          return true;
      }

      return false;
    }

    public void ExecuteFunctions()
    {
      paddingleft = StringFormat.FormatDoubleToIntString(RangeY.To.DoubleValue).Length*10;
      this.ClearPaintArea();
      RefreshCoordinateAxis();
      ClearThreads();
      //LineParameters lp = new LineParameters(2, 3, 1, 2, Brushes.Red, new FunctionLiN());
      //DrawLine(lp);
      foreach (FunctionExecute fe in this.m_FunctionExecutes)
      {
        fe.Function.Reset();
        //FunctionThread t = new FunctionThread(fe, dl, GetXStart(double.Parse(fe.Range.From.ToString())), this.Dispatcher);
        FunctionThread t = new FunctionThread(fe, dl, fe.Range.From.DoubleValue, this.Dispatcher);
        if (this.OnFunctionStart != null)
          t.OnFunctionStart += this.OnFunctionStart;
        if (this.OnFunctionStop != null)
          t.OnFunctionStop += this.OnFunctionStop;
        t.Start();
        m_Threads.Add(fe.Function, t);
      }
    }

    private void ExecuteFunktion(object fe_)
    {
    }

    private BoolWrapper DrawLine(LineParameters lineparams)
    {
      BoolWrapper result = new BoolWrapper();
      result.BoolValue = true;
      double x1 = ((lineparams.X1 - RangeX.From.DoubleValue) * m_UnitSizeX) + paddingleft;
      double x2 = ((lineparams.X2 - RangeX.From.DoubleValue) * m_UnitSizeX) + paddingleft;
      double y1 = Ordinate.Y1 - ((lineparams.Y1-RangeY.From.DoubleValue) * m_UnitSizeY);
      double y2 = Ordinate.Y1 - ((lineparams.Y2 - RangeY.From.DoubleValue) * m_UnitSizeY);
      Line l = new Line();
      l.X1 = x1;
      l.X2 = x2;
      l.Y1 = y1;
      l.Y2 = y2;
      l.Stroke = lineparams.Color;
      l.StrokeThickness = 1;
      m_FunctionLines.Add(l,lineparams.Function);
      AddLine(l);

      return result;

    }

    //private BoolWrapper DrawLine(LineParameters lineparams)
    //{
    //  BoolWrapper result = new BoolWrapper();
    //  result.BoolValue = true;
    //  if (lineparams.Y1 > 0)
    //  {
    //    Line l = new Line();
    //    l.X1 = (lineparams.X1 * m_UnitSizeX) + m_Padding;
    //    l.X2 = (lineparams.X2 * m_UnitSizeX) + m_Padding;
    //    l.Y1 = (((lineparams.Y1 - double.Parse(this.RangeY.From.ToString())) * m_UnitSizeY) * -1) + (double.Parse(RangeY.GetZeroPosition().ToString()) * m_UnitSizeY);

    //    l.Y2 = (((lineparams.Y2 - double.Parse(this.RangeY.From.ToString())) * m_UnitSizeY) * -1) + (double.Parse(RangeY.GetZeroPosition().ToString()) * m_UnitSizeY);
    //    if (l.Y1 > Ordinate.Y2) l.Y1 = Ordinate.Y2;
    //    if (l.Y2 > Ordinate.Y2) l.Y2 = Ordinate.Y2;
    //    //if (l.X2 >= this.Abscissa.X2)
    //    //{

    //    //  result.BoolValue = false;
    //    //  return result;
    //    //}
    //    l.Stroke = lineparams.Color;
    //    l.StrokeThickness = 1;
    //    l.MouseLeftButtonDown += new MouseButtonEventHandler(LineMouseMove);
    //    m_FunctionLines.Add(l,lineparams.Function);
    //    AddLine(l);
    //  }
    //  return result;

    //}


    private void AddLine(Line l)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, ac, l);
    }

    private void AddLabel(TextBlock l)
    {
      this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, ac, l);
    }

    private void AddChild(UIElement element)
    {
      lock (m_AddChildLockObject)
      {
        this.PaintArea.Children.Add(element);
      }
    }

    //private double GetXStart(double from)
    //{
    //  double rangexfrom = double.Parse(this.RangeX.From.ToString());
    //  return Math.Abs(from) - Math.Abs(rangexfrom);
    //}

    private double GetX()
    {
      double unit = RangeX.RangeAmount.DoubleValue / 10.0;
      double log = (int)Math.Log10(unit);
      double firstPower = Math.Pow(10, log);
      if ( (5*unit <= firstPower) && (log > 0) ) log--;
      double first = double.Parse(unit.ToString().Substring(0, 1));
      return first * Math.Pow(10, log);
    }

    private double GetY()
    {
      double unit = RangeY.RangeAmount.DoubleValue / 10.0;
      double log = (int)Math.Log10(unit);
      double first = double.Parse(unit.ToString().Substring(0, 1));
      if (first <= 0) first = 1;
      return first * Math.Pow(10, log);
    }

    public void RefreshCoordinateAxis()
    {
      if (RangeX != null && RangeY != null)
      {
        foreach (UIElement element in m_CooardinateAxisElements)
          PaintArea.Children.Remove(element);

        m_UnitSizeX = (this.ActualWidth - paddingleft) / RangeX.RangeAmount.DoubleValue;
        m_UnitSizeY = (this.ActualHeight - paddingbottom) / RangeY.RangeAmount.DoubleValue;
        
        // draw Ordinate
        Ordinate.Y1 = this.ActualHeight - paddingbottom;
        Ordinate.Y2 = 0;
        Ordinate.X1 = Ordinate.X2 = paddingleft;
        Ordinate.Visibility = Visibility.Visible;

        // draw Abscissa
        Abscissa.Y1 = Abscissa.Y2 = Ordinate.Y1;
        Abscissa.X1 = paddingleft;
        Abscissa.X2 = this.ActualWidth;
        Abscissa.Visibility = Visibility.Visible;
        
        //// Name Y-Axis
        //Canvas.SetTop(lblOrdinateText,0);
        //Canvas.SetLeft(lblOrdinateText, Ordinate.X1+8);
        //lblOrdinateText.Visibility = Visibility.Visible;

        //// Name X-Axis
        //Canvas.SetTop(lblAbscissaText, Abscissa.Y1-16);
        //Canvas.SetLeft(lblAbscissaText, Abscissa.X2);
        //lblAbscissaText.Visibility = Visibility.Visible;

        // Arrows
        //AddLine(CreateLine(Ordinate.X1, Ordinate.X1 - 5, Ordinate.Y2, Ordinate.Y2 + 5));
        //AddLine(CreateLine(Ordinate.X1, Ordinate.X1 + 5, Ordinate.Y2, Ordinate.Y2 + 5));

        //AddLine(CreateLine(Abscissa.X2, Abscissa.X2 - 5, Abscissa.Y1, Abscissa.Y1 - 5));
        //AddLine(CreateLine(Abscissa.X2, Abscissa.X2 - 5, Abscissa.Y1, Abscissa.Y1 + 5));

        // Label Axis
        DrawAxisLabels();

        //AddLine(CreateLine(0, this.ActualWidth, 0, this.ActualHeight));
      }

    }

    public void DrawAxisLabels()
    {
      double startX = GetX();
      // X-Koordinaten-Beschriftung
      for (int i = 1; i < 100; i++)
      {
        double x = (((startX * i) - RangeX.From.DoubleValue) * m_UnitSizeX) + paddingleft;
        if (x >= Abscissa.X1)
        {
          // draw a horizontal Line
          if (x >= Abscissa.X2) break;
          Line l = CreateLine(x, x, Ordinate.Y1 - 3, Ordinate.Y1 + 3);
          AddLine(l);

          // Draw the Text
          TextBlock tb = CreateLabel(x,Ordinate.Y1 + 5,StringFormat.FormatDoubleToIntString(startX*i));
          AddLabel(tb);
          tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
          Rect measureRect = new Rect(tb.DesiredSize);
          tb.Arrange(measureRect);
          Canvas.SetLeft(tb, Canvas.GetLeft(tb) - tb.ActualWidth/2);
        }

      }

      double startY = GetY();
      // Y-Koordindate-Beschriftung
      for (int i = 1; i < 100; i++)
      {
        double diff = (((startY * i) - RangeY.From.DoubleValue) * m_UnitSizeY);
        if (diff >= 0)
        {
          // Draw a vertical Line
          double y = Ordinate.Y1 - diff;
          if (y <= Ordinate.Y2) break;
          Line l = CreateLine(Abscissa.X1 - 3, Abscissa.X1 + 3, y, y);
          AddLine(l);

          // Draw the Text
          TextBlock tb = CreateLabel(0, y, StringFormat.FormatDoubleToIntString(startY * i));
          AddLabel(tb);
          tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
          Rect measureRect = new Rect(tb.DesiredSize);
          tb.Arrange(measureRect);
          Canvas.SetTop(tb, Canvas.GetTop(tb) - tb.ActualHeight / 2);
        }

      }

    }
    //public void RefreshCoordinateAxis()
    //{
    //  if (!double.IsNaN(this.Width) && !double.IsNaN(this.Height) && RangeX != null && RangeY != null && RangeX.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0 && RangeY.RangeAmount.CompareTo(PrimesBigInteger.Zero) > 0)
    //  {
    //    foreach (UIElement element in m_CooardinateAxisElements)
    //    {
    //      PaintArea.Children.Remove(element);
    //    }
    //    m_CooardinateAxisElements.Clear();
    //    lblAbscissaText.Visibility = Visibility.Visible;
    //    lblOrdinateText.Visibility = Visibility.Visible;
    //    Ordinate.Visibility = Visibility.Visible;
    //    Abscissa.Visibility = Visibility.Visible;

    //    m_UnitSizeX = ((this.ActualWidth) / double.Parse(RangeX.RangeAmount.ToString()));
    //    m_UnitSizeY = this.Height / double.Parse(RangeY.RangeAmount.ToString());
    //    Abscissa.Y1 = Abscissa.Y2 = double.Parse(RangeY.GetZeroPosition().ToString()) * m_UnitSizeY;
    //    Abscissa.X1 = m_Padding;
    //    Abscissa.X2 = (double.Parse(RangeX.RangeAmount.ToString()) * m_UnitSizeX) - m_Padding;
    //    double usx = Abscissa.ActualWidth / 10.0;


    //    double test = GetX();
    //    for (int i = 0; i < 20; i++)
    //    {

    //      double x = m_Padding;
    //      if (i > 0)
    //      {
    //        x = i*test * m_UnitSizeX;
    //       //x = (i * ((Abscissa.X2-Abscissa.X1) / 10)) + m_Padding;
    //      }
    //      if (x > (double.Parse(RangeX.RangeAmount.ToString()) * m_UnitSizeX) - m_Padding) break;

    //      Line l = CreateLine(x, x, Abscissa.Y2 - 2, Abscissa.Y2 + 3); 
    //      AddLine(l);

    //      Label lbl = new Label();

    //      double lblValue = i * test;//Math.Floor( (double.Parse(RangeX.From.ToString()) + (double)i * ((double.Parse(RangeX.RangeAmount.ToString())) / 10)));
    //      if (!lblValue.Equals(0.0) && !lblValue.Equals(RangeX.To) && !lblValue.Equals(double.Parse(RangeX.From.ToString())))
    //      {
    //        lbl.Content = (lblValue).ToString();
    //        Canvas.SetTop(lbl, l.Y2);
    //        Canvas.SetLeft(lbl, l.X1 - 10);
    //        AddLabel(lbl);
    //        m_CooardinateAxisElements.Add(lbl);
    //      }

    //    }

    //    Ordinate.X1 = Ordinate.X2 = (double.Parse(RangeX.GetZeroPosition().ToString()) * m_UnitSizeX) + m_Padding;
    //    Ordinate.Y1 = m_Padding;
    //    Ordinate.Y2 = (double.Parse(RangeY.RangeAmount.ToString()) * m_UnitSizeY);

    //    for (int i = 0; i < 10; i++)
    //    {
    //      double y = (i * ((Ordinate.Y2-Ordinate.Y1) / 10)) + m_Padding;
    //      if (y == Ordinate.Y1) continue;

    //      Line l = CreateLine(Ordinate.X2 - 2, Ordinate.X2 + 3, y, y);
    //      AddLine(l);

    //      Label lbl = new Label();
    //      double lblValue = Math.Floor(double.Parse(RangeY.To.ToString()) - (double)i * ((double.Parse(RangeY.RangeAmount.ToString())) / 10));
    //      if (!lblValue.Equals(0.0) &&  !lblValue.Equals(double.Parse(RangeY.To.ToString())))
    //      {
    //        lbl.Content = (lblValue).ToString();
    //        Canvas.SetTop(lbl, l.Y2 - 10);
    //        Canvas.SetLeft(lbl, 0);
    //        AddLabel(lbl);
    //        m_CooardinateAxisElements.Add(lbl);

    //      }

    //    }

    //    /*
    //     * Draw Line-Ends as Arrowheads
    //     */
    //    AddLine(CreateLine(Ordinate.X1,Ordinate.X1 - 6,Ordinate.Y1,Ordinate.Y1+7));
    //    AddLine(CreateLine(Ordinate.X1, Ordinate.X1 + 6, Ordinate.Y1, Ordinate.Y1 + 7));

    //    AddLine(CreateLine(Abscissa.X2, Abscissa.X2 - 7, Abscissa.Y1, Abscissa.Y1 - 7));
    //    AddLine(CreateLine(Abscissa.X2, Abscissa.X2 - 7, Abscissa.Y1, Abscissa.Y1 + 7));

    //    Canvas.SetTop(lblAbscissaText, Abscissa.Y2);
    //    Canvas.SetLeft(lblAbscissaText, Abscissa.X2 - 10);

    //    Canvas.SetTop(lblOrdinateText, Ordinate.Y1 - m_Padding);
    //    Canvas.SetLeft(lblOrdinateText, Ordinate.X1 - m_Padding);
    //  }

    //}

    private Line CreateLine(double x1, double x2, double y1, double y2)
    {
      Line result = new Line();
      result.Y1 = y1;
      result.Y2 = y2;
      result.X1 = x1;
      result.X2 = x2;
      result.Stroke = Brushes.Black;
      result.StrokeThickness = 1;
      m_CooardinateAxisElements.Add(result);

      return result;
    }

    private TextBlock CreateLabel(double x, double y, string value)
    {
      TextBlock result = new TextBlock();
      result.Text = value;
      Canvas.SetTop(result, y);
      Canvas.SetLeft(result, x);

      m_CooardinateAxisElements.Add(result);

      return result;
    }

    public void CleanUp()
    {
      ClearThreads();
    }

    private void ClearThreads()
    {
      foreach (SuspendableThread t in this.m_Threads.Values)
      {
        try { t.Terminate(); }
        catch { }
      }
      this.m_Threads.Clear();

    }

    public void StopFunction(IFunction function)
    {
      if (function != null)
      {
        if (this.m_Threads.ContainsKey(function))
        {
          SuspendableThread t = this.m_Threads[function];
          t.Suspend();
        }
      }
    }

    public void CancelFunction(IFunction function)
    {
      if (function != null)
      {
        if (this.m_Threads.ContainsKey(function))
        {
          SuspendableThread t = this.m_Threads[function];
          function.FunctionState = FunctionState.Stopped;
          t.Thread.Abort();
          if (this.OnFunctionStop != null)
            this.OnFunctionStop(function);
        }
      }
    }

    public void ResumeFunction(IFunction function)
    {
      if (function != null)
      {
        if (this.m_Threads.ContainsKey(function))
        {
          SuspendableThread t = this.m_Threads[function];
          t.Resume();
        }
      }
    }   
  }

  public class FunctionExecute
  {
    FunctionType m_FunctionType;

    public FunctionType FunctionType
    {
      get { return m_FunctionType; }
      set { m_FunctionType = value; }
    }
    public FunctionExecute()
    {
      m_Range = new RangeX(PrimesBigInteger.Zero, PrimesBigInteger.Zero);
    }
    private IFunction m_Function;
    public IFunction Function
    {
      get { return this.m_Function; }
      set { this.m_Function = value; }
    }

    private Range m_Range = null;

    public Range Range
    {
      get { return m_Range; }
      set { m_Range = value; }
    }

    private Brush m_Color;

    public Brush Color
    {
      get { return m_Color; }
      set { m_Color = value; }
    }
  }

  public class LineParameters
  {
    #region Properties
    private double m_X1;

    public double X1
    {
      get { return m_X1; }
      set { m_X1 = value; }
    }
    private double m_X2;

    public double X2
    {
      get { return m_X2; }
      set { m_X2 = value; }
    }
    private double m_Y1;

    public double Y1
    {
      get { return m_Y1; }
      set { m_Y1 = value; }
    }
    private double m_Y2;

    public double Y2
    {
      get { return m_Y2; }
      set { m_Y2 = value; }
    }

    private Brush m_Color;

    public Brush Color
    {
      get { return m_Color; }
      set { m_Color = value; }
    }

    private IFunction m_Function;

    public IFunction Function
    {
      get { return m_Function; }
      set { m_Function = value; }
    }
    #endregion

    #region Constructors
    public LineParameters(double x1, double x2, double y1, double y2, Brush color, IFunction function)
    {
      X1 = x1;
      X2 = x2;
      Y1 = y1;
      Y2 = y2;
      Color = color;
      Function = function;

    }
    #endregion

  }
  public class BoolWrapper
  {
    private bool m_BoolValue;

    public bool BoolValue
    {
      get { return m_BoolValue; }
      set { m_BoolValue = value; }
    }
  }

}

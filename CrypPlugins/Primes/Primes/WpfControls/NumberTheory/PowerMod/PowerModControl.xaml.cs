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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Primes.Library;
using Primes.WpfControls.Validation.Validator;
using Primes.WpfControls.Validation;
using Primes.Bignum;
using System.Threading;
using Primes.WpfControls.Components;
using Primes.WpfControls.Components.Arrows;
using System.Diagnostics;
using System.Globalization;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
    /// <summary>
    /// Interaction logic for TestOfFermatControl.xaml
    /// </summary>
    public partial class PowerModControl : UserControl
    {
        #region initializing

        private Thread m_Thread;
        private bool m_Initialized;
        private bool m_Resume = false;

        private const double Margin = 25.0;
        private const double PointRadius = 3.0;

        public PowerModControl()
        {
            InitializeComponent();
            initBindings();
            ConfigureIntegerInputs();
            m_Points = new Dictionary<int, Point>();
            //m_Ellipses = new Dictionary<int, Ellipse>();
            m_SourceDestination = new List<Pair<Ellipse, Ellipse>>();
            m_CirclesSource = new Dictionary<Ellipse, Polyline>();
            m_ArrowsWithSourceAndDestination = new Dictionary<Pair<Ellipse, Ellipse>, ArrowLine>();
            //m_Arrows = new List<ArrowLine>();
            m_ArrowsMark = new Dictionary<PrimesBigInteger, ArrowLine>();
            //m_Circles = new List<Polyline>();
            m_CirclesMark = new Dictionary<PrimesBigInteger, Polyline>();
            m_RunningLockObject = new object();
            m_Initialized = true;
            m_StepWiseEvent = new ManualResetEvent(false);
            log.RowMouseOver += new ExecuteIntegerDelegate(log_RowMouseOver);
        }

        void log_RowMouseOver(PrimesBigInteger value)
        {
            MarkPath(value);
        }

        public void MarkPath(PrimesBigInteger iteration)
        {
            if (m_ArrowsMark.TryGetValue(iteration, out ArrowLine targetArrowLine))
            {
                var beforeTarget = true;
                foreach (ArrowLine alTmp in m_ArrowsWithSourceAndDestination.Values)
                {
                    if (alTmp != targetArrowLine)
                    {
                        alTmp.Stroke = beforeTarget ? Brushes.Black : Brushes.LightGray;
                    }
                    else
                    {
                        beforeTarget = false;
                        targetArrowLine.Stroke = Brushes.Red;
                    }
                }
            }
            else if (m_CirclesMark.TryGetValue(iteration, out Polyline pl))
            {
                foreach (Polyline plTmp in m_CirclesSource.Values)
                {
                    if (pl != plTmp)
                    {
                        plTmp.Stroke = Brushes.Black;
                    }
                }
                pl.Stroke = Brushes.Red;
            }
        }

        private void ConfigureIntegerInputs()
        {
            iscBase.Execute += new ExecuteSingleDelegate(iscBase_Execute);
            iscBase.SetText(InputSingleControl.Free, "2");
            InputValidator<PrimesBigInteger> ivBase = new InputValidator<PrimesBigInteger>();
            ivBase.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Two);
            iscBase.AddInputValidator(
              Primes.WpfControls.Components.InputSingleControl.Free,
              ivBase);

            iscExp.Execute += new ExecuteSingleDelegate(iscExp_Execute);
            iscExp.SetText(InputSingleControl.Free, "28");
            InputValidator<PrimesBigInteger> ivExp = new InputValidator<PrimesBigInteger>();
            ivExp.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One);
            iscExp.AddInputValidator(
              Primes.WpfControls.Components.InputSingleControl.Free,
              ivExp);

            iscMod.Execute += new ExecuteSingleDelegate(iscMod_Execute);
            iscMod.KeyDown += new ExecuteSingleDelegate(iscMod_KeyDown);
            iscMod.SetText(InputSingleControl.Free, "13");
            InputValidator<PrimesBigInteger> ivMod = new InputValidator<PrimesBigInteger>();
            ivMod.Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Two, PrimesBigInteger.ValueOf(150));
            iscMod.AddInputValidator(
              Primes.WpfControls.Components.InputSingleControl.Free,
              ivMod);

            this.Start += new VoidDelegate(PowerModControl_Start);
            this.Stop += new VoidDelegate(PowerModControl_Stop);
            this.Cancel += new VoidDelegate(PowerModControl_Cancel);
            this.Exp = PrimesBigInteger.ValueOf(28);
            this.Base = PrimesBigInteger.ValueOf(2);
            this.Mod = PrimesBigInteger.ValueOf(13);
        }

        void iscMod_KeyDown(PrimesBigInteger value)
        {
            if (value != null)
                this.Mod = value;
        }

        void iscMod_Execute(PrimesBigInteger value)
        {
            this.Mod = value;
            StartThread();
        }

        private void slidermodulus_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Mod = PrimesBigInteger.ValueOf((long)e.NewValue);
        }

        void iscExp_Execute(PrimesBigInteger value)
        {
            this.Exp = value;
            StartThread();
        }

        void iscBase_Execute(PrimesBigInteger value)
        {
            this.Base = value;
            StartThread();
        }

        #endregion

        #region Properties

        private object m_RunningLockObject;
        private bool m_Running;
        private IDictionary<int, Point> m_Points;
        //private IDictionary<int, Ellipse> m_Ellipses;
        private IList<Pair<Ellipse, Ellipse>> m_SourceDestination;
        //private IList<Polyline> m_Circles;
        private IDictionary<Ellipse, Polyline> m_CirclesSource;
        private IDictionary<PrimesBigInteger, Polyline> m_CirclesMark;
        //private IList<ArrowLine> m_Arrows;
        private IDictionary<Pair<Ellipse, Ellipse>, ArrowLine> m_ArrowsWithSourceAndDestination;
        private IDictionary<PrimesBigInteger, ArrowLine> m_ArrowsMark;

        double offset = 0;

        private PrimesBigInteger m_Base;

        public PrimesBigInteger Base
        {
            get { return m_Base; }
            set { m_Base = value; }
        }

        private PrimesBigInteger m_Exp;

        public PrimesBigInteger Exp
        {
            get { return m_Exp; }
            set { m_Exp = value; }
        }

        private PrimesBigInteger m_Mod;

        public PrimesBigInteger Mod
        {
            get { return m_Mod; }
            set
            {
                m_Mod = value;
                if (m_Mod != null)
                {
                    if (m_Mod.Equals(iscMod.GetValue()))
                    {
                        slidermodulus.Value = m_Mod.DoubleValue;
                    }
                    else
                    {
                        iscMod.SetText(InputSingleControl.Free, m_Mod.ToString());
                    }
                    if (m_Initialized)
                    {
                        Reset();
                        CreatePoints();
                    }
                }
            }
        }

        #endregion

        #region painting the points

        private void Reset()
        {
            m_Points.Clear();
            //m_Ellipses.Clear();
            m_SourceDestination.Clear();
            m_CirclesSource.Clear();
            m_ArrowsWithSourceAndDestination.Clear();
            //m_Arrows.Clear();
            m_ArrowsMark.Clear();
            //m_Circles.Clear();
            m_CirclesMark.Clear();
            ControlHandler.ClearChildren(PaintArea);
            ControlHandler.ClearChildren(ArrowArea);
            ControlHandler.ClearChildren(LabelArea);
        }

        public void GetCoords(int value, out double x, out double y)
        {
            double angle = 2 * Math.PI * (value / (double)m_Mod.IntValue + offset / 360.0) * (m_SortAsc ? 1 : -1);
            x = (Math.Sin(angle) + 1) * Radius + Margin;
            y = (Math.Cos(angle) + 1) * Radius + Margin;
        }

        private void CreatePoint(int value)
        {
            double x, y;
            GetCoords(value, out x, out y);
            y -= PointRadius;
            x -= PointRadius;

            Ellipse point = m_Points.ContainsKey(value) ? GetEllipseAt(m_Points[value]) : null;

            if (point == null)
            {
                point = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
                point.ToolTip = new ToolTip();
                (point.ToolTip as ToolTip).Content = value.ToString();
                ControlHandler.SetPropertyValue(point, "Width", PointRadius*2);
                ControlHandler.SetPropertyValue(point, "Height", PointRadius*2);
                ControlHandler.SetPropertyValue(point, "Fill", Brushes.Black);
                ControlHandler.SetPropertyValue(point, "Stroke", Brushes.Black);
                ControlHandler.SetPropertyValue(point, "StrokeThickness", 1.0);
                ControlHandler.AddChild(point, PaintArea);
            }
            
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { point, x });
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { point, y });

            if (m_CirclesSource.ContainsKey(point))
                MoveCircle(point);

            if (m_Points.ContainsKey(value))
                m_Points[value] = new Point(x, y);
            else
                m_Points.Add(value, new Point(x, y));

            Label lbl = ControlHandler.CreateLabel(value.ToString(), null);
            double _left = x - 5;
            double _top = y;
            _left += (x < Radius) ? -7 + (-2 * (int)Math.Log(value)) : 2;
            _top += (y < Radius) ? -20 : 0;
            ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { lbl, _left });
            ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { lbl, _top });
            ControlHandler.AddChild(lbl, LabelArea);
        }

        #endregion

        #region Painting the Ellipse

        private void Paint()
        {
            Ellipse el = ControlHandler.CreateObject(typeof(Ellipse)) as Ellipse;
            ControlHandler.SetPropertyValue(el, "Width", Aperture);
            ControlHandler.SetPropertyValue(el, "Height", Aperture);
            ControlHandler.ExecuteMethod(CircleArea, "SetTop", new object[] { el, Margin });
            ControlHandler.ExecuteMethod(CircleArea, "SetLeft", new object[] { el, Margin });
            ControlHandler.SetPropertyValue(el, "StrokeThickness", 1.0);
            ControlHandler.SetPropertyValue(el, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(el, "Name", "dontremove");
            ControlHandler.AddChild(el, CircleArea);
        }

        private double Aperture
        {
            get { return Math.Max(Math.Min(PaintArea.Width, PaintArea.Height) - 2*Margin, 0); }
        }

        private double _radius = -1;
        private double Radius
        {
            get
            {
                return Aperture / 2.0;
            }
            set { _radius = value; }
        }

        private double Perimeter
        {
            get { return (2 * Math.PI * Aperture); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            try
            {
                if (m_Initialized)
                {
                    //ClearArrows();
                    log.RenderSize = sizeInfo.NewSize;
                    ArrowArea.Width = PaintArea.Width = CircleArea.Width = LabelArea.Width = ContentArea.Width = Math.Max(0, PaintPanel.ActualWidth - 20);
                    ArrowArea.Height = PaintArea.Height = CircleArea.Height = ContentArea.Height = LabelArea.Height = Math.Max(0, PaintPanel.ActualHeight - spslider.ActualHeight - 30);
                    //ContentArea.Width = PaintPanel.ActualWidth;

                    //ContentArea.Height = PaintPanel.ActualHeight - spslider.ActualHeight;

                    //if (log.ActualWidth < 100)
                    //{
                    //  log.Width = this.ActualWidth - 5;
                    //}
                    if (sizeInfo.NewSize.Height != double.NaN && sizeInfo.NewSize.Height > 0 &&
                      sizeInfo.NewSize.Width != double.NaN && sizeInfo.NewSize.Width > 0)
                    {
                        if (!m_Running)
                        {
                            if (CircleArea.Children.Count != 0)
                                CircleArea.Children.Clear();
                            Paint();
                            if (this.m_Mod != null)
                            {
                                //IDictionary<ArrowLine, int> m_StartPoints = new Dictionary<ArrowLine, int>();
                                //IDictionary<ArrowLine, int> m_EndPoints = new Dictionary<ArrowLine, int>();

                                //foreach (ArrowLine al in m_Arrows)
                                //{
                                //    Point from = new Point(al.X1, al.Y1);
                                //    Point to = new Point(al.X2, al.Y2);
                                //    foreach (int key in m_Points.Keys)
                                //    {
                                //        Point p = m_Points[key];
                                //        if (from.X == p.X && from.Y == p.Y)
                                //        {
                                //            m_StartPoints.Add(al, key);
                                //        }
                                //        else if (to.X == p.X && to.Y == p.Y)
                                //        {
                                //            m_EndPoints.Add(al, key);
                                //        }
                                //    }
                                //}
                                CreatePoints();
                                foreach (Ellipse e in m_CirclesSource.Keys)
                                {
                                    MoveCircle(e);
                                }
                                //foreach (ArrowLine al in m_StartPoints.Keys)
                                //{
                                //    al.X1 = m_Points[m_StartPoints[al]].X;
                                //    al.Y1 = m_Points[m_StartPoints[al]].Y;

                                //}
                                //foreach (ArrowLine al in m_EndPoints.Keys)
                                //{
                                //    al.X2 = m_Points[m_EndPoints[al]].X;
                                //    al.Y2 = m_Points[m_EndPoints[al]].Y;

                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void ClearPoints()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
        }

        private void ClearLabels()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(LabelArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
        }

        private void ClearArrows()
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");
            //m_Arrows.Clear();
            //m_Circles.Clear();
            m_ArrowsWithSourceAndDestination.Clear();
            m_CirclesSource.Clear();
            m_ArrowsMark.Clear();
            m_CirclesMark.Clear();
        }

        #endregion

        #region Events

        private event VoidDelegate Start;

        private void FireStartEvent()
        {
            if (Start != null) Start();
        }

        private event VoidDelegate Stop;

        private void FireStopEvent()
        {
            if (Stop != null) Stop();
        }

        private event VoidDelegate Cancel;

        private void FireCancelEvent()
        {
            if (Cancel != null) Cancel();
        }

        void PowerModControl_Cancel()
        {
            CancelThread();
            SetupStop();
        }

        void PowerModControl_Stop()
        {
            SetupStop();
        }

        void PowerModControl_Start()
        {
            SetupStart();
        }

        #endregion

        #region SettingUpButtons

        private void SetupStart()
        {
            ControlHandler.ExecuteMethod(this, "_SetupStart");
        }

        public void _SetupStart()
        {
            btnCancel.IsEnabled = true;
            btnExecute.IsEnabled = false;
            dpStepwiseButtons.Visibility = (!rbAutomatic.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
            iscBase.LockControls();
            iscExp.LockControls();
            iscMod.LockControls();
            rbAutomatic.IsEnabled = false;
            rbStepwise.IsEnabled = false;
            slidermodulus.IsEnabled = false;
        }

        private void SetupStop()
        {
            ControlHandler.ExecuteMethod(this, "_SetupStop");
        }

        public void _SetupStop()
        {
            btnCancel.IsEnabled = false;
            btnExecute.IsEnabled = true;
            dpStepwiseButtons.Visibility = Visibility.Collapsed;
            btnNextStep.IsEnabled = true;
            btnResumeAutomatic.IsEnabled = true;
            m_Resume = false;
            iscBase.UnLockControls();
            iscExp.UnLockControls();
            iscMod.UnLockControls();
            rbAutomatic.IsEnabled = true;
            rbStepwise.IsEnabled = true;
            slidermodulus.IsEnabled = true;
        }

        #endregion

        #region Execute

        private void CancelThread()
        {
            if (m_Thread != null)
            {
                lock (m_RunningLockObject)
                {
                    m_Running = false;
                }

                m_Thread.Abort();
                m_Thread = null;
            }
        }

        private void StartThread()
        {
            m_Base = iscBase.GetValue();
            m_Exp = iscExp.GetValue();
            m_Mod = iscMod.GetValue();
            if (m_Base != null && m_Exp != null && m_Mod != null)
            {
                m_Thread = new Thread(new ThreadStart(DoExecuteGraphic));
                m_Thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                m_Thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                m_Thread.Start();
                //rteDoExecuteGraphic();
            }
        }

        //public void Execute(PrimesBigInteger value)
        //{
        //  tabItemGraphic.IsEnabled = true;
        //  tcStats.SelectedIndex = 0;

        //  this.m_Value = value;
        //  m_Arrows.Clear();
        //  ArrowArea.Children.Clear();
        //  log.Columns = 1;
        //  log.Clear();
        //  if (value.CompareTo(PrimesBigInteger.ValueOf(150))>0)
        //  {
        //    tcStats.SelectedIndex = 1;
        //    tabItemGraphic.IsEnabled = false;
        //  }
        //  else
        //  {
        //    CreatePoints();
        //  }
        //}

        private bool m_SortAsc = true;

        private void CreatePoints()
        {
            //ClearArrows();
            //ClearPoints();
            ClearLabels();
            for (int i = 0; i < m_Mod.IntValue; i++)
            {
                CreatePoint(i);
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            StartThread();
        }

        private ManualResetEvent m_StepWiseEvent;

        private void WaitStepWise()
        {
            if (m_StepWiseEvent != null && _ExecutionMethod == ExecutionMethod.stepwise && !m_Resume)
            {
                m_StepWiseEvent.Reset();
                m_StepWiseEvent.WaitOne();
            }
        }

        public void SetFormula()
        {
            Formula.Formula = $"{m_Base}^i \\text{{ mod }} {m_Mod} \\text{{          }} i = 1,\\ldots,{m_Exp}";
        }

        private void DoExecuteGraphic()
        {
            FireStartEvent();

            ClearArrows();
            m_SourceDestination.Clear();
            log.Clear();
            log.Columns = 1;

            lock (m_RunningLockObject)
            {
                m_Running = true;
            }

            Point lastPoint = new Point(-1, -1);
            Ellipse lastEllipse = null;
            PrimesBigInteger result = null;
            PrimesBigInteger tmp = m_Base.Mod(m_Mod);
            Ellipse e = this.GetEllipseAt(m_Points[tmp.IntValue]);

            if (e != null)
            {
                ControlHandler.SetPropertyValue(e, "Fill", Brushes.Red);
                ControlHandler.SetPropertyValue(e, "Stroke", Brushes.Red);
            }

            ControlHandler.ExecuteMethod(this, nameof(SetFormula), new object[0]);

            PrimesBigInteger i = 1;
            while (i <= m_Exp)
            {
                Thread.Sleep(100);

                if (result == null)
                {
                    result = m_Base.Mod(m_Mod);
                }
                else
                {
                    result = (result * m_Base).Mod(m_Mod);
                }
                log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.powermod_execution, i, m_Base, i, m_Mod, result));

                if (lastPoint.X == -1 && lastPoint.Y == -1)
                {
                    lastPoint = m_Points[result.IntValue];
                    lastEllipse = e;
                }
                else
                {
                    Ellipse _e = this.GetEllipseAt(m_Points[result.IntValue]);
                    Point newPoint = m_Points[result.IntValue];
                    //CreateArrow(i.Subtract(PrimesBigInteger.One), lastPoint, newPoint);
                    CreateArrow(i-1, lastEllipse, _e);
                    lastPoint = newPoint;
                    lastEllipse = _e;
                }

                i = i+1;
                if (i>=3)
                    WaitStepWise();
            }

            lock (m_RunningLockObject)
            {
                m_Running = false;
            }

            FireStopEvent();
        }

        private Ellipse GetEllipseAt(Point p)
        {
            return (Ellipse)ControlHandler.ExecuteMethod(this, "_GetEllipseAt", new object[] { p });
        }

        public Ellipse _GetEllipseAt(Point p)
        {
            foreach (UIElement e in PaintArea.Children)
            {
                if (e.GetType() == typeof(Ellipse))
                {
                    if (Canvas.GetTop(e) == p.Y && Canvas.GetLeft(e) == p.X) return e as Ellipse;
                }
            }

            return null;
        }

        //private void CreateArrow(PrimesBigInteger counter, Point from, Point to)
        //{
        //    ArrowLine l = null;
        //    if (from.X == to.X && from.Y == to.Y)
        //    {
        //        AddCircle(counter, from.X, from.Y);
        //    }
        //    else
        //    {
        //        l = ControlHandler.CreateObject(typeof(ArrowLine)) as ArrowLine;
        //        ControlHandler.SetPropertyValue(l, "Stroke", Brushes.Black);
        //        ControlHandler.SetPropertyValue(l, "StrokeThickness", 1.5);
        //        //ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X1Property, new Binding("(Point.X)") { Source = from  }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
        //        //ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y1Property, new Binding("(Point.Y)") { Source = from }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
        //        //ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X2Property, new Binding("(Point.X)") { Source = to }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
        //        //ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y2Property, new Binding("(Point.Y)") { Source = to }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
        //        //l.SetBinding(Line.X1Property, new Binding("(Point.X)") { Source = from, Converter = new myc() });
        //        //l.SetBinding(Line.Y1Property, new Binding("(Point.Y)") { Source = from, Converter = new myc() });
        //        //l.SetBinding(Line.X2Property, new Binding("(Point.X)") { Source = to, Converter = new myc() });
        //        //l.SetBinding(Line.Y2Property, new Binding("(Point.Y)") { Source = to, Converter = new myc() });

        //        ControlHandler.SetPropertyValue(l, "X1", from.X);
        //        ControlHandler.SetPropertyValue(l, "Y1", from.Y);
        //        ControlHandler.SetPropertyValue(l, "X2", to.X);
        //        ControlHandler.SetPropertyValue(l, "Y2", to.Y);
        //        if (!ContainsLine(l))
        //        {
        //            ControlHandler.AddChild(l, ArrowArea);
        //            m_Arrows.Add(l);
        //            m_ArrowsMark.Add(counter, l);
        //        }
        //        else
        //        {
        //            ResetLine(counter, l);
        //        }
        //    }
        //}

        private void CreateArrow(PrimesBigInteger counter, Ellipse from, Ellipse to)
        {
            if (from == to)
            {
                AddCircle(counter, from);
            }
            else
            {
                ArrowLine l = ControlHandler.CreateObject(typeof(ArrowLine)) as ArrowLine;
                ControlHandler.SetPropertyValue(l, "Stroke", Brushes.Black);
                ControlHandler.SetPropertyValue(l, "StrokeThickness", 1.5);
                //ControlHandler.SetPropertyValue(l, "X1", (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", from)+3);
                //ControlHandler.SetPropertyValue(l, "Y1", (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", from)+3);
                //ControlHandler.SetPropertyValue(l, "X2", (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", to)+3);
                //ControlHandler.SetPropertyValue(l, "Y2", (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", to)+3);
                var arrowPositionConverter = new ArrowPositionConverter(PointRadius);
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X1Property, new Binding("(Canvas.Left)") { Source = from, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y1Property, new Binding("(Canvas.Top)") { Source = from, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.X2Property, new Binding("(Canvas.Left)") { Source = to, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                ControlHandler.ExecuteMethod(l, "SetBinding", new object[] { ArrowLine.Y2Property, new Binding("(Canvas.Top)") { Source = to, Converter = arrowPositionConverter }, new Type[] { typeof(DependencyProperty), typeof(BindingBase) } });
                //ControlHandler.SetPropertyValue(l, "RenderTransform", new TranslateTransform(PointRadius, PointRadius));
                //l.RenderTransform = new TranslateTransform(PointRadius, PointRadius);

                Pair<Ellipse, Ellipse> pair = new Pair<Ellipse, Ellipse>(from, to);
                if (!m_SourceDestination.Contains(pair))
                {
                    m_SourceDestination.Add(pair);
                    m_ArrowsWithSourceAndDestination.Add(pair, l);
                    ControlHandler.AddChild(l, ArrowArea);
                    //m_Arrows.Add(l);
                    m_ArrowsMark.Add(counter, l);
                }
                else if (m_ArrowsWithSourceAndDestination.ContainsKey(pair))
                {
                    l = m_ArrowsWithSourceAndDestination[pair];
                    ResetLine(counter, l);
                }                
            }

            ControlHandler.ExecuteMethod(this, nameof(MarkPath), new object[] { counter });
        }

        private class ArrowPositionConverter : IValueConverter
        {
            private readonly double offset;

            public ArrowPositionConverter(double offset)
            {
                this.offset = offset;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                //Correct for the fact that arrow target positions are not pointing exactly at the middle of target ellipses:
                return (double)value + offset;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        //class myc : IValueConverter
        //{
        //    #region IValueConverter Members

        //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //    {
        //        return value;
        //    }

        //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //    {
        //        return value;
        //    }

        //    #endregion
        //}

        private void ResetLine(PrimesBigInteger counter, ArrowLine l)
        {
            if (l != null)
            {
                m_ArrowsMark.Add(counter, l);
                UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
                ControlHandler.ExecuteMethod(children, "Remove", new object[] { l });
                ControlHandler.ExecuteMethod(children, "Add", new object[] { l });
            }
        }

        //private ArrowLine GetLine(ArrowLine l)
        //{
        //    foreach (ArrowLine line in m_Arrows)
        //    {
        //        double srcx1 = (double)ControlHandler.GetPropertyValue(line, "X1");
        //        double srcx2 = (double)ControlHandler.GetPropertyValue(line, "X2");
        //        double srcy1 = (double)ControlHandler.GetPropertyValue(line, "Y1");
        //        double srcy2 = (double)ControlHandler.GetPropertyValue(line, "Y2");
        //        double destx1 = (double)ControlHandler.GetPropertyValue(l, "X1");
        //        double destx2 = (double)ControlHandler.GetPropertyValue(l, "X2");
        //        double desty1 = (double)ControlHandler.GetPropertyValue(l, "Y1");
        //        double desty2 = (double)ControlHandler.GetPropertyValue(l, "Y2");

        //        if (srcx1 == destx1 && srcx2 == destx2 && srcy1 == desty1 && srcy2 == desty2) return line;
        //    }
        //    return null;
        //}

        //private bool ContainsLine(ArrowLine l)
        //{
        //    return GetLine(l) != null;
        //}

        //public void AddCircle(PrimesBigInteger counter, double x, double y)
        //{
        //    Polyline p = ControlHandler.CreateObject(typeof(Polyline)) as Polyline;
        //    ControlHandler.SetPropertyValue(p, "Stroke", Brushes.Black);
        //    ControlHandler.SetPropertyValue(p, "StrokeThickness", 1.5);
        //    PointCollection pc = ControlHandler.GetPropertyValue(p, "Points") as PointCollection;
        //    int c = 16;
        //    double radius = 25;
        //    for (int value = 0; value <= c; value++)
        //    {
        //        double angle = (360.0 / (double)c) * value;
        //        double top = radius / 2 + (Math.Sin((angle * 2 * Math.PI) / 360.0) * radius / 2);
        //        double left = radius / 2 + (Math.Cos((angle * 2 * Math.PI) / 360.0) * radius / 2);
        //        ControlHandler.ExecuteMethod(pc, "Add", new object[] { new Point(top, left) });
        //    }
        //    if (!ContainsCircle(x, y))
        //    {
        //        m_Circles.Add(p);
        //        m_CirclesMark.Add(counter, p);
        //        ControlHandler.ExecuteMethod(ArrowArea, "SetLeft", new object[] { p, x });
        //        ControlHandler.ExecuteMethod(ArrowArea, "SetTop", new object[] { p, y });
        //        ControlHandler.AddChild(p, ArrowArea);
        //    }
        //    else
        //    {
        //        ResetCircle(counter, x, y);
        //    }
        //}

        public void AddCircle(PrimesBigInteger counter, Ellipse source)
        {
            Polyline p = ControlHandler.CreateObject(typeof(Polyline)) as Polyline;
            ControlHandler.SetPropertyValue(p, "Stroke", Brushes.Black);
            ControlHandler.SetPropertyValue(p, "StrokeThickness", 1.5);
            PointCollection pc = ControlHandler.GetPropertyValue(p, "Points") as PointCollection;
            int c = 16;
            double radius = 25;
            double x = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", source);
            double y = (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", source);

            for (int value = 0; value <= c; value++)
            {
                double angle = (360.0 / (double)c) * value;
                double top = radius / 2 + (Math.Sin((angle * 2 * Math.PI) / 360.0) * radius / 2);
                double left = radius / 2 + (Math.Cos((angle * 2 * Math.PI) / 360.0) * radius / 2);
                ControlHandler.ExecuteMethod(pc, "Add", new object[] { new Point(top, left) });
            }
            if (!m_CirclesSource.ContainsKey(source))
            {
                m_CirclesSource.Add(source, p);
                m_CirclesMark.Add(counter, p);
                ControlHandler.ExecuteMethod(ArrowArea, "SetLeft", new object[] { p, x });
                ControlHandler.ExecuteMethod(ArrowArea, "SetTop", new object[] { p, y });
                ControlHandler.AddChild(p, ArrowArea);
            }
            else
            {
                p = m_CirclesSource[source];
                ResetCircle(counter, p);
            }
        }

        public void MoveCircle(Ellipse source)
        {
            Polyline pl = m_CirclesSource[source];
            double x = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", source);
            double y = (double)ControlHandler.ExecuteMethod(PaintArea, "GetTop", source);
            ControlHandler.ExecuteMethod(ArrowArea, "SetLeft", new object[] { pl, x });
            ControlHandler.ExecuteMethod(ArrowArea, "SetTop", new object[] { pl, y });
        }

        private void ResetCircle(PrimesBigInteger counter, Polyline p)
        {
            if (p != null)
            {
                m_CirclesMark.Add(counter, p);

                UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
                ControlHandler.ExecuteMethod(children, "Remove", new object[] { p });
                Thread.Sleep(100);
                ControlHandler.ExecuteMethod(children, "Add", new object[] { p });
            }
        }

        //private void ResetCircle(PrimesBigInteger counter, double x, double y)
        //{

        //    Polyline ltmp = GetCircle(x, y);
        //    if (ltmp != null)
        //    {
        //        m_CirclesMark.Add(counter, ltmp);

        //        UIElementCollection children = ControlHandler.GetPropertyValue(ArrowArea, "Children") as UIElementCollection;
        //        ControlHandler.ExecuteMethod(children, "Remove", new object[] { ltmp });
        //        Thread.Sleep(100);
        //        ControlHandler.ExecuteMethod(children, "Add", new object[] { ltmp });
        //    }
        //}

        //private Polyline GetCircle(double x, double y)
        //{
        //    foreach (Polyline line in m_Circles)
        //    {
        //        double _top = (double)ControlHandler.ExecuteMethod(ArrowArea, "GetTop", new object[] { line });
        //        double _left = (double)ControlHandler.ExecuteMethod(ArrowArea, "GetLeft", new object[] { line });
        //        if (y == _top && x == _left) return line;
        //    }
        //    return null;
        //}

        //private bool ContainsCircle(double x, double y)
        //{
        //    return GetCircle(x, y) != null;
        //}
		  //

        #endregion

        private ExecutionMethod _ExecutionMethod
        {
            get
            {
                bool? ischecked = (bool?)ControlHandler.GetPropertyValue(rbAutomatic, "IsChecked");
                return (ischecked.Value) ? ExecutionMethod.auto : ExecutionMethod.stepwise;
            }
        }

        private enum ExecutionMethod { auto, stepwise }

        private void btnNextStep_Click(object sender, RoutedEventArgs e)
        {
            m_StepWiseEvent.Set();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            FireCancelEvent();
            CancelThread();
        }

        public void CleanUp()
        {
            CancelThread();
        }

        private void btnResumeAutomatic_Click(object sender, RoutedEventArgs e)
        {
            m_Resume = true;
            m_StepWiseEvent.Set();
            ControlHandler.SetPropertyValue(btnNextStep, "IsEnabled", false);
            ControlHandler.SetPropertyValue(btnResumeAutomatic, "IsEnabled", false);
        }

        private void ContentArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ArrowArea.Width = PaintArea.Width = CircleArea.Width = e.NewSize.Width;
            ArrowArea.Height = PaintArea.Height = CircleArea.Height = e.NewSize.Height;
        }

        bool rotate = false;

        private void PaintArea_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(e.Source.ToString());
            if (e.Source != null && e.Source == PaintArea)
            {
                rotate = true;
            }
            Cursor = Cursors.ScrollAll;
        }

        private void ArrowArea_MouseLeave(object sender, MouseEventArgs e)
        {
            releaseRotate();
        }

        private void ArrowArea_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            releaseRotate();
        }

        void releaseRotate()
        {
            lock (m_lockobject)
            {
                Cursor = Cursors.Arrow;
                rotate = false;
                diff = 0;
            }
        }

        private void ArrowArea_PreviewDragOver(object sender, DragEventArgs e)
        {
        }

        object m_lockobject = new object();
        int diff = 0;
        private Point previous = new Point(-10000, -10000);

        private void ArrowArea_MouseMove(object sender, MouseEventArgs e)
        {
            lock (m_lockobject)
            {
                if (rotate)
                {
                    double centerY = Aperture / 2;
                    double centerX = Aperture / 2;
                    double diffx = 0;
                    double diffy = 0;
                    if (previous.X == -10000 && previous.Y == -10000)
                    {
                        previous = e.GetPosition(PaintArea);
                    }
                    else
                    {
                        Point actual = e.GetPosition(PaintArea);
                        diffx = previous.X - actual.X;
                        diffy = centerY - previous.Y;
                        previous = actual;
                    }
                    Point currentPosition = e.GetPosition(CircleArea);
                    double diffXp = Math.Abs(((centerX - currentPosition.X)) / 120);
                    if ((diffx < 0 && diffy <= 0) || (diffx >= 0 && diffy >= 0))
                    {
                        diffXp *= -1;
                    }
                    diffXp *= (!m_SortAsc) ? -1 : 1;
                    offset += diffXp;
                    CreatePoints();
                }
            }
        }
    }
}

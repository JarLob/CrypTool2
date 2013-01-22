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
using System.Diagnostics;
using System.Collections;
using Primes.Bignum;

namespace Primes.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        #region Constants

        const int counterwidth = 15;

        #endregion

        #region Properties

        private TextBox m_Edit;
        private TextBlock m_TextBlock;
        TextStyle m_TextStyle;
        private int m_CurrentRow;
        private int m_FormerRow;

        private Style m_InfoStyle;
        private IList<TextBlock> m_Selected;

        public Style InfoStyle
        {
            get { return m_InfoStyle; }
            set { m_InfoStyle = value; }
        }

        private Style m_ErrorStyle;

        public Style ErrorStyle
        {
            get { return m_ErrorStyle; }
            set { m_ErrorStyle = value; }
        }

        public object Title
        {
            get { if (gbHeader != null) return gbHeader.Header; else return string.Empty; }
            set
            {
                if (gbHeader != null)
                {
                    gbHeader.Header = value;
                    if (string.IsNullOrEmpty(value.ToString()))
                    {
                        this.gbHeader.BorderThickness = new Thickness(0.0, 0.0, 0.0, 0.0);
                    }
                    else
                    {
                        this.gbHeader.BorderThickness = new Thickness(0.1, 0.1, 0.1, 0.1);
                    }
                }
            }
        }

        private int m_Columns;

        public int Columns
        {
            get { return m_Columns; }
            set
            {
                m_Columns = value + 1;
                ColumnDefinitionCollection columnDefinitions =
                  ControlHandler.GetPropertyValue(gridMessages, "ColumnDefinitions") as ColumnDefinitionCollection;
                if (m_Columns < columnDefinitions.Count)
                {
                    this.Clear();
                }
                for (int i = 0; i < m_Columns; i++)
                {
                    if (i >= columnDefinitions.Count)
                    {
                        ColumnDefinition cd = ControlHandler.CreateObject(typeof(ColumnDefinition)) as ColumnDefinition;
                        ControlHandler.ExecuteMethod(columnDefinitions, "Add", new object[] { cd });
                    }
                    double width = counterwidth;
                    GridUnitType unittype = GridUnitType.Auto;
                    if (i > 0)
                    {
                        width = Math.Max((this.ActualWidth / (double)this.Columns) - counterwidth, 0);
                        unittype = GridUnitType.Star;
                    }
                    GridLength gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { width, unittype });
                    ControlHandler.SetPropertyValue(columnDefinitions[i], "Width", gl);
                }
            }
        }

        private bool m_ShowCounter;

        public bool ShowCounter
        {
            get { return m_ShowCounter; }
            set { m_ShowCounter = value; }
        }

        private bool m_OverrideText;
        public bool OverrideText { get { return m_OverrideText; } set { m_OverrideText = value; } }

        //private double m_Widht = double.NaN;
        //public new double Width
        //{
        //  get { return m_Widht; }
        //  set { m_Widht = value; }
        //}

        public new double Width
        {
            get { return gridMessages.ActualWidth; }
            set
            {
                gridMessages.Width = value;
                foreach (UIElement e in this.gridMessages.Children)
                {
                    if (e.GetType() == typeof(TextBlock))
                        (e as TextBlock).Width = value - 50;
                    else if (e.GetType() == typeof(Rectangle))
                        (e as Rectangle).Width = value - 50;
                }
            }
        }

        private int counter;

        #endregion

        #region Constructors

        private bool m_Initialized = false;
        private object logobjext = null;

        public LogControl()
        {
            InitializeComponent();
            m_CurrentRow = -1;
            m_FormerRow = -1;
            m_Selected = new List<TextBlock>();
            counter = 1;
            m_Initialized = false;
            logobjext = new object();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (m_Initialized)
                gridMessages.Width = sizeInfo.NewSize.Width;
        }

        #endregion

        #region Events

        public event ExecuteIntegerDelegate RowMouseOver;
        private void FireRowMouseOverEvent(int value)
        {
            if (RowMouseOver != null) RowMouseOver(PrimesBigInteger.ValueOf(value).Divide(PrimesBigInteger.Two));
        }

        #endregion

        #region Messaging

        public void Info(string message)
        {
            int row = NewLine();
            Info(message, 0, row);
        }

        public void Info(string message, int column, int row)
        {
            AddMessage(message, TextStyle.InfoStyle.Foreground, column, row);
        }

        public void Error(string message)
        {
            int row = NewLine();
            Error(message, 0, row);
        }

        public void Error(string message, int column, int row)
        {
            AddMessage(message, TextStyle.ErrorStyle.Foreground, column, row);
        }

        private TextBlock Get(int colum, int row)
        {
            UIElementCollection childs = ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;

            if (childs != null)
            {
                IEnumerator _enum = ControlHandler.ExecuteMethod(childs, "GetEnumerator") as IEnumerator;
                while ((bool)ControlHandler.ExecuteMethod(_enum, "MoveNext"))
                {
                    UIElement element = ControlHandler.GetPropertyValue(_enum, "Current") as UIElement;
                    if (element.GetType() == typeof(TextBlock))
                    {
                        int _row = (int)ControlHandler.ExecuteMethod(gridMessages, "GetRow", new object[] { element });
                        int _col = (int)ControlHandler.ExecuteMethod(gridMessages, "GetColumn", new object[] { element });
                        if (_row == row && _col == colum)
                        {
                            return element as TextBlock;
                        }
                    }
                }
            }

            return null;
        }

        public void _AddMessage(string message, Brush color, int column, int row)
        {
            lock (logobjext)
            {
                if (message != null)
                {
                    UIElementCollection childs = ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;
                    column++;
                    TextBlock tb = null;
                    if (m_OverrideText)
                    {
                        TextBlock _tb = this.Get(column, row);
                        if (_tb != null)
                        {
                            tb = _tb;
                            gridMessages.Children.Remove(tb);
                        }
                        else
                        {
                            tb = ControlHandler.CreateObject(typeof(TextBlock)) as TextBlock;
                        }
                    }
                    else
                    {
                        tb = ControlHandler.CreateObject(typeof(TextBlock)) as TextBlock;
                    }
                    ControlHandler.SetPropertyValue(tb, "TextWrapping", TextWrapping.Wrap);
                    ControlHandler.SetPropertyValue(tb, "Text", message);
                    ControlHandler.SetPropertyValue(tb, "Foreground", color);
                    //ControlHandler.SetPropertyValue(tb, "Width", Math.Max(this.ActualWidth - 100, 50));
                    ControlHandler.SetPropertyValue(tb, "FontSize", 12);
                    ControlHandler.SetPropertyValue(tb, "HorizontalAlignment", HorizontalAlignment.Left);
                    tb.Padding = new Thickness(10, 5, 10, 5);

                    if (!string.IsNullOrEmpty(message.Trim()))
                    {
                        tb.MouseLeftButtonDown += new MouseButtonEventHandler(tb_MouseLeftButtonDown);
                        tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                        tb.MouseLeave += new MouseEventHandler(tb_MouseLeave);
                    }
                    Grid.SetColumn(tb, column);
                    Grid.SetRow(tb, row);
                    gridMessages.Children.Add(tb);
                    //ControlHandler.ExecuteMethod(gridMessages, "SetColumn", new object[] { tb, column });
                    //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { tb, row });
                    //ControlHandler.AddChild(tb, gridMessages);
                    //ControlHandler.ExecuteMethod(childs, "Add", new object[] { tb });
                    if (m_FormerRow != m_CurrentRow)
                    {
                        NewLine();
                        if (m_ShowCounter)
                        {
                            TextBlock tb1 = ControlHandler.CreateObject(typeof(TextBlock)) as TextBlock;
                            ControlHandler.SetPropertyValue(tb1, "TextWrapping", TextWrapping.Wrap);
                            ControlHandler.SetPropertyValue(tb1, "Text", counter.ToString() + ". ");
                            ControlHandler.SetPropertyValue(tb1, "Foreground", color);
                            ControlHandler.SetPropertyValue(tb1, "FontSize", 12);
                            ControlHandler.SetPropertyValue(tb1, "HorizontalAlignment", HorizontalAlignment.Left);

                            Grid.SetColumn(tb1, 0);
                            Grid.SetRow(tb1, row);
                            gridMessages.Children.Add(tb1);

                            //ControlHandler.ExecuteMethod(gridMessages, "SetColumn", new object[] { tb1, 0 });
                            //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { tb1, row });
                            //ControlHandler.AddChild(tb1, gridMessages);
                        }
                        Rectangle rec = (Rectangle)ControlHandler.CreateObject(typeof(Rectangle));
                        ControlHandler.SetPropertyValue(rec, "Width", Math.Max(this.ActualWidth - 100, 50));
                        ControlHandler.SetPropertyValue(rec, "Fill", Brushes.LightGray);
                        ControlHandler.SetPropertyValue(rec, "Height", 1.0);
                        ControlHandler.SetPropertyValue(rec, "HorizontalAlignment", HorizontalAlignment.Left);
                        ControlHandler.SetPropertyValue(rec, "VerticalAlignment", VerticalAlignment.Bottom);

                        if (m_Columns > 0)
                            Grid.SetColumnSpan(rec, m_Columns);
                        Grid.SetRow(rec, m_CurrentRow);
                        gridMessages.Children.Add(rec);

                        //ControlHandler.ExecuteMethod(gridMessages, "SetColumnSpan", new object[] { rec, m_Columns });
                        //ControlHandler.ExecuteMethod(gridMessages, "SetRow", new object[] { rec, m_CurrentRow });
                        //ControlHandler.AddChild(rec, gridMessages);
                        counter++;
                        m_FormerRow = m_CurrentRow;
                    }

                    ControlHandler.ExecuteMethod(scroller, "ScrollToEnd");
                }
            }
        }

        private void AddMessage(string message, Brush color, int column, int row)
        {
            ControlHandler.ExecuteMethod(this, "_AddMessage", new object[] { message, color, column, row });
        }

        void tb_MouseLeave(object sender, MouseEventArgs e)
        {
            //  if (m_TextStyle != null)
            //  {
            //    MarkRow(sender as TextBlock, m_TextStyle.Background, m_TextStyle.Foreground);
            //  }
            //  else
            //  {
            //    MarkRow(sender as TextBlock, Brushes.White, Brushes.Black);
            //  }
        }

        private void MarkRow(TextBlock sender, Brush background, Brush foreground)
        {
            if (!gridMessages.ContextMenu.IsOpen)
            {
                foreach (TextBlock tb in m_Selected)
                {
                    if (m_TextStyle != null)
                    {
                        DoMarkRow(tb, m_TextStyle.Background, m_TextStyle.Foreground);
                    }
                    else
                    {
                        DoMarkRow(tb, Brushes.White, Brushes.Black);
                    }
                }
                m_Selected.Clear();
                m_Selected = DoMarkRow(sender, background, foreground);
            }
        }

        private IList<TextBlock> DoMarkRow(TextBlock sender, Brush background, Brush foreground)
        {
            IList<TextBlock> result = new List<TextBlock>();

            if (!gridMessages.ContextMenu.IsOpen)
            {
                int index = gridMessages.Children.IndexOf(sender);
                int row = Grid.GetRow(sender as UIElement);
                if (index >= 0 && row >= 0)
                {
                    int start = Math.Max(index - m_Columns, 0);
                    int end = Math.Min(index + m_Columns, gridMessages.Children.Count - 1);
                    for (int i = start; i <= end; i++)
                    {
                        if (gridMessages.Children[i].GetType() == typeof(TextBlock))
                        {
                            TextBlock tb = gridMessages.Children[i] as TextBlock;
                            if (Grid.GetRow(tb) == row)
                            {
                                tb.Background = background;
                                tb.Foreground = foreground;
                                result.Add(tb);
                            }
                        }
                    }
                }
            }

            return result;
        }

        void tb_MouseMove(object sender, MouseEventArgs e)
        {
            TextStyle textstyle = new TextStyle((sender as TextBlock).Foreground, (sender as TextBlock).Background);
            if (textstyle.Equals(TextStyle.InfoStyle) || textstyle.Equals(TextStyle.ErrorStyle))
                m_TextStyle = textstyle;
            MarkRow(sender as TextBlock, Brushes.Blue, Brushes.WhiteSmoke);
            if (sender != null)
            {
                int row = Grid.GetRow(sender as UIElement);
                FireRowMouseOverEvent(row);
            }
        }

        #region Handles KeysDown and MouseClicks

        private void RemoveEdit()
        {
            if (m_Edit != null)
            {
                int row = Grid.GetRow(m_Edit);
                int col = Grid.GetColumn(m_Edit);
                gridMessages.Children.Remove(m_Edit);
                Grid.SetRow(m_TextBlock, row);
                Grid.SetColumn(m_TextBlock, col);
                gridMessages.Children.Insert((int)m_TextBlock.Tag, m_TextBlock);
                m_Edit = null;
            }
        }

        void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(TextBlock))
            {
                RemoveEdit();
                m_TextBlock = sender as TextBlock;
                int row = Grid.GetRow(m_TextBlock);
                int col = Grid.GetColumn(m_TextBlock);
                m_Edit = new TextBox();
                m_Edit.MouseLeave += new MouseEventHandler(m_Edit_MouseLeave);
                m_Edit.KeyDown += new KeyEventHandler(m_Edit_KeyDown);
                m_Edit.KeyUp += new KeyEventHandler(m_Edit_KeyDown);

                m_Edit.Text = m_TextBlock.Text;
                m_TextBlock.Tag = gridMessages.Children.IndexOf(m_TextBlock);
                gridMessages.Children.Remove(m_TextBlock);
                Grid.SetColumn(m_Edit, col);
                Grid.SetRow(m_Edit, row);
                gridMessages.Children.Add(m_Edit);
                m_Edit.SelectAll();
            }
        }

        void m_Edit_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Escape)
            {
                MarkRow(m_TextBlock, m_TextStyle.Background, m_TextStyle.Foreground);
                RemoveEdit();
            }
        }

        void m_Edit_MouseLeave(object sender, MouseEventArgs e)
        {
            RemoveEdit();
            MarkRow(m_TextBlock, m_TextStyle.Background, m_TextStyle.Foreground);
        }

        #endregion

        public void Clear()
        {
            ColumnDefinitionCollection columnDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "ColumnDefinitions") as ColumnDefinitionCollection;
            ControlHandler.ExecuteMethod(columnDefinitions, "Clear");

            RowDefinitionCollection rowDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "RowDefinitions") as RowDefinitionCollection;
            ControlHandler.ExecuteMethod(rowDefinitions, "Clear");

            UIElementCollection childs =
              ControlHandler.GetPropertyValue(gridMessages, "Children") as UIElementCollection;
            ControlHandler.ExecuteMethod(childs, "Clear");

            m_CurrentRow = m_FormerRow = -1;

            counter = 1;
        }

        public int NewLine()
        {
            m_CurrentRow++;
            RowDefinitionCollection rowDefinitions =
              ControlHandler.GetPropertyValue(gridMessages, "RowDefinitions") as RowDefinitionCollection;
            RowDefinition rd = ControlHandler.CreateObject(typeof(RowDefinition)) as RowDefinition;

            GridLength gl = (GridLength)ControlHandler.CreateObject(typeof(GridLength), new Type[] { typeof(double), typeof(GridUnitType) }, new object[] { 1, GridUnitType.Auto });
            ControlHandler.SetPropertyValue(rd, "Height", gl);

            ControlHandler.ExecuteMethod(rowDefinitions, "Add", new object[] { rd });
            return m_CurrentRow;
        }

        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(MenuItem))
                {
                    MenuItem mi = sender as MenuItem;
                    if (mi == miCopySelection)
                    {
                        StringBuilder result = new StringBuilder();
                        string[] msg = new string[m_Columns];
                        foreach (TextBlock tb in this.m_Selected)
                        {
                            int column = Grid.GetColumn(tb);
                            if (column > -1) msg[column] = tb.Text;
                            //result.Append(tb.Text);
                            //result.Append("\t");
                        }
                        foreach (string s in msg)
                        {
                            if (!string.IsNullOrEmpty(s))
                            {
                                result.Append(s);
                                result.Append("\t");
                            }
                        }
                        Clipboard.SetText(result.ToString(), TextDataFormat.Text);
                    }
                    else if (mi == miCopyAll)
                    {
                        int row = -1;
                        StringBuilder result = new StringBuilder();
                        string[] msg = new string[m_Columns];
                        foreach (UIElement element in gridMessages.Children)
                        {
                            if (element.GetType() == typeof(TextBlock))
                            {
                                if (row != Grid.GetRow(element))
                                {
                                    if (row > -1)
                                    {
                                        foreach (string s in msg)
                                        {
                                            if (!string.IsNullOrEmpty(s))
                                            {
                                                result.Append(s);
                                                result.Append("\t");
                                            }
                                        }
                                        result.Remove(result.Length - 1, 1);
                                        result.Append("\r\n");
                                    }
                                    row = Grid.GetRow(element);
                                }
                                int column = Grid.GetColumn(element);
                                if (column > -1 && column < msg.Length)
                                    msg[column] = (element as TextBlock).Text;
                            }
                        }
                        Clipboard.SetText(result.ToString(), TextDataFormat.Text);
                    }
                }
                gridMessages.ContextMenu.IsOpen = false;
                if (m_Selected.Count > 0 && m_TextStyle != null)
                {
                    RemoveEdit();
                    MarkRow(m_Selected[0], m_TextStyle.Background, m_TextStyle.Foreground);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region Class TextStyle

        private class TextStyle
        {
            private static TextStyle m_InfoStyle;
            private static TextStyle m_ErrorStye;

            private Brush m_Foreground;

            public Brush Foreground
            {
                get { return m_Foreground; }
                set { m_Foreground = (value == null) ? m_Foreground = Brushes.Blue : value; }
            }

            private Brush m_Background;

            public Brush Background
            {
                get { return m_Background; }
                set { m_Background = (value == null) ? m_Background = Brushes.Transparent : value; }
            }

            public TextStyle(Brush foreground, Brush background)
            {
                Foreground = foreground;
                Background = background;
            }

            public override bool Equals(object obj)
            {
                if (obj != null && obj.GetType() == typeof(TextStyle))
                {
                    return (obj as TextStyle).Foreground == m_Foreground && (obj as TextStyle).Background == m_Background;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static TextStyle InfoStyle
            {
                get
                {
                    if (m_InfoStyle == null) m_InfoStyle = new TextStyle(Brushes.Blue, Brushes.Transparent);
                    return m_InfoStyle;
                }
            }

            public static TextStyle ErrorStyle
            {
                get
                {
                    if (m_ErrorStye == null) m_ErrorStye = new TextStyle(Brushes.Red, Brushes.Transparent);
                    return m_ErrorStye;
                }
            }
        }

        #endregion

        private void scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}

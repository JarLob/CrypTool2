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
using Primes.Library;
using System.Windows.Media;
using Primes.Bignum;

namespace Primes.WpfControls.Factorization.QS
{
    public abstract class BaseStep : IQSStep
    {
        protected Grid m_Container;
        protected QSData m_QSData;
        protected int m_Delay = 1;

        public BaseStep(Grid container)
        {
            this.m_Container = container;
        }

        #region IQSStep Members

        public virtual QSResult Execute(ref QSData data)
        {
            this.m_QSData = data;
            return QSResult.Ok;
        }

        #endregion

        public void AddToGrid(Grid g, string text, int row, int col, int rowspan = 0, int columnspan = 0)
        {
            ControlHandler.ExecuteMethod(
              this,
              "_AddToGrid",
              new object[] { g, text, row, col, rowspan, columnspan });
        }

        public void _AddToGrid(Grid g, string text, int row, int col, int rowspan, int columnspan)
        {
            if (!string.IsNullOrEmpty(text))
            {
                TextBlock tb = new TextBlock();
                tb.FontFamily = new FontFamily("Arial Unicode MS");
                tb.Text = text;
                tb.Margin = new Thickness(5);
                if (columnspan > 0) Grid.SetColumnSpan(tb, columnspan);
                if (rowspan > 0) Grid.SetRowSpan(tb, rowspan);
                Grid.SetColumn(tb, col);
                Grid.SetRow(tb, row);

                g.Children.Add(tb);
            }
        }

        public TextBlock AddTextBlock(int row, int colum)
        {
            return ControlHandler.ExecuteMethod(this, "_AddTextBlock", new object[] { row, colum }) as TextBlock;
        }

        public TextBlock _AddTextBlock(int row, int colum)
        {
            TextBlock result = new TextBlock();
            result.Margin = new Thickness(5);
            System.Windows.Controls.Grid.SetColumn(result, (int)colum);
            System.Windows.Controls.Grid.SetRow(result, (int)row);
            Grid.Children.Add(result);

            return result;
        }

        #region IQSStep Members

        public virtual void PreStep()
        {
        }

        public virtual void PostStep() { }

        #endregion

        protected Grid Grid
        {
            get { return m_Container as Grid; }
            set { m_Container = value; }
        }

        protected bool ModuloTest(long a, long b, long mod)
        {
            if ((a - b) % mod == 0) return false;
            return (((a * a - b * b) % mod) == 0);
        }

        #region IQSStep Members

        public event FoundFactor FoundFactor;

        protected void FireFoundFactorEvent(object o)
        {
            if (FoundFactor != null) FoundFactor(o);
        }

        #endregion
    }
}

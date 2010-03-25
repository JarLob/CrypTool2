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
using System.Windows.Shapes;

namespace Primes.WpfControls.Components
{
  /// <summary>
  /// Interaction logic for ToolTipWindow.xaml
  /// </summary>
  public partial class ToolTipWindow : Window
  {
    public ToolTipWindow()
    {
      InitializeComponent();
      lineSpacer.Width = this.Width;
    }


    public string ToolTipTitle
    {
      get { return lblTitle.Content as string; }
      set { lblTitle.Content = value; }
    }

    public string ToolTipContent
    {
      get { return textBlockContent.Text; }
      set { textBlockContent.Text = value; }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      this.Close();
      
    }
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      base.OnMouseDown(e);
      this.Close();
    }
  }
}

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

using Primes.Bignum;
using Primes.Library;

namespace Primes.WpfControls.Components
{
  /// <summary>
  /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///
  /// Step 1a) Using this custom control in a XAML file that exists in the current project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:Primes.WpfControls.Components"
  ///
  ///
  /// Step 1b) Using this custom control in a XAML file that exists in a different project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:Primes.WpfControls.Components;assembly=Primes.WpfControls.Components"
  ///
  /// You will also need to add a project reference from the project where the XAML file lives
  /// to this project and Rebuild to avoid compilation errors:
  ///
  ///     Right click on the target project in the Solution Explorer and
  ///     "Add Reference"->"Projects"->[Browse to and select this project]
  ///
  ///
  /// Step 2)
  /// Go ahead and use your control in the XAML file. Note that Intellisense in the
  /// XML editor does not currently work on custom controls and its child elements.
  ///
  ///     <MyNamespace:NumberButton/>
  ///
  /// </summary>
  public enum NumberButtonStyle { Button, Ellipse}
  public class NumberButton : Button
  {
    private static readonly PrimesBigInteger MAX = PrimesBigInteger.ValueOf(999);
    private PrimesBigInteger m_Number;
    private string m_StrNumber;
    private bool m_ShowContent;
    public bool ShowContent {
      get { return m_ShowContent; }
      set { m_ShowContent=value; } 
    }
    public string Number
    {
      get { return m_StrNumber; }
      set {
        m_StrNumber = value;
        if (m_StrNumber != null)
        {
          if (ShowContent)
          {
            if (m_StrNumber.Length <= 3)
            {
              ControlHandler.SetElementContent(this, m_StrNumber);
            }
            else
            {
              ControlHandler.SetElementContent(this, m_StrNumber.Substring(0, 2) + "..");
            }
          }
          else
          {
            ControlHandler.SetElementContent(this, "   ");
          }
          ToolTip tt = ControlHandler.CreateObject(typeof(ToolTip)) as ToolTip;
          ControlHandler.SetElementContent(tt, m_StrNumber);
          ControlHandler.SetPropertyValue(this, "ToolTip", tt);
        }
      }
    }

    public PrimesBigInteger BINumber
    {
      get { return new PrimesBigInteger(m_StrNumber); }
      set { 
        m_Number = value;
        Number = m_Number.ToString();
      
      }
    }

    private NumberButtonStyle m_NumberButtonStyle;
    public string NumberButtonStyle
    {
      get { return m_NumberButtonStyle.ToString(); }
      set {
        if (this.Resources.Count == 0)
        {
          this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("Primes;component/WpfControls/Resources/Shared.xaml", UriKind.Relative)) as ResourceDictionary);
        }
        m_NumberButtonStyle = ParseNumberButtonStyle(value);
        switch (m_NumberButtonStyle)
        {
          case Primes.WpfControls.Components.NumberButtonStyle.Button:
            this.Template = this.Resources["NBtnTempl"] as ControlTemplate;
            break;
          case Primes.WpfControls.Components.NumberButtonStyle.Ellipse:
            this.Template = this.Resources["NBtnEllipseTmpl"] as ControlTemplate;
            this.Background = Brushes.Transparent;
            break;
        }
   
      }
    }
    private static Primes.WpfControls.Components.NumberButtonStyle ParseNumberButtonStyle(string s)
    {
      Primes.WpfControls.Components.NumberButtonStyle result = Primes.WpfControls.Components.NumberButtonStyle.Button;
      if (s != null)
      {
        if(s.Equals(Primes.WpfControls.Components.NumberButtonStyle.Ellipse.ToString()))
          result = Primes.WpfControls.Components.NumberButtonStyle.Ellipse;
          
      }
      return result;
    }
    static NumberButton()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberButton), new FrameworkPropertyMetadata(typeof(NumberButton)));
    }

    public NumberButton():base()
    {
    }
  }
}

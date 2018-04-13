/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace TextOutput
{
    /// <summary>
    /// Interaction logic for TextOutputPresentation.xaml
    /// </summary>
    [TabColor("pink")]
    public partial class TextOutputPresentation : UserControl, INotifyPropertyChanged
    {
        public TextOutput _textOutput = null;

        public TextOutputPresentation()
        {
            InitializeComponent();
            Width = double.NaN;
            Height = double.NaN;
            DataContext = this;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (_textOutput != null && ((bool)args.NewValue))
            {
                _textOutput.ShowInPresentation(_textOutput.CurrentValue);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        private FontFamily fontFamily;

        public FontFamily MyFontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                fontFamily = value;
                OnPropertyChanged("MyFontFamily");
            }
        }

        private double fontsize;

        public double MyFontSize
        {
            get
            {
                return fontsize;
            }
            set
            {
                fontsize = value;
                OnPropertyChanged("MyFontSize");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

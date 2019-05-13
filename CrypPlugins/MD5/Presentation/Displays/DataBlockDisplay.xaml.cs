﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für DataBlockDisplay.xaml
    /// </summary>
    public partial class DataBlockDisplay : UserControl
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IList<Byte>), typeof(DataBlockDisplay), null);
        public IList<Byte> Data { get { return (IList<Byte>)GetValue(DataProperty); } set { SetValue(DataProperty, value); } }
        
        public DataBlockDisplay()
        {
            InitializeComponent();
        }
    }
}

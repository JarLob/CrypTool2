﻿/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DCAPathFinder.UI.Tutorial3
{
    /// <summary>
    /// Interaktionslogik für WaitingSlide.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class WaitingSlide : UserControl, INotifyPropertyChanged
    {
        private bool _isUIEnabled = true;
        private string _inputDifference;
        private string _expectedDifference;
        private string _probability;

        /// <summary>
        /// Constructor
        /// </summary>
        public WaitingSlide()
        {
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for the input difference
        /// </summary>
        public string InputDifference
        {
            get { return _inputDifference; }
            set
            {
                _inputDifference = value;
                OnPropertyChanged("InputDifference");
            }
        }

        /// <summary>
        /// Property for the expected difference
        /// </summary>
        public string ExpectedDifference
        {
            get { return _expectedDifference; }
            set
            {
                _expectedDifference = value;
                OnPropertyChanged("ExpectedDifference");
            }
        }

        /// <summary>
        /// Property for the probability
        /// </summary>
        public string Probability
        {
            get { return _probability; }
            set
            {
                _probability = value;
                OnPropertyChanged("Probability");
            }
        }

        /// <summary>
        /// Property to disable / enable the ui
        /// </summary>
        public bool IsUIEnabled
        {
            get { return _isUIEnabled; }
            set
            {
                _isUIEnabled = value;
                OnPropertyChanged("IsUIEnabled");
            }
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call if data changes
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
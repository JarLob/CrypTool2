﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DCAPathFinder.UI.Tutorial2
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
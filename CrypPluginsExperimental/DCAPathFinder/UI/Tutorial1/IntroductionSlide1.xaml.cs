﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAPathFinder.UI.Tutorial1
{
    /// <summary>
    /// Interaktionslogik für IntroductionSlide1.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class IntroductionSlide1 : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<XorTableMapping> _xorData = null; 

        /// <summary>
        /// Constructor
        /// </summary>
        public IntroductionSlide1()
        {
            XorData = new ObservableCollection<XorTableMapping>();
            addData();

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Fills the data for the xor table
        /// </summary>
        private void addData()
        {
            XorData.Add(new XorTableMapping()
            {
                Col1 = "0",
                Col2 = "0",
                ColResult = "0"
            });

            XorData.Add(new XorTableMapping()
            {
                Col1 = "0",
                Col2 = "1",
                ColResult = "1"
            });

            XorData.Add(new XorTableMapping()
            {
                Col1 = "1",
                Col2 = "0",
                ColResult = "1"
            });

            XorData.Add(new XorTableMapping()
            {
                Col1 = "1",
                Col2 = "1",
                ColResult = "0"
            });
        }

        #region properties

        /// <summary>
        /// Property for data grid
        /// </summary>
        public ObservableCollection<XorTableMapping> XorData
        {
            get { return _xorData; }
            set
            {
                _xorData = value;
                OnPropertyChanged();
            }
        }

        #endregion

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using DCAKeyRecovery.UI.Models;

namespace DCAKeyRecovery.UI.Cipher3
{
    /// <summary>
    /// Interaktionslogik für Cipher3AnyRoundResultView.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAKeyRecovery.Properties.Resources")]
    public partial class Cipher3AnyRoundResultView : UserControl, INotifyPropertyChanged
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private int _round;
        private double _currentExpectedProbability;
        private string _expectedDifference;
        private int _expectedHitCount;
        private string _currentKeyCandidate;
        private int _messagePairCountToExamine;
        private string _currentRecoveredRoundKey;
        private int _currentKeysToTestThisRound;
        private ObservableCollection<KeyResult> _keyResults;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3AnyRoundResultView()
        {
            _keyResults = new ObservableCollection<KeyResult>();
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _startTime
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _endTime
        /// </summary>
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round
        /// </summary>
        public int Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentExpectedProbability
        /// </summary>
        public double CurrentExpectedProbability
        {
            get { return _currentExpectedProbability; }
            set
            {
                _currentExpectedProbability = value;
                OnPropertyChanged();
                OnPropertyChanged("CurrentExpectedProbabilityStr");
            }
        }

        /// <summary>
        /// Property for formatted _currentExpectedProbability
        /// </summary>
        public string CurrentExpectedProbabilityStr
        {
            get { return String.Format("{0:0.0000}", CurrentExpectedProbability); }
        }

        /// <summary>
        /// Property for _expectedDifference
        /// </summary>
        public string ExpectedDifference
        {
            get { return _expectedDifference; }
            set
            {
                _expectedDifference = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _expectedHitCount
        /// </summary>
        public int ExpectedHitCount
        {
            get { return _expectedHitCount; }
            set
            {
                _expectedHitCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentKeyCandidate
        /// </summary>
        public string CurrentKeyCandidate
        {
            get { return _currentKeyCandidate; }
            set
            {
                _currentKeyCandidate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messagePairCountToExamine
        /// </summary>
        public int MessagePairCountToExamine
        {
            get { return _messagePairCountToExamine; }
            set
            {
                _messagePairCountToExamine = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentRecoveredRoundKey
        /// </summary>
        public string CurrentRecoveredRoundKey
        {
            get { return _currentRecoveredRoundKey; }
            set
            {
                _currentRecoveredRoundKey = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentKeysToTestThisRound
        /// </summary>
        public int CurrentKeysToTestThisRound
        {
            get { return _currentKeysToTestThisRound; }
            set
            {
                _currentKeysToTestThisRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _keyResults
        /// </summary>
        public ObservableCollection<KeyResult> KeyResults
        {
            get { return _keyResults; }
            set
            {
                _keyResults = value;
                OnPropertyChanged();
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

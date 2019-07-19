﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DCAPathVisualiser.Logic;
using DCAPathVisualiser.UI.Models;

namespace DCAPathVisualiser.UI.Cipher3
{
    /// <summary>
    /// Interaktionslogik für Cipher3Table.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathVisualiser.Properties.Resources")]
    public partial class Cipher3Table : UserControl, INotifyPropertyChanged
    {
        private int _currentRound;
        private string _currentProbability;
        private string _currentInputDiff;
        private string _currentExpectedDiff;
        private int _currentCountOfCharacteristics;
        private string _currentActiveSBoxes;
        private DifferentialAttackRoundConfiguration _currentConfigurationToDisplay;
        private ObservableCollection<Cipher3CharacteristicUI> _characteristics;
        public event EventHandler<Cipher3CharacteristicSelectionEventArgs> SelectionChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3Table()
        {
            _characteristics = new ObservableCollection<Cipher3CharacteristicUI>();

            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _characteristics
        /// </summary>
        public ObservableCollection<Cipher3CharacteristicUI> Characteristics
        {
            get { return _characteristics; }
            set
            {
                _characteristics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentConfigurationToDisplay
        /// </summary>
        public DifferentialAttackRoundConfiguration CurrentConfigurationToDisplay
        {
            set
            {
                _currentConfigurationToDisplay = value;
                OnPropertyChanged();
            }
            get { return _currentConfigurationToDisplay; }
        }

        /// <summary>
        /// Property for _currentActiveSBoxes
        /// </summary>
        public string CurrentActiveSBoxes
        {
            get { return _currentActiveSBoxes; }
            set
            {
                _currentActiveSBoxes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentCountOfCharacteristics
        /// </summary>
        public int CurrentCountOfCharacteristics
        {
            get { return _currentCountOfCharacteristics; }
            set
            {
                _currentCountOfCharacteristics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentExpectedDiff
        /// </summary>
        public string CurrentExpectedDiff
        {
            get { return _currentExpectedDiff; }
            set
            {
                _currentExpectedDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentInputDiff
        /// </summary>
        public string CurrentInputDiff
        {
            get { return _currentInputDiff; }
            set
            {
                _currentInputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentProbability
        /// </summary>
        public string CurrentProbability
        {
            get { return _currentProbability; }
            set
            {
                _currentProbability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _currentRound
        /// </summary>
        public int CurrentRound
        {
            get { return _currentRound; }
            set
            {
                _currentRound = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Listener for change of selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CharacteristicSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            Cipher3CharacteristicUI item = dataGrid.SelectedItem as Cipher3CharacteristicUI;

            SelectionChanged.Invoke(this, new Cipher3CharacteristicSelectionEventArgs()
            {
                SelectedCharacteristic = item
            });
        }

        /// <summary>
        /// OnPropertyChanged-method for INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

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

namespace DCAPathFinder.UI.Tutorial3
{
    /// <summary>
    /// Interaktionslogik für AttackSearchResult.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("DCAPathFinder.Properties.Resources")]
    public partial class AttackSearchResult : UserControl, INotifyPropertyChanged
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private string _searchPolicy;
        private int _characteristicCount;
        private int _round;
        private string _sboxes;
        private ICollectionView _viewSource;
        private ObservableCollection<Cipher3CharacteristicUI> _characteristics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AttackSearchResult()
        {
            _characteristics = new ObservableCollection<Cipher3CharacteristicUI>();

            _viewSource = CollectionViewSource.GetDefaultView(Characteristics);
            SortDescription sort = new SortDescription("Probability", ListSortDirection.Descending);
            _viewSource.SortDescriptions.Add(sort);

            DataContext = this;
            InitializeComponent();
        }

        #region properties

        /// <summary>
        /// Property for _viewSource
        /// </summary>
        public ICollectionView ViewSource
        {
            get { return _viewSource; }
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
        /// Property for _searchPolicy
        /// </summary>
        public string SearchPolicy
        {
            get { return _searchPolicy; }
            set
            {
                _searchPolicy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _messageCount
        /// </summary>
        public int CharacteristicCount
        {
            get { return _characteristicCount; }
            set
            {
                _characteristicCount = value;
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
        /// Property for _sboxes
        /// </summary>
        public string SBoxes
        {
            get { return _sboxes; }
            set
            {
                _sboxes = value;
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
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
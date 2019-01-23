﻿/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace Cryptool.Plugins.FEAL.Controls
{
    /// <summary>
    /// Interaktionslogik für CryptRound.xaml
    /// </summary>
    public partial class CryptRound : UserControl, INotifyPropertyChanged
    {
        private bool _lastRound = false;
        private string _roundName = "Roundx";
        private string _roundKeyName = "Kx";
        private string _roundKey = "00";
        

        public CryptRound()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Sets, if this round is the last round
        /// </summary>
        public bool LastRound
        {

            get
            {
                return _lastRound;
            }

            set
            {
                _lastRound = value;
                NotifyPropertyChanged("LastRound");                
            }
        }

        /// <summary>
        /// Sets the display name of the round keys
        /// </summary>
        public string RoundName
        {

            get
            {
                return _roundName;
            }

            set
            {
                _roundName = value;
                NotifyPropertyChanged("RoundName");
            }
        }

        /// <summary>
        /// Sets the display name of the round keys
        /// </summary>
        public string RoundKeyName
        {

            get
            {
                return _roundKeyName;
            }

            set
            {
                _roundKeyName = value;
                NotifyPropertyChanged("RoundKeyName");                
            }
        }

        /// <summary>
        /// Sets the display of the round key
        /// </summary>
        public string RoundKey
        {

            get
            {
                return _roundKey;
            }

            set
            {
                _roundKey = value;
                NotifyPropertyChanged("RoundKey");                
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.UI.Models
{
    public class Cipher2CharacteristicUI : CharacteristicUI, INotifyPropertyChanged
    {
        private string _inputDiff;
        private string _inputDiffR1;
        private string _outputDiffR1;
        private string _inputDiffR2;
        private string _outputDiffR2;
        private string _expectedDiff;
        private string _probability;

        private int _inputDiffInt;
        private int _inputDiffR1Int;
        private int _outputDiffR1Int;
        private int _inputDiffR2Int;
        private int _outputDiffR2Int;
        private int _expectedDiffInt;

        private string _colBackgroundColor;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2CharacteristicUI()
        {
        }

        /// <summary>
        /// Property for the background color of the displaying row
        /// </summary>
        public string ColBackgroundColor
        {
            get { return _colBackgroundColor; }
            set
            {
                _colBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _probability
        /// </summary>
        public string Probability
        {
            get { return _probability; }
            set
            {
                _probability = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffInt
        /// </summary>
        public int InputDiffInt
        {
            get { return _inputDiffInt; }
            set
            {
                _inputDiffInt = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR1Int
        /// </summary>
        public int InputDiffR1Int
        {
            get { return _inputDiffR1Int; }
            set
            {
                _inputDiffR1Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR1Int
        /// </summary>
        public int OutputDiffR1Int
        {
            get { return _outputDiffR1Int; }
            set
            {
                _outputDiffR1Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR2Int
        /// </summary>
        public int InputDiffR2Int
        {
            get { return _inputDiffR2Int; }
            set
            {
                _inputDiffR2Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR2Int
        /// </summary>
        public int OutputDiffR2Int
        {
            get { return _outputDiffR2Int; }
            set
            {
                _outputDiffR2Int = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _expectedDiffInt
        /// </summary>
        public int ExpectedDiffInt
        {
            get { return _expectedDiffInt; }
            set
            {
                _expectedDiffInt = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiff
        /// </summary>
        public string InputDiff
        {
            get => _inputDiff;
            set
            {
                _inputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR1
        /// </summary>
        public string InputDiffR1
        {
            get => _inputDiffR1;
            set
            {
                _inputDiffR1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR1
        /// </summary>
        public string OutputDiffR1
        {
            get => _outputDiffR1;
            set
            {
                _outputDiffR1 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _inputDiffR2
        /// </summary>
        public string InputDiffR2
        {
            get => _inputDiffR2;
            set
            {
                _inputDiffR2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _outputDiffR2
        /// </summary>
        public string OutputDiffR2
        {
            get => _outputDiffR2;
            set
            {
                _outputDiffR2 = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for 
        /// </summary>
        public string ExpectedDiff
        {
            get => _expectedDiff;
            set
            {
                _expectedDiff = value;
                OnPropertyChanged();
            }
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
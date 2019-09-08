﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DCAPathFinder.UI.Tutorial2
{
    public class DifferenceDistribution : INotifyPropertyChanged
    {
        private string _inVal;

        private string _zeroOutVal;
        private string _oneOutVal;
        private string _twoOutVal;
        private string _threeOutVal;

        private string _fourOutVal;
        private string _fiveOutVal;
        private string _sixOutVal;
        private string _sevenOutVal;

        private string _eightOutVal;
        private string _nineOutVal;
        private string _tenOutVal;
        private string _elevenOutVal;

        private string _twelveOutVal;
        private string _thirteenOutVal;
        private string _fourteenOutVal;
        private string _fifteenOutVal;

        /// <summary>
        /// Constructor
        /// </summary>
        public DifferenceDistribution()
        {
        }

        /// <summary>
        /// Property for _inVal
        /// </summary>
        public string InVal
        {
            get { return _inVal; }
            set
            {
                _inVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _zeroOutVal
        /// </summary>
        public string ZeroOutVal
        {
            get { return _zeroOutVal; }
            set
            {
                _zeroOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _oneOutVal
        /// </summary>
        public string OneOutVal
        {
            get { return _oneOutVal; }
            set
            {
                _oneOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _twoOutVal
        /// </summary>
        public string TwoOutVal
        {
            get { return _twoOutVal; }
            set
            {
                _twoOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _threeOutVal
        /// </summary>
        public string ThreeOutVal
        {
            get { return _threeOutVal; }
            set
            {
                _threeOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourOutVal
        /// </summary>
        public string FourOutVal
        {
            get { return _fourOutVal; }
            set
            {
                _fourOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fiveOutVal
        /// </summary>
        public string FiveOutVal
        {
            get { return _fiveOutVal; }
            set
            {
                _fiveOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sixOutVal
        /// </summary>
        public string SixOutVal
        {
            get { return _sixOutVal; }
            set
            {
                _sixOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _sevenOutVal
        /// </summary>
        public string SevenOutVal
        {
            get { return _sevenOutVal; }
            set
            {
                _sevenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _eightOutVal
        /// </summary>
        public string EightOutVal
        {
            get { return _eightOutVal; }
            set
            {
                _eightOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _nineOutVal
        /// </summary>
        public string NineOutVal
        {
            get { return _nineOutVal; }
            set
            {
                _nineOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _tenOutVal
        /// </summary>
        public string TenOutVal
        {
            get { return _tenOutVal; }
            set
            {
                _tenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _elevenOutVal
        /// </summary>
        public string ElevenOutVal
        {
            get { return _elevenOutVal; }
            set
            {
                _elevenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _twelveOutVal
        /// </summary>
        public string TwelveOutVal
        {
            get { return _twelveOutVal; }
            set
            {
                _twelveOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _thirteenOutVal
        /// </summary>
        public string ThirteenOutVal
        {
            get { return _thirteenOutVal; }
            set
            {
                _thirteenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fourteenOutVal
        /// </summary>
        public string FourteenOutVal
        {
            get { return _fourteenOutVal; }
            set
            {
                _fourteenOutVal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _fifteenOutVal
        /// </summary>
        public string FifteenOutVal
        {
            get { return _fifteenOutVal; }
            set
            {
                _fifteenOutVal = value;
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
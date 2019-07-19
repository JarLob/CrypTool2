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

namespace DCAPathFinder.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für _16BitPermutation3RSPN.xaml
    /// </summary>
    public partial class _16BitPermutation3RSPN : UserControl, INotifyPropertyChanged
    {
        private bool[] _coloredBits;
        private string _activeColor = "Red";
        private string _inActiveColor = "Black";
        private int _activeZIndex = 1;
        private int _inactiveZIndex = 0;
        private int _activeThickness = 4;
        private int _inActiveThickness = 2;


        /// <summary>
        /// Constructor
        /// </summary>
        public _16BitPermutation3RSPN()
        {
            _coloredBits = new bool[16];
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Property for _coloredBits
        /// </summary>
        public bool[] ColoredBits
        {
            get { return _coloredBits; }
            set
            {
                _coloredBits = value;
                OnPropertyChanged();

                //call all bits
                OnPropertyChanged("BitZeroColor");
                OnPropertyChanged("BitOneColor");
                OnPropertyChanged("BitTwoColor");
                OnPropertyChanged("BitThreeColor");
                OnPropertyChanged("BitFourColor");
                OnPropertyChanged("BitFiveColor");
                OnPropertyChanged("BitSixColor");
                OnPropertyChanged("BitSevenColor");
                OnPropertyChanged("BitEightColor");
                OnPropertyChanged("BitNineColor");
                OnPropertyChanged("BitTenColor");
                OnPropertyChanged("BitElevenColor");
                OnPropertyChanged("BitTwelveColor");
                OnPropertyChanged("BitThirteenColor");
                OnPropertyChanged("BitFourteenColor");
                OnPropertyChanged("BitFifteenColor");

                //ZIndex
                OnPropertyChanged("BitZeroZIndex");
                OnPropertyChanged("BitOneZIndex");
                OnPropertyChanged("BitTwoZIndex");
                OnPropertyChanged("BitThreeZIndex");
                OnPropertyChanged("BitFourZIndex");
                OnPropertyChanged("BitFiveZIndex");
                OnPropertyChanged("BitSixZIndex");
                OnPropertyChanged("BitSevenZIndex");
                OnPropertyChanged("BitEightZIndex");
                OnPropertyChanged("BitNineZIndex");
                OnPropertyChanged("BitTenZIndex");
                OnPropertyChanged("BitElevenZIndex");
                OnPropertyChanged("BitTwelveZIndex");
                OnPropertyChanged("BitThirteenZIndex");
                OnPropertyChanged("BitFourteenZIndex");
                OnPropertyChanged("BitFifteenZIndex");

                //thickness
                OnPropertyChanged("BitZeroThickness");
                OnPropertyChanged("BitOneThickness");
                OnPropertyChanged("BitTwoThickness");
                OnPropertyChanged("BitThreenThickness");
                OnPropertyChanged("BitFourThickness");
                OnPropertyChanged("BitFiveThickness");
                OnPropertyChanged("BitSixThickness");
                OnPropertyChanged("BitSevenThickness");
                OnPropertyChanged("BitEightThickness");
                OnPropertyChanged("BitNineThickness");
                OnPropertyChanged("BitTenThickness");
                OnPropertyChanged("BitElevenThickness");
                OnPropertyChanged("BitTwelveThickness");
                OnPropertyChanged("BitThirteenThickness");
                OnPropertyChanged("BitFourteenThickness");
                OnPropertyChanged("BitFifteenThickness");


            }
        }

        #region thickness

        /// <summary>
        /// Property for BitZeroThickness
        /// </summary>
        public int BitZeroThickness
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitOneThickness
        /// </summary>
        public int BitOneThickness
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitTwoThickness
        /// </summary>
        public int BitTwoThickness
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitThreeThickness
        /// </summary>
        public int BitThreeThickness
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitFourThickness
        /// </summary>
        public int BitFourThickness
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitFiveThickness
        /// </summary>
        public int BitFiveThickness
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitSixThickness
        /// </summary>
        public int BitSixThickness
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitSevenThickness
        /// </summary>
        public int BitSevenThickness
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitEightThickness
        /// </summary>
        public int BitEightThickness
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitNineThickness
        /// </summary>
        public int BitNineThickness
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitTenThickness
        /// </summary>
        public int BitTenThickness
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitElevenThickness
        /// </summary>
        public int BitElevenThickness
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitTwelveThickness
        /// </summary>
        public int BitTwelveThickness
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitThirteenThickness
        /// </summary>
        public int BitThirteenThickness
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitFourteenThickness
        /// </summary>
        public int BitFourteenThickness
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        /// <summary>
        /// Property for BitFifteenThickness
        /// </summary>
        public int BitFifteenThickness
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeThickness;
                }
                else
                {
                    return _inActiveThickness;
                }
            }
        }

        #endregion

        #region zIndex

        /// <summary>
        /// Property for BitZeroZIndex
        /// </summary>
        public int BitZeroZIndex
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitOneZIndex
        /// </summary>
        public int BitOneZIndex
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitTwoZIndex
        /// </summary>
        public int BitTwoZIndex
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitThreeZIndex
        /// </summary>
        public int BitThreeZIndex
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitFourZIndex
        /// </summary>
        public int BitFourZIndex
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitFiveZIndex
        /// </summary>
        public int BitFiveZIndex
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitSixZIndex
        /// </summary>
        public int BitSixZIndex
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitSevenZIndex
        /// </summary>
        public int BitSevenZIndex
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitEightZIndex
        /// </summary>
        public int BitEightZIndex
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitNineZIndex
        /// </summary>
        public int BitNineZIndex
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitTenZIndex
        /// </summary>
        public int BitTenZIndex
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitElevenZIndex
        /// </summary>
        public int BitElevenZIndex
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitTwelveZIndex
        /// </summary>
        public int BitTwelveZIndex
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitThirteenZIndex
        /// </summary>
        public int BitThirteenZIndex
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitFourteenZIndex
        /// </summary>
        public int BitFourteenZIndex
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        /// <summary>
        /// Property for BitFifteenZIndex
        /// </summary>
        public int BitFifteenZIndex
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeZIndex;
                }
                else
                {
                    return _inactiveZIndex;
                }
            }
        }

        #endregion

        #region bitcolor

        /// <summary>
        /// Property for BitZeroColor
        /// </summary>
        public string BitZeroColor
        {
            get
            {
                if (ColoredBits[0])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitOneColor
        /// </summary>
        public string BitOneColor
        {
            get
            {
                if (ColoredBits[1])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitTwoColor
        /// </summary>
        public string BitTwoColor
        {
            get
            {
                if (ColoredBits[2])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitThreeColor
        /// </summary>
        public string BitThreeColor
        {
            get
            {
                if (ColoredBits[3])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitFourColor
        /// </summary>
        public string BitFourColor
        {
            get
            {
                if (ColoredBits[4])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitFiveColor
        /// </summary>
        public string BitFiveColor
        {
            get
            {
                if (ColoredBits[5])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitSixColor
        /// </summary>
        public string BitSixColor
        {
            get
            {
                if (ColoredBits[6])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitSevenColor
        /// </summary>
        public string BitSevenColor
        {
            get
            {
                if (ColoredBits[7])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitEightColor
        /// </summary>
        public string BitEightColor
        {
            get
            {
                if (ColoredBits[8])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitNineColor
        /// </summary>
        public string BitNineColor
        {
            get
            {
                if (ColoredBits[9])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitTenColor
        /// </summary>
        public string BitTenColor
        {
            get
            {
                if (ColoredBits[10])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitElevenColor
        /// </summary>
        public string BitElevenColor
        {
            get
            {
                if (ColoredBits[11])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitTwelveColor
        /// </summary>
        public string BitTwelveColor
        {
            get
            {
                if (ColoredBits[12])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitThirteenColor
        /// </summary>
        public string BitThirteenColor
        {
            get
            {
                if (ColoredBits[13])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitFourteenColor
        /// </summary>
        public string BitFourteenColor
        {
            get
            {
                if (ColoredBits[14])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        /// <summary>
        /// Property for BitFifteenColor
        /// </summary>
        public string BitFifteenColor
        {
            get
            {
                if (ColoredBits[15])
                {
                    return _activeColor;
                }
                else
                {
                    return _inActiveColor;
                }
            }
        }

        #endregion

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

﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DCAPathVisualiser.UI.Cipher2
{
    /// <summary>
    /// Interaktionslogik für Cipher2Characteristic.xaml
    /// </summary>
    public partial class Cipher2Characteristic : UserControl, INotifyPropertyChanged
    {
        private string _inputDiff;

        private bool[] _bitsKeyRoundOne;
        private bool[] _permutationBitsRoundOne;

        private bool[] _bitsKeyRoundTwo;
        private bool[] _permutationBitsRoundTwo;

        private bool[] _bitsKeyRoundThree;
        private bool[] _bitsKeyRoundFour;

        private string _round1InputDiff;
        private string _round1OutputDiff;

        private string _round2InputDiff;
        private string _round2OutputDiff;

        private string _round3InputDiff;

        private bool[] _activeSBoxes;

        private int _round;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2Characteristic()
        {
            _bitsKeyRoundOne = new bool[16];
            _permutationBitsRoundOne = new bool[16];

            _bitsKeyRoundTwo = new bool[16];
            _permutationBitsRoundTwo = new bool[16];

            _bitsKeyRoundThree = new bool[16];
            _bitsKeyRoundFour = new bool[16];

            _activeSBoxes = new bool[4];

            DataContext = this;
            InitializeComponent();

            RenderView();
        }

        /// <summary>
        /// Renders the view
        /// </summary>
        private void RenderView()
        {
            KeyRound1.ColoredBits = BitsKeyRoundOne;
            PermutationRound1.ColoredBits = PermutationBitsRoundOne;

            KeyRound2.ColoredBits = BitsKeyRoundTwo;
            PermutationRound2.ColoredBits = PermutationBitsRoundTwo;

            KeyRound3.ColoredBits = BitsKeyRoundThree;
            KeyRound4.ColoredBits = BitsKeyRoundFour;

            //set active SBoxes: an active SBox is red colored
            switch (Round)
            {
                case 1:
                {

                }
                    break;
                case 2:
                {
                    SBox1Round2.IsActive = ActiveSBoxes[0];
                    SBox2Round2.IsActive = ActiveSBoxes[1];
                    SBox3Round2.IsActive = ActiveSBoxes[2];
                    SBox4Round2.IsActive = ActiveSBoxes[3];

                    SBox1Round3.IsActive = false;
                    SBox2Round3.IsActive = false;
                    SBox3Round3.IsActive = false;
                    SBox4Round3.IsActive = false;
                }
                    break;
                case 3:
                {
                    SBox1Round2.IsActive = false;
                    SBox2Round2.IsActive = false;
                    SBox3Round2.IsActive = false;
                    SBox4Round2.IsActive = false;

                    SBox1Round3.IsActive = ActiveSBoxes[0];
                    SBox2Round3.IsActive = ActiveSBoxes[1];
                    SBox3Round3.IsActive = ActiveSBoxes[2];
                    SBox4Round3.IsActive = ActiveSBoxes[3];
                }
                    break;
            }
        }

        /// <summary>
        /// Property for Round
        /// </summary>
        public int Round
        {
            get { return _round; }
            set
            {
                _round = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for ActiveSBoxes
        /// </summary>
        public bool[] ActiveSBoxes
        {
            get { return _activeSBoxes; }
            set
            {
                _activeSBoxes = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _inputDiff
        /// </summary>
        public string InputDiff
        {
            get { return _inputDiff; }
            set
            {
                _inputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round1InputDiff
        /// </summary>
        public string Round1InputDiff
        {
            get { return _round1InputDiff; }
            set
            {
                _round1InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round1OutputDiff
        /// </summary>
        public string Round1OutputDiff
        {
            get { return _round1OutputDiff; }
            set
            {
                _round1OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round2InputDiff
        /// </summary>
        public string Round2InputDiff
        {
            get { return _round2InputDiff; }
            set
            {
                _round2InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round2OutputDiff
        /// </summary>
        public string Round2OutputDiff
        {
            get { return _round2OutputDiff; }
            set
            {
                _round2OutputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _round3InputDiff
        /// </summary>
        public string Round3InputDiff
        {
            get { return _round3InputDiff; }
            set
            {
                _round3InputDiff = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundOne
        /// </summary>
        public bool[] BitsKeyRoundOne
        {
            get { return _bitsKeyRoundOne; }
            set
            {
                _bitsKeyRoundOne = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundOne
        /// </summary>
        public bool[] PermutationBitsRoundOne
        {
            get { return _permutationBitsRoundOne; }
            set
            {
                _permutationBitsRoundOne = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundTwo
        /// </summary>
        public bool[] BitsKeyRoundTwo
        {
            get { return _bitsKeyRoundTwo; }
            set
            {
                _bitsKeyRoundTwo = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _permutationBitsRoundTwo
        /// </summary>
        public bool[] PermutationBitsRoundTwo
        {
            get { return _permutationBitsRoundTwo; }
            set
            {
                _permutationBitsRoundTwo = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundThree
        /// </summary>
        public bool[] BitsKeyRoundThree
        {
            get { return _bitsKeyRoundThree; }
            set
            {
                _bitsKeyRoundThree = value;
                OnPropertyChanged();
                RenderView();
            }
        }

        /// <summary>
        /// Property for _bitsKeyRoundFour
        /// </summary>
        public bool[] BitsKeyRoundFour
        {
            get { return _bitsKeyRoundFour; }
            set
            {
                _bitsKeyRoundFour = value;
                OnPropertyChanged();
                RenderView();
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

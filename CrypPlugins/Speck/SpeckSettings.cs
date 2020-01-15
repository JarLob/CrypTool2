/*
   Copyright 2020 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Speck
{
    /// <summary>
    /// Enumeration for all Speck variants
    /// </summary>
    public enum SpeckParameters
    {
        Speck32_64,
        Speck48_72,
        Speck48_96,
        Speck64_96,
        Speck64_128,
        Speck96_96,
        Speck96_144,
        Speck128_128,
        Speck128_192,
        Speck128_256
    }

    /// <summary>
    /// Enumeration for all implemented modes of operation
    /// </summary>
    public enum ModeOfOperation
    {
        ElectronicCodeBook
    }

    /// <summary>
    /// Enumeration for operating mode
    /// </summary>
    public enum OperatingMode
    {
        Encrypt,
        Decrypt
    }

    /// <summary>
    /// Enumeration for the padding mode
    /// </summary>
    public enum PaddingMode
    {
        Zeros
    }

    /// <summary>
    /// Settings class for Speck
    /// </summary>
    public class SpeckSettings : ISettings
    {
        #region Private Variables

        private int _blockSize_2n = 32;
        private int _keySize_mn = 64;
        private int _wordSize_n = 16;
        private int _keyWords_m = 4;
        private int _leftShift_alpha = 7;
        private int _rightShift_beta = 2;
        private int _rounds_T = 22;
        private SpeckParameters _currentSpeckParameters = SpeckParameters.Speck32_64;
        private ModeOfOperation _modeOfOperation = ModeOfOperation.ElectronicCodeBook;
        private OperatingMode _operatingMode = OperatingMode.Encrypt;
        private PaddingMode _paddingmode = PaddingMode.Zeros;

        #endregion

        #region Properties

        public int BlockSize_2n
        {
            get { return _blockSize_2n; }
        }

        public int KeySize_mn
        {
            get { return _keySize_mn; }
        }

        public int WordSize_n
        {
            get { return _wordSize_n; }
        }

        public int KeyWords_m
        {
            get { return _keyWords_m; }
        }

        public int LeftShift_alpha
        {
            get { return _leftShift_alpha; }
        }

        public int RightShift_beta
        {
            get { return _rightShift_beta; }
        }

        public int Rounds_T
        {
            get { return _rounds_T; }
        }

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Property for block and key size
        /// </summary>
        [TaskPane("ChoiceOfVariant", "ChoiceOfVariantToolTip", "ChoiceOfVariantGroup", 1, false, ControlType.ComboBox, new string[] { "Speck32_64", "Speck48_72", "Speck48_96", "Speck64_96", "Speck64_128", "Speck96_96", "Speck96_144", "Speck128_128", "Speck128_192", "Speck128_256" })]
        public SpeckParameters ChoiceOfVariant
        {
            get
            {
                return _currentSpeckParameters;
            }
            set
            {
                if (_currentSpeckParameters != value)
                {
                    switch (value)
                    {
                        case SpeckParameters.Speck32_64:

                            _blockSize_2n = 32;
                            _keySize_mn = 64;
                            _wordSize_n = 16;
                            _keyWords_m = 4;
                            _leftShift_alpha = 7;
                            _rightShift_beta = 2;
                            _rounds_T = 22;

                            break;

                        case SpeckParameters.Speck48_72:

                            _blockSize_2n = 48;
                            _keySize_mn = 72;
                            _wordSize_n = 24;
                            _keyWords_m = 3;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 22;

                            break;

                        case SpeckParameters.Speck48_96:

                            _blockSize_2n = 48;
                            _keySize_mn = 96;
                            _wordSize_n = 24;
                            _keyWords_m = 4;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 23;

                            break;

                        case SpeckParameters.Speck64_96:

                            _blockSize_2n = 64;
                            _keySize_mn = 96;
                            _wordSize_n = 32;
                            _keyWords_m = 3;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 26;

                            break;

                        case SpeckParameters.Speck64_128:

                            _blockSize_2n = 64;
                            _keySize_mn = 128;
                            _wordSize_n = 32;
                            _keyWords_m = 4;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 27;

                            break;

                        case SpeckParameters.Speck96_96:

                            _blockSize_2n = 96;
                            _keySize_mn = 96;
                            _wordSize_n = 48;
                            _keyWords_m = 2;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 28;

                            break;

                        case SpeckParameters.Speck96_144:

                            _blockSize_2n = 96;
                            _keySize_mn = 144;
                            _wordSize_n = 48;
                            _keyWords_m = 3;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 29;

                            break;

                        case SpeckParameters.Speck128_128:

                            _blockSize_2n = 128;
                            _keySize_mn = 128;
                            _wordSize_n = 64;
                            _keyWords_m = 2;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 32;

                            break;

                        case SpeckParameters.Speck128_192:

                            _blockSize_2n = 128;
                            _keySize_mn = 192;
                            _wordSize_n = 64;
                            _keyWords_m = 3;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 33;

                            break;

                        case SpeckParameters.Speck128_256:

                            _blockSize_2n = 128;
                            _keySize_mn = 256;
                            _wordSize_n = 64;
                            _keyWords_m = 4;
                            _leftShift_alpha = 8;
                            _rightShift_beta = 3;
                            _rounds_T = 34;

                            break;

                        default:
                            break;
                    }

                    OnPropertyChanged("ChoiceOfVariant");
                }
            }
        }

        /// <summary>
        /// Property for mode of operation
        /// </summary>
        [TaskPane("ChoiceOfModeOfOperation", "ChoiceOfModeOfOperationToolTip", "ChoiceOfModeOfOperationGroup", 1, false, ControlType.ComboBox, new string[] { "ElectronicCodeBook" })]
        public ModeOfOperation OperationMode
        {
            get { return _modeOfOperation; }
            set
            {
                if (_modeOfOperation != value)
                {
                    _modeOfOperation = value;
                    OnPropertyChanged("OperationMode");
                }
            }
        }

        /// <summary>
        /// Property to set if the component encrypts or decrypts the input
        /// </summary>
        [TaskPane("ChoiceOfOperatingMode", "ChoiceOfOperatingModeToolTip", "ChoiceOfOperatingModeGroup", 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public OperatingMode OpMode
        {
            get { return _operatingMode; }
            set
            {
                if (_operatingMode != value)
                {
                    _operatingMode = value;
                    OnPropertyChanged("OpMode");
                }
            }
        }

        /// <summary>
        /// Property to set the padding mode of the cipher
        /// </summary>
        [TaskPane("ChoiceOfPaddingMode", "ChoiceOfPaddingModeToolTip", "ChoiceOfPaddingModeGroup", 1, false, ControlType.ComboBox, new string[] { "PaddingList1" })]
        public PaddingMode PadMode
        {
            get { return _paddingmode; }
            set
            {
                if (_paddingmode != value)
                {
                    _paddingmode = value;
                    OnPropertyChanged("PadMode");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}

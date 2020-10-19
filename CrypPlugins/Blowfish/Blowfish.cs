/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System;
using Cryptool.Plugins.Blowfish.Properties;
using static Cryptool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace Cryptool.Plugins.Blowfish
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.Blowfish.Properties.Resources", "PluginCaption", "PluginTooltip", "", "Blowfish/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Blowfish : ICrypComponent
    {
        #region Private Variables

        private BlowfishSettings _BlowfishSettings = new BlowfishSettings();

        private byte[] _InputKey;
        private byte[] _InputIV;
        private ICryptoolStream _OutputStreamWriter;
        private ICryptoolStream _InputStream;
        
        private bool _stop = false;
                
        private byte[] _lastInputBlock = null; //needed for the visualization of the cipher; we only show the last encrypted/decrypted block

        #endregion

        #region Data Properties

        public ISettings Settings
        {
            get { return _BlowfishSettings; }
            set { _BlowfishSettings = (BlowfishSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get; 
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get
            {
                return _InputKey;
            }
            set
            {
                _InputKey = value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get
            {
                return _InputIV;
            }
            set
            {
                _InputIV = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return _OutputStreamWriter;
            }
            set
            {
                // empty
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Blowfish()
        {            
        }
       
        #region IPlugin Members
      
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {       
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            
            _lastInputBlock = null;
            _OutputStreamWriter = null;
            _stop = false;           

            CheckKeyLength();

            //Select crypto function based on algorithm, blockmode, and action
            BlockCipher blockCipher = null;
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.BlockMode == BlockMode.CFB)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }           
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.BlockMode == BlockMode.OFB)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }         
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.Action == CipherAction.Encrypt)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish && _BlowfishSettings.Action == CipherAction.Decrypt)
            {
                BlowfishAlgorithm algorithm = new BlowfishAlgorithm();
                algorithm.KeySchedule(_InputKey);
                blockCipher = new BlockCipher(algorithm.Decrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.BlockMode == BlockMode.CFB)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.BlockMode == BlockMode.OFB)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.Action == CipherAction.Encrypt)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Encrypt);
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish && _BlowfishSettings.Action == CipherAction.Decrypt)
            {
                TwofishAlgorithm algorithm = new TwofishAlgorithm(_InputKey);
                blockCipher = new BlockCipher(algorithm.Decrypt);
            }

            //Check, if we found a crypto function that we can use
            //this error should NEVER occur. Only in case someone adds functionality and misses
            //to create a valid configuration
            if (blockCipher == null)
            {
                GuiLogMessage("No crypto function could be selected based on your settings", NotificationLevel.Error);
                return;
            }

            if (_BlowfishSettings.Action == CipherAction.Encrypt)
            {
                //in case of encryption, we have to add padding
                _InputStream = AppendPadding(InputStream, _BlowfishSettings.Padding, 8);
            }
            else
            {
                //with decryption, we have to do nothing
                _InputStream = InputStream;
            }

            //parity rule is: if parity is used, zero the least significant bit of each byte
            if (_BlowfishSettings.EnableKeyParityBits == true)
            {
                for (int i = 0; i < _InputKey.Length; i++)
                {
                    _InputKey[i] = (byte)(_InputKey[i] & 254);
                }
            }

            int blocksize = 8;
            if(_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                blocksize = 16;
            }

            switch (_BlowfishSettings.BlockMode)
            {
                case BlockMode.ECB:
                    ExecuteECB(blockCipher,
                        _BlowfishSettings.Action,
                        ref _InputStream,
                        ref _OutputStreamWriter,
                        _InputKey,
                        _BlowfishSettings.Padding,
                        ref _stop,
                        ProgressChanged,
                        ref _lastInputBlock,
                        blocksize);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    ExecuteCBC(blockCipher,
                       _BlowfishSettings.Action,
                       ref _InputStream,
                       ref _OutputStreamWriter,
                       _InputKey,
                       _InputIV,
                       _BlowfishSettings.Padding,
                       ref _stop,
                       ProgressChanged,
                       ref _lastInputBlock,
                       blocksize);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    ExecuteCFB(blockCipher,
                      _BlowfishSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _BlowfishSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock,
                      blocksize);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    ExecuteOFB(blockCipher,
                      _BlowfishSettings.Action,
                      ref _InputStream,
                      ref _OutputStreamWriter,
                      _InputKey,
                      _InputIV,
                      _BlowfishSettings.Padding,
                      ref _stop,
                      ProgressChanged,
                      ref _lastInputBlock,
                      blocksize);
                    break;   
                default:
                    throw new NotImplementedException(string.Format("The mode {0} has not been implemented.", _BlowfishSettings.BlockMode));
            }
           
            OnPropertyChanged("OutputStream");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Checks the given key and extends/cuts it, if needed
        /// </summary>
        private void CheckKeyLength()
        {
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Blowfish)
            {
                //blowfish specifies key lengths between 32 bit (4 bytes) and 448 bit (56 bytes)
                //usual case is 16 byte (= 128 bit key)
                if (_InputKey.Length < 4)
                {
                    byte[] key = new byte[4];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 4), NotificationLevel.Warning);
                    _InputKey = key;
                }
                if (_InputKey.Length > 56)
                {
                    byte[] key = new byte[8];
                    Array.Copy(_InputKey, 0, key, 0, 56);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_long, _InputKey.Length, 56), NotificationLevel.Warning);
                    _InputKey = key;
                }
            }
            else if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                //twofish specifies keylengths of 128 bit (16 byte), 192 bit (24 byte), and 256 bit (32 byte)
                if (_InputKey.Length < 16)
                {
                    byte[] key = new byte[16];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 16), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 16 && _InputKey.Length < 24)
                {
                    byte[] key = new byte[24];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 24), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length != 16 && _InputKey.Length != 24 && _InputKey.Length < 32)
                {
                    byte[] key = new byte[32];
                    Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_short, _InputKey.Length, 32), NotificationLevel.Warning);
                    _InputKey = key;
                }
                else if (_InputKey.Length > 32)
                {
                    byte[] key = new byte[32];
                    Array.Copy(_InputKey, 0, key, 0, 32);
                    GuiLogMessage(string.Format(Resources.Blowfish_Execute_Key_too_long, _InputKey.Length, 32), NotificationLevel.Warning);
                    _InputKey = key;
                }
            }
        }

        /// <summary>
        /// Checks the given initialization vector and extends/cuts it, if needed
        /// </summary>
        private void CheckIV()
        {
            int blocksize = 8;
            if (_BlowfishSettings.BlowfishAlgorithmType == BlowfishAlgorithmType.Twofish)
            {
                blocksize = 16;
            }
            //if no IV is given, we set it to an array with length 0
            if (_InputIV == null)
            {
                _InputIV = new byte[0];
            }
            //Extend or cut IV to length 8
            if (_InputIV.Length < blocksize)
            {
                byte[] iv = new byte[blocksize];
                Array.Copy(_InputIV, 0, iv, 0, _InputIV.Length);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckIV_IV_too_short, _InputIV.Length, blocksize), NotificationLevel.Warning);
                _InputIV = iv;
            }
            if (_InputIV.Length > blocksize)
            {
                byte[] iv = new byte[blocksize];
                Array.Copy(_InputIV, 0, iv, 0, 8);
                GuiLogMessage(string.Format(Resources.Blowfish_CheckIV_IV_too_long, _InputIV.Length, blocksize), NotificationLevel.Warning);
                _InputIV = iv;
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {

        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        /// <summary>
        /// Helper method to invoke property change event
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Helper method to invoke progress changed event
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}

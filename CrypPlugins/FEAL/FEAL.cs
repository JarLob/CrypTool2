/*
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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System;
using System.Windows.Threading;
using System.Threading;
using Cryptool.Plugins.FEAL.Properties;

namespace Cryptool.Plugins.FEAL
{
    [Author("Nils Kopal", "Nils.Kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.Feal.Properties.Resources", "PluginCaption", "PluginTooltip", "", "FEAL/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class FEAL : ICrypComponent
    {
        #region Private Variables

        private FEALSettings _FEALSettings = new FEALSettings();
        private UserControl _presentation = new FEALPresentation();

        private byte[] _InputKey;
        private byte[] _InputIV;
        private ICryptoolStream _OutputStreamWriter;
        private ICryptoolStream _InputStream;
        
        private bool _stop = false;
        
        private delegate byte[] CryptoFunction(byte[] text, byte[] key);
        
        private byte[] _lastInputBlock = null; //needed for the visualization of the cipher; we only show the last encrypted/decrypted block

        #endregion

        #region Data Properties

        public ISettings Settings
        {
            get { return _FEALSettings; }
            set { _FEALSettings = (FEALSettings)value; }
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
        public FEAL()
        {
            _FEALSettings.PropertyChanged += _FEALSettings_PropertyChanged;
            UpdatePresentation();
        }

        /// <summary>
        /// Called, when a property of the settings changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _FEALSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //shows the visualization of the selected FEAL algorithm type
            if (e.PropertyName.Equals("FealAlgorithmType"))
            {
                UpdatePresentation();
            }
        }

        /// <summary>
        /// Updates the presentation by only showing the selected presentation
        /// </summary>
        private void UpdatePresentation()
        {
            FEALPresentation fealPresentation = (FEALPresentation)_presentation;

            fealPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4)
                    {
                        fealPresentation.ShowFEAL4Presentation();
                    }
                    else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8)
                    {
                        fealPresentation.ShowFEAL8Presentation();
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Exception occured during updating of visualization: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);        
        }

        #region IPlugin Members
      
        public UserControl Presentation
        {
            get { return _presentation; }
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

            //Extend or cut key to length 8
            if (_InputKey.Length < 8)
            {
                byte[] key = new byte[8];
                Array.Copy(_InputKey, 0, key, 0, _InputKey.Length);
                GuiLogMessage(String.Format(Resources.FEAL_Execute_Key_too_short,_InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }
            if (_InputKey.Length > 8)
            {
                byte[] key = new byte[8];
                Array.Copy(_InputKey, 0, key, 0, 8);
                GuiLogMessage(String.Format(Resources.FEAL_Execute_Key_too_long, _InputKey.Length), NotificationLevel.Warning);
                _InputKey = key;
            }

            //Select crypto function based on algorithm, blockmode, and action
            CryptoFunction cryptoFunction = null;
            if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.BlockMode == BlockMode.CFB)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.BlockMode == BlockMode.CFB)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL8_EncryptBlock); //uses encryption since it only XORs result of cipher
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.BlockMode == BlockMode.OFB)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.BlockMode == BlockMode.OFB)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL8_EncryptBlock); //uses encryption since it only XORs result of cipher
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.Action == Action.Encrypt)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL4_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL4 && _FEALSettings.Action == Action.Decrypt)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL4_DecryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.Action == Action.Encrypt)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL8_EncryptBlock);
            }
            else if (_FEALSettings.FealAlgorithmType == FealAlgorithmType.FEAL8 && _FEALSettings.Action == Action.Decrypt)
            {
                cryptoFunction = new CryptoFunction(FEAL_Algorithms.FEAL8_DecryptBlock);
            }            

            //Check, if we found a crypto function that we can use
            //this error should NEVER occur. Only in case someone adds functionality and misses
            //to create a valid configuration
            if (cryptoFunction == null)
            {
                GuiLogMessage("No crypto function could be selected based on your settings", NotificationLevel.Error);
                return;
            }

            if (_FEALSettings.Action == Action.Encrypt)
            {
                //in case of encryption, we have to add padding
                _InputStream = BlockCipherHelper.AppendPadding(InputStream, _FEALSettings.Padding, 8);
            }
            else
            {
                //with decryption, we have to do nothing
                _InputStream = InputStream;
            }

            //parity rule is: if parity is used, zero the least significant bit of each byte
            if (_FEALSettings.EnableKeyParityBits == true)
            {
                for (int i = 0; i < _InputKey.Length; i++)
                {
                    _InputKey[i] = (byte)(_InputKey[i] & 254);
                }
            }

            switch (_FEALSettings.BlockMode)
            {
                case BlockMode.ECB:
                    Execute_ECB(cryptoFunction);
                    break;
                case BlockMode.CBC:
                    CheckIV();
                    Execute_CBC(cryptoFunction);
                    break;
                case BlockMode.CFB:
                    CheckIV();
                    Execute_CFB(cryptoFunction);
                    break;
                case BlockMode.OFB:
                    CheckIV();
                    Execute_OFB(cryptoFunction);
                    break;               
            }

            if (_lastInputBlock != null)
            {
                FEALPresentation fealPresentation = (FEALPresentation) _presentation;
                fealPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        if (_FEALSettings.Action == Action.Encrypt)
                        {
                            fealPresentation.VisualizeEncryptBlock(_lastInputBlock, _InputKey);
                        }
                        else
                        {
                            fealPresentation.VisualizeDecryptBlock(_lastInputBlock, _InputKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(String.Format("Exception occured during building of visualization: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);
            }
            OnPropertyChanged("OutputStream");

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Checks the given initialization vector and extends/cuts it, if needed
        /// </summary>
        private void CheckIV()
        {
            //if no IV is give, we set it to an array with length 0
            if (_InputIV == null)
            {
                _InputIV = new byte[0];
            }
            //Extend or cut key to length 8
            if (_InputIV.Length < 8)
            {
                byte[] iv = new byte[8];
                Array.Copy(_InputIV, 0, iv, 0, _InputIV.Length);
                GuiLogMessage(String.Format(Resources.FEAL_CheckIV_IV_too_short, _InputIV.Length), NotificationLevel.Warning);
                _InputIV = iv;
            }
            if (_InputIV.Length > 8)
            {
                byte[] iv = new byte[8];
                Array.Copy(_InputIV, 0, iv, 0, 8);
                GuiLogMessage(String.Format(Resources.FEAL_CheckIV_IV_too_long, _InputIV.Length), NotificationLevel.Warning);
                _InputIV = iv;
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using ECB
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_ECB(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _InputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {

                    byte[] inputBlock = new byte[8];
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, 8 - readcount)) < 8 &&
                               reader.Position < reader.Length && !_stop);
                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == 8)
                        {
                            outputblock = cryptoFunction(inputBlock, _InputKey);
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            byte[] block = new byte[8];
                            Array.Copy(inputBlock, 0, block, 0, readcount);
                            outputblock = cryptoFunction(block, _InputKey);
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && _FEALSettings.Action == Action.Decrypt && _FEALSettings.Padding != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputblock, 8, _FEALSettings.Padding, 8);
                            if (valid != 8)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if(valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            _lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    _OutputStreamWriter = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using CBC
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_CBC(CryptoFunction cryptoFunction)
        {            
            using (CStreamReader reader = _InputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {

                    byte[] lastBlock = InputIV;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        byte[] inputBlock = new byte[8];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, 8 - readcount)) < 8 &&
                               reader.Position < reader.Length && !_stop);
                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == 8)
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {
                                inputBlock = FEAL_Algorithms.XOR(inputBlock, lastBlock);
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                                lastBlock = outputblock;
                            }
                            else
                            {
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, lastBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {
                                byte[] block = new byte[8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                inputBlock = FEAL_Algorithms.XOR(block, lastBlock);
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                            }
                            else
                            {
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && _FEALSettings.Action == Action.Decrypt && _FEALSettings.Padding != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputblock, 8, _FEALSettings.Padding, 8);
                            if (valid != 8)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if(valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            _lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    _OutputStreamWriter = writer;
                }
            }
        }       

        /// <summary>
        /// Encrypts/Decrypts using CFB
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_CFB(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _InputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] lastBlock = InputIV;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        byte[] inputBlock = new byte[8];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, 8 - readcount)) < 8 &&
                               reader.Position < reader.Length && !_stop) ;
                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == 8)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {                                
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, inputBlock);                             
                                lastBlock = outputblock;
                            }
                            else
                            {
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, inputBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {
                                byte[] block = new byte[8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, block);
                            }
                            else
                            {
                                byte[] block = new byte[8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && _FEALSettings.Action == Action.Decrypt && _FEALSettings.Padding != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputblock, 8, _FEALSettings.Padding, 8);
                            if (valid != 8)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if(valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            _lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    _OutputStreamWriter = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using OFB
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_OFB(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _InputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] lastBlock = InputIV;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        byte[] inputBlock = new byte[8];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, 8 - readcount)) < 8 &&
                               reader.Position < reader.Length && !_stop) ;
                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == 8)
                        {
                            //Compute XOR with lastblock for OFB mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                lastBlock = outputblock;
                                outputblock = FEAL_Algorithms.XOR(outputblock, inputBlock);                                
                            }
                            else
                            {
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                lastBlock = outputblock;
                                outputblock = FEAL_Algorithms.XOR(outputblock, inputBlock);                                
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (_FEALSettings.Action == Action.Encrypt)
                            {
                                byte[] block = new byte[8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(lastBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, block);
                            }
                            else
                            {
                                byte[] block = new byte[8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(inputBlock, _InputKey);
                                outputblock = FEAL_Algorithms.XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && _FEALSettings.Action == Action.Decrypt && _FEALSettings.Padding != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputblock, 8, _FEALSettings.Padding, 8);
                            if (valid != 8)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if (valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            _lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    _OutputStreamWriter = writer;
                }
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

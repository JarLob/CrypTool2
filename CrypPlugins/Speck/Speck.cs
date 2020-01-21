﻿/*
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
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Speck;
using Speck.Properties;

namespace Cryptool.Plugins.Speck
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("Speck.Properties.Resources", "PluginCaption", "PluginTooltip", "Speck/userdoc.xml", "Speck/Images/IC_Speck.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Speck : ICrypComponent
    {
        #region Private Variables

        private readonly SpeckSettings settings = new SpeckSettings();
        private ICryptoolStream _inputStream;
        private ICryptoolStream _outputStream;
        private byte[] _inputKey;
        private byte[] _inputIV;

        private bool _stop = false;

        private delegate byte[] CryptoFunction(byte[] text, byte[] key);

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get { return _inputStream; }
            set
            {
                _inputStream = value;
                OnPropertyChanged("InputStream");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get
            {
                return _inputKey;
            }
            set
            {
                _inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return _outputStream;
            }
            set
            {
                _outputStream = value;
                OnPropertyChanged("OutputStream");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get
            {
                return _inputIV;
            }
            set
            {
                _inputIV = value;
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
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

            _outputStream = null;

            //Cut key to length 
           if (_inputKey.Length > (settings.KeySize_mn / 8))
            {
                byte[] key = new byte[settings.KeySize_mn / 8];
                Array.Copy(_inputKey, 0, key, 0, key.Length);
                GuiLogMessage(String.Format(Resources.Speck_Execute_Key_too_long, _inputKey.Length, key.Length), NotificationLevel.Warning);
                _inputKey = key;
            }else if (_inputKey.Length < (settings.KeySize_mn / 8))
           {
                GuiLogMessage(String.Format(Resources.Speck_Execute_Key_too_short, _inputKey.Length, (settings.KeySize_mn / 8)), NotificationLevel.Error);
                return;
           }

            //Check for padding
            if (settings.OpMode == OperatingMode.Encrypt && (_inputStream.Length % (settings.BlockSize_2n / 8)) != 0)
            {
                if (settings.PadMode != BlockCipherHelper.PaddingType.None)
                {
                    //in case of encryption, we have to add padding
                    _inputStream = BlockCipherHelper.AppendPadding(_inputStream, settings.PadMode, (settings.BlockSize_2n / 8));
                    GuiLogMessage(String.Format(Resources.Speck_Input_padded, settings.PadMode), NotificationLevel.Warning);
                }
                else
                {
                    int missingBytes = (int)((settings.BlockSize_2n / 8) -
                                        (_inputStream.Length % (settings.BlockSize_2n / 8)));

                    //check if a single or multiple byte 
                    GuiLogMessage(
                        missingBytes == 1
                            ? String.Format(Resources.Speck_Input_too_short_singleByte,
                                ((settings.BlockSize_2n / 8) - (_inputStream.Length % (settings.BlockSize_2n / 8))))
                            : String.Format(Resources.Speck_Input_too_short_multipleByte,
                                ((settings.BlockSize_2n / 8) - (_inputStream.Length % (settings.BlockSize_2n / 8)))),
                        NotificationLevel.Warning);
                    return;
                }
            }

            //Select crypto function based on algorithm and action
            CryptoFunction cryptoFunction = null;

           if (settings.ChoiceOfVariant == SpeckParameters.Speck32_64)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck32_64_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck32_64_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck48_72)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck48_72_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck48_72_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck48_96)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck48_96_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck48_96_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck64_96)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck64_96_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck64_96_Decryption);
           } 
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck64_128)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck64_128_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck64_128_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck96_96)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck96_96_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck96_96_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck96_144)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck96_144_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck96_144_Decryption);
           } 
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_128)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck128_128_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck128_128_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_192)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck128_192_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck128_192_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_256)
           {
               if (settings.OpMode == OperatingMode.Encrypt || settings.OperationMode == ModeOfOperation.CipherFeedback || settings.OperationMode == ModeOfOperation.OutputFeedback)
                   cryptoFunction = SpeckCiphers.Speck128_256_Encryption;
               else
                   cryptoFunction = new CryptoFunction(SpeckCiphers.Speck128_256_Decryption);
           }

           //Check, if we found a crypto function that we can use
           //this error should NEVER occur. 
           if (cryptoFunction == null)
           {
               GuiLogMessage(Resources.Speck_no_cryptofunction, NotificationLevel.Error);
               return;
           }

            //Select block mode and execute cryptoFunction
            switch (settings.OperationMode)
           {
                case ModeOfOperation.ElectronicCodeBook:
                    Execute_ECB(cryptoFunction);
                    break;
                case ModeOfOperation.CipherBlockChaining:
                    CheckIV();
                    Execute_CBC(cryptoFunction);
                    break;
                case ModeOfOperation.CipherFeedback:
                    CheckIV();
                    Execute_CFB(cryptoFunction);
                    break;

                default:
                    throw new NotImplementedException(String.Format(Resources.Speck_blockmode_not_implemented, settings.OperationMode));
            }


            ProgressChanged(1, 1);
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

        #region Encryption/Decryption

        private void CheckIV()
        {
            int neededBlockLength = settings.BlockSize_2n / 8;

            //if no IV is given, we set it to an array with needed block length
            if (_inputIV == null)
            {
                _inputIV = new byte[neededBlockLength];
                GuiLogMessage(String.Format(Resources.Speck_No_IV_was_given), NotificationLevel.Warning);
            }

            if (_inputIV.Length < neededBlockLength)
            {
                byte[] iv = new byte[neededBlockLength];
                Array.Copy(_inputIV, 0, iv, 0, _inputIV.Length);
                GuiLogMessage(String.Format(Resources.Speck_IV_too_short, _inputIV.Length), NotificationLevel.Warning);
                _inputIV = iv;
            }

            if(_inputIV.Length > neededBlockLength)
            {
                byte[] iv = new byte[neededBlockLength];
                Array.Copy(_inputIV, 0, iv, 0, neededBlockLength);
                GuiLogMessage(String.Format(Resources.Speck_IV_too_long, _inputIV.Length), NotificationLevel.Warning);
                _inputIV = iv;
            }

        }

        /// <summary>
        /// Encrypts/Decrypts using ECB
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_ECB(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] inputBlock = new byte[settings.BlockSize_2n / 8];
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, inputBlock.Length - readcount)) < inputBlock.Length &&
                               reader.Position < reader.Length && !_stop)
                        {
                        }

                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputBlock = null;
                        outputBlock = cryptoFunction(inputBlock, _inputKey);

                        //if we (de)crypted something, we output it
                        if (outputBlock != null)
                        {
                            writer.Write(outputBlock, 0, outputBlock.Length);
                        }

                    }

                    writer.Flush();
                    OutputStream = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using CBC
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_CBC(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {

                    byte[] lastBlock = InputIV;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        byte[] inputBlock = new byte[settings.BlockSize_2n / 8];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, (settings.BlockSize_2n / 8) - readcount)) < (settings.BlockSize_2n / 8) &&
                               reader.Position < reader.Length && !_stop) ;
                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputBlock = null;
                        //we read a complete block
                        if (readcount == (settings.BlockSize_2n / 8))
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (settings.OpMode == OperatingMode.Encrypt)
                            {
                                inputBlock = SpeckCiphers.XOR(inputBlock, lastBlock);
                                outputBlock = cryptoFunction(inputBlock, _inputKey);
                                lastBlock = outputBlock;
                            }
                            else
                            {
                                outputBlock = cryptoFunction(inputBlock, _inputKey);
                                outputBlock = SpeckCiphers.XOR(outputBlock, lastBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (settings.OpMode == OperatingMode.Encrypt)
                            {
                                byte[] block = new byte[settings.BlockSize_2n / 8];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                inputBlock = SpeckCiphers.XOR(block, lastBlock);
                                outputBlock = cryptoFunction(inputBlock, _inputKey);
                            }
                            else
                            {
                                outputBlock = cryptoFunction(inputBlock, _inputKey);
                                outputBlock = SpeckCiphers.XOR(outputBlock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && settings.OpMode == OperatingMode.Decrypt && settings.PadMode != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputBlock, (settings.BlockSize_2n / 8), settings.PadMode, (settings.BlockSize_2n / 8));
                            if (valid != settings.BlockSize_2n / 8)
                            {
                                byte[] newOutputBlock = new byte[valid];
                                if (outputBlock != null)
                                    Array.Copy(outputBlock, 0, newOutputBlock, 0, valid);
                                outputBlock = newOutputBlock;
                            }
                            else if (valid == 0)
                            {
                                outputBlock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputBlock != null)
                        {
                            writer.Write(outputBlock, 0, outputBlock.Length);
                        }
                    }

                    writer.Flush();
                    OutputStream = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using CFB
        /// </summary>
        /// <param name="cryptoFunction"></param>
        private void Execute_CFB(CryptoFunction cryptoFunction)
        {
            using (CStreamReader reader = _inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] lastBlock = InputIV;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !_stop)
                    {
                        //we always try to read a complete block (=8 bytes)
                        byte[] inputBlock = new byte[(settings.BlockSize_2n / 8)];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, (settings.BlockSize_2n / 8) - readcount)) < (settings.BlockSize_2n / 8) &&
                               reader.Position < reader.Length && !_stop) ;

                        if (_stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        ProgressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == (settings.BlockSize_2n / 8))
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (settings.OpMode == OperatingMode.Encrypt)
                            {
                                outputblock = cryptoFunction(lastBlock, _inputKey);
                                outputblock = SpeckCiphers.XOR(outputblock, inputBlock);
                                lastBlock = outputblock;
                            }
                            else
                            {
                                outputblock = cryptoFunction(lastBlock, _inputKey);
                                outputblock = SpeckCiphers.XOR(outputblock, inputBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (settings.OpMode == OperatingMode.Encrypt)
                            {
                                byte[] block = new byte[(settings.BlockSize_2n / 8)];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(lastBlock, _inputKey);
                                outputblock = SpeckCiphers.XOR(outputblock, block);
                            }
                            else
                            {
                                byte[] block = new byte[(settings.BlockSize_2n / 8)];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = cryptoFunction(inputBlock, _inputKey);
                                outputblock = SpeckCiphers.XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && settings.OpMode == OperatingMode.Decrypt && settings.PadMode != BlockCipherHelper.PaddingType.None)
                        {
                            int valid = BlockCipherHelper.StripPadding(outputblock, (settings.BlockSize_2n / 8), settings.PadMode, (settings.BlockSize_2n / 8));
                            if (valid != (settings.BlockSize_2n / 8))
                            {
                                byte[] newoutputblock = new byte[valid];
                                if (outputblock != null)
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
                        }
                    }

                    writer.Flush();
                    OutputStream = writer;
                }
            }
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

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}

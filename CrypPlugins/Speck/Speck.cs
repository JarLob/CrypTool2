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
                    GuiLogMessage(String.Format(Resources.Speck_Input_padded, settings.PadMode), NotificationLevel.Error);
                }
                else
                {
                    GuiLogMessage(String.Format(Resources.Speck_Input_too_short, ((settings.BlockSize_2n / 8) - (_inputStream.Length % (settings.BlockSize_2n / 8)))), NotificationLevel.Error);
                    return;
                }
            }

            //Select crypto function based on algorithm and action
            CryptoFunction cryptoFunction = null;
           if (settings.ChoiceOfVariant == SpeckParameters.Speck32_64)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck32_64_Encryption : new CryptoFunction(SpeckCiphers.Speck32_64_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck48_72)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck48_72_Encryption : new CryptoFunction(SpeckCiphers.Speck48_72_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck48_96)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck48_96_Encryption : new CryptoFunction(SpeckCiphers.Speck48_96_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck64_96)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck64_96_Encryption : new CryptoFunction(SpeckCiphers.Speck64_96_Decryption);
           } 
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck64_128)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck64_128_Encryption : new CryptoFunction(SpeckCiphers.Speck64_128_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck96_96)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck96_96_Encryption : new CryptoFunction(SpeckCiphers.Speck96_96_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck96_144)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck96_144_Encryption : new CryptoFunction(SpeckCiphers.Speck96_144_Decryption);
           } 
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_128)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck128_128_Encryption : new CryptoFunction(SpeckCiphers.Speck128_128_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_192)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck128_192_Encryption : new CryptoFunction(SpeckCiphers.Speck128_192_Decryption);
           }
           else if (settings.ChoiceOfVariant == SpeckParameters.Speck128_256)
           {
               cryptoFunction = settings.OpMode == OperatingMode.Encrypt ? SpeckCiphers.Speck128_256_Encryption : new CryptoFunction(SpeckCiphers.Speck128_256_Decryption);
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

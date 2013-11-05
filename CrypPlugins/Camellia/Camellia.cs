﻿/*
   Copyright 2013 Nils Kopal, University of Kassel

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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Camellia
{

    [Author("Nils Kopal", "nils.kopal@uni-kassel.de", "Universität Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("Camellia.Properties.Resources", "PluginCaption", "PluginTooltip", null, "Camellia/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Camellia : ICrypComponent
    {
        private CamelliaSettings _settings = new CamelliaSettings();

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get;
            set;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void Dispose()
        {
            InputIV = null;
            InputKey = null;
            InputStream = null;
            OutputStream = null;
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings {
            get
            {
                return _settings;
            } 
            set
            {
                _settings = (CamelliaSettings)value;
            } 
        }

        public UserControl Presentation { get; private set; }

        public void Execute()
        {
            int keysize;
            switch (_settings.Keysize)
            {                
                case 1:
                    keysize = 24;
                    break;
                case 2:
                    keysize = 32;
                    break;
                default:
                    keysize = 16;
                    break;
            }   

            if (InputKey.Length != keysize)
            {
                GuiLogMessage(String.Format("Wrong keysize given. Keysize was {0} Bits but needed is {1} Bits.",InputKey.Length * 8, keysize * 8),NotificationLevel.Error);
                return;
            }

            if (InputIV == null)
            {
                if (_settings.Mode > 0)
                {
                    GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
                }
                InputIV = new byte[16];
            }

            if (InputIV.Length < 16 && _settings.Mode > 0)
            {
                GuiLogMessage(String.Format("NOTE: Wrong IV size given. IV size was {0} Bits but needed is 128 Bits. Appending with zeros.", InputIV.Length * 8), NotificationLevel.Info);
                var newIV = new byte[16];
                Array.Copy(InputIV,0,newIV,0,InputIV.Length);
                InputIV = newIV;
            }
            else if (InputIV.Length > 16 && _settings.Mode > 0)
            {
                GuiLogMessage(String.Format("NOTE: Wrong IV size given. IV size was {0} Bits but needed is 128 Bits. Removing bytes from position 15.", InputIV.Length * 8), NotificationLevel.Info);
                var newIV = new byte[16];
                Array.Copy(InputIV, 0, newIV, 0, 16);
                InputIV = newIV;
            }
           
            var keyParameter = new KeyParameter(InputKey);            
            var engine = new CamelliaEngine();
            var keyParameterWithIv = new ParametersWithIV(keyParameter, InputIV);        

            BufferedBlockCipher cipher;
            switch (_settings.Mode)
            {
                case 1:                   
                    cipher = new BufferedBlockCipher(new CbcBlockCipher(engine));
                    break;
                case 2:
                    cipher = new BufferedBlockCipher(new CfbBlockCipher(engine,128));                    
                    break;
                case 3:
                    cipher = new BufferedBlockCipher(new OfbBlockCipher(engine, 128));
                    break;
                default:
                    cipher = new BufferedBlockCipher(engine);
                    break;
            }

            if (_settings.Mode > 0)
            {
                cipher.Init(_settings.Action == 0, keyParameterWithIv);
            }
            else
            {
                cipher.Init(_settings.Action == 0, keyParameter);
            }

            //Add padding
            if (_settings.Action == 0 && _settings.Padding > 0)
            {
                var paddingType = BlockCipherHelper.PaddingType.None;
                switch (_settings.Padding)
                {
                    case 1:
                        paddingType = BlockCipherHelper.PaddingType.Zeros;
                        break;
                    case 2:
                        paddingType = BlockCipherHelper.PaddingType.PKCS7;
                        break;
                    case 3:
                        paddingType = BlockCipherHelper.PaddingType.ANSIX923;
                        break;
                    case 4:
                        paddingType = BlockCipherHelper.PaddingType.ISO10126;
                        break;
                    case 5:
                        paddingType = BlockCipherHelper.PaddingType.OneZeros;
                        break;
                }
                InputStream = BlockCipherHelper.AppendPadding(InputStream, paddingType, 16);
            }

            var inputText = InputStream.CreateReader().ReadFully();
            var outputText = new byte[cipher.GetOutputSize(inputText.Length)];            
            var outputLen = cipher.ProcessBytes(inputText,0, inputText.Length,outputText, 0);
            cipher.DoFinal(outputText,outputLen);
            OutputStream = new CStreamWriter();

            int offset = outputText.Length;
            //Remove the padding from the output
            if (_settings.Action == 1 && _settings.Padding > 0)
            {
                var paddingType = BlockCipherHelper.PaddingType.None;
                switch (_settings.Padding)
                {
                    case 1:
                        paddingType = BlockCipherHelper.PaddingType.Zeros;
                        break;
                    case 2:
                        paddingType = BlockCipherHelper.PaddingType.PKCS7;
                        break;
                    case 3:
                        paddingType = BlockCipherHelper.PaddingType.ANSIX923;
                        break;
                    case 4:
                        paddingType = BlockCipherHelper.PaddingType.ISO10126;
                        break;
                    case 5:
                        paddingType = BlockCipherHelper.PaddingType.OneZeros;
                        break;
                }                
                offset = BlockCipherHelper.StripPadding(outputText, outputText.Length - outputText.Length % 16, paddingType, outputText.Length);
            }
           
            //Output encrypted or decrypted text
            ((CStreamWriter)OutputStream).Write(outputText,0,offset);
            ((CStreamWriter)OutputStream).Close();
            OnPropertyChanged("OutputStream");            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void PostExecution()
        {
            Dispose();
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }
    }
}

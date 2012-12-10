﻿/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using NativeCryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Control;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "AES/DetailedDescription/doc.xml", "AES/Images/AES.png", "AES/Images/encrypt.png", "AES/Images/decrypt.png", "AES/Images/Rijndael.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class AES : ContextBoundObject, ICrypComponent
    {
        #region Private variables
        private AESSettings settings;
        private CStreamWriter outputStreamWriter;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        #endregion

        public AES()
        {
            this.settings = new AESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
          if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (AESSettings)value; }
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
            get { return this.inputKey; }
            set 
            { 
              this.inputKey = value;
              OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set 
            { 
              this.inputIV = value;
              OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get 
            {
                return outputStreamWriter;
            }
            set 
            {
                // empty
            }
        }

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            // first set all paramter, then assign input values!
            switch (settings.Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                // case 3: alg.Mode = CipherMode.OFB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }

            switch (settings.Padding)
            { //0="Zeros"=default, 1="None", 2="PKCS7" , 3="ANSIX923", 4="ISO10126", 5="1-0-padding"
                case 0: alg.Padding = PaddingMode.None; break;
                case 1: alg.Padding = PaddingMode.Zeros; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                case 5: alg.Padding = PaddingMode.None; break;  // 1-0 padding, use PaddingMode.None, as it's handeled separately
                default: alg.Padding = PaddingMode.Zeros; break;
            }

            if (settings.CryptoAlgorithm == 1)
            {
                switch (settings.Blocksize)
                {
                    case 1: alg.BlockSize = 192; break;
                    case 2: alg.BlockSize = 256; break;
                    default: alg.BlockSize = 128; break;
                }
            }

            //check for a valid key
            if (this.inputKey == null)
            {                
                //create a trivial key 
                inputKey = new byte[16];
                // write a warning to the ouside word
                GuiLogMessage("ERROR: No key provided. Using 0x000..00!", NotificationLevel.Error);
            }

            int keySizeInBytes = (16 + settings.Keysize * 8);

            //prepare a long enough key
            byte[] key = new byte[keySizeInBytes];

            // copy the input key into the temporary key array
            Array.Copy(inputKey, key, Math.Min(inputKey.Length, key.Length));

            // Note: the SymmetricAlgorithm.Key setter clones the passed byte[] array and keeps his own copy
            alg.Key = key;

            if (inputKey.Length > keySizeInBytes)
            {
                GuiLogMessage("Overlength (" + inputKey.Length * 8 + " Bits) key provided. Removing trailing bytes to fit the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            if (inputKey.Length < keySizeInBytes)
            {
                GuiLogMessage("Short (" + inputKey.Length * 8 + " Bits) key provided. Adding zero bytes to fill up to the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            //check for a valid IV
            if (this.inputIV == null)
            {
                //create a trivial key 
                inputIV = new byte[alg.BlockSize / 8];
                GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
            }

            alg.IV = this.inputIV;
        }

        private string bytesToHexString(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public void Execute()
        {
            process(settings.Action);
        }

        public bool isStopped()
        {
            return this.stop;
        }
                
        private void process(int action)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }

                SymmetricAlgorithm p_alg = null;
                if (settings.CryptoAlgorithm == 1)
                    p_alg = new RijndaelManaged();
                else
                    p_alg = new AesCryptoServiceProvider();

                ConfigureAlg(p_alg);

                ICryptoTransform p_encryptor = null;
                switch (action)
                {
                    case 0:
                        p_encryptor = p_alg.CreateEncryptor();
                        break;
                    case 1:
                        p_encryptor = p_alg.CreateDecryptor();
                        break;
                }

                outputStreamWriter = new CStreamWriter();

                ICryptoolStream inputdata = InputStream;

                string mode = action == 0 ? "encryption" : "decryption";
                long inbytes, outbytes;

                GuiLogMessage("Starting " + mode + " [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);

                DateTime startTime = DateTime.Now;

                // special handling of OFB mode, as it's not available for AES in .Net
                if (settings.Mode == 3)    // OFB - bei OFB ist encrypt = decrypt, daher keine Fallunterscheidung
                {
                    if (action == 0)
                        inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);

                    ICryptoTransform encrypt = p_alg.CreateEncryptor(p_alg.Key,p_alg.IV);

                    byte[] IV = new byte[p_alg.IV.Length];
                    Array.Copy(p_alg.IV, IV, p_alg.IV.Length);
                    byte[] tmpInput = BlockCipherHelper.StreamToByteArray(inputdata);
                    byte[] outputData = new byte[tmpInput.Length];

                    for (int pos = 0; pos <= tmpInput.Length - encrypt.InputBlockSize; )
                    {
                        int l = encrypt.TransformBlock(IV, 0, encrypt.InputBlockSize, outputData, pos);
                        for (int i = 0; i < l; i++)
                        {
                            IV[i] = outputData[pos + i];
                            outputData[pos + i] ^= tmpInput[pos + i];
                        }
                        pos += l;
                    }

                    int validBytes = (int)inputdata.Length;

                    if (action == 1)
                        validBytes = BlockCipherHelper.StripPadding(outputData, validBytes, settings.padmap[settings.Padding], p_alg.BlockSize / 8);

                    encrypt.Dispose();
                    outputStreamWriter.Write(outputData, 0, validBytes);
                    inbytes = inputdata.Length;
                }
                else
                {
                    // append 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                    if (action == 0 && settings.Padding == 5)
                        inputdata = BlockCipherHelper.AppendPadding(InputStream, BlockCipherHelper.PaddingType.OneZeros, p_alg.BlockSize / 8);

                    CStreamReader reader = inputdata.CreateReader();

                    p_crypto_stream = new CryptoStream(reader, p_encryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;
                    int position = 0;

                    while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        // remove 1-0 padding (special handling, as it's not present in System.Security.Cryptography.PaddingMode)
                        if (action == 1 && settings.Padding == 5 && reader.Position == reader.Length)
                            bytesRead = BlockCipherHelper.StripPadding(buffer, bytesRead, BlockCipherHelper.PaddingType.OneZeros, buffer.Length);

                        outputStreamWriter.Write(buffer, 0, bytesRead);

                        if ((int)(reader.Position * 100 / reader.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / reader.Length);
                            ProgressChanged(reader.Position, reader.Length);
                        }
                    }

                    p_crypto_stream.Flush();
                    inbytes = reader.Length;
                }

                outbytes = outputStreamWriter.Length;

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                // (outputStream as CryptoolStream).FinishWrite();

                if (!stop)
                {
                    mode = action == 0 ? "Encryption" : "Decryption";
                    GuiLogMessage(mode + " complete! (in: " + inbytes + " bytes, out: " + outbytes + " bytes)", NotificationLevel.Info);
                    GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                    outputStreamWriter.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                    outputStreamWriter.Close();
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            catch (CryptographicException cryptographicException)
            {
                // TODO: For an unknown reason p_crypto_stream can not be closed after exception.
                // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                // the error messages will be doubled. 
                // As a workaround we set p_crypto_stream to null here.
                p_crypto_stream = null;
                GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        public void Encrypt()
        {
            //Encrypt Stream
            process(0);
        }
       
        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        #region IPlugin Member


        public System.Windows.Controls.UserControl Presentation
        {
          get { return null; }
        }

        public void Initialize()
        {            
        }

        public void Dispose()
        {
          try
          {
            stop = false;
            inputKey = null;
            inputIV = null;

                if (outputStreamWriter != null)
            {
                    outputStreamWriter.Dispose();
                    outputStreamWriter = null;
            }

            if (p_crypto_stream != null)
            {
              p_crypto_stream.Flush();
              p_crypto_stream.Close();
              p_crypto_stream = null;
            }
          }
          catch (Exception ex)
          {          
             GuiLogMessage(ex.Message, NotificationLevel.Error);
          }
          this.stop = false;
        }

        public void Stop()
        {
          this.stop = true;
        }

        public void PostExecution()
        {
          Dispose();
        }

        public void PreExecution()
        {
          Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
          //if (PropertyChanged != null)
          //{
          //  PropertyChanged(this, new PropertyChangedEventArgs(name));
          //}
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;        

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        private IControlEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlEncryption ControlSlave
        {
          get 
          {
              if (controlSlave == null)
                  controlSlave = new AESControl(this);
              return controlSlave; 
          }
        }     
    }

    public class AESControl : IControlEncryption
    {
        public event KeyPatternChanged keyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;

        private AES plugin;
        private AESSettings settings;
     
        public AESControl(AES plugin)
        {
            this.plugin = plugin;
            this.settings = (AESSettings)plugin.Settings;
            plugin.Settings.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
                                                   {
                                                       if (e.PropertyName == "Keysize")
                                                       {
                                                           FireKeyPatternChanged();
                                                       }
                                                   };
        }

        private void FireKeyPatternChanged()
        {
            if (keyPatternChanged != null)
            {
                keyPatternChanged();
            }
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] key, int blocksize)
        {
            // not implemented, currently not needed
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return Decrypt(ciphertext, key, IV, ciphertext.Length);
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            // Don't allow sizes greater than the length of the input
            int newBytesToUse = Math.Min(bytesToUse, ciphertext.Length);
            // Note: new size is assumed to be multiple of 16

            return NativeCryptography.Crypto.decryptAES(ciphertext, key, IV, this.settings.KeysizeAsBits, newBytesToUse, settings.Mode);
        }

        public int GetBlockSize()
        {
            return settings.BlocksizeAsBytes;
        }

        public string GetKeyPattern()
        {
            int bytes = 0;
            switch (((AESSettings)plugin.Settings).Keysize)
            {
                case 0:
                    bytes = 16;
                    break;
                case 1:
                    bytes = 24;
                    break;
                case 2:
                    bytes = 32;
                    break;
            }
            string pattern = "";
            for (int i = 1; i < bytes; i++)
                pattern += "[0-9A-F][0-9A-F]-";
            pattern += "[0-9A-F][0-9A-F]";
            return pattern;
        }

        public IControlEncryption clone()
        {
            AESControl aes = new AESControl(plugin);
            return aes;
        }

        public void Dispose()
        {
        }

        public string GetOpenCLCode(int decryptionLength, byte[] iv)
        {
            string opencl = Properties.Resources.AESOpenCL;

            int bits = -1;
            switch (((AESSettings)plugin.Settings).Keysize)
            {
                case 0:
                    bits = 16 * 8;
                    break;
                case 1:
                    bits = 24 * 8;
                    break;
                case 2:
                    bits = 32 * 8;
                    break;
            }
            if (bits == -1)
                return null;

            opencl = opencl.Replace("$$BITS$$", "" + bits);

            //if there is a relevant IV:
            bool useIV = false;
            if (iv != null && iv.Length != 0 && iv.Count(x => x != 0) > 0)
            {
                useIV = true;
                string IV = "";
                foreach (byte i in iv)
                {
                    IV += string.Format("0x{0:X}, ", i);
                }
                IV = IV.Substring(0, IV.Length - 2);
                opencl = opencl.Replace("$$IVARRAY$$", string.Format("__constant u8 IV[] = {{ {0} }};", IV));
            }
            else
            {
                opencl = opencl.Replace("$$IVARRAY$$", "");
            }

            string decryptionCode = string.Format("int decryptionLength = {0}; \n", decryptionLength);
            int blocks = decryptionLength / 16;
            if (blocks >= 1)
            {
                if (!useIV)
                    decryptionCode = AddOpenCLBlockDecryption(decryptionCode, 16);
                else
                    decryptionCode = AddOpenCLBlockDecryptionWithIV(decryptionCode, 16);

                if (blocks > 1)
                {
                    decryptionCode += string.Format("for (int b = 1; b < {0}; b++) \n {{ \n ", blocks);
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, 16, "b");
                    decryptionCode += "}\n";
                }
            }

            if (decryptionLength % 16 != 0)
            {
                if (blocks == 0)
                {
                    if (!useIV)
                        decryptionCode = AddOpenCLBlockDecryption(decryptionCode, decryptionLength%16);
                    else
                        decryptionCode = AddOpenCLBlockDecryptionWithIV(decryptionCode, decryptionLength % 16);
                }
                else
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, decryptionLength % 16, "" + blocks);
            }

            opencl = opencl.Replace("$$AESDECRYPT$$", decryptionCode);

            return opencl;
        }

        private string AddOpenCLBlockDecryption(string decryptionCode, int size)
        {
            decryptionCode += "AES_decrypt(inn, block, &(key)); \n " 
                              + string.Format("for (int i = 0; i < {0}; i++) \n ", size)
                              + "{ \n unsigned char c = block[i]; \n "
                              + "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        private string AddOpenCLBlockDecryptionWithIV(string decryptionCode, int size)
        {
            switch (((AESSettings)plugin.Settings).Mode)
            {
                case 0: //ECB
                    return AddOpenCLBlockDecryption(decryptionCode, size);
                case 1: //CBC
                    decryptionCode += "AES_decrypt(inn, block, &(key)); \n " 
                              + string.Format("for (int i = 0; i < {0}; i++) \n ", size)
                              + "{ \n unsigned char c = block[i] ^ IV[i]; \n "
                              + "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
                    break;
                case 2: //CFB
                    throw new NotImplementedException("CFB for OpenCL is not implemented!"); //not supported
                default:
                    throw new NotImplementedException("Mode not supported by OpenCL!");
            }
        }

        private string AddOpenCLBlockDecryptionWithMode(string decryptionCode, int size, string block)
        {
            decryptionCode += string.Format("AES_decrypt((inn+{0}*16), block, &(key)); \n ", block)
                              + string.Format("for (int i = 0; i < {0}; i++) \n {{ \n ", size);
            switch (((AESSettings) plugin.Settings).Mode)
            {
                case 0: //ECB
                    decryptionCode += "unsigned char c = block[i]; \n";
                    break;
                case 1: //CBC
                    decryptionCode += string.Format("unsigned char c = block[i] ^ (inn+({0}-1)*16)[i]; \n", block);
                    break;
                case 2: //CFB
                    throw new NotImplementedException("CFB for OpenCL is not implemented!"); //not supported
                default:
                    throw new NotImplementedException("Mode not supported by OpenCL!");
            }
            decryptionCode += "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        public void changeSettings(string setting, object value)
        {
        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }

        #endregion
    }
}
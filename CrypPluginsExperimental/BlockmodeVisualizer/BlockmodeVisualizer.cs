﻿/*
   Copyright 2018 Olaf Versteeg <olaf.42.versteeg@gmail.com>

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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using BlockmodeVisualizer;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using static Cryptool.PluginBase.Miscellaneous.BlockCipherHelper;

namespace Cryptool.Plugins.BlockmodeVisualizer
{
    [Author("Olaf Versteeg", "olaf.42.versteeg@gmail.com", "University of Kassel",
        "http://www.uni-kassel.de/eecs/en/homepage.html")]
    [PluginInfo("Cryptool.Plugins.BlockmodeVisualizer.Properties.Resources", "plugin_caption", "plugin_tooltip",
        "BlockmodeVisualizer/userdoc.xml", "BlockmodeVisualizer/Images/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class BlockmodeVisualizer : ICrypComponent
    {
        #region Private Fields

        private readonly BlockmodeVisualizerSettings settings = new BlockmodeVisualizerSettings();
        private BVPresentation presentation;

        // Constants
        private const string KEY = "key";
        private const string IV = "iv";
        private const string FAIL = "authentication_error";

        // Blockcipher attributes
        private string ciphername;
        private int blocksize;
        private int keysize;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "text_input_caption", "text_input_tooltip", true)]
        public ICryptoolStream TextInput { get; set; }

        [PropertyInfo(Direction.InputData, "tag_input_caption", "tag_input_tooltip", false)]
        public byte[] TagInput { get; set; }

        [PropertyInfo(Direction.InputData, "key_caption", "key_tooltip", true)]
        public byte[] Key { get; set; }

        [PropertyInfo(Direction.InputData, "iv_caption", "iv_tooltip", false)]
        public byte[] InitializationVector { get; set; }

        [PropertyInfo(Direction.InputData, "ad_caption", "ad_tooltip", false)]
        public ICryptoolStream AssociatedData { get; set; }

        [PropertyInfo(Direction.OutputData, "text_output_caption", "text_output_tooltip", true)]
        public ICryptoolStream TextOutput { get; set; }

        [PropertyInfo(Direction.OutputData, "tag_output_caption", "tag_output_tooltip", false)]
        public byte[] TagOutput { get; set; }

        #endregion

        #region IControlEncryption

        [PropertyInfo(Direction.ControlMaster, "icontrol_caption", "icontrol_tooltip", true)]
        public IControlEncryption Blockcipher { get; set; }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => presentation;

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            presentation = new BVPresentation();
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            // If a blockcipher has been selected, get its settings.
            if (Blockcipher != null)
            {
                ciphername = Blockcipher.GetCipherShortName();
                blocksize = Blockcipher.GetBlockSizeAsBytes();
                keysize = Blockcipher.GetKeySizeAsBytes();
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // Execution started.
            ProgressChanged(0, 1);

            // If no blockcipher has been selected, the inputs will be simply passed to the outputs...
            if (Blockcipher == null)
            {
                GuiLogMessage("missing_blockcipher_warning", NotificationLevel.Warning);
                WriteOutputs(StreamToByteArray(TextInput), TagInput);
            }
            // ...otherwise run the selected mode of operation.
            else
            {
                switch (settings.Blockmode)
                {
                    case Blockmodes.ECB:
                        ECB(StreamToByteArray(TextInput), Key);
                        break;
                    case Blockmodes.CBC:
                        CBC(StreamToByteArray(TextInput), Key, InitializationVector);
                        break;
                    case Blockmodes.CFB:
                        CFB(StreamToByteArray(TextInput), Key, InitializationVector, settings.DataSegmentLength);
                        break;
                    case Blockmodes.OFB:
                        OFB(StreamToByteArray(TextInput), Key, InitializationVector);
                        break;
                    case Blockmodes.CTR:
                        CTR(StreamToByteArray(TextInput), Key, InitializationVector);
                        break;
                    case Blockmodes.XTS:
                        XTS(StreamToByteArray(TextInput), Key, InitializationVector);
                        break;
                    case Blockmodes.CCM:
                        CCM(StreamToByteArray(TextInput), StreamToByteArray(AssociatedData), TagInput, Key, InitializationVector, settings.TagLength);
                        break;
                    case Blockmodes.GCM:
                        GCM(StreamToByteArray(TextInput), StreamToByteArray(AssociatedData), TagInput, Key, InitializationVector, settings.TagLength);
                        break;
                    default:
                        throw new InvalidOperationException("should_not_happen_exception");
                }
            }

            // Execution finished
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {

        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Blockmode Functions

        /// <summary>
        /// Encrypts or decrypts with the Electronic Codebook Mode (ECB).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        private void ECB(byte[] input, byte[] key)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);
            
            // Append selected padding for encryption if necessary.
            if (settings.Action == Actions.ENCRYPTION)
            {
                result = AppendPadding(result, settings.Padding, blocksize);
            }

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Encrypt/Decrypt
            int numberOfBlocks = result.Length / blocksize;
            byte[] currentBlock = new byte[blocksize];
            for (int i = 0; i < numberOfBlocks; i++)
            {
                Array.Copy(result, i * blocksize, currentBlock, 0, blocksize);
                if (settings.Action == Actions.ENCRYPTION)
                {
                    currentBlock = Blockcipher.Encrypt(currentBlock, key);
                }
                else
                {
                    currentBlock = Blockcipher.Decrypt(currentBlock, key);
                }
                Array.Copy(currentBlock, 0, result, i * blocksize, blocksize);

                // Update progress bar
                double progress = (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Strip selected padding from result after decryption.
            if (settings.Action == Actions.DECRYPTION)
            {
                result = StripPadding(result, settings.Padding, blocksize);
            }

            // Pass result to output.
            WriteOutputs(result, null);
        }
        
        /// <summary>
        /// Encrypts or decrypts with the Cipher Block Chaining Mode (CBC).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the first block.</param>
        private void CBC(byte[] input, byte[] key, byte[] iv)
        {
            // Get result for regular CBC mode.
            byte[] result = CBC(input, key, iv, false);

            // Pass result to output.
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Cipher Block Chaining Mode (CBC).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the first block.</param>
        /// <param name="mac">Indicates if CBC is used as mac algorithm.</param>
        /// <returns>The encrypted or decrypted input or the message authentication code.</returns>
        private byte[] CBC(byte[] input, byte[] key, byte[] iv, bool mac)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Append selected padding for encryption if necessary.
            if (settings.Action == Actions.ENCRYPTION && !mac)
            {
                result = AppendPadding(result, settings.Padding, blocksize);
            }

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // Check iv length. Shorten or pad if necessary.
            iv = CheckLength(iv, blocksize, IV);

            // Set progress bar to 10%.
            if (!mac) ProgressChanged(0.1, 1);

            // Encrypt/Decrypt
            int numberOfBlocks = result.Length / blocksize;
            byte[] previousBlock = iv;
            byte[] currentBlock = new byte[blocksize];
            for (int i = 0; i < numberOfBlocks; i++)
            {
                byte[] currentResultBlock;
                Array.Copy(result, i * blocksize, currentBlock, 0, blocksize);
                // Using CBC as mac algorithm only uses the encryption mode.
                if (settings.Action == Actions.ENCRYPTION || mac)
                {
                    currentResultBlock = Blockcipher.Encrypt(XOR(previousBlock, currentBlock), key);
                    // The current result will be XOR-ed in the next iteration.
                    previousBlock = currentResultBlock;
                }
                else
                {
                    currentResultBlock = XOR(Blockcipher.Decrypt(currentBlock, key), previousBlock);
                    // The current ciphertext block will be XOR-ed in the next iteration.
                    Array.Copy(currentBlock, previousBlock, blocksize);
                }
                Array.Copy(currentResultBlock, 0, result, i * blocksize, blocksize);

                // Update progress bar
                double progress = (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Strip selected padding from result after decryption.
            if (settings.Action == Actions.DECRYPTION && !mac)
            {
                result = StripPadding(result, settings.Padding, blocksize);
            }

            // For CBC-MAC only the last block will be used.
            if (mac) result = LSB(result, blocksize);

            // Return result for further 
            return result;
        }

        /// <summary>
        /// Encrypts or decrypts with the Cipher Feedback Mode (CFB).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the first block.</param>
        /// <param name="s">The length of the data segment to be used for feedback.</param>
        private void CFB(byte[] input, byte[] key, byte[] iv, int s)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Ensure that s is not greater than the blocksize.
            if (s > blocksize)
            {
                s %= blocksize;
                string message = "data_segment_overflow_warning";
                GuiLogMessage(message + s, NotificationLevel.Warning);
            }

            // Append selected padding for encryption. For CFB the length must be a multiple of s.
            if (settings.Action == Actions.ENCRYPTION)
            {
                result = AppendPadding(result, settings.Padding, s);
            }

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // Check iv length. Shorten or pad if necessary.
            iv = CheckLength(iv, blocksize, IV);

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Encrypt/Decrypt
            int numberOfBlocks = result.Length / s;
            byte[] cipherInput = iv;
            byte[] currentBlock = new byte[s];
            for (int i = 0; i < numberOfBlocks; i++)
            {
                byte[] cipherOutput = Blockcipher.Encrypt(cipherInput, key);

                Array.Copy(result, i * s, currentBlock, 0, s);
                byte[] currentResultBlock = XOR(currentBlock, MSB(cipherOutput, s));
                Array.Copy(currentResultBlock, 0, result, i * s, s);

                if (settings.Action == Actions.ENCRYPTION)
                {
                    // The current result will be shifted into the next cipher input.
                    cipherInput = LSB(cipherInput, blocksize - s).Concat(currentResultBlock).ToArray();
                }
                else
                {
                    // The current ciphertext block will be shifted into the next cipher input.
                    cipherInput = LSB(cipherInput, blocksize - s).Concat(currentBlock).ToArray();
                }

                // Update progress bar
                double progress = (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Strip selected padding from result after decryption.
            if (settings.Action == Actions.DECRYPTION)
            {
                result = StripPadding(result, settings.Padding, s);
            }

            // Pass result to output.
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Output Feedback Mode (OFB).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the first block.</param>
        private void OFB(byte[] input, byte[] key, byte[] iv)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // Check iv length. Shorten or pad if necessary.
            iv = CheckLength(iv, blocksize, IV);

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Encrypt/Decrypt
            int numberOfBlocks = (int)Math.Ceiling((double)result.Length / blocksize);
            byte[] currentBlock = new byte[blocksize];
            byte[] cipherOutput = iv;
            for (int i = 0; i < numberOfBlocks; i++)
            {
                cipherOutput = Blockcipher.Encrypt(cipherOutput, key);

                // The last block may be shorter than the blocksize.
                int currentBlocksize = i + 1 == numberOfBlocks ? result.Length - i * blocksize : blocksize;

                Array.Copy(result, i * blocksize, currentBlock, 0, currentBlocksize);
                byte[] currentResultBlock = XOR(currentBlock, cipherOutput);
                Array.Copy(currentResultBlock, 0, result, i * blocksize, currentBlocksize);

                // Update progress bar
                double progress = (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Pass result to output.
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Counter Mode (CTR).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the counter.</param>
        private void CTR(byte[] input, byte[] key, byte[] iv)
        {
            // Get result for regular CTR mode.
            byte[] result = CTR(input, key, iv, false, blocksize);

            // Pass result to output.
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Counter Mode (CTR).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the counter.</param>
        /// <param name="ae">Indicates if CTR is used for authenticated encryption.</param>
        /// <param name="counterLength">The last number of iv's bytes used for the counter.</param>
        /// <returns>The CTR computed input</returns>
        private byte[] CTR(byte[] input, byte[] key, byte[] iv, bool ae, int counterLength)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // Check iv length. Shorten or pad if necessary.
            iv = CheckLength(iv, blocksize, IV);

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Encrypt/Decrypt
            int numberOfBlocks = (int)Math.Ceiling((double)result.Length / blocksize);
            byte[] currentBlock = new byte[blocksize];
            byte[] counter = iv;
            for (int i = 0; i < numberOfBlocks; i++)
            {
                // The last block may be shorter than the blocksize.
                int currentBlocksize = i + 1 == numberOfBlocks ? result.Length - i * blocksize : blocksize;

                Array.Copy(result, i * blocksize, currentBlock, 0, currentBlocksize);
                byte[] currentResultBlock = XOR(currentBlock, Blockcipher.Encrypt(counter, key));
                Array.Copy(currentResultBlock, 0, result, i * blocksize, currentBlocksize);

                // Increment counter
                // Raw CTR uses the total iv as counter. For authenticated encryption only some last part of it.
                if (i < blocksize - 1) counter = INC(counter, counterLength);

                // Update progress bar
                // For authenticated encryption CTR is only the first part.
                double progress = ae ? (i + 1) * 0.4 / numberOfBlocks : (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Return the encrypted or decrypted input.
            return result;
        }

        /// <summary>
        /// Encrypts or decrypts with the XTS-Mode (XTS).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="key">Keys for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the multiplication.</param>
        private void XTS(byte[] input, byte[] key, byte[] iv)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Encryption or Decryption is only possible, if the length of the input is at least equals the blocksize.
            if (result.Length < blocksize)
            {
                GuiLogMessage("short_input_warning", NotificationLevel.Warning);
                WriteOutputs(null, null);
                return;
            }

            // Check key length. Shorten or pad if necessary. (XTS needs a key twice the length!)
            key = CheckLength(key, 2 * keysize, KEY);

            // Check iv length. Shorten or pad if necessary.
            iv = CheckLength(iv, blocksize, IV);

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Create subkeys key1 and key2
            byte[] key1 = new byte[keysize];
            byte[] key2 = new byte[keysize];
            Array.Copy(key, key1, keysize);
            Array.Copy(key, keysize, key2, 0, keysize);

            // Encrypt the iv with key2
            byte[] L = Blockcipher.Encrypt(iv, key2);

            // Check if ciphertext stealing is necessary.
            bool cts = result.Length % blocksize != 0;

            // Encrypt/Decrypt
            int numberOfBlocks = (int)Math.Ceiling((double)result.Length / blocksize);

            // If ciphertext stealing is necessary, the last two blocks will be treated specially.
            if (cts) numberOfBlocks -= 2;

            byte[] currentBlock = new byte[blocksize];
            byte[] currentResultBlock;
            int i = 0;
            for (; i < numberOfBlocks; i++)
            {
                Array.Copy(result, i * blocksize, currentBlock, 0, blocksize);
                if (settings.Action == Actions.ENCRYPTION)
                {
                    currentResultBlock = XOR(L, Blockcipher.Encrypt(XOR(currentBlock, L), key1));
                }
                else
                {
                    currentResultBlock = XOR(L, Blockcipher.Decrypt(XOR(currentBlock, L), key1));
                }
                Array.Copy(currentResultBlock, 0, result, i * blocksize, blocksize);

                // Multiply L by alpha = 0x87
                L = GF_MULT_0x87(L);

                // Update progress bar
                double progress = (i + 1) * 0.8 / numberOfBlocks;
                ProgressChanged(0.1 + progress, 1);
            }

            // Special treatment of the last two blocks
            if (cts)
            {
                // Get the second last and the last block from the input
                int lastBlockLength = result.Length % blocksize;
                byte[] lastBlock = new byte[lastBlockLength];
                Array.Copy(result, i * blocksize, currentBlock, 0, blocksize);
                Array.Copy(result, (i + 1) * blocksize, lastBlock, 0, lastBlockLength);

                if (settings.Action == Actions.ENCRYPTION)
                {
                    // Encrypt second last block and copy the first 'lastBlockLength' bytes to the end of the result array.
                    currentResultBlock = XOR(L, Blockcipher.Encrypt(XOR(currentBlock, L), key1));
                    Array.Copy(MSB(currentResultBlock, lastBlockLength), 0, result, (i + 1) * blocksize, lastBlockLength);

                    // Recalculate the XOR-operand
                    L = GF_MULT_0x87(L);

                    // Concat the last block with the rest of second last block's result, encrypt it and copy it to the second last position of the result array.
                    currentBlock = lastBlock.Concat(LSB(currentResultBlock, blocksize - lastBlockLength)).ToArray();
                    currentResultBlock = XOR(L, Blockcipher.Encrypt(XOR(currentBlock, L), key1));
                    Array.Copy(currentResultBlock, 0, result, i * blocksize, blocksize);

                    // Set progress bar to 90%.
                    ProgressChanged(0.9, 1);
                }
                else
                {
                    // Get the XOR-operand for the last block
                    byte[] lastL = GF_MULT_0x87(L);

                    // Decrypt second last block and copy the first 'lastBlockLength' bytes to the end of the result array.
                    currentResultBlock = XOR(lastL, Blockcipher.Decrypt(XOR(currentBlock, lastL), key1));
                    Array.Copy(MSB(currentResultBlock, lastBlockLength), 0, result, (i + 1) * blocksize, lastBlockLength);

                    // Concat the last block with the rest of second last block's result, decrypt it and copy it to the second last position of the result array.
                    currentBlock = lastBlock.Concat(LSB(currentResultBlock, blocksize - lastBlockLength)).ToArray();
                    currentResultBlock = XOR(L, Blockcipher.Decrypt(XOR(currentBlock, L), key1));
                    Array.Copy(currentResultBlock, 0, result, i * blocksize, blocksize);

                    // Set progress bar to 90%.
                    ProgressChanged(0.9, 1);
                }
            }

            // Pass result to output.
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Counter with CBC-MAC Mode (CCM).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="a">The associated data.</param>
        /// <param name="tag">The message tag for authentification.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the counter.</param>
        /// <param name="t">The length of the message tag to be calculated.</param>
        private void CCM(byte[] input, byte[] a, byte[] tag, byte[] key, byte[] iv, int t)
        {
            byte[] result = CBC(input, key, iv, true);
            WriteOutputs(result, null);
        }

        /// <summary>
        /// Encrypts or decrypts with the Galois/Counter Mode (GCM).
        /// </summary>
        /// <param name="input">Plaintext for encryption or ciphertext for decryption.</param>
        /// <param name="a">The associated data.</param>
        /// <param name="tag">The message tag for authentification.</param>
        /// <param name="key">Key for the selected blockcipher.</param>
        /// <param name="iv">Initialization vector for the counter.</param>
        /// <param name="t">The length of the message tag to be calculated.</param>
        private void GCM(byte[] input, byte[] a, byte[] tag, byte[] key, byte[] iv, int t)
        {
            byte[] result = new byte[input.Length];
            Array.Copy(input, result, input.Length);

            // Ensure that t is not greater than the blocksize.
            if (t > blocksize)
            {
                t %= blocksize;
                string message = "tag_length_overflow_warning";
                GuiLogMessage(message + t, NotificationLevel.Warning);
            }

            // Check key length. Shorten or pad if necessary.
            key = CheckLength(key, keysize, KEY);

            // GCM has special treatment of the iv
            // Append with zeroes and a single 1 if length is exactly 3/4 the blocksize.
            if (iv.Length == blocksize / 4 * 3)
            {
                byte[] suffix = new byte[blocksize / 4];
                suffix[suffix.Length - 1]++;
                iv = iv.Concat(suffix).ToArray();
            }
            else
            {
                // Pad with zeroes up to the next multiple of the blocksize, concat with its length and GHASH down to blocksize.
                int ivLength = iv.Length * 8;
                iv = AppendPadding(iv, PaddingType.Zeros, blocksize);
                iv = iv.Concat(IntToByteArray(ivLength, blocksize)).ToArray();
                iv = GHASH(iv, key);
            }

            // Set progress bar to 10%.
            ProgressChanged(0.1, 1);

            // Encrypt initial counter for final XOR
            byte[] finalXorOperand = Blockcipher.Encrypt(iv, key);

            // Increase counter for initializing CTR
            iv = INC(iv, blocksize / 4);

            // Encrypt/decrypt input with CTR
            result = CTR(result, key, iv, true, blocksize / 4);
            byte[] ciphertext = settings.Action == Actions.ENCRYPTION ? result : input;

            // Pad associated data and ciphertext with zeroes
            int aLength = a.Length * 8;
            int ciphertextLength = ciphertext.Length * 8;
            a = AppendPadding(a, PaddingType.Zeros, blocksize);
            byte[] ciphertext_padded = AppendPadding(ciphertext, PaddingType.Zeros, blocksize);

            // Concat a, ciphertext, length of a and lengt of ciphertext
            byte[] ghashInput = a.Concat(ciphertext_padded).ToArray();
            ghashInput = ghashInput.Concat(IntToByteArray(aLength, blocksize / 2)).ToArray();
            ghashInput = ghashInput.Concat(IntToByteArray(ciphertextLength, blocksize / 2)).ToArray();

            // Calculate message authentication tag
            byte[] ownTag = MSB(XOR(GHASH(ghashInput, key), finalXorOperand), t);

            // Set progress bar to 90%
            ProgressChanged(0.9, 1);

            // For encryption pass ciphertext and authentication tag to output.
            if (settings.Action == Actions.ENCRYPTION)
            {
                WriteOutputs(result, ownTag);
            }
            // For decryption verify tag and return FAIL or the plaintext
            else
            {
                WriteOutputs(Equals(tag, ownTag) ? result : Encoding.UTF8.GetBytes(FAIL), null);
            }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Writes the given values or an empty string to the text output and the tag output.
        /// </summary>
        /// <param name="text">The result of the encryption or decryption.</param>
        /// <param name="tag">The message authentication tag.</param>
        private void WriteOutputs(byte[] text, byte[] tag)
        {
            if (text == null) text = Encoding.UTF8.GetBytes("");
            TextOutput = new CStreamWriter(text);
            ((CStreamWriter)TextOutput).Close();
            OnPropertyChanged("TextOutput");

            TagOutput = tag ?? Encoding.UTF8.GetBytes("");
            OnPropertyChanged("TagOutput");
        }

        /// <summary>
        /// Creates a hexadecimal string representation of the given byte array.
        /// </summary>
        /// <param name="input">A byte array.</param>
        /// <returns>The hexadecimal representation of the array.</returns>
        private string ByteArrayToHexString(byte[] input)
        {
            String result = "";
            foreach (byte b in input)
            {
                result += b.ToString("X2") + " ";
            }
            // Return without the last blank.
            return result.Substring(0, result.Length - 1);
        }

        /// <summary>
        /// Checks if the given byte array's length is equals to the given length. Shortens or pads with zeroes if necessary.
        /// </summary>
        /// <param name="input">Typically a key or an initialization vector.</param>
        /// <param name="length">The target length of the given byte array.</param>
        /// <param name="inputName">The name of the byte array for generating the correct message.</param>
        private byte[] CheckLength(byte[] input, int length, string inputName)
        {
            // Input is too short
            if (input.Length < length)
            {
                byte[] result = input.Concat(new byte[length - input.Length]).ToArray();

                // Print warning and actual input for user.
                string message = null;
                if (inputName.Equals(KEY)) message = "short_key_warning";
                if (inputName.Equals(IV)) message = "short_iv_warning";
                GuiLogMessage(message + ByteArrayToHexString(result), NotificationLevel.Warning);
                return result;
            }

            // Input is too long
            if (input.Length > length)
            {
                byte[] result = new byte[length];
                Array.Copy(input, result, length);

                // Print warning and actual input for user.
                string message = null;
                if (inputName.Equals(KEY)) message = "long_key_warning";
                if (inputName.Equals(IV)) message = "long_iv_warning";
                GuiLogMessage(message + ByteArrayToHexString(result), NotificationLevel.Warning);
                return result;
            }

            // Input length is ok.
            return input;
        }

        /// <summary>
        /// Calculates the bitwise XOR of two byte arrays. The result's length is equal to the length of the shorter array.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <returns>a XOR b.</returns>
        private byte[] XOR(byte[] a, byte[] b)
        {
            int newLength = Math.Min(a.Length, b.Length);

            byte[] result = new byte[newLength];

            for (int i = 0; i < newLength; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }

            return result;
        }

        /// <summary>
        /// Returns the t most significant bytes of the given byte array.
        /// </summary>
        /// <param name="input">The array.</param>
        /// <param name="t">The number of bytes to be returned.</param>
        /// <returns>The first t bytes of the array.</returns>
        private byte[] MSB(byte[] input, int t)
        {
            byte[] result;

            if (t >= input.Length)
            {
                result = input;
            }
            else
            {
                result = new byte[t];
                Array.Copy(input, result, t);
            }

            return result;
        }

        /// <summary>
        /// Returns the t least significant bytes of the given byte array.
        /// </summary>
        /// <param name="input">The array.</param>
        /// <param name="t">The number of bytes to be returned.</param>
        /// <returns>The last t bytes of the array.</returns>
        private byte[] LSB(byte[] input, int t)
        {
            byte[] result;

            if (t >= input.Length)
            {
                result = input;
            }
            else
            {
                result = new byte[t];
                Array.Copy(input, input.Length - t, result, 0, t);
            }

            return result;
        }

        /// <summary>
        /// Increments the value of the given byte array only on the last n bytes.
        /// </summary>
        /// <param name="input">The array which is to be increased.</param>
        /// <param name="n">The number of the last bytes which will be increased.</param>
        /// <returns>MSB(input, blocksize - n) || (LSB(input, n) + 1) modulo 2^blocksize.</returns>
        private byte[] INC(byte[] input, int n)
        {
            byte[] counter = LSB(input, n);

            for (int i = counter.Length - 1; i >= 0; i--)
            {
                // Increment every previous byte until there is no carry.
                bool carry = counter[i] == 0xFF;
                counter[i]++;
                if (!carry) break;
            }

            return MSB(input, blocksize - n).Concat(counter).ToArray();
        }

        /// <summary>
        /// Multiplies the given byte array by 0x87 in GF(2^blocksize).
        /// </summary>
        /// <param name="input">The array for the multiplication.</param>
        /// <returns>The result of the multiplication.</returns>
        private byte[] GF_MULT_0x87(byte[] input)
        {
            byte[] result = new byte[input.Length];

            int carryIn = 0, carryOut = 0;
            for (int i = 0; i < input.Length; i++)
            {
                carryOut = (input[i] >> 7) & 1;
                result[i] = (byte)(((input[i] << 1) + carryIn) & 0xFF);
                carryIn = carryOut;
            }

            if (carryOut == 1)
            {
                // alpha = 0x87
                result[0] ^= 0x87;
            }

            return result;
        }

        /// <summary>
        /// Shifts an entire byte array one bit to the right.
        /// </summary>
        /// <param name="input">The array whicht will be shifted.</param>
        /// <returns>Input &gt;&gt; 1</returns>
        private byte[] ShiftRightByOneBit(byte[] input)
        {
            byte[] result = new byte[input.Length];

            // Shift the last byte
            result[result.Length - 1] = input[input.Length - 1] >>= 1;

            for (int i = input.Length - 2; i >= 0; i--)
            {
                // Check for carry in current byte and add it to following byte.
                if ((input[i] & 0000_0001) != 0)
                {
                    result[i + 1] += 0xE0;
                }
                result[i] = input[i] >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Multiplicates two byte arrays in GF(2^blocksize).
        /// </summary>
        /// <param name="a">The multiplicand.</param>
        /// <param name="H">The GHASH constant.</param>
        /// <returns>(a * H) mod 2^blocksize.</returns>
        private byte[] GF_BYTE_MULT(byte[] a, byte[] H)
        {
            byte[] result = new byte[blocksize];
            byte[] temp = a;

            for (int i = 0; i < blocksize; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // Recalculate result
                    if ((H[i] & (1 << (7 - j))) != 0)
                    {
                        result = XOR(result, temp);
                    }

                    // Recalculate temp
                    if ((temp[blocksize - 1] & 0000_0001) == 0)
                    {
                        temp = ShiftRightByOneBit(temp);
                    }
                    else
                    {
                        temp = ShiftRightByOneBit(temp);
                        // temp XOR (1110_0001 || 0^120)
                        temp[0] ^= 0xE1;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Compresses a byte array with multiple length of the blocksize to a byte array with blocksize's length.
        /// </summary>
        /// <param name="input">The array which will be compressed.</param>
        /// <param name="key">The key for encrypting H.</param>
        /// <returns>The compressed input.</returns>
        private byte[] GHASH(byte[] input, byte[] key)
        {
            byte[] temp = new byte[blocksize];
            byte[] result = new byte[blocksize];
            byte[] H = Blockcipher.Encrypt(result, key);

            int numberOfBlocks = input.Length / blocksize;

            for (int i = 0; i < numberOfBlocks; i++)
            {
                Array.Copy(input, i * blocksize, temp, 0, blocksize);
                result = GF_BYTE_MULT(XOR(result, temp), H);
            }

            return result;
        }

        /// <summary>
        /// Creates a byte array representation of the integer in a specific length.
        /// </summary>
        /// <param name="value">The integer.</param>
        /// <param name="length">The length of the resulting byte array.</param>
        /// <returns>A byte array with the input's value.</returns>
        private byte[] IntToByteArray(int value, int length)
        {
            byte[] result = new byte[length];
            byte[] byteValue = BitConverter.GetBytes((uint)value);
            int resultLength = Math.Min(length, byteValue.Length);

            // Need to reverse the byte array?
            if (BitConverter.IsLittleEndian)
            {
                Array.Copy(byteValue, result, resultLength);
                Array.Reverse(result);
            }
            else
            {
                Array.Copy(byteValue, 0, result, length - resultLength, resultLength);
            }

            return result;
        }

        /// <summary>
        /// Compares two byte arrays if they are equals or not.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <returns>True if a == b, false if not.</returns>
        private bool Equals(byte[] a, byte[] b)
        {
            if (a == null && b != null || a != null && b == null) return false;
            if (a == null && b == null) return true;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
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
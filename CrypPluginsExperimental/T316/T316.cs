/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System;
using System.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.T316
{
    [Author("Michael Altenhuber", "michael@altenhuber.net", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.T316.Properties.Resources", "PluginCaption", "PluginTooltip", "T316/userdoc.xml", "T316/images/t316_icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class T316 : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly T316Settings settings = new T316Settings();
        private RandomNumberGenerator rng;
        private bool stopPressed = false;
        private byte[] iV;

        private const uint maxMessageSize = 1000;
        private const byte figs = 0x1B; // A CCITT-2 control character, indicates a figure is following
        private const byte ltrs = 0x1F; // A CCITT-2 control character, indicates a letter is following
        private static readonly byte[] charPadding = new byte[] { 0x24, 0x24, 0x24, 0x24 }; // represents $ in ASCII

        /*
         * Lookup and reverse lookup tables for converting between ASCII and CCITT-2
         */
        private static Dictionary<byte, byte> ccitLetters = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x01, 0x45}, // E
            {0x02, 0x0A}, // LR (Line Feed)
            {0x03, 0x41}, // A
            {0x04, 0x20}, // Space
            {0x05, 0x53}, // S
            {0x06, 0x49}, // I 
            {0x07, 0x55}, // U
            {0x08, 0x0D}, // CR (Carriage Return)
            {0x09, 0x44}, // D
            {0x0A, 0x52}, // R
            {0x0B, 0x4A}, // J
            {0x0C, 0x4E}, // N
            {0x0D, 0x46}, // F
            {0x0E, 0x43}, // C
            {0x0F, 0x4B}, // K
            {0x10, 0x54}, // T
            {0x11, 0x5A}, // Z
            {0x12, 0x4C}, // L
            {0x13, 0x57}, // W
            {0x14, 0x48}, // H
            {0x15, 0x59}, // Y
            {0x16, 0x50}, // P
            {0x17, 0x51}, // Q
            {0x18, 0x4F}, // O
            {0x19, 0x42}, // B
            {0x1A, 0x47}, // G
            {0x1B, 0x00}, // FIGS (control character - no ASCII representation)
            {0x1C, 0x4D}, // M
            {0x1D, 0x58}, // X
            {0x1E, 0x56}, // V
            {0x1F, 0x00}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitFigures = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x01, 0x33}, // 3
            {0x02, 0x0A}, // LR (Line Feed)
            {0x03, 0x2D}, // -
            {0x04, 0x20}, // Space
            {0x05, 0x27}, // '
            {0x06, 0x38}, // 8 
            {0x07, 0x37}, // 7
            {0x08, 0x0D}, // CR (Carriage Return)
            {0x09, 0x05}, // ENC (Enquiry, Who are you?, WRU)
            {0x0A, 0x34}, // 4 
            {0x0B, 0x07}, // BEL (Bell, ring at other end)
            {0x0c, 0x2C}, // ,
            {0x0D, 0x21}, // !
            {0x0E, 0x3A}, // :
            {0x0F, 0x28}, // (
            {0x10, 0x35}, // 5
            {0x11, 0x2B}, // +
            {0x12, 0x29}, // )
            {0x13, 0x32}, // 2
            {0x14, 0x24}, // $
            {0x15, 0x36}, // 6
            {0x16, 0x30}, // 0
            {0x17, 0x31}, // 1
            {0x18, 0x39}, // 9
            {0x19, 0x3F}, // ?
            {0x1A, 0x26}, // &
            {0x1B, 0x00}, // FIGS (control character - no ASCII representation)
            {0x1C, 0x2E}, // .
            {0x1D, 0x2F}, // /
            {0x1E, 0x3B}, // ;
            {0x1F, 0x00}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitLettersReverse = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x45, 0x01}, // E
            {0x0A, 0x02}, // LR (Line Feed)
            {0x41, 0x03}, // A
            {0x20, 0x04}, // Space
            {0x53, 0x05}, // S
            {0x49, 0x06}, // I 
            {0x55, 0x07}, // U
            {0x0D, 0x08}, // CR (Carriage Return)
            {0x44, 0x09}, // D
            {0x52, 0x0A}, // R
            {0x4A, 0x0B}, // J
            {0x4E, 0x0C}, // N
            {0x46, 0x0D}, // F
            {0x43, 0x0E}, // C
            {0x4B, 0x0F}, // K
            {0x54, 0x10}, // T
            {0x5A, 0x11}, // Z
            {0x4C, 0x12}, // L
            {0x57, 0x13}, // W
            {0x48, 0x14}, // H
            {0x59, 0x15}, // Y
            {0x50, 0x16}, // P
            {0x51, 0x17}, // Q
            {0x4F, 0x18}, // O
            {0x42, 0x19}, // B
            {0x47, 0x1A}, // G
            {0xFE, 0x1B}, // FIGS (control character - no ASCII representation)
            {0x4D, 0x1C}, // M
            {0x58, 0x1D}, // X
            {0x56, 0x1E}, // V
            {0xFF, 0x1F}  // LTRS (control character - no ASCII representation)
        };
        private static Dictionary<byte, byte> ccitFiguresReverse = new Dictionary<byte, byte>
        {
            {0x00, 0x00}, // Null (blank tape)
            {0x33, 0x01}, // 3
            {0x0A, 0x02}, // LR (Line Feed)
            {0x2D, 0x03}, // -
            {0x20, 0x04}, // Space
            {0x27, 0x05}, // '
            {0x38, 0x06}, // 8 
            {0x37, 0x07}, // 7
            {0x0D, 0x08}, // CR (Carriage Return)
            {0x05, 0x09}, // ENC (Enquiry, Who are you?, WRU)
            {0x34, 0x0A}, // 4 
            {0x07, 0x0B}, // BEL (Bell, ring at other end)
            {0x2C, 0x0c}, // ,
            {0x21, 0x0D}, // !
            {0x3A, 0x0E}, // :
            {0x28, 0x0F}, // (
            {0x35, 0x10}, // 5
            {0x2B, 0x11}, // +
            {0x29, 0x12}, // )
            {0x32, 0x13}, // 2
            {0x24, 0x14}, // $
            {0x36, 0x15}, // 6
            {0x30, 0x16}, // 0
            {0x31, 0x17}, // 1
            {0x39, 0x18}, // 9
            {0x3F, 0x19}, // ?
            {0x26, 0x1A}, // &
            {0xFE, 0x1B}, // FIGS (control character - no ASCII representation)
            {0x2E, 0x1C}, // .
            {0x2F, 0x1D}, // /
            {0x3B, 0x1E}, // ;
            {0xFF, 0x1F}  // LTRS (control character - no ASCII representation)
        };

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputData", "InputDataTooltip")]
        public byte[] InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKey", "InputKeyTooltip")]
        public byte[] InputKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputData", "OutputDataTooltip")]
        public byte[] OutputData
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members


        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }


        public void PreExecution()
        {
            // empty
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
            stopPressed = false;

            if (!checkInput())
                return;

            if (InputData.Length > maxMessageSize)
                GuiLogMessage(Properties.Resources.WarningMaximumLength, NotificationLevel.Warning);


            if (settings.Mode == OperationMode.Encrypt)
                PrepareAndEncrypt();
            else
                PrepareAndDecrypt();

        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            Dispose();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// </summary>
        public void Stop()
        {
            stopPressed = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            rng = RandomNumberGenerator.Create();
            iV = new byte[Lambda1.BlockSize];
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            InputData = null;
            InputKey = null;
            OutputData = null;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Takes the InputData and encrypts it with the LAMBDA1-Algorithm
        /// </summary>
        private void PrepareAndEncrypt()
        {
            InputData = RemoveUnicode(InputData);
            MemoryStream outputStream = new MemoryStream();
            byte[] invalidCharacters, paddedCharacters, chunks;
            byte[] convertedChars = AsciiToCcitt2(InputData, out invalidCharacters);
            rng.GetBytes(iV);

            // if necessary, communicate truncated characters to the user
            if (invalidCharacters != null && invalidCharacters.Length > 0)
                handleInvalidCharacters(invalidCharacters);

            // Check if a message is left after converting 
            if (convertedChars.Length <= 0)
            {
                GuiLogMessage(Properties.Resources.ErrorEmptyConversion, NotificationLevel.Error);
                return;
            }

            // Make "chunks" (Concatenating the ASCII size CCITT-2 letters to 6-bit chunks)
            PadCharacters(convertedChars, out paddedCharacters);
            ContractChunks(paddedCharacters, out chunks);

            Lambda1 lambda1 = new Lambda1(InputKey, settings.Mode);
            byte[] tmpBlock = new byte[Lambda1.BlockSize];
            byte[] oldBlock = new byte[Lambda1.BlockSize];
            byte[] currentBlock = new byte[Lambda1.BlockSize];

            byte[] inputData = BlockCipherHelper.AppendPadding(chunks, BlockCipherHelper.PaddingType.Zeros, Lambda1.BlockSize);
            Array.Copy(iV, oldBlock, Lambda1.BlockSize);
            outputStream.Write(iV, 0, Lambda1.BlockSize);

            for (int i = 0; i < inputData.Length && !stopPressed; i += Lambda1.BlockSize)
            {
                Array.Copy(inputData, i, currentBlock, 0, Lambda1.BlockSize);
                lambda1.ProcessBlock(XorBlock(oldBlock, currentBlock), out tmpBlock);
                outputStream.Write(tmpBlock, 0, tmpBlock.Length);
                Array.Copy(tmpBlock, oldBlock, Lambda1.BlockSize);
                ProgressChanged(i, inputData.Length - 1);
            }

            ProgressChanged(1, 1);
            OutputData = outputStream.ToArray();
            OnPropertyChanged("OutputData");
        }


        /// <summary>
        /// Takes the InputData and decrypts it
        /// </summary>
        private void PrepareAndDecrypt()
        {
            byte[] decryptionBuffer = new byte[InputData.Length];

            Lambda1 lambda1 = new Lambda1(InputKey, settings.Mode);
            byte[] tmpBlock = new byte[Lambda1.BlockSize];
            byte[] oldBlock = new byte[Lambda1.BlockSize];
            byte[] currentBlock = new byte[Lambda1.BlockSize];

            if (InputData.Length % Lambda1.BlockSize != 0)
            {
                GuiLogMessage(String.Format(Properties.Resources.ErrorInputBlockLength, InputData.Length, Lambda1.BlockSize), NotificationLevel.Error);
                return;
            }

            Array.Copy(InputData, oldBlock, Lambda1.BlockSize);

            // Decryption Loop with CBC mode
            for (int i = Lambda1.BlockSize; i < InputData.Length && !stopPressed; i += Lambda1.BlockSize)
            {
                Array.Copy(InputData, i, currentBlock, 0, Lambda1.BlockSize);
                lambda1.ProcessBlock(currentBlock, out tmpBlock);
                Array.Copy(XorBlock(tmpBlock, oldBlock), 0, decryptionBuffer, i - 8, Lambda1.BlockSize);
                Array.Copy(currentBlock, oldBlock, Lambda1.BlockSize);
                ProgressChanged(i, InputData.Length - 1);
            }

            //Strip padding and convert back to ASCII
            decryptionBuffer = BlockCipherHelper.StripPadding(decryptionBuffer, BlockCipherHelper.PaddingType.Zeros, Lambda1.BlockSize);

            // Expand the chunks from 6 to 8 bit
            byte[] buffer;
            ExpandChunks(decryptionBuffer, out buffer);

            // Strip the character padding (don't confuse with the encryption padding)
            StripCharacterPadding(buffer, out decryptionBuffer);


            ProgressChanged(1, 1);
            OutputData = Ccitt2ToAscii(decryptionBuffer);
            OnPropertyChanged("OutputData");

        }

        /// <summary>
        /// Checks if keys and input connectors are correct
        /// </summary>
        /// <returns></returns>
        private bool checkInput()
        {
            if (InputData == null)
            {
                GuiLogMessage(Properties.Resources.ErrorInputDataNull, NotificationLevel.Error);
                return false;
            }

            if (InputKey == null)
            {
                GuiLogMessage(Properties.Resources.ErrorKeyNull, NotificationLevel.Error);
                return false;
            }

            if (InputData.Length == 0)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorInputDataEmpty, InputData.Length), NotificationLevel.Error);
                return false;
            }

            if (InputKey.Length < Lambda1.KeySize)
            {
                GuiLogMessage(string.Format(InputKey.Length == 0 ?
                    Properties.Resources.ErrorKeyLengthEmpty :
                    Properties.Resources.ErrorKeyLengthShort,
                    InputKey.Length, Lambda1.KeySize), NotificationLevel.Error);
                return false;
            }

            if (InputKey.Length > Lambda1.KeySize)
            {
                GuiLogMessage(string.Format(Properties.Resources.ErrorKeyOverlength,
                    InputKey.Length, Lambda1.KeySize), NotificationLevel.Warning);
                byte[] tmp = new byte[Lambda1.KeySize];
                Array.Copy(InputKey, tmp, Lambda1.KeySize);
                InputKey = tmp;
            }
            return true;
        }


        /// <summary>
        /// Maps an array of ASCII characters to available 5 bit CCITT-2 characters
        /// </summary>
        /// CCITT-2 is a character encoding developed for telegraphy technology. It was used by the T310/50
        /// and therefore needs to be mapped accordingly. It knows only upper case letters (LTRS) and some special
        /// figures and numbers (FIGS). To switch between eachother, special control characters are included.
        /// 
        /// See <cref="T310:Ccitt2ToAscii"/> for the conversion in the opposite direction 
        /// 
        /// <param name="message">A 7-bit ASCII character, which has a respective representation in CCITT-2</param>
        /// <param name="invalidCharacter">Includes a list of all non-convertable characters. May return null if none occured</param>
        /// <returns>The byte value of the character in CCITT-2 encoding</returns>
        /// 
        private byte[] AsciiToCcitt2(byte[] message, out byte[] invalidCharacters)
        {
            bool figureShift = false;
            List<byte> encodedBytes = new List<byte>();
            List<byte> invalidBytes = new List<byte>();
            for (int i = 0; i < message.Length; ++i)
            {
                byte character = message[i];
                byte ccittChar = 0x00;

                // convert to upper case if needed
                if (character >= 0x61 && character <= 0x7A)
                    character -= 0x20;

                /*
                * Note on these lookups: We use containsKey and an assignment instead of TryGetValue()
                * because elsewise we can't check if the key really exists and is 0x0 or is a default 0x0
                */

                // Check if we can find it in the letter table
                if (ccitLettersReverse.ContainsKey(character))
                {
                    ccittChar = ccitLettersReverse[character];

                    //check if we are in figure mode, if we are we have to switch back to letters
                    if (figureShift)
                    {
                        encodedBytes.Add(ltrs);
                        figureShift = false;
                        encodedBytes.Add(ccittChar);
                    }
                    else
                        encodedBytes.Add(ccittChar);
                }
                // Check if we can find it in the figures table
                else if (ccitFiguresReverse.ContainsKey(character))
                {
                    ccittChar = ccitFiguresReverse[character];

                    //check if we are NOT in figure mode, if we are we have to switch to figure mode
                    if (!figureShift)
                    {
                        encodedBytes.Add(figs);
                        figureShift = true;
                        encodedBytes.Add(ccittChar);
                    }
                    else
                        encodedBytes.Add(ccittChar);
                }
                // If we couldn't find anything, we add it to the list of invalid characters
                else
                {
                    // We only want a single instance of invalid characters in the list
                    if (!invalidBytes.Contains(character))
                        invalidBytes.Add(character);
                }

            }

            // Pack the converted characters and the invalid ones into arrays
            if (invalidBytes.Count > 0)
                invalidCharacters = invalidBytes.ToArray();
            else
                invalidCharacters = null;
            return encodedBytes.ToArray();
        }


        /// <summary>
        /// Maps an array of CCITT-2 characters to ASCII characters
        /// </summary>
        /// <param name="message">a string as bytearray which will be converted</param>
        /// <returns>an ASCII encoded byte[]</returns>
        private byte[] Ccitt2ToAscii(byte[] message)
        {
            bool figureShift = false;
            byte tmpByte;
            List<byte> encodedBytes = new List<byte>();
            foreach (byte character in message)
            {
                if (character == figs)
                {
                    figureShift = true;
                    continue;
                }
                if (character == ltrs)
                {
                    figureShift = false;
                    continue;
                }

                /*
                * Note on these lookups: We don't check for non-existent characters here,
                * because we don't expect them to be coming out of the machine.
                */
                if (figureShift)
                    ccitFigures.TryGetValue(character, out tmpByte);
                else
                    ccitLetters.TryGetValue(character, out tmpByte);

                encodedBytes.Add(tmpByte);

            }

            return encodedBytes.ToArray();
        }


        /// <summary>
        /// Pad an ASCII-Message to a multiple of 4 characters, as they are pulled together to a group 
        /// </summary>
        /// <param name="characters">A character array</param>
        /// <param name="paddedCharacters">A character array padded to a multiple of length 4</param>
        /// In the further encryption process, groups of 4 ASCII characters are converted to CCITT-2 and then
        /// encrypted. Therefore we need an extra padding here (Fill character is 0x24 = $)
        private void PadCharacters(byte[] characters, out byte[] paddedCharacters)
        {
            if (characters.Length % 4 != 0)
            {
                paddedCharacters = new byte[characters.Length + (4 - (characters.Length % 4))];
                int len1 = paddedCharacters.Length - (4 - (characters.Length % 4));
                int len2 = 4 - (characters.Length % 4);
                Array.Copy(charPadding, 0, paddedCharacters, len1, len2);
                Array.Copy(characters, paddedCharacters, characters.Length);
            }
            else
            {
                paddedCharacters = new byte[characters.Length];
                Array.Copy(characters, paddedCharacters, characters.Length);
            }
        }


        /// <summary>
        /// Contracts an array with length multiple of 4 bytes with 4 characters to 3 bytes.
        /// Counterpart function to MakeChunks(). Call PadCharacters() beforehand.
        /// </summary>
        /// <param name="ccittCharacters">CCITT-2 encoded byte[] with one character per byte and padding</param>
        /// <param name="chunks">CCITT-2 encoded byte[] with 1 and a 1/4th character per byte</param>
        /// This function omits the high bits 7 and 8 and pulls together the array, saving 1/4th of spacee
        private void ContractChunks(byte[] ccittCharacters, out byte[] chunks)
        {
            UInt32 tmp = ccittCharacters[0];

            chunks = new byte[ccittCharacters.Length / 4 * 3];

            for (int i = 1, k = 0; i < ccittCharacters.Length; ++i)
            {
                tmp <<= 6;
                tmp |= ccittCharacters[i];

                if ((i + 1) % 4 == 0)
                {
                    Array.Copy(IntToByte3(tmp), 0, chunks, k, 3);
                    tmp = 0;
                    k += 3;
                }
            }
        }


        /// <summary>
        ///  Expands an array with length multiple of 3 bytes with 4 characters to 4 bytes. Counterpart function to MakeChunks().
        /// </summary>
        /// <param name="chunkedBytes"></param>
        /// <param name="ccittCharacters"></param>
        private void ExpandChunks(byte[] chunkedBytes, out byte[] ccittCharacters)
        {
            UInt32 buffer = chunkedBytes[0];
            ccittCharacters = new byte[(chunkedBytes.Length / 3) * 4];

            for (int i = 1, k = 3, tmp = 0; i < chunkedBytes.Length; ++i)
            {

                buffer <<= 8;
                buffer |= chunkedBytes[i];


                if ((i + 1) % 3 == 0)
                {
                    for (int j = 3; k >= tmp; --k, --j)
                        ccittCharacters[tmp + (3 - j)] = (byte)((buffer >> (j * 6)) & 0x3F);
                    tmp += 4;
                    k = tmp + 3;
                    buffer = 0;
                }
            }
        }


        /// <summary>
        /// Strips the 0x24 ($) character padding from the 4 character chunks
        /// </summary>
        /// <param name="ccittCharacters">CCITT-2 encoded byte[] with one character per byte and padding</param>
        /// <param name="strippedCharacters">CCITT-2 encoded byte[] with one character per byte WITHOUT padding</param>
        private void StripCharacterPadding(byte[] ccittCharacters, out byte[] strippedCharacters)
        {
            int i = ccittCharacters.Length - 1;
            for (; ccittCharacters[i] == 0x24; --i) ;

            strippedCharacters = new byte[++i];
            Array.Copy(ccittCharacters, strippedCharacters, i);
        }


        /// <summary>
        /// Converts an 32 bit Integer to a byte[] of length 3.
        /// High bits of the integer are the low bits of the byte[]
        /// </summary>
        /// <param name="a">an Integer which gets converted</param>
        /// <returns>a byte[] of length 3.</returns>
        private byte[] IntToByte3(UInt32 a)
        {
            byte[] array = new byte[3];
            for (int i = 2, k = 0; i >= 0; --i, ++k)
                array[k] = (byte)((a >> (i * 8)) & 0xFF);

            return array;
        }

        /// <summary>
        /// Purge a byte array of all non ASCII characters
        /// </summary>
        /// <param name="stringAsBytes">A byte array which gets interpreted as UTF-8 string</param>
        /// <returns>A byte array string with only ASCII characters</returns>
        private byte[] RemoveUnicode(byte[] stringAsBytes)
        {
            string asAscii = Encoding.ASCII.GetString(
                Encoding.Convert(
                    Encoding.UTF8,
                    Encoding.GetEncoding(
                        Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                stringAsBytes
               )
            );
            return Encoding.ASCII.GetBytes(asAscii);
        }

        /// <summary>
        /// Communicates truncated characters in the plaintext to the user
        /// </summary>
        /// <param name="invalidCharacters">A byte array containing a single instance of characters</param>
        private void handleInvalidCharacters(byte[] invalidCharacters)
        {
            string truncatedMessage = invalidCharacters.Length == 1 ?
                           string.Format(Properties.Resources.ErrorUnconvertableBeginningSingular, invalidCharacters.Length) :
                           string.Format(Properties.Resources.ErrorUnconvertableBeginningPlural, invalidCharacters.Length);

            //we will only print the non-convertable characters if there are less than 10
            if (invalidCharacters.Length <= 10)
            {

                truncatedMessage += ": ";
                for (int i = 0; i < invalidCharacters.Length; ++i)
                    truncatedMessage += "'" + Encoding.ASCII.GetString(invalidCharacters, i, 1) + "', ";
                // Truncate the ", " from the last character
                truncatedMessage = truncatedMessage.Remove(truncatedMessage.Length - 2);
            }
            truncatedMessage += invalidCharacters.Length == 1 ?
                Properties.Resources.ErrorUnconvertableEndSingular :
                Properties.Resources.ErrorUnconvertableEndPlural;

            GuiLogMessage(truncatedMessage, NotificationLevel.Warning);
        }

        /// <summary>
        ///  XORs a block for CBC mode
        /// </summary>
        /// <param name="a">a 4 byte block or initialisation vector</param>
        /// <param name="b">a 4 byte block </param>
        /// <returns> the XORed 4 byte block</returns>
        private byte[] XorBlock(byte[] a, byte[] b)
        {
            int length = System.Math.Min(a.Length, b.Length);
            byte[] tmp = new byte[length];
            for (int i = 0; i < length; ++i)
                tmp[i] = (byte)(a[i] ^ b[i]);
            return tmp;
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

#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Windows;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase;
using Keccak.Properties;

namespace Cryptool.Plugins.Keccak
{
    public static class KeccakHashFunction
    {

        public static byte[] Hash(byte[] input, int outputLength, int rate, int capacity, ref KeccakPres pres, Keccak plugin, KeccakSettings settings)
        {
#if _DEBUG_
            Console.WriteLine("#Keccak: running Keccak with the following parameters:");
            Console.WriteLine(String.Format("#Keccak: {0}: {1} bits", "output length", outputLength));
            Console.WriteLine(String.Format("#Keccak: {0}: {1} bits", "state size", rate + capacity));
            Console.WriteLine(String.Format("#Keccak: {0}: {1} bits", "bit rate", rate));
            Console.WriteLine(String.Format("#Keccak: {0}: {1} bits", "capacity", capacity));
            Console.WriteLine();
#endif


            /* map each bit of the input to a byte */
            byte[] inputInBitsWithoutSuffix = ByteArrayToBitArray(input, (KeccakSettings.InputTypeEnum) settings.InputType, plugin);

            /* append domain separation suffix bits */
            byte[] inputInBits = appendSuffixBits(settings.SuffixBits, inputInBitsWithoutSuffix);

            /* for presentation: estimate number of keccak-f executions */
            int progressionSteps = (int)Math.Ceiling((double)(inputInBits.Length + 8) / rate) + ((int)Math.Ceiling((double)outputLength / rate) - 1);

            /* create sponge instance */
            Sponge sponge = new Sponge(rate, capacity, ref pres, plugin, progressionSteps);

            /* absorb input */
            sponge.Absorb(inputInBits);

            /* squeeze sponge to obtain output */
            Debug.Assert(outputLength % 8 == 0);
            byte[] outputInBits = sponge.Squeeze(outputLength);

            /* reverse 'bit to byte' mapping */
            byte[] output = BitArrayToByteArray(outputInBits);

#if _DEBUG_
            Console.WriteLine("#Keccak: successfully hashed {0} input bits to {1} output bits!", inputInBits.Length, outputInBits.Length);
            Console.WriteLine("#Keccak: all work is done!");
#endif


            return output;
        }

        private static byte[] appendSuffixBits(string suffixBits, byte[] inputInBitsWithoutSuffix)
        {
            if (suffixBits.Length == 0)
                return inputInBitsWithoutSuffix;

            int newSize = inputInBitsWithoutSuffix.Length + suffixBits.Length;
            byte[] inputInBits = new byte[newSize];

            if (inputInBitsWithoutSuffix.Length > 0)
                Array.Copy(inputInBitsWithoutSuffix, inputInBits, inputInBitsWithoutSuffix.Length);

            char[] suffixBitsArray = suffixBits.ToCharArray();
            for (int i = 0; i < suffixBitsArray.Length; i++)
            {
                byte b = suffixBitsArray[i] == '1' ? (byte)0x01 : (byte)0x00;
                inputInBits[inputInBitsWithoutSuffix.Length + i] = b;
            }


            //int indexOfLastOne = inputInBitsWithoutSuffix.Contains((byte)0x01) ?
            //    Array.LastIndexOf(inputInBitsWithoutSuffix, (byte)0x01) : -1;
            //int size = indexOfLastOne + 1 + suffixBits.Length;
            //byte[] inputInBits = new byte[size];

            //if (size != suffixBits.Length)
            //    Array.Copy(inputInBitsWithoutSuffix, inputInBits, inputInBitsWithoutSuffix.Length);

            //char[] suffixBitsArray = suffixBits.ToCharArray();
            //for (int i = 0; i < suffixBitsArray.Length; i++)
            //{
            //    byte b = suffixBitsArray[i] == '1' ? (byte)0x01 : (byte)0x00;
            //    inputInBits[indexOfLastOne + 1 + i] = b;
            //}          

            return inputInBits;
        }

        #region helper methods

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ByteArrayToIntString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                hex.Append((int)b);
            }
            return hex.ToString();
        }

        public static void PrintByteArray(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2} ", b);
            }
            Console.WriteLine(hex.ToString());
            Console.WriteLine(" - " + bytes.Length + " bytes");
        }

        public static void PrintBits(byte[] bytes)
        {
            string hexStr = ByteArrayToIntString(bytes);
            Console.WriteLine(hexStr + " - " + hexStr.Length + " bits");
        }

        public static void PrintBits(byte[] bytes, int laneSize)
        {
            /* only print bit state if lane size is small enough*/
            if (laneSize >= 16)
            {
                return;
            }

            string hexStr = "";
            StringBuilder hex = new StringBuilder(bytes.Length);
            int j = 0;

            foreach (byte b in bytes)
            {
                if (j % laneSize == 0)
                {
                    hex.AppendFormat("{0:00}: ", j / laneSize);
                }

                hex.Append((int)b);

                j++;
                if (j % laneSize == 0)
                {
                    hex.Append(Environment.NewLine);
                }
            }

            hexStr = hex.ToString();
            hex.Clear();
            Console.WriteLine(hexStr); // + " - " + (hexStr.Length - (j / value)) + " bits");
        }

        public static void PrintBytes(byte[] bytes, int laneSize)
        {
            /* only print byte state if lane size is large enough*/
            if (laneSize < 16 && laneSize % 8 != 0)
            {
                return;
            }

            StringBuilder binaryBytes = new StringBuilder(bytes.Length);
            StringBuilder bitString = new StringBuilder(8);
            char[] bitChars = new char[8];

            for (int i = 0; i < bytes.Length; i += 8)
            {
                if (i % laneSize == 0)
                {
                    binaryBytes.AppendFormat(Environment.NewLine + "{0:00}: ", i / laneSize);
                }

                for (int j = 0; j < 8; j++)
                {
                    bitString.Append((int)bytes[i + j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                }

                binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                bitString.Clear();

            }
            Console.WriteLine(binaryBytes.ToString());
        }

        /** 
         * returns a hex string presentation of the byte array `bytes`
         * the parameter `laneSize` determines after how many bytes a line break is inserted           
         */
        public static string GetByteArrayAsString(byte[] bytes, int laneSize)
        {          
            /* get bit state if lane size is small */
            if (laneSize < 16) // && laneSize % 8 != 0)
            {
                string hexStr = "";
                StringBuilder hex = new StringBuilder(bytes.Length);
                int j = 0;

                foreach (byte b in bytes)
                {
                    //if (j % laneSize == 0)
                    //{
                    //    hex.AppendFormat("{0:00}: ", j / laneSize);
                    //}

                    hex.Append((int)b);

                    j++;
                    if (j % laneSize == 0)
                    {
                        hex.Append(Environment.NewLine);
                    }
                }

                hexStr = hex.ToString();
                hex.Clear();

                return hexStr;
            }
            /* get byte presentation of state otherwise (lane size at least 2 bytes) */
            else
            {
                StringBuilder binaryBytes = new StringBuilder(bytes.Length);
                StringBuilder bitString = new StringBuilder(8);
                char[] bitChars = new char[8];

                for (int i = 0; i < bytes.Length; i += 8)
                {
                    /* append line break at the end of a lane */
                    if (i != 0 && i % laneSize == 0)
                    {
                        binaryBytes.Append(Environment.NewLine);
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        bitString.Append((int)bytes[i + j]);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                    }

                    binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                                       
                    bitString.Clear();
                }

                return binaryBytes.ToString();
            }
        }

        /**
         * Converts a byte array to an another byte array. The returned byte array contains the bit representation of the input byte array
         * where each byte represents a bit of the input byte array
         * */
        public static byte[] ByteArrayToBitArray(byte[] bytes, KeccakSettings.InputTypeEnum inputType, Keccak plugin = null)
        {
            List<byte> bitsInBytes = new List<byte>(bytes.Length * 8);
            string bitString;
            char[] bitChars = new char[8];

            switch (inputType)
            {
                case KeccakSettings.InputTypeEnum.Binary:
                    foreach (byte b in bytes)
                    {
                        if (b == 48)
                            bitsInBytes.Add(0x00);
                        else if (b == 49)
                            bitsInBytes.Add(0x01);
                        else
                        {
                            if (plugin != null)
                                plugin.GuiLogMessage(String.Format(Resources.InputTypeWarning, Resources.InputTypeBinary), NotificationLevel.Warning);
                        }
                            
                    }
                    break;
                case KeccakSettings.InputTypeEnum.Hexadecimal:
                    int i, j;
                    for (i = j = 0; i < bytes.Length; i += 2, j = i) {

                        if (i + 1 < bytes.Length)
                            j++;
                        
                        for (; j >= i; j--)
                        {
                            switch(bytes[j])
                            {
                                case 48: // 0
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); break;
                                case 49: // 1
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); break;
                                case 50: // 2
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); break;
                                case 51: // 3
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); break;
                                case 52: // 4
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); break;
                                case 53: // 5
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); break;
                                case 54: // 6
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); break;
                                case 55: // 7
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); break;
                                case 56: // 8
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); break;
                                case 57: // 9
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); break;
                                case 65: // A
                                case 97: // a
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); break;
                                case 66: // B
                                case 98: // b
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); break;
                                case 67: // C
                                case 99: // c
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); break;
                                case 68:  // D
                                case 100: // d
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); break;
                                case 69:  // E
                                case 101: // e
                                    bitsInBytes.Add(0x00); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); break;
                                case 70:  // F
                                case 102: // f
                                    bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); bitsInBytes.Add(0x01); break;
                                default:
                                    if (plugin != null)
                                        plugin.GuiLogMessage(String.Format(Resources.InputTypeWarning, Resources.InputTypeHexadecimal), NotificationLevel.Warning); break;
                            }
                        }                            
                    }
                    break;
                default:
                case KeccakSettings.InputTypeEnum.Text:
                    foreach (byte b in bytes)
                    {
                        /* convert each byte into a bit-string */
                        bitString = Convert.ToString(b, 2).PadLeft(8, '0');

                        /* swap every bit to get the right order of bits in memory */
                        for (int k = 0; k < 8; k++)
                        {
                            bitChars[k] = bitString.ElementAt(8 - 1 - k);
                        }

                        foreach (char c in bitChars)
                        {
                            if (c == '0')
                            {
                                bitsInBytes.Add(0x00);
                            }
                            else if (c == '1')
                            {
                                bitsInBytes.Add(0x01);
                            }
                        }
                    }
                    break;
            }

            return bitsInBytes.ToArray();
        }

        public static byte[] BitArrayToByteArray(byte[] bitsInBytes)
        {
            string c;
            char[] bitChars = new char[8];
            StringBuilder bitString = new StringBuilder(8);

            Debug.Assert(bitsInBytes.Length % 8 == 0);
            byte[] bytes = new byte[bitsInBytes.Length / 8];

            for (int i = 0; i < bytes.Length; i++)
            {
                bitString.Clear();
                for (int j = 0; j < 8; j++)
                {
                    c = bitsInBytes[i * 8 + j] == 0x01 ? "1" : "0";
                    bitString.Append(c);
                }

                /* swap back every bit to get the right order of bits in a byte */
                for (int k = 0; k < 8; k++)
                {
                    bitChars[k] = bitString.ToString().ElementAt(8 - 1 - k);
                }

                bytes[i] = Convert.ToByte(new string(bitChars), 2);
            }

            return bytes;
        }

        #endregion

    }
}

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
using System.Collections;

namespace Speck
{
    public static class SpeckCiphers
    {
        private static int _wordSize;
        private static int _blockLength;
        private static int _keyLength;
        private static int _rounds;

        #region Speck32/xx

        /// <summary>
        /// Speck32/64 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck32_64_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 16;
            _blockLength = 32;
            _keyLength = 64;
            _rounds = 22;

            //convert plaintext bytes to plaintext words
            UInt16[] ptWords = BytesToWords16(text, 2);

            //convert key bytes to key words
            UInt16[] K = BytesToWords16(key, 4);

            //calculate round keys
            UInt16[] rk = (UInt16[])KeySchedule(K);

            //encrypt
            UInt16[] ctWords = Speck32Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words16ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck32/64 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck32_64_Decryption(byte[] text, byte[] key)
        {
            //convert ciphertext bytes to ciphertext words
            UInt16[] ctWords = BytesToWords16(text, 2);

            //convert key bytes to key words
            UInt16[] K = BytesToWords16(key, 4);

            //calculate round keys
            UInt16[] rk = KeySchedule(K);

            //decrypt
            UInt16[] ptWords = Speck32Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words16ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion

        #region Speck48/xx

        /// <summary>
        /// Speck48/72 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_72_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck48/72 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_72_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck48/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_96_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck48/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_96_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Speck64/xx

        /// <summary>
        /// Speck64/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_96_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck64/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_96_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck64/128 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_128_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck64/128 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_128_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Speck96/xx

        /// <summary>
        /// Speck96/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_96_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck96/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_96_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck96/144 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_144_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck96/144 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_144_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Speck128/xx

        /// <summary>
        /// Speck128/128 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_128_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck128/128 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_128_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck128/192 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_192_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck128/192 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_192_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck128/256 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_256_Encryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Speck128/256 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_256_Decryption(byte[] text, byte[] key)
        {
            throw new NotImplementedException();
        }

        #endregion



        #region BytesToWordsRegion

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static UInt16[] BytesToWords16(byte[] bytes, int wordCount)
        {
            UInt16[] words = new UInt16[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 2; i++)
            {
                words[i] = (UInt16)(bytes[j] | (bytes[j + 1] << 8));
                j += 2;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static UInt32[] BytesToWords24(byte[] bytes, int wordCount)
        {
            UInt32[] words = new UInt32[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 3; i++)
            {
                words[i] = bytes[j] | ((uint)bytes[j + 1] << 8) | ((uint)bytes[j + 2] << 16);
                j += 3;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static UInt32[] BytesToWords32(byte[] bytes, int wordCount)
        {
            UInt32[] words = new UInt32[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 4; i++)
            {
                words[i] = bytes[j] | ((UInt32)bytes[j + 1] << 8) | ((UInt32)bytes[j + 2] << 16) |
                           ((UInt32)bytes[j + 3] << 24);
                j += 4;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static UInt64[] BytesToWords48(byte[] bytes, int wordCount)
        {
            UInt64[] words = new UInt64[wordCount];

            int i, j = 0;
            for (i = 0; i < (bytes.Length / 6); i++)
            {
                words[i] = bytes[j] | ((UInt64)bytes[j + 1] << 8) | ((UInt64)bytes[j + 2] << 16) |
                           ((UInt64)bytes[j + 3] << 24) | ((UInt64)bytes[j + 4] << 32) |
                           ((UInt64)bytes[j + 5] << 40);
                j += 6;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static UInt64[] BytesToWords64(byte[] bytes, int wordCount)
        {
            UInt64[] words = new UInt64[wordCount];

            int i, j = 0;
            for (i = 0; i < (bytes.Length / 8); i++)
            {
                words[i] = bytes[j] | ((UInt64)bytes[j + 1] << 8) | ((UInt64)bytes[j + 2] << 16) |
                           ((UInt64)bytes[j + 3] << 24) | ((UInt64)bytes[j + 4] << 32) |
                           ((UInt64)bytes[j + 5] << 40) | ((UInt64)bytes[j + 6] << 48) |
                           ((UInt64)bytes[j + 7] << 56);
                j += 8;
            }

            return words;
        }

        #endregion

        #region WordsToBytesRegion

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words16ToBytes(UInt16[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(UInt16)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                j += 2;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words24ToBytes(UInt32[] words, int numWords)
        {
            byte[] result = new byte[numWords * 3];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                j += 3;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words32ToBytes(UInt32[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(UInt32)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                j += 4;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words48ToBytes(UInt64[] words, int numWords)
        {
            byte[] result = new byte[numWords * 6];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                result[j + 4] = (byte)(words[i] >> 32);
                result[j + 5] = (byte)(words[i] >> 40);
                j += 6;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words64ToBytes(UInt64[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(UInt64)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                result[j + 4] = (byte)(words[i] >> 32);
                result[j + 5] = (byte)(words[i] >> 40);
                result[j + 6] = (byte)(words[i] >> 48);
                result[j + 7] = (byte)(words[i] >> 56);
                j += 8;
            }

            return result;
        }

        #endregion

        #region RoundFunctions

        /// <summary>
        /// Round function with 64 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt64[] EncryptionRoundFunction(UInt64 x, UInt64 y, UInt64 z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            UInt64 xIntVal = BitConverter.ToUInt64(xBytesTrimmed, 0);

            xIntVal = xIntVal + y;
            xIntVal = xIntVal ^ z;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt64 yIntVal = BitConverter.ToUInt64(yBytesTrimmed, 0);

            yIntVal = yIntVal ^ xIntVal;

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 32 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt32[] EncryptionRoundFunction(UInt32 x, UInt32 y, UInt32 z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);

            UInt32 xIntVal = BitConverter.ToUInt32(xBytesTrimmed, 0);

            xIntVal = xIntVal + y;
            xIntVal = xIntVal ^ z;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 3);


            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[4];
            }


            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt32 yIntVal = BitConverter.ToUInt32(yBytesTrimmed, 0);

            yIntVal = yIntVal ^ xIntVal;

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 16 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt16[] EncryptionRoundFunction(UInt16 x, UInt16 y, UInt16 z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 7);

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            UInt16 xIntVal = BitConverter.ToUInt16(xBytesTrimmed, 0);

            xIntVal = (UInt16)(xIntVal + y);
            xIntVal = (UInt16)(xIntVal ^ z);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 2);

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt16 yIntVal = BitConverter.ToUInt16(yBytesTrimmed, 0);

            yIntVal = (UInt16)(yIntVal ^ xIntVal);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 64 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt64[] DecryptionRoundFunction(UInt64 x, UInt64 y, UInt64 z)
        {
            y = (UInt64)(y ^ x);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt64 yIntVal = BitConverter.ToUInt64(yBytesTrimmed, 0);

            x = (UInt64)(x ^ z);
            x = (UInt64)(x - yIntVal);

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            UInt64 xIntVal = BitConverter.ToUInt64(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 32 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt32[] DecryptionRoundFunction(UInt32 x, UInt32 y, UInt32 z)
        {
            y = (UInt32)(y ^ x);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt32 yIntVal = BitConverter.ToUInt32(yBytesTrimmed, 0);

            x = (UInt32)(x ^ z);
            x = (UInt32)(x - yIntVal);

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            UInt32 xIntVal = BitConverter.ToUInt32(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 16 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static UInt16[] DecryptionRoundFunction(UInt16 x, UInt16 y, UInt16 z)
        {
            y = (UInt16)(y ^ x);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 2);

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            UInt16 yIntVal = BitConverter.ToUInt16(yBytesTrimmed, 0);

            x = (UInt16)(x ^ z);
            x = (UInt16)(x - yIntVal);

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 7);

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            UInt16 xIntVal = BitConverter.ToUInt16(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        #endregion

        #region KeySchedules

        /// <summary>
        /// Key Schedule with 16 bit data types
        /// </summary>
        /// <param name="K"></param>
        /// <returns></returns>
        private static UInt16[] KeySchedule(UInt16[] K)
        {
            UInt16 rounds = (UInt16)_rounds;
            UInt16 i, D = K[3], C = K[2], B = K[1], A = K[0];
            UInt16[] rk = new UInt16[rounds];

            for (i = 0; i < rounds - 1; i += 3)
            {
                rk[i] = A;
                UInt16[] res = EncryptionRoundFunction(B, A, i);
                A = res[1];
                B = res[0];

                rk[i + 1] = A;
                res = EncryptionRoundFunction(C, A, (UInt16)(i + 1));
                A = res[1];
                C = res[0];

                rk[i + 2] = A;
                res = EncryptionRoundFunction(D, A, (UInt16)(i + 2));
                A = res[1];
                D = res[0];
            }

            rk[rounds - 1] = A;
            return rk;
        }

        /// <summary>
        /// Key Schedule with 32 bit data types
        /// </summary>
        /// <param name="K"></param>
        /// <returns></returns>
        private static UInt32[] KeySchedule(UInt32[] K)
        {
            switch (_blockLength)
            {
                case 48:

                    if (_keyLength == 72)
                    {
                        UInt32 rounds = (UInt32) _rounds;
                        UInt32 i, C = K[2], B = K[1], A = K[0];
                        UInt32[] rk = new UInt32[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt32[] res = EncryptionRoundFunction(B, A, i++);
                            A = res[1];
                            B = res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = res[1];
                            C = res[0];
                        }

                        return rk;
                    }
                    else if (_keyLength == 96)
                    {
                        UInt32 rounds = (UInt32)_rounds;
                        UInt32 i, D = K[3], C = K[2], B = K[1], A = K[0];
                        UInt32[] rk = new UInt32[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt32[] res = EncryptionRoundFunction(B, A, i++);
                            A = (UInt32)res[1];
                            B = (UInt32)res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = (UInt32)res[1];
                            C = (UInt32)res[0];

                            if (i == rounds)
                                break;

                            rk[i] = A;
                            res = EncryptionRoundFunction(D, A, i++);
                            A = (UInt32)res[1];
                            D = (UInt32)res[0];
                        }

                        return rk;
                    }

                    break;

                case 64:

                    if (_keyLength == 96)
                    {
                        UInt32 rounds = (UInt32)_rounds;
                        UInt32 i, C = K[2], B = K[1], A = K[0];
                        UInt32[] rk = new UInt32[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt32[] res = EncryptionRoundFunction(B, A, i++);
                            A = res[1];
                            B = res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = res[1];
                            C = res[0];
                        }

                        return rk;
                    }
                    else if (_keyLength == 128)
                    {
                        UInt32 rounds = (UInt32)_rounds;
                        UInt32 i, D = (UInt32)K[3], C = (UInt32)K[2], B = (UInt32)K[1], A = (UInt32)K[0];
                        UInt32[] rk = new UInt32[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt32[] res = EncryptionRoundFunction(B, A, i++);
                            A = (UInt32)res[1];
                            B = (UInt32)res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = (UInt32)res[1];
                            C = (UInt32)res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(D, A, i++);
                            A = (UInt32)res[1];
                            D = (UInt32)res[0];
                        }

                        return rk;
                    }

                    break;

                default:
                    break;
            }

            throw new ArgumentException("One or more parameter is invalid. Check configuration of the Cipher.");
        }

        /// <summary>
        /// Key Schedule with 64 bit data types
        /// </summary>
        /// <param name="K"></param>
        /// <returns></returns>
        private static UInt64[] KeySchedule(UInt64[] K)
        {
            switch (_blockLength)
            {
                case 96:

                    if (_keyLength == 96)
                    {
                        UInt64 rounds = (UInt64)_rounds;
                        UInt64 i, B = K[1], A = K[0];
                        UInt64[] rk = new UInt64[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt64[] res = EncryptionRoundFunction(B, A, i++);
                            A = (UInt64)res[1];
                            B = (UInt64)res[0];
                        }

                        rk[rounds - 1] = A;
                        return rk;
                    }
                    else if (_keyLength == 144)
                    {
                        UInt64 rounds = (UInt64)_rounds;
                        UInt64 i, C = K[2], B = K[1], A = K[0];
                        UInt64[] rk = new UInt64[rounds];

                        for (i = 0; i < rounds - 2;)
                        {
                            rk[i] = A;
                            UInt64[] res = EncryptionRoundFunction(B, A, i++);
                            A = res[1];
                            B = res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = res[1];
                            C = res[0];
                        }

                        rk[i] = A;
                        return rk;
                    }

                    break;

                case 128:

                    if (_keyLength == 128)
                    {
                        UInt64 rounds = (UInt64)_rounds;
                        UInt64 i, B = K[1], A = K[0];
                        UInt64[] rk = new UInt64[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = A;
                            UInt64[] res = EncryptionRoundFunction(B, A, i++);
                            A = (UInt64)res[1];
                            B = (UInt64)res[0];
                        }

                        rk[rounds - 1] = A;
                        return rk;
                    }
                    else if (_keyLength == 192)
                    {
                        UInt64 rounds = (UInt64)_rounds;
                        UInt64 i, C = K[2], B = K[1], A = K[0];
                        UInt64[] rk = new UInt64[rounds];

                        for (i = 0; i < rounds - 2;)
                        {
                            rk[i] = A;
                            UInt64[] res = EncryptionRoundFunction(B, A, i++);
                            A = res[1];
                            B = res[0];

                            rk[i] = A;
                            res = EncryptionRoundFunction(C, A, i++);
                            A = res[1];
                            C = res[0];
                        }

                        rk[i] = A;
                        return rk;
                    }
                    else if (_keyLength == 256)
                    {
                        UInt64 rounds = (UInt64)_rounds;
                        UInt64 i, D = K[3], C = K[2], B = K[1], A = K[0];
                        UInt64[] rk = new UInt64[rounds];

                        for (i = 0; i < rounds - 1; i += 3)
                        {
                            rk[i] = A;
                            UInt64[] res = EncryptionRoundFunction(B, A, i);
                            A = res[1];
                            B = res[0];

                            rk[i + 1] = A;
                            res = EncryptionRoundFunction(C, A, i + 1);
                            A = res[1];
                            C = res[0];

                            rk[i + 2] = A;
                            res = EncryptionRoundFunction(D, A, i + 2);
                            A = res[1];
                            D = res[0];
                        }

                        rk[rounds - 1] = A;
                        return rk;
                    }

                    break;

                default:
                    break;
            }

            throw new ArgumentException("One or more parameter is invalid. Check configuration of the Cipher.");
        }

        #endregion

        #region Encryption

        /// <summary>
        /// Encryption with 16 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static UInt16[] Speck32Encrypt(UInt16[] pt, UInt16[] rk)
        {
            UInt16 i;
            int rounds = _rounds;
            UInt16[] ct = new UInt16[2];

            ct[0] = pt[0];
            ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {
                UInt16[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        #endregion

        #region Decryption

        /// <summary>
        /// Decryption with 16 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static UInt16[] Speck32Decrypt(UInt16[] ctWords, UInt16[] rk)
        {
            int i = _rounds - 1;

            UInt16[] pt = new UInt16[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0; i--)
            {
                UInt16[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        #endregion

        #region misc

        /// <summary>
        /// Circular left shift of BitArray arr with amount shiftToLeft
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="shiftToLeft"></param>
        /// <returns></returns>
        private static BitArray RotateLeftBitArray(BitArray arr, int shiftToLeft)
        {
            BitArray result = new BitArray(arr.Length, false);
            BitArray tmp = new BitArray(shiftToLeft, false);

            shiftToLeft = shiftToLeft % arr.Length;

            int j = shiftToLeft - 1;
            for (int i = (arr.Length - 1); i >= (arr.Length - shiftToLeft); i--)
            {
                tmp[j] = arr[i];
                j--;
            }

            for (int i = 0; i < arr.Length - shiftToLeft; i++)
            {
                result[i + shiftToLeft] = arr[i];
            }

            for (int i = 0; i < shiftToLeft; i++)
            {
                result[i] = tmp[i];
            }

            return result;
        }

        /// <summary>
        /// Circular right shift of BitArray arr with amount shiftToRight
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="shiftToRight"></param>
        /// <returns></returns>
        private static BitArray RotateRightBitArray(BitArray arr, int shiftToRight)
        {
            return RotateLeftBitArray(arr, arr.Length - shiftToRight);
        }

        #endregion
    }
}

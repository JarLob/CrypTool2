// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

#endregion

namespace voluntLib.common.utils
{
    public static class SerializationHelper
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        #region string

        public static byte[] SerializeString(string s)
        {
            return Encoding.GetBytes(s + Char.MinValue);
        }

        public static string DeserializeString(byte[] bytes, int startIndex)
        {
            int length;
            return DeserializeString(bytes, startIndex, out length);
        }

        public static string DeserializeString(byte[] bytes, int startIndex, out int length)
        {
            var stringBytes = bytes.Skip(startIndex).TakeWhile(b => b != 0).ToArray();
            var s = Encoding.GetString(stringBytes);
            if (ContainsNonePrintables(s))
            {
                throw new InvalidDataException("String contains none printable Chars");
            }
            length = stringBytes.Length + 1;
            return s;
        }

        #endregion

        #region BigInt

        public static byte[] SerializeBigInt(BigInteger value)
        {
            var bytesInt = value.ToByteArray();
            var bytes = new byte[4 + bytesInt.Length];
            BitConverter.GetBytes(bytesInt.Length).CopyTo(bytes, 0);
            bytesInt.CopyTo(bytes, 4);
            return bytes;
        }

        public static BigInteger DeserializeBigInt(byte[] bytes, int startIndex)
        {
            int length;
            return DeserializeBigInt(bytes, startIndex, out length);
        }

        public static BigInteger DeserializeBigInt(byte[] bytes, int startIndex, out int length)
        {
            length = BitConverter.ToInt32(bytes, startIndex) + 4;
            return new BigInteger(bytes.Skip(4 + startIndex).Take(length - 4).ToArray());
        }

        #endregion BigInt

        #region JobID

        public static byte[] SerializeJobID(BigInteger value)
        {
            var resultArray = new byte[16];
            var bytesInt = value.ToByteArray();
            bytesInt.Take(16).ToArray().CopyTo(resultArray, 0);
            return resultArray;
        }

        public static BigInteger DeserializeJobID(byte[] bytes, int startIndex)
        {
            int length;
            return DeserializeJobID(bytes, startIndex, out length);
        }

        public static BigInteger DeserializeJobID(byte[] bytes, int startIndex, out int length)
        {
            length = 16;
            var array = bytes.Skip(startIndex).Take(length).ToArray();
            return new BigInteger(array);
        }

        #endregion JobID

        private static bool ContainsNonePrintables(string s)
        {
            return s.Any(char.IsControl);
        }
    }
}
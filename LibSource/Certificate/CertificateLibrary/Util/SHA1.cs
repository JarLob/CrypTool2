﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CrypTool.Util.Cryptography
{
    public static class SHA1
    {
        static SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();

        public static byte[] ComputeHash(string value)
        {
            return provider.ComputeHash(Encoding.ASCII.GetBytes(value));
        }

        public static string ComputeHashString(string value)
        {
            byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(value));
            StringBuilder hashValue = new StringBuilder();
            foreach (byte b in hash)
            {
                hashValue.Append(b.ToString("x2"));
            }
            return hashValue.ToString();
        }

        public static string ConvertToString(byte[] hash)
        {
            StringBuilder hashValue = new StringBuilder();
            foreach (byte b in hash)
            {
                hashValue.Append(b.ToString("x2"));
            }
            return hashValue.ToString();
        }
    }
}

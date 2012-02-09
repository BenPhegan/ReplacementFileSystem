﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ReplacementFileSystem
{
    public static class EncryptionUtil
    {
        static readonly byte[] entropy = { 4, 17, 253, 94, 16 };

        public static string DecryptString(string stringToDecrypt)
        {
            byte[] result = ProtectedData.Unprotect(Convert.FromBase64String(stringToDecrypt), entropy, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(result);
        }

        public static string EncryptString(string stringToEncrypt)
        {
            byte[] result = ProtectedData.Protect(Encoding.Unicode.GetBytes(stringToEncrypt), entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(result);
        }

        public static byte[] GetHash(Stream stream)
        {
            return new MD5CryptoServiceProvider().ComputeHash(stream);
        }
    }
}
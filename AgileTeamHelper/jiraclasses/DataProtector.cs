using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace JiraTrayApp
{
    internal static class DataProtector
    {
        private static byte[] _aditionalEntropy = { 0, 5, 0, 8, 8, 0 };

        internal static byte[] Protect(string data)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return ProtectedData.Protect(encoding.GetBytes(data), _aditionalEntropy, DataProtectionScope.CurrentUser);
        }

        internal static string UnProtect(byte[] data)
        {
            Byte[] bytes = ProtectedData.Unprotect(data, _aditionalEntropy, DataProtectionScope.CurrentUser);

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetString(bytes);
        }
    }
}

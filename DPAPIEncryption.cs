namespace WebApplication1.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class DPAPIEncryption
    {
        public static string Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedData);
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] data = Convert.FromBase64String(encryptedText);
            byte[] decryptedData = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}

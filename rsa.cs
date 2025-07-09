using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class CngRsaExample
{
    const string KeyName = "MyCngRsaKey";
    const string PrivateKeyPath = "privateKey.p8";
    const string PublicKeyPath = "publicKey.pem";

    static void Main()
    {
        // 1. 创建或重新创建可导出的 CNG 密钥
        CngKey key = EnsureExportableCngKey(KeyName);

        // 2. 绑定密钥到 RSACng
        using (var rsa = new RSACng(key))
        {
            // 3. 导出私钥为 PKCS#8（.p8）
            byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            File.WriteAllBytes(PrivateKeyPath, privateKeyBytes);

            // 4. 导出公钥为 PEM 格式
            string publicPem = ExportPublicKeyPem(rsa);
            File.WriteAllText(PublicKeyPath, publicPem);

            Console.WriteLine("✅ 公钥和私钥已导出到文件。");
        }

        // 5. 使用公钥进行加密
        byte[] encrypted;
        using (RSA rsaPub = RSA.Create())
        {
            rsaPub.ImportFromPem(File.ReadAllText(PublicKeyPath));
            encrypted = rsaPub.Encrypt(Encoding.UTF8.GetBytes("Hello RSA with CNG!"), RSAEncryptionPadding.OaepSHA256);
            Console.WriteLine($"🔐 加密数据（Base64）: {Convert.ToBase64String(encrypted)}");
        }

        // 6. 使用私钥（从文件）进行解密
        byte[] privateKeyFromFile = File.ReadAllBytes(PrivateKeyPath);
        using (RSA rsaPri = RSA.Create())
        {
            rsaPri.ImportPkcs8PrivateKey(privateKeyFromFile, out _);
            byte[] decrypted = rsaPri.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
            Console.WriteLine($"🔓 解密结果: {Encoding.UTF8.GetString(decrypted)}");
        }

        // 7. 删除 CNG 密钥容器
        if (CngKey.Exists(KeyName))
        {
            CngKey.Open(KeyName).Delete();
            Console.WriteLine("🗑️ CNG Key Store 中的密钥已删除。");
        }
    }

    /// <summary>
    /// 确保 CNG 密钥存在且可导出，必要时重新创建。
    /// </summary>
    static CngKey EnsureExportableCngKey(string keyName)
    {
        if (CngKey.Exists(keyName))
        {
            var existing = CngKey.Open(keyName);
            var exportPolicy = existing.ExportPolicy;

            if ((exportPolicy & CngExportPolicies.AllowPlaintextExport) == 0)
            {
                Console.WriteLine("⚠️ 密钥存在但不支持导出，正在删除并重建...");
                existing.Delete();
                return CreateExportableCngKey(keyName);
            }

            return existing;
        }

        return CreateExportableCngKey(keyName);
    }

    /// <summary>
    /// 创建允许导出的 CNG RSA 密钥。
    /// </summary>
    static CngKey CreateExportableCngKey(string keyName)
    {
        var creationParams = new CngKeyCreationParameters
        {
            ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowPlaintextExport,
            KeyUsage = CngKeyUsages.AllUsages,
            Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider
        };

        return CngKey.Create(CngAlgorithm.Rsa, keyName, creationParams);
    }

    /// <summary>
    /// 导出 RSA 公钥为 PEM 格式。
    /// </summary>
    static string ExportPublicKeyPem(RSA rsa)
    {
        byte[] pubBytes = rsa.ExportSubjectPublicKeyInfo();
        string base64 = Convert.ToBase64String(pubBytes);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("-----BEGIN PUBLIC KEY-----");
        for (int i = 0; i < base64.Length; i += 64)
            sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
        sb.AppendLine("-----END PUBLIC KEY-----");
        return sb.ToString();
    }
}

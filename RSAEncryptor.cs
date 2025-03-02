using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WebApplication1.Services
{
    public class RSAEncryptor
    {
        public static string Encrypt(string plainText, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string Decrypt(string encryptedText, string privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }

        public static X509Certificate2 CreateX509(string subjectName)
        {

            // 从当前用户存储加载证书
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);

                // 根据主题名称查找证书
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);

                if (certs.Count > 0)
                {
                    return certs[0];
                }
                else
                {
                    throw new CryptographicException("未找到指定名称的证书！");
                }
            }
        }


        public static string EncryptData(string data)
        {
            X509Certificate2 cert = CreateX509("RSATESTCert");
            using (RSA rsa = cert.GetRSAPublicKey() ?? throw new InvalidOperationException("证书不包含 RSA 公钥！"))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string DecryptData(string encryptedText)
        {
            X509Certificate2 cert = CreateX509("RSATESTCert");
            using (RSA rsa = cert.GetRSAPrivateKey() ?? throw new InvalidOperationException("证书不包含 RSA 私钥！"))
            {
                // 使用 RSA 私钥解密数据
                //byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
    }
}

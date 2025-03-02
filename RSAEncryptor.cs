using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace WinFormsApp1
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
            }

            return null;
        }


        public static string EncryptData(string data)
        {
            X509Certificate2 cert = CreateX509("RSATESTCert");
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                // 将字符串转换为字节数组
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                // 使用 RSA 公钥加密数据
                byte[] encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);

                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string DecryptData(string encryptedText)
                {
            X509Certificate2 cert = CreateX509("RSATESTCert");
            using (RSA rsa = cert.GetRSAPrivateKey())
                    {
                // 使用 RSA 私钥解密数据
                //byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedText), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
                //byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);

                //        // 将字节数组转换为字符串
                //        string decryptedText = Encoding.UTF8.GetString(decryptedData);

                //        return decryptedText;
                    }
                }
            }
}

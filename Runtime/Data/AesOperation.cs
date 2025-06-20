using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataEngine.Data
{
    public class AesOperation
    {
        public const int ALGORITHM_BASE = 64;
        public const string IV = m_iv;
        public const string KEY = m_key;

        private const string m_iv = "aEM5puDebU+PFKrJ2vPkWQ==";
        private const string m_key = "ZzP5rMHiMkWzGzh8fHP9JQ==";

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = Convert.FromBase64String(m_iv);
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static void EncryptString(string key, string plainText, FileStream stream)
        {
            byte[] iv = Convert.FromBase64String(m_iv);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                }
            }
        }

        public static string DecryptString(string key, byte[] buffer)
        {
            byte[] iv = Convert.FromBase64String(m_iv);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
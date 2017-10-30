namespace Common
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class AesCryptography
    {
        #region Settings

        private const int _iterations = 2;
        private const int _keySize = 256;
        private const string _hash = "SHA1";
        private readonly string _password;
        private readonly byte[] _saltBytes;
        private readonly byte[] _vectorBytes;

        #endregion

        public AesCryptography(string password)
        {
            Guard.ArgumentNotNullOrEmpty(password, nameof(password));

            _password = password;
            using (var csp = new MD5CryptoServiceProvider())
            {
                _saltBytes = Utility.ComputeHash(password, csp, Encoding.UTF8);
                _vectorBytes = Utility.ComputeHash(password, csp, Encoding.ASCII);
            }
        }

        public string Encrypt(string stringToEncrypt)
        {
            Guard.ArgumentNotNullOrEmpty(stringToEncrypt, nameof(stringToEncrypt));

            return Encrypt<AesManaged>(stringToEncrypt);
        }

        private string Encrypt<T>(string stringToEncrypt) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = Encoding.ASCII.GetBytes(stringToEncrypt);

            byte[] encrypted;
            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(_password, _saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, _vectorBytes))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string stringToDecrypt)
        {
            Guard.ArgumentNotNullOrEmpty(stringToDecrypt, nameof(stringToDecrypt));

            return Decrypt<AesManaged>(stringToDecrypt);
        }

        private string Decrypt<T>(string stringToDecrypt) where T : SymmetricAlgorithm, new()
        {
            byte[] valueBytes = Convert.FromBase64String(stringToDecrypt);

            byte[] decrypted;
            int decryptedByteCount;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(_password, _saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, _vectorBytes))
                    {
                        using (MemoryStream from = new MemoryStream(valueBytes))
                        {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[valueBytes.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CrossCutting.Utils
{
   
        public static class Criptografia
        {
            private static string PluginKeyV1 => "Luke...ImYourFatherNOOOOOOOOOOOO";
            private static string PluginIvv1 => "LukeImYourFather";

            private static string WebAppKey => "SW..WasTheBestMovieOf2016..SUREE";
            private static string WebAppIv => "Imustfullyagreee";


            public static string AES_Encrypt(string clearText, SourceCripto source)
            {

                switch (source)
                {
                    case SourceCripto.Plugin:
                        return EncryptStringAES(clearText,
                                                 key: Encoding.UTF8.GetBytes(PluginKeyV1),
                                                 iv: Encoding.UTF8.GetBytes(PluginIvv1));

                    case SourceCripto.WebApp:
                        return EncryptStringAES(clearText,
                                                      key: Encoding.UTF8.GetBytes(WebAppKey),
                                                      iv: Encoding.UTF8.GetBytes(WebAppIv));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(source), source, null);
                }
            }

            public static string SenhaUsuario(string senha)
            {
                var password = senha;
                byte[] salt;
                byte[] bytes;
                if (password == null)
                {
                    throw new ArgumentNullException("password");
                }

                using (var rfc2898DeriveByte = new Rfc2898DeriveBytes(password, 16, 1000))
                {
                    salt = rfc2898DeriveByte.Salt;
                    bytes = rfc2898DeriveByte.GetBytes(32);
                }
                var numArray = new byte[49];
                Buffer.BlockCopy(salt, 0, numArray, 1, 16);
                Buffer.BlockCopy(bytes, 0, numArray, 17, 32);
                return Convert.ToBase64String(numArray);
            }


            public static string AES_Decrypt(string encrypted, SourceCripto source)
            {
                if (encrypted == null) return "";
                try
                {
                    var encryptedBytes = Convert.FromBase64String(encrypted);
                    switch (source)
                    {
                        case SourceCripto.Plugin:
                            return DecryptStringFromBytes(encryptedBytes,
                                                          key: Encoding.UTF8.GetBytes(PluginKeyV1),
                                                          iv: Encoding.UTF8.GetBytes(PluginIvv1));

                        case SourceCripto.WebApp:
                            return DecryptStringFromBytes(encryptedBytes,
                                                          key: Encoding.UTF8.GetBytes(WebAppKey),
                                                          iv: Encoding.UTF8.GetBytes(WebAppIv));
                        default:
                            throw new ArgumentOutOfRangeException(nameof(source), source, null);
                    }
                }
                catch (Exception)
                {
                    return encrypted;
                }
            }


            private static string EncryptStringAES(string plainText, byte[] key, byte[] iv)
            {
                if (string.IsNullOrEmpty(plainText))
                    return "";

                string outStr = null;
                Aes aesAlg = null;
                aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = iv;
                // Create a decrytor to perform the stream transform.             
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for encryption.             
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.                         
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
                // Return the encrypted bytes from the memory stream.         
                return outStr;
            }


            private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
            {
                // Check arguments.  
                if (cipherText == null || cipherText.Length <= 0)
                {
                    throw new ArgumentNullException("cipherText");
                }
                if (key == null || key.Length <= 0)
                {
                    throw new ArgumentNullException("key");
                }
                if (iv == null || iv.Length <= 0)
                {
                    throw new ArgumentNullException("key");
                }

                // Declare the string used to hold  
                // the decrypted text.  
                string plaintext = null;

                // Create an RijndaelManaged object  
                // with the specified key and IV.  
                using (var rijAlg = Aes.Create())
                {
                    //Settings  
                    rijAlg.Mode = CipherMode.CBC;
                    rijAlg.Padding = PaddingMode.PKCS7;
                    rijAlg.Key = key;
                    rijAlg.IV = iv;
                    // Create a decrytor to perform the stream transform.  
                    var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                    try
                    {
                        // Create the streams used for decryption.  
                        using (var msDecrypt = new MemoryStream(cipherText))
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (var srDecrypt = new StreamReader(csDecrypt))
                                {
                                    // Read the decrypted bytes from the decrypting stream  
                                    // and place them in a string.  
                                    plaintext = srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                return plaintext;
            }

            public static string GetPluginKey()
            {
                return PluginKeyV1;
            }

            public static string EncryptMD5(string x)
            {
                System.Security.Cryptography.MD5 test123 = MD5.Create();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(x);
                data = test123.ComputeHash(data);
                var md5Hash = System.Text.Encoding.ASCII.GetString(data);

                return md5Hash;
            }

            public static string GetPluginIV()
            {
                return PluginIvv1;
            }

            public static bool ValidarHeadersAuth(string AuthToken, string AuthValue)
            {
                try
                {
                    AuthToken = AES_Decrypt(AuthToken, SourceCripto.Plugin);
                    AuthValue = AES_Decrypt(AuthValue, SourceCripto.Plugin);
                    return (AuthToken == (AuthValue + GetPluginIV()));
                }
                catch (Exception)
                {
                    return false;
                }

            }
        }
 
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    // From StackOverflow: http://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string/10366194#10366194
    // https://gist.github.com/jbtule/4336842#file-aesthenhmac-cs
    public class SimpleEncrypter {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();
        public static readonly int BlockBitSize = 128;
        public static readonly int KeyBitSize = 256;

        public static byte[] NewKey() {
            var key = new byte[KeyBitSize / 8];
            Random.GetBytes(key);
            return key;
        }

        public static Tuple<string, byte[]> Encrypt(string plainText, byte[] cryptKey, byte[] existingIv = null) {
            byte[] encrypted;
            byte[] iv;
            using (var aes = Aes.Create()) {
                aes.GenerateIV();
                if (existingIv != null) {
                    iv = existingIv;
                } else {
                    iv = aes.IV;
                }
                var encryptor = aes.CreateEncryptor(cryptKey, iv);
                using (var memStream = new MemoryStream()) {
                    using (var cryptStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write)) {
                        using (var streamWriter = new StreamWriter(cryptStream)) {
                            streamWriter.Write(plainText);
                        }
                        encrypted = memStream.ToArray();
                    }
                }
            }
            return Tuple.Create(Convert.ToBase64String(encrypted), iv);
        }

        public static string EncryptToString(string plainText, byte[] cryptKey) {
            var encryptedTuple = Encrypt(plainText, cryptKey);
            var ivAsString = Convert.ToBase64String(encryptedTuple.Item2);
            return $"{encryptedTuple.Item1}.stm.{ivAsString}";
        }

        public static string DecryptFromString(string encryptedMessage, byte[] cryptKey) {
            string[] parts = encryptedMessage.Split(new string[] { ".stm." }, StringSplitOptions.None);
            return Decrypt(parts[0], cryptKey, Convert.FromBase64String(parts[1]));
        }

        public static string Decrypt(string encryptedMessage, byte[] cryptKey, byte[] aesIV) {
            string plaintext = null;
            var cipherText = Convert.FromBase64String(encryptedMessage);
            using (var aes = Aes.Create()) {
                using (var decryptor = aes.CreateDecryptor(cryptKey, aesIV)) {
                    using (var memStream = new MemoryStream(cipherText)) {
                        using (var decrypt = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader decrypted = new StreamReader(decrypt)) {
                                plaintext = decrypted.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    // From StackOverflow: http://stackoverflow.com/questions/202011/encrypt-and-decrypt-a-string/10366194#10366194
    // https://gist.github.com/jbtule/4336842#file-aesthenhmac-cs
    public class SimpleEncrypter {
        public static readonly int BlockBitSize = 128;
        public static readonly int KeyBitSize = 256;

        public static Tuple<string, byte[]> SimpleEncrypt(string secretMessage, byte[] cryptKey) {
            var plainText = Encoding.UTF8.GetBytes(secretMessage);
            var cipherText = SimpleEncrypt(plainText, cryptKey);
            return Tuple.Create(Convert.ToBase64String(cipherText.Item1), cipherText.Item2);
        }

        private static Tuple<byte[], byte[]> SimpleEncrypt(byte[] plainText, byte[] cryptKey) {
            using (var aes = Aes.Create()) {
                aes.GenerateIV();
                using (var encryptor = aes.CreateEncryptor(cryptKey, aes.IV)) {
                    return Tuple.Create(encryptor.TransformFinalBlock(plainText, 0, 0), aes.IV);
                }
            }
        }

        public static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] aesIV) {
            var cipherText = Convert.FromBase64String(encryptedMessage);
            var plainText = SimpleDecrypt(cipherText, cryptKey, aesIV);
            return plainText == null ? null : Encoding.UTF8.GetString(plainText);
        }

        private static byte[] SimpleDecrypt(byte[] cipherText, byte[] cryptKey, byte[] aesIV) {
            using (var aes = Aes.Create()) {
                using (var decrypter = aes.CreateDecryptor(cryptKey, aesIV)) {
                    return decrypter.TransformFinalBlock(cipherText, 0, 0);
                }
            }
        }
    }
}

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    /// <summary>
    /// Used to encrypt things like passwords and session variables.
    /// Currently stores the encrypted message in memory. I may want to avoid that.
    /// </summary>
    public class SimpleEncryptor {
        private readonly AppSecrets _secrets;

        #region Properties
        private byte[] _secretKey;
        private byte[] _iv;
        private byte[] _encryptedMessage;

        private void SetSecretKey(byte[] key) {
            _secretKey = key;
        }

        private void SetSecretKey(string key) {
            _secretKey = Convert.FromBase64String(key);
        }

        public void SetIV(byte[] iv) {
            _iv = iv;
        }

        public void SetIV(string iv) {
            _iv = Convert.FromBase64String(iv);
        }

        public byte[] GetIVBA() {
            return _iv;
        }

        public string GetIVS() {
            return Convert.ToBase64String(_iv);
        }

        private void SetEncryptedMessage(byte[] message) {
            _encryptedMessage = message;
        }

        private void SetEncryptedMessage(string message) {
            _encryptedMessage = Convert.FromBase64String(message);
        }

        public byte[] GetEncryptedMessageBA() {
            return _encryptedMessage;
        }

        public string GetEncryptedMessageS() {
            return Convert.ToBase64String(_encryptedMessage);
        }
        #endregion Properites

        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        public readonly int BlockBitSize = 128;
        public readonly int KeyBitSize = 256;

        /// <summary>
        /// Inject AppSecrets configuration which holds the SecretKey property, used as AES encryption key
        /// </summary>
        /// <param name="secrets">Configuration option loaded from Startup using DI</param>
        public SimpleEncryptor(IOptions<AppSecrets> secrets) {
            _secrets = secrets.Value;
            SetSecretKey(_secrets.SecretKey);
        }

        /// <summary>
        /// This helper can be used to generate a key of the appropriate size, which should then be saved in a secret config file.
        /// </summary>
        /// <returns>New Random key</returns>
        private byte[] NewKey() {
            var key = new byte[KeyBitSize / 8];
            _random.GetBytes(key);
            return key;
        }

        /// <summary>
        /// Takes a plain text message and encrypts it using AES encryption with the SecretKey.
        /// An IV may be provided, for instances where the value will be hashed and it is necessary to generate the same value.
        /// </summary>
        /// <param name="plainText">Plain text string.</param>
        /// <param name="existingIv">IV if needed to match encrypted value to stored value.</param>
        public void Encrypt(string plainText, byte[] existingIv = null) {
            using (var aes = Aes.Create()) {
                if (existingIv != null) {
                    SetIV(existingIv);
                } else {
                    aes.GenerateIV();
                    SetIV(aes.IV);
                }

                var encryptor = aes.CreateEncryptor(_secretKey, _iv);
                using (var memoryStream = new MemoryStream()) {
                    using (var cryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                        using (var streamWriter = new StreamWriter(cryptStream)) {
                            streamWriter.Write(plainText);
                        }
                        SetEncryptedMessage(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Generates a string which can be stored in the database or a cookie and decrypted later.
        /// </summary>
        /// <param name="plainText">Plain text string.</param>
        /// <param name="existingIv">IV if needed to match encrypted value to stored value.</param>
        /// <returns>String in the format [encryptedMessage].stm.[IV]</returns>
        public string EncryptToString(string plainText, byte[] existingIv = null) {
            Encrypt(plainText, existingIv);
            return StmJoiner(new string[] { GetEncryptedMessageS(), GetIVS() });
        }

        /// <summary>
        /// Expects _secretKey and _iv to be set already using the appropriate setters.
        /// </summary>
        /// <returns>String containing the decrypted message</returns>
        public string Decrypt() {
            string plainText = null;
            using (var aes = Aes.Create()) {
                using (var decryptor = aes.CreateDecryptor(_secretKey, _iv)) {
                    using (var memoryStream = new MemoryStream(GetEncryptedMessageBA())) {
                        using (var cryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                            using (StreamReader decrypted = new StreamReader(cryptStream)) {
                                plainText = decrypted.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return plainText;
        }

        /// <summary>
        /// Takes a string that may have been stored in the database and concatenated using the 'EncryptToSTring' method.
        /// </summary>
        /// <param name="messageWithIv">String in the format [encryptedMessage].stm.[IV]</param>
        /// <returns>String containing the decrypted message</returns>
        public string DecryptFromString(string messageWithIv) {
            string[] parts = StmSplitter(messageWithIv);
            // TODO: Add checks on parts
            SetEncryptedMessage(parts[0]);
            SetIV(parts[1]);
            return Decrypt();
        }

        /// <summary>
        /// Helper used here and by the hash method.
        /// </summary>
        /// <param name="concatenatedString">A string in the format [value].stm.[value]</param>
        /// <returns>A string array of the pieces that were concatenated.</returns>
        public string[] StmSplitter(string concatenatedString) {
            return concatenatedString.Split(new string[] { ".stm." }, StringSplitOptions.None);
        }

        /// <summary>
        /// Helper used here and by the hash method.
        /// </summary>
        /// <param name="strings">An array of strings to concatenate</param>
        /// <returns>A string in the format [value].stm.[value]</returns>
        public string StmJoiner(string[] strings) {
            return string.Join(".stm.", strings);
        }
    }
}

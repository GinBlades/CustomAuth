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
    public class SimpleEncryptor {
        private readonly AppSecrets _secrets;

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

        public void SetEncryptedMessage(byte[] message) {
            _encryptedMessage = message;
        }

        public void SetEncryptedMessage(string message) {
            _encryptedMessage = Convert.FromBase64String(message);
        }

        public byte[] GetEncryptedMessageBA() {
            return _encryptedMessage;
        }

        public string GetEncryptedMessageS() {
            return Convert.ToBase64String(_encryptedMessage);
        }

        private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
        public readonly int BlockBitSize = 128;
        public readonly int KeyBitSize = 256;

        public SimpleEncryptor(IOptions<AppSecrets> secrets) {
            _secrets = secrets.Value;
            SetSecretKey(_secrets.SecretKey);
        }

        public byte[] NewKey() {
            var key = new byte[KeyBitSize / 8];
            _random.GetBytes(key);
            return key;
        }

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

        public string EncryptToString(string plainText, byte[] existingIv = null) {
            Encrypt(plainText, existingIv);
            return StmJoiner(new string[] { GetEncryptedMessageS(), GetIVS() });
        }

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

        public string DecryptFromString(string messageWithIv) {
            string[] parts = StmSplitter(messageWithIv);
            // TODO: Add checks on parts
            SetEncryptedMessage(parts[0]);
            SetIV(parts[1]);
            return Decrypt();
        }

        public string[] StmSplitter(string concatenatedString) {
            return concatenatedString.Split(new string[] { ".stm." }, StringSplitOptions.None);
        }

        public string StmJoiner(string[] strings) {
            return string.Join(".stm.", strings);
        }
    }
}

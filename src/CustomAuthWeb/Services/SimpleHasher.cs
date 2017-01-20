using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    public class SimpleHasher {
        private readonly SimpleEncryptor _encryptor;

        private byte[] _salt;

        public void SetSalt() {
            _salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(_salt);
            }
        }

        public void SetSalt(byte[] salt) {
            _salt = salt;
        }

        public void SetSalt(string salt) {
            _salt = Convert.FromBase64String(salt);
        }

        public byte[] GetSaltBA() {
            return _salt;
        }

        public string GetSaltS() {
            return Convert.ToBase64String(_salt);
        }

        public SimpleHasher(SimpleEncryptor encryptor) {
            _encryptor = encryptor;
        }

        public byte[] Hash(string password, byte[] salt) {
            SetSalt(salt);
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: _salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            );
        }

        public byte[] Hash(string password) {
            SetSalt();
            return Hash(password, _salt);
        }

        public byte[] Hash(string password, string salt) {
            SetSalt(salt);
            return Hash(password, _salt);
        }

        public string HashWithEncryption(string password) {
            _encryptor.Encrypt(password);
            var hash = Hash(_encryptor.GetEncryptedMessageS());
            return _encryptor.StmJoiner(new string[] {
                Convert.ToBase64String(hash),
                _encryptor.GetIVS(),
                GetSaltS()
            });
        }

        public bool CompareWithEncryption(string guess, string password) {
            // TODO: Validate parts
            string[] parts = _encryptor.StmSplitter(password);
            _encryptor.SetIV(parts[1]);
            _encryptor.Encrypt(guess, _encryptor.GetIVBA());
            var hashedGuess = Hash(_encryptor.GetEncryptedMessageS(), parts[2]);
            return Compare(parts[0], Convert.ToBase64String(hashedGuess));
        }

        // TODO: Prevent returning early to not give unnecessary info
        public bool Compare(string stringA, string stringB) {
            return ConstantTimeComparison(Convert.FromBase64String(stringA), Convert.FromBase64String(stringB);
        }

        private static bool ConstantTimeComparison(byte[] passwordGuess, byte[] actualPassword) {
            uint difference = (uint)passwordGuess.Length ^ (uint)actualPassword.Length;
            for (var i = 0; i < passwordGuess.Length && i < actualPassword.Length; i++) {
                difference |= (uint)(passwordGuess[i] ^ actualPassword[i]);
            }

            return difference == 0;
        }
    }
}

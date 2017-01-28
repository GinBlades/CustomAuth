using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace CustomAuthWeb.Services {
    /// <summary>
    /// The primary purpose of this class is to hash and compare hashed passwords with the 'Hash', 'HashToString',
    /// and 'Compare' methods. The other methods may be useful in other areas, but for now they are marked
    /// as private so that its clear they are not currently used.
    /// </summary>
    public class SimpleHasher {

        #region SaltProperties
        private byte[] _salt;

        /// <summary>
        /// Sets a random byte array to the appropriate length for a salt and uses a random number generator to fill the bytes
        /// with random values.
        /// </summary>
        private void SetSalt() {
            _salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(_salt);
            }
        }

        private void SetSalt(byte[] salt) {
            if (salt == null) {
                SetSalt();
            } else {
                _salt = salt;
            }
        }

        private void SetSalt(string salt) {
            if (salt == null) {
                SetSalt();
            } else {
                _salt = Convert.FromBase64String(salt);
            }
        }

        private byte[] GetSaltBA() {
            return _salt;
        }

        private string GetSaltS() {
            return Convert.ToBase64String(_salt);
        }
        #endregion SaltProperties
        
        #region HashOverloads
        /// <summary>
        /// From https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
        /// Allows a salt to be passed in for matching existing hashes.
        /// </summary>
        /// <param name="password">String to be used as a password or some other secret</param>
        /// <param name="salt">Byte array of length (128 / 8)</param>
        /// <returns>Byte[] representing the hashed password.</returns>
        public byte[] Hash(string password, byte[] salt) {
            SetSalt(salt);
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: _salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
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
        #endregion HashOverloads

        public string HashToString(string password, string salt = null) {
            byte[] hash;
            if (salt != null) {
                hash = Hash(password, salt);
            } else {
                hash = Hash(password);
            }

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Based on byte[] version found on web.
        /// </summary>
        /// <param name="stringA"></param>
        /// <param name="stringB"></param>
        /// <returns></returns>
        public bool Compare(string stringA, string stringB) {
            uint difference = (uint)Math.Abs(stringA.Length - stringB.Length);
            for (var i = 0; i < stringA.Length && i < stringB.Length; i++) {
                difference += (uint)Math.Abs(stringA[i].CompareTo(stringB[i]));
            }
            return difference == 0;
        }

        /// <summary>
        /// From https://lockmedown.com/hash-right-implementing-pbkdf2-net/
        /// This prevents an attacker from inferring when they got a set of characters correct.
        /// Uses bit operators to compare each item in the list
        /// </summary>
        /// <param name="passwordGuess"></param>
        /// <param name="actualPassword"></param>
        /// <returns></returns>
        private bool Compare(byte[] passwordGuess, byte[] actualPassword) {
            uint difference = (uint)passwordGuess.Length ^ (uint)actualPassword.Length;
            for (var i = 0; i < passwordGuess.Length && i < actualPassword.Length; i++) {
                difference |= (uint)(passwordGuess[i] ^ actualPassword[i]);
            }

            return difference == 0;
        }
    }
}

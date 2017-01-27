using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace CustomAuthWeb.Services {
    /// <summary>
    /// The primary purpose of this class is to hash and compare hashed passwords with the 'HashWithEncryption'
    /// and 'CompareWithEncryption' methods. The other methods may be useful in other areas, but for now they are marked
    /// as private so that its clear they are not currently used.
    /// </summary>
    public class SimpleHasher {
        private readonly SimpleEncryptor _encryptor;

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

        /// <summary>
        /// Inject SimpleEncryptor for use with passwords that must be encrypted before being hashed.
        /// </summary>
        public SimpleHasher(SimpleEncryptor encryptor) {
            _encryptor = encryptor;
        }

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
        #endregion HashOverloads

        /// <summary>
        /// First encrypts the password using the secret key, then hashes it.
        /// The string returned can be split to get all the required parameters for matching against the existing hash.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>A string in the format [hash].stm.[encryptionIV].stm.[salt]</returns>
        public string HashWithEncryption(string password) {
            _encryptor.Encrypt(password);
            var hash = Hash(_encryptor.GetEncryptedMessageS());
            return _encryptor.StmJoiner(new string[] {
                Convert.ToBase64String(hash),
                _encryptor.GetIVS(),
                GetSaltS()
            });
        }

        /// <summary>
        /// Takes in a new guess string and matches it against an existing password. The password is expected to be in the
        /// format provided by 'HashWithEncryption'
        /// </summary>
        /// <param name="guess">Any string</param>
        /// <param name="password">String in the format [hash].stm.[encryptionIV].stm.[salt]</param>
        /// <returns>Returns true if guess matches existing password, otherwise false.</returns>
        public bool CompareWithEncryption(string guess, string password) {
            // TODO: Validate parts
            string[] parts = _encryptor.StmSplitter(password);
            _encryptor.SetIV(parts[1]);
            _encryptor.Encrypt(guess, _encryptor.GetIVBA());
            var hashedGuess = Hash(_encryptor.GetEncryptedMessageS(), parts[2]);
            return Compare(parts[0], Convert.ToBase64String(hashedGuess));
        }

        /// <summary>
        /// Based on byte[] version found on web.
        /// </summary>
        /// <param name="stringA"></param>
        /// <param name="stringB"></param>
        /// <returns></returns>
        private bool Compare(string stringA, string stringB) {
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

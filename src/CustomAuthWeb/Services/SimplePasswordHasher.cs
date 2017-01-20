using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    public class SimplePasswordHasher {
        public static string Hash(string password, byte[] salt) {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));
        }
        public static string HashWithEncryption(string password, byte[] salt, byte[] key) {
            var encryptedPassword = SimpleEncrypter.Encrypt(password, key);
            var ivAsString = Convert.ToBase64String(encryptedPassword.Item2);
            var hash = Hash(encryptedPassword.Item1, salt);
            // password, iv, salt
            return $"{hash}.stm.{ivAsString}.stm.{Convert.ToBase64String(salt)}";
        }

        public static bool CompareWithEncryption(string guess, string password, byte[] key) {
            // password, iv, salt
            string[] parts = password.Split(new string[] { ".stm." }, StringSplitOptions.None);
            var encryptedGuess = SimpleEncrypter.Encrypt(guess, key, Convert.FromBase64String(parts[1]));
            var hashedGuess = Hash(encryptedGuess.Item1, Convert.FromBase64String(parts[2]));
            int result = string.Compare(parts[0], hashedGuess);
            return result == 0;
        }

        public static bool Compare(string stringA, string stringB) {
            int result = string.Compare(stringA, stringB);
            return result == 0;
        }
    }
}

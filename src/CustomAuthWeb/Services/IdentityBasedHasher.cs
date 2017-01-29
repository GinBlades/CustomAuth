using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CustomAuthWeb.Services {
    /// <summary>
    /// Shamelessly appropriated from ASP.NET Core Identity Framework
    /// https://github.com/aspnet/Identity/blob/a8ba99bc5b11c5c48fc31b9b0532c0d6791efdc8/src/Microsoft.AspNetCore.Identity/PasswordHasher.cs
    /// I don't understand enough about byte arrays to accurately describe the implementation, but it creates the storable password from a
    /// byte array with the PRF type, salt, and iteration count at particular positions in the array.
    /// </summary>
    public static class IdentityBasedHasher {
        private static readonly int _saltSize = 128 / 8;
        private static readonly int _iterCount = 10000;
        private static readonly int _keyBytes = 256 / 8;
        private static readonly KeyDerivationPrf _prf = KeyDerivationPrf.HMACSHA256;

        public static byte[] HashPassword(string password) {
            var salt = new byte[_saltSize];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(salt);
            }

            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, _prf, _iterCount, _keyBytes);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)_prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)_iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)_saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + _saltSize, subkey.Length);
            return outputBytes;
        }

        public static bool VerifyHashedPassword(string hashedPassword, string guess) {
            return VerifyHashedPassword(hashedPassword.FromHashString(), guess);
        }

        public static bool VerifyHashedPassword(byte[] hashedPassword, string guess) {
            KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            int iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

            int subkeyLength = hashedPassword.Length - 13 - salt.Length;
            byte[] expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            // Hash incoming password and verify it
            byte[] actualSubkey = KeyDerivation.Pbkdf2(guess, salt, prf, iterCount, subkeyLength);
            bool verified = ByteArraysEqual(actualSubkey, expectedSubkey);

            return verified;
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b) {
            if (a == null && b == null) {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length) {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++) {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value) {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset) {
            return ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | ((uint)(buffer[offset + 3]));
        }

        public static string ToHashString(this byte[] ba) {
            return Convert.ToBase64String(ba);
        }

        public static byte[] FromHashString(this string str) {
            return Convert.FromBase64String(str);
        }
    }
}

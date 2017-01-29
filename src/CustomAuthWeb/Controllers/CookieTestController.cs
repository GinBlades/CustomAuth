using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CustomAuthWeb.Services;
using Microsoft.Extensions.Options;
using CustomAuthWeb.Filters;
using CustomAuthWeb.Models;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class CookieTestController : Controller {
        private readonly AppSecrets _secrets;
        private readonly IDataProtector _protector;
        public CookieTestController(IOptions<AppSecrets> secrets, IDataProtectionProvider dpProvider) {
            _secrets = secrets.Value;
            _protector = dpProvider.CreateProtector("CustomAuthWeb.Manual");
        }
        public IActionResult Index() {
            string protectedPayload = _protector.Protect("Derp");
            string unprotectedPayload = _protector.Unprotect(protectedPayload);

            string hashed = IdentityBasedHasher.HashPassword("secret").ToHashString();
            bool valid = IdentityBasedHasher.VerifyHashedPassword(hashed, "secret");

            var result = valid == true ? "True" : "False";

            CookieOptions options = new CookieOptions() {
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Append("TestCookie", "This is a test", options);
            var hashTests = new string[] {
                IdentityBasedHasher.HashPassword("This is the full length of a possible password.").ToHashString(),
                IdentityBasedHasher.HashPassword("small").ToHashString(),
                IdentityBasedHasher.HashPassword("With!Speci#@@$%&!12345").ToHashString()
            };
            return Content(string.Join("<br/>", hashTests));
        }

        public IActionResult TestIdentityHash() {
            var hashedSecret = IdentityBasedHasher.HashPassword("secret").ToHashString();
            var verifySecret = IdentityBasedHasher.VerifyHashedPassword(hashedSecret.FromHashString(), "secret");
            var verifyPassed = verifySecret ? "True" : "False";

            return Content($"{hashedSecret} is verified: {verifyPassed}");
        }

        private bool ByteArraysEqual(byte[] a, byte[] b) {
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

        public IActionResult ReadCookie() {
            return Content(Request.Cookies["TestCookie"]);
        }

        [AuthorizationFilter(UserRole.MemberAccess)]
        public IActionResult MemberTest() {
            return Content("You are a member!");
        }

        [AuthorizationFilter]
        public IActionResult GuestTest() {
            return Content("You are at least a guest!");
        }

        [AuthorizationFilter(UserRole.AdministratorAccess)]
        public IActionResult AdminTest() {
            return Content("You are an admin!");
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CustomAuthWeb.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class CookieTestController : Controller {
        private readonly AppSecrets _secrets;
        public CookieTestController(IOptions<AppSecrets> secrets) {
            _secrets = secrets.Value;
        }
        public IActionResult Index() {
            var encrypted = SimpleEncrypter.EncryptToString("This is a test with encryption", Convert.FromBase64String(_secrets.SecretKey));
            var decrypted = SimpleEncrypter.DecryptFromString(encrypted, Convert.FromBase64String(_secrets.SecretKey));

            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(salt);
            }

            string hashed = SimplePasswordHasher.HashWithEncryption("secret", salt, Convert.FromBase64String(_secrets.SecretKey));
            bool valid = SimplePasswordHasher.CompareWithEncryption(guess: "secret", password: hashed, key: Convert.FromBase64String(_secrets.SecretKey));

            var result = valid == true ? "True" : "False";

            CookieOptions options = new CookieOptions() {
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Append("TestCookie", "This is a test", options);
            return Content(result);
        }

        public IActionResult ReadCookie() {
            return Content(Request.Cookies["TestCookie"]);
        }
    }
}

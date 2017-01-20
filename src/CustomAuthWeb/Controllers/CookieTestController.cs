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
        private readonly SimpleEncryptor _encryptor;
        private readonly SimpleHasher _hasher;
        public CookieTestController(IOptions<AppSecrets> secrets, SimpleEncryptor encryptor, SimpleHasher hasher) {
            _secrets = secrets.Value;
            _encryptor = encryptor;
            _hasher = hasher;
        }
        public IActionResult Index() {
            string encrypted = _encryptor.EncryptToString("This is a test using DI");
            string decrypted = _encryptor.DecryptFromString(encrypted);

            string hashed = _hasher.HashWithEncryption("secret");
            bool valid = _hasher.CompareWithEncryption("secret", hashed);

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

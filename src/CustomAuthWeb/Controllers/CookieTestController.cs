﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CustomAuthWeb.Services;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class CookieTestController : Controller {
        private readonly AppSecrets _secrets;
        public CookieTestController(IOptions<AppSecrets> secrets) {
            _secrets = secrets.Value;
        }
        public IActionResult Index() {
            var key = _secrets.SecretKey;

            var encrypted = SimpleEncrypter.SimpleEncrypt("This is a test with encryption", Convert.FromBase64String("derp"));
            var decrypted = SimpleEncrypter.SimpleDecrypt(encrypted.Item1, Convert.FromBase64String("derp"), encrypted.Item2);

            CookieOptions options = new CookieOptions() {
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Append("TestCookie", "This is a test", options);
            return Content(decrypted);
        }

        public IActionResult ReadCookie() {
            return Content(Request.Cookies["TestCookie"]);
        }
    }
}

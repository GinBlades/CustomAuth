using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CustomAuthWeb.Services;
using Microsoft.Extensions.Options;
using CustomAuthWeb.Filters;
using CustomAuthWeb.Models;
using Microsoft.AspNetCore.DataProtection;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class CookieTestController : Controller {
        private readonly AppSecrets _secrets;
        private readonly SimpleHasher _hasher;
        private readonly IDataProtector _protector;
        public CookieTestController(IOptions<AppSecrets> secrets, IDataProtectionProvider dpProvider, SimpleHasher hasher) {
            _secrets = secrets.Value;
            _hasher = hasher;
            _protector = dpProvider.CreateProtector("CustomAuthWeb.Manual");
        }
        public IActionResult Index() {
            string protectedPayload = _protector.Protect("Derp");
            string unprotectedPayload = _protector.Unprotect(protectedPayload);

            string hashed = _hasher.HashToString("secret");
            bool valid = _hasher.Compare("secret", hashed);

            var result = valid == true ? "True" : "False";

            CookieOptions options = new CookieOptions() {
                Expires = DateTime.Now.AddDays(1)
            };
            Response.Cookies.Append("TestCookie", "This is a test", options);
            var hashTests = new string[] {
                _hasher.HashToString("This is the full length of a possible password."),
                _hasher.HashToString("small"),
                _hasher.HashToString("With!Speci#@@$%&!12345")
            };
            return Content(string.Join("<br/>", hashTests));
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
    }
}

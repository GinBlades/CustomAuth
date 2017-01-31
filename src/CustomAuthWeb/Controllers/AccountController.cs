using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CustomAuthWeb.FormObjects.Account;
using CustomAuthWeb.Data;
using Microsoft.EntityFrameworkCore;
using CustomAuthWeb.Models;
using CustomAuthWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class AccountController : Controller {
        private const string SESSION_NAME = "CustomAuth";
        private readonly ApplicationDbContext _db;
        private readonly IDataProtector _protector;

        public AccountController(ApplicationDbContext db, IDataProtectionProvider dpProvider) {
            _db = db;
            _protector = dpProvider.CreateProtector("CustomAuthWeb.Manual");
        }
        public IActionResult Login(string returnUrl = null) {
            return View(new LoginFormObject() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginFormObject lfo) {
            if (ModelState.IsValid) {
                User user = await GetUserFromForm(lfo);
                if (user != null) {

                    // Built ClaimsPrincipal based on
                    // http://stackoverflow.com/questions/20254796/why-is-my-claimsidentity-isauthenticated-always-false-for-web-api-authorize-fil
                    var claims = new List<Claim>() {
                        new Claim(ClaimTypes.Name, "AuthUser"),
                        new Claim(ClaimTypes.Role, user.Roles.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.UserName)
                    };
                    var identity = new ClaimsIdentity(claims, "Password");
                    var principal = new ClaimsPrincipal(new[] { identity });

                    await HttpContext.Authentication.SignInAsync("CustomAuthMiddleware", principal);
                    // Save user session
                    return RedirectToLocal(lfo.ReturnUrl);
                } else {
                    TempData["Alert"] = "Invalid username/password";
                }
            }
            return View(lfo);
        }

        public IActionResult Register(string returnUrl = null) {
            return View(new RegisterFormObject() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterFormObject rfo) {
            if (ModelState.IsValid) {
                var user = await CreateUserAsync(rfo);
                SaveToSession(user.Id, false);
                // Save new user and add to session
                return RedirectToLocal(rfo.ReturnUrl);
            }
            return View(rfo);
        }

        private async Task<User> CreateUserAsync(RegisterFormObject rfo) {
            rfo.Password = IdentityBasedHasher.HashPassword(rfo.Password).ToHashString();
            var user = rfo.ToUser();
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        [HttpDelete]
        public IActionResult LogOut() {
            // Delete user session
            Response.Cookies.Delete(SESSION_NAME);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            } else {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private async Task<User> GetUserFromForm(LoginFormObject lfo) {
            User user;
            if (lfo.Email != null) {
                user = await _db.Users.SingleOrDefaultAsync(u => u.Email == lfo.Email);
            } else if (lfo.UserName != null) {
                user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == lfo.UserName);
            } else {
                ModelState.AddModelError("Email", "Either the email or username must be entered");
                return null;
            }

            if (user == null) {
                ModelState.AddModelError("Email", "User not found.");
                return null;
            }
            // TODO: Rebuild how this compared after hashing password with correct salt.
            var valid = IdentityBasedHasher.VerifyHashedPassword(user.Password, lfo.Password);

            if (!valid) {
                ModelState.AddModelError("Email", "Password is incorrect.");
                return null;
            }

            return user;
        }

        private void SaveToSession(int userId, bool rememberMe) {
            string encryptedId = _protector.Protect(userId.ToString());
            CookieOptions options = new CookieOptions();
            if (rememberMe) {
                options.Expires = DateTime.Now.AddMonths(1);
            }
            Response.Cookies.Append(SESSION_NAME, encryptedId, options);
        }
    }
}

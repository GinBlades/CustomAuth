using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CustomAuthWeb.FormObjects.Account;
using CustomAuthWeb.Data;
using Microsoft.EntityFrameworkCore;
using CustomAuthWeb.Models;
using CustomAuthWeb.Services;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomAuthWeb.Controllers {
    public class AccountController : Controller {
        private const string SESSION_NAME = "CustomAuth";
        private readonly ApplicationDbContext _db;
        private readonly SimpleHasher _hasher;
        private readonly SimpleEncryptor _encryptor;

        public AccountController(ApplicationDbContext db, SimpleHasher hasher, SimpleEncryptor encryptor) {
            _db = db;
            _hasher = hasher;
            _encryptor = encryptor;
        }
        public IActionResult Login(string returnUrl = null) {
            return View(new LoginFormObject() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginFormObject lfo) {
            if (ModelState.IsValid) {
                User user = await GetUserFromForm(lfo);
                SaveToSession(user.Id, lfo.RememberMe);
                // Save user session
                return RedirectToLocal(lfo.ReturnUrl);
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
            rfo.Password = _hasher.HashWithEncryption(rfo.Password);
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

            var valid = _hasher.CompareWithEncryption(lfo.Password, user.Password);

            if (!valid) {
                ModelState.AddModelError("Email", "Password is incorrect.");
                return null;
            }

            return user;
        }

        private void SaveToSession(int userId, bool rememberMe) {
            string encryptedId = _encryptor.EncryptToString(userId.ToString());
            CookieOptions options = new CookieOptions();
            if (rememberMe) {
                options.Expires = DateTime.Now.AddMonths(1);
            }
            Response.Cookies.Append(SESSION_NAME, encryptedId, options);
        }
    }
}

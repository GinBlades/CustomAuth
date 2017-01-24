using CustomAuthWeb.Data;
using CustomAuthWeb.Services;
using CustomAuthWeb.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAuthWeb.Filters {
    public class AuthenticationFilter : ActionFilterAttribute {
        private readonly ApplicationDbContext _db;
        private readonly SimpleEncryptor _encryptor;

        public AuthenticationFilter(ApplicationDbContext db, SimpleEncryptor encryptor) {
            _db = db;
            _encryptor = encryptor;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            string authCookie = context.HttpContext.Request.Cookies["CustomAuth"];
            if (authCookie != null) {
                int id;
                if (int.TryParse(_encryptor.DecryptFromString(authCookie), out id)) {
                    var user = _db.Users.SingleOrDefault(u => u.Id == id);
                    if (user != null) {
                        context.RouteData.Values.Add("AuthUser", new AuthenticatedUser(user));
                    }
                }
            }
            base.OnActionExecuting(context);
        }
    }
}

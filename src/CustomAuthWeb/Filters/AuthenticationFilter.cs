using CustomAuthWeb.Data;
using CustomAuthWeb.Services;
using CustomAuthWeb.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Cryptography;

namespace CustomAuthWeb.Filters {
    public class AuthenticationFilter : ActionFilterAttribute {
        private readonly ApplicationDbContext _db;
        private readonly IDataProtector _protector;

        public AuthenticationFilter(ApplicationDbContext db, IDataProtectionProvider dpProvider) {
            _db = db;
            _protector = dpProvider.CreateProtector("CustomAuthWeb.Manual");
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            string authCookie = context.HttpContext.Request.Cookies["CustomAuth"];
            if (authCookie != null) {
                int id;
                try {
                    if (int.TryParse(_protector.Unprotect(authCookie), out id)) {
                        var user = _db.Users.SingleOrDefault(u => u.Id == id);
                        if (user != null) {
                            context.RouteData.Values.Add("AuthUser", new AuthenticatedUser(user));
                        }
                    }
                } catch(CryptographicException) {
                    // No action needed
                }
            }
            base.OnActionExecuting(context);
        }
    }
}

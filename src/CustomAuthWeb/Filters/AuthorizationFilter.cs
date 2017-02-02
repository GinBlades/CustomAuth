using System;
using System.Linq;
using System.Security.Claims;
using CustomAuthWeb.Models;
using CustomAuthWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace CustomAuthWeb.Filters {
    public class AuthorizationFilter : ActionFilterAttribute {
        private readonly UserRole _requiredRoles;
        private readonly RedirectToRouteResult _redirect = new RedirectToRouteResult(
                                                                new RouteValueDictionary {
                                                                    { "controller", "Home" },
                                                                    {"action", "Index" }
                                                                }
                                                            );
        public AuthorizationFilter() {
            _requiredRoles = UserRole.Guest;
        }

        public AuthorizationFilter(UserRole requiredRoles) {
            _requiredRoles = requiredRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            var currentUser = context.HttpContext.User;
            // TempData requires controller to be converted to a proper controller class,
            // rather than the object you get from context.Controller.
            var controller = context.Controller as Controller;
            if (currentUser == null || currentUser.Identity.IsAuthenticated == false) {
                controller.TempData["Alert"] = "Sign in to access.";
                context.Result = _redirect;
            } else {
                var roles = GetUserRoles(currentUser);
                if (roles != null && (roles & _requiredRoles) == _requiredRoles) {
                    base.OnActionExecuting(context);
                } else {
                    controller.TempData["Alert"] = "You do not have access to that resource.";
                    context.Result = _redirect;
                }
            }
        }

        /// <summary>
        /// Loops through identities to find a Role claim that can be converted to a UserRole.
        /// I believe this to be secure, since accepting an Identity assumes I trust it. But maybe there is some
        /// flag I should be looking for on Identities to ensure that I trust a particular Identity.
        /// </summary>
        /// <param name="currentUser">Currently logged in user</param>
        /// <returns>Nullable UserRole to be used for authorization</returns>
        private UserRole? GetUserRoles(ClaimsPrincipal currentUser) {
            UserRole roles;
            var claims = currentUser.Identities.SelectMany(i => i.Claims).Where(c => c.Type == ClaimTypes.Role);
            foreach(var claim in claims) {
                if (Enum.TryParse(claim.Value, out roles)) {
                    return roles;
                }
            }
            return null;
        }
    }
}

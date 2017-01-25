using CustomAuthWeb.Models;
using CustomAuthWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var currentUser = context.RouteData.Values["AuthUser"] as AuthenticatedUser;
            if (currentUser == null) {
                context.Result = _redirect;
            } else {
                var roles = currentUser.Roles;
                if ((roles & _requiredRoles) == _requiredRoles) {
                    base.OnActionExecuting(context);
                } else {
                    context.Result = _redirect;
                }
            }
        }
    }
}

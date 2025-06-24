using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace EasyHousingSolution.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; } // We will pass roles like "Admin,Seller"

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userType = context.HttpContext.Session.GetString("UserType");

            // 1. Check if the user is logged in at all
            if (string.IsNullOrEmpty(userType))
            {
                // User is not logged in, redirect to the Login page.
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            // 2. Check if the user has the required role
            var allowedRoles = Roles.Split(',').Select(r => r.Trim()).ToList();
            if (!allowedRoles.Contains(userType))
            {
                // User is logged in, but doesn't have the right role.
                // Redirect to an "Access Denied" page.
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
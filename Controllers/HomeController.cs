using System.Diagnostics;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Mvc;
// Add this using statement to access HttpContext.Session
using Microsoft.AspNetCore.Http;

namespace EasyHousingSolution.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // === THIS IS THE MODIFIED ACTION METHOD ===
        public IActionResult Index()
        {
            // Get the user type from the session
            var userType = HttpContext.Session.GetString("UserType");

            // Check if a user is logged in (i.e., the UserType session variable exists)
            if (!string.IsNullOrEmpty(userType))
            {
                // User is logged in, so redirect them to their correct dashboard
                switch (userType)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Seller":
                        return RedirectToAction("Dashboard", "Seller");
                    case "Buyer":
                        return RedirectToAction("Dashboard", "Buyer");
                    default:
                        // If role is unknown, clear session and show homepage
                        HttpContext.Session.Clear();
                        return View();
                }
            }

            // If no one is logged in, just show the regular homepage
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AccessDenied()
        {
            // This action can be used to show an "Access Denied" page
            return View();
        }
    }
}
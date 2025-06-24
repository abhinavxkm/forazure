using BCrypt.Net;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace EasyHousingSolution.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Step 1: Find the user by their UserName ONLY.
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);

                // Step 2: If the user exists, verify their password against the stored hash.
                if (user != null && (BCrypt.Net.BCrypt.Verify(model.Password, user.Password)))
                {
                    // If we get here, the login is successful!
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("UserType", user.UserType);

                    if (user.UserType == "Buyer")
                    {
                        var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.UserName == user.UserName);
                        if (buyer != null)
                        {
                            HttpContext.Session.SetInt32("BuyerId", buyer.BuyerId);
                        }
                    }
                    else if (user.UserType == "Seller")
                    {
                        var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserName == user.UserName);
                        if (seller != null)
                        {
                            HttpContext.Session.SetInt32("SellerId", seller.SellerId);
                        }
                    }

                    return RedirectToAction("Index", "Home");
                }

                // If user is null or password verification fails, add a generic error message.
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }

            return View(model);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
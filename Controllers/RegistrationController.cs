using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Populates State and City dropdowns for the registration form.
        /// </summary>
        private void PopulateDropdowns(RegistrationViewModel model)
        {
            model.States = new SelectList(_context.States.ToList(), "StateId", "StateName");
            model.Cities = new SelectList(new List<City>(), "CityId", "CityName"); // Start with empty cities
        }

        /// <summary>
        /// Displays the user registration form.
        /// </summary>
        /// <returns>The registration view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegistrationViewModel();
            PopulateDropdowns(model);
            return View(model);
        }

        /// <summary>
        /// Handles the submission of the registration form.
        /// </summary>
        /// <param name="model">The registration data submitted by the user.</param>
        /// <returns>Redirects to login on success, or redisplays form on failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            // Conditional validation for Sellers
            if (model.UserType == "Seller")
            {
                if (string.IsNullOrEmpty(model.Address)) { ModelState.AddModelError("Address", "The Address field is required for Sellers."); }
                if (!model.StateId.HasValue) { ModelState.AddModelError("StateId", "The State field is required for Sellers."); }
                if (!model.CityId.HasValue) { ModelState.AddModelError("CityId", "The City field is required for Sellers."); }
            }

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Create the User entity with a hashed password
                    var user = new User
                    {
                        UserName = model.UserName,
                        UserType = model.UserType,
                        Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
                    };
                    _context.Users.Add(user);

                    // 2. Create the corresponding Buyer or Seller entity
                    if (model.UserType == "Seller")
                    {
                        var seller = new Seller
                        {
                            UserName = model.UserName,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            PhoneNo = model.PhoneNo,
                            EmailId = model.EmailId,
                            Address = model.Address,
                            StateId = model.StateId.Value,
                            CityId = model.CityId.Value
                        };
                        _context.Sellers.Add(seller);
                    }
                    else if (model.UserType == "Buyer")
                    {
                        var buyer = new Buyer
                        {
                            UserName = model.UserName,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            PhoneNo = model.PhoneNo,
                            EmailId = model.EmailId
                        };
                        _context.Buyers.Add(buyer);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login", "Login");
                }
                catch (DbUpdateException ex)
                {
                    // Log the error (in a real app, use a logging framework)
                    ModelState.AddModelError("", "An error occurred while saving to the database. Please try again.");
                }
            }

            // If we get here, validation failed.
            PopulateDropdowns(model);
            return View(model);
        }

        /// <summary>
        /// Gets a list of cities for a given state, used for dynamic dropdowns.
        /// </summary>
        /// <param name="stateId">The ID of the state.</param>
        /// <returns>A JSON result containing a list of cities.</returns>
        [HttpGet]
        public JsonResult GetCitiesByState(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { value = c.CityId, text = c.CityName })
                .ToList();
            return new JsonResult(cities);
        }
    }
}
using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List<IFormFile>

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentSellerId()
        {
            return HttpContext.Session.GetInt32("SellerId");
        }

        /// <summary>
        /// Displays the dashboard for the logged-in seller.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var viewModel = new SellerDashboardViewModel
            {
                VerifiedCount = await _context.Properties
                    .CountAsync(p => p.SellerId == sellerId.Value && p.IsActive == true),

                PendingCount = await _context.Properties
                    .CountAsync(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason == null),

                DeactivatedCount = await _context.Properties
                    .CountAsync(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason != null)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the form to add a new property.
        /// </summary>
        [HttpGet]
        public IActionResult AddProperty()
        {
            return View();
        }

        /// <summary>
        /// Handles the submission of the new property form.
        /// </summary>
        /// 
        /// //blob here
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var property = new Property
                {
                    PropertyName = model.PropertyName,
                    PropertyType = model.PropertyType,
                    PropertyOption = model.PropertyOption,
                    Description = model.Description,
                    Address = model.Address,
                    Region = model.Region,
                    PriceRange = model.PriceRange,
                    InitialDeposit = model.InitialDeposit,
                    Landmark = model.Landmark,
                    IsActive = false,
                    SellerId = sellerId.Value
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync(); // Save property first to get PropertyId

                if (model.NewImages != null)
                {
                    foreach (var file in model.NewImages.Take(6)) // Max 6 images
                    {
                        if (file.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);
                            var image = new Image
                            {
                                PropertyId = property.PropertyId,
                                ImageData = ms.ToArray()
                            };
                            _context.Images.Add(image);
                        }
                    }
                    await _context.SaveChangesAsync(); // Save images
                }

                TempData["SuccessMessage"] = "Property and images added successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        /// <summary>
        /// Displays the form to edit an existing property.
        /// </summary>
        /// <param name="id">The ID of the property to edit.</param>
        //[HttpGet]
        //public async Task<IActionResult> EditProperty(int id)
        //{
        //    var sellerId = GetCurrentSellerId();
        //    if (sellerId == null) return RedirectToAction("Login", "Login");

        //    var property = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == id && p.SellerId == sellerId.Value);
        //    if (property == null) return NotFound();

        //    var model = new PropertyViewModel
        //    {
        //        PropertyId = property.PropertyId,
        //        PropertyName = property.PropertyName,
        //        PropertyType = property.PropertyType,
        //        PropertyOption = property.PropertyOption,
        //        Description = property.Description,
        //        Address = property.Address,
        //        Region = property.Region,
        //        PriceRange = property.PriceRange,
        //        InitialDeposit = property.InitialDeposit,
        //        Landmark = property.Landmark
        //    };
        //    return View(model);
        //}

        ///// <summary>
        ///// Handles the submission of the edited property form.
        ///// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditProperty(PropertyViewModel model)
        //{
        //    var sellerId = GetCurrentSellerId();
        //    if (sellerId == null) return RedirectToAction("Login", "Login");

        //    if (ModelState.IsValid)
        //    {
        //        var propertyToUpdate = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == model.PropertyId && p.SellerId == sellerId.Value);
        //        if (propertyToUpdate == null) return NotFound();

        //        propertyToUpdate.PropertyName = model.PropertyName;
        //        propertyToUpdate.PropertyType = model.PropertyType;
        //        propertyToUpdate.PropertyOption = model.PropertyOption;
        //        propertyToUpdate.Description = model.Description;
        //        propertyToUpdate.Address = model.Address;
        //        propertyToUpdate.Region = model.Region;
        //        propertyToUpdate.PriceRange = model.PriceRange;
        //        propertyToUpdate.InitialDeposit = model.InitialDeposit;
        //        propertyToUpdate.Landmark = model.Landmark;
        //        propertyToUpdate.IsActive = false; // Resubmitted properties must be re-verified
        //        propertyToUpdate.DeactivationReason = null; // Clear any previous deactivation reason

        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //            TempData["SuccessMessage"] = "Property has been successfully updated and resubmitted for verification.";
        //            return RedirectToAction("Dashboard");
        //        }
        //        catch (DbUpdateException)
        //        {
        //            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
        //        }
        //    }
        //    return View(model);
        //}

        /// <summary>
        /// Displays the image upload form for a specific property.
        /// </summary>
        /// <param name = "id" > The ID of the property for which to upload images.</param>
        // This replaces the GET method
        // In SellerController.cs, REPLACE the [HttpGet] EditProperty method with this:
        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            var property = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == id && p.SellerId == sellerId.Value);
            if (property == null) return NotFound();

            var model = new PropertyViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                PropertyOption = property.PropertyOption,
                Description = property.Description,
                Address = property.Address,
                Region = property.Region,
                PriceRange = property.PriceRange,
                InitialDeposit = property.InitialDeposit,
                Landmark = property.Landmark
            };
            return View(model);
        }

        /// <summary>
        /// Handles the submission of the edited property form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(PropertyViewModel model)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var propertyToUpdate = await _context.Properties.FirstOrDefaultAsync(p => p.PropertyId == model.PropertyId && p.SellerId == sellerId.Value);
                if (propertyToUpdate == null) return NotFound();

                propertyToUpdate.PropertyName = model.PropertyName;
                propertyToUpdate.PropertyType = model.PropertyType;
                propertyToUpdate.PropertyOption = model.PropertyOption;
                propertyToUpdate.Description = model.Description;
                propertyToUpdate.Address = model.Address;
                propertyToUpdate.Region = model.Region;
                propertyToUpdate.PriceRange = model.PriceRange;
                propertyToUpdate.InitialDeposit = model.InitialDeposit;
                propertyToUpdate.Landmark = model.Landmark;
                propertyToUpdate.IsActive = false; // Resubmitted properties must be re-verified
                propertyToUpdate.DeactivationReason = null; // Clear any previous deactivation reason

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Property has been successfully updated and resubmitted for verification.";
                    return RedirectToAction("Dashboard");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(model);
        }


        // In SellerController.cs, you can now DELETE the old [HttpGet] and [HttpPost] methods for UploadImages.




        /// <summary>
        /// Shows a list of properties for the current seller that have been verified by an admin.
        /// </summary>
        // Find this method in SellerController.cs
        public async Task<IActionResult> VerifiedProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            // Add .Include(p => p.Images) to this query
            var properties = await _context.Properties
                .Include(p => p.Images) // <-- ADD THIS LINE
                .Where(p => p.SellerId == sellerId.Value && p.IsActive == true)
                .ToListAsync();

            return View(properties);
        }

        /// <summary>
        /// Shows a list of properties for the current seller that are pending verification.
        /// </summary>
        // Find this method in SellerController.cs
        public async Task<IActionResult> PendingProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");

            // Add .Include(p => p.Images) to this query
            var properties = await _context.Properties
                .Include(p => p.Images) // <-- ADD THIS LINE
                .Where(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason == null)
                .ToListAsync();

            return View(properties);
        }

        /// <summary>
        /// Shows a list of properties for the current seller that have been deactivated by an admin.
        /// </summary>
        public async Task<IActionResult> DeactivatedProperties()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null) return RedirectToAction("Login", "Login");
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Where(p => p.SellerId == sellerId.Value && !p.IsActive && p.DeactivationReason != null)
                .ToListAsync();
            return View(properties);
        }
    }
}
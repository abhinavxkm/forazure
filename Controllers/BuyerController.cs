using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Buyer")]
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BuyerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentBuyerId()
        {
            return HttpContext.Session.GetInt32("BuyerId");
        }

        /// <summary>
        /// Displays the dashboard for the logged-in buyer.
        /// </summary>
        // Replace the existing Dashboard() method in BuyerController.cs
        public async Task<IActionResult> Dashboard()
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var viewModel = new BuyerDashboardViewModel
            {
                CartItemCount = await _context.Carts
                    .CountAsync(c => c.BuyerId == buyerId.Value)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Searches for active properties with optional filters and sorting.
        /// </summary>
        /// <param name="region">Optional: Filter by region.</param>
        /// <param name="type">Optional: Filter by property type.</param>
        /// <param name="sortOrder">Optional: Sort by 'price' or 'name'.</param>
        public IActionResult Search(string region = "", string type = "", string sortOrder = "name")
        {
            var properties = _context.Properties.Include(p => p.Seller)
                .Where(p => p.IsActive == true &&
                              (string.IsNullOrEmpty(region) || p.Address.Contains(region)) &&
                              (string.IsNullOrEmpty(type) || p.PropertyType == type));

            properties = sortOrder == "price"
                ? properties.OrderBy(p => p.PriceRange)
                : properties.OrderBy(p => p.PropertyName);

            return View(properties.ToList());
        }

        /// <summary>
        /// Displays the full details for a single property.
        /// The corresponding View should display the Seller's contact details (EmailId and PhoneNo) from the included Seller object.
        /// </summary>
        /// <param name="id">The ID of the property to display.</param>
        public async Task<IActionResult> PropertyDetails(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PropertyId == id); // Remove IsActive for debugging

            if (property == null)
            {
                return Content($"No property found with ID {id}"); // For debugging
            }
            return View(property);
        }


        /// <summary>
        /// Adds a property to the current buyer's cart.
        /// </summary>
        /// <param name="propertyId">The ID of the property to add.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int propertyId)
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null)
            {
                return Json(new { success = false, message = "You must be logged in to add items to the cart." });
            }

            // --- THIS IS THE FIX ---
            // First, check if this property is already in the buyer's cart.
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.PropertyId == propertyId && c.BuyerId == buyerId.Value);

            // Only add the item if it does NOT already exist.
            if (existingCartItem == null)
            {
                var cartItem = new Cart { PropertyId = propertyId, BuyerId = buyerId.Value };
                _context.Carts.Add(cartItem);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Property successfully added to your cart." });
                }
                catch (DbUpdateException)
                {
                    return Json(new { success = false, message = "An error occurred while saving to your cart." });
                }
            }

            // If the item already exists, we still return a success response,
            // but with a different message. This prevents duplicates.
            return Json(new { success = true, message = "This property is already in your cart." });
        }


        /// <summary>
        /// Displays the contents of the current buyer's cart.
        /// </summary>
        // Replace the existing ViewCart() method in BuyerController.cs
        public async Task<IActionResult> ViewCart()
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null) return RedirectToAction("Login", "Login");

            var cartItems = await _context.Carts
                .Where(c => c.BuyerId == buyerId.Value)
                .Include(c => c.Property).ThenInclude(p => p.Images) // Eager load property and its images
                .Select(c => new CartViewModel
                {
                    CartId = c.CartId,
                    PropertyName = c.Property.PropertyName,
                    PropertyType = c.Property.PropertyType,
                    PropertyOption = c.Property.PropertyOption,
                    PriceRange = c.Property.PriceRange,
                    PropertyId = c.Property.PropertyId,
                    // Get the ID of the first image, or null if no images exist
                    FirstImageId = c.Property.Images.Select(i => (int?)i.ImageId).FirstOrDefault()
                })
                .ToListAsync();

            return View(cartItems);
        }

        /// <summary>
        /// Removes an item from the current buyer's cart.
        /// </summary>
        /// <param name="cartId">The ID of the cart item to remove.</param>
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null) return RedirectToAction("Login", "Login");

            var item = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.BuyerId == buyerId.Value);
            if (item != null)
            {
                _context.Carts.Remove(item);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Optionally handle the error, e.g., TempData["ErrorMessage"] = "Could not remove item."
                }
            }
            return RedirectToAction("ViewCart");
        }

        /// <summary>
        /// Displays a comparison view for two selected properties.
        /// </summary>
        /// <param name="ids">An array containing the IDs of the two properties to compare.</param>
        [HttpPost]
        public async Task<IActionResult> Compare(int[] ids)
        {
            if (ids == null || ids.Length != 2)
            {
                TempData["ErrorMessage"] = "Please select exactly two properties to compare.";
                return RedirectToAction("Search");
            }

            var propertiesToCompare = await _context.Properties
                                              .Include(p => p.Seller)
                                              .Include(p => p.Images)
                                              .Where(p => ids.Contains(p.PropertyId))
                                              .ToListAsync();

            return View(propertiesToCompare);
        }
        /// <summary>
        /// Gets the number of items in the current buyer's cart.
        /// Called via AJAX to update the cart badge in the navigation bar.
        /// </summary>
        /// <returns>A JSON object with the cart item count.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var buyerId = GetCurrentBuyerId();
            if (buyerId == null)
            {
                // Return 0 if the user is not a logged-in buyer
                return Json(new { count = 0 });
            }

            var count = await _context.Carts
                .CountAsync(c => c.BuyerId == buyerId.Value);

            return Json(new { count = count });
        }
    }

}
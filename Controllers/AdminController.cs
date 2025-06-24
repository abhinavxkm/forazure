using EasyHousingSolution.Filters;
using EasyHousingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EasyHousingSolution.Controllers
{
    [AuthorizeUser(Roles = "Admin")] // Secures the entire controller for Admins only
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the admin dashboard with site statistics and recent pending properties.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.PendingCount = await _context.Properties.CountAsync(p => !p.IsActive && p.DeactivationReason == null);
            ViewBag.LiveCount = await _context.Properties.CountAsync(p => p.IsActive);
            ViewBag.SellerCount = await _context.Sellers.CountAsync();
            ViewBag.BuyerCount = await _context.Buyers.CountAsync();

            var recentPending = await _context.Properties
                .Where(p => !p.IsActive && p.DeactivationReason == null)
                .Include(p => p.Seller)
                .OrderByDescending(p => p.PropertyId)
                .Take(5)
                .ToListAsync();

            return View(recentPending);
        }

        /// <summary>
        /// Displays a list of all properties pending verification.
        /// </summary>
        public async Task<IActionResult> PendingProperties()
        {
            var pending = await _context.Properties
                .Where(p => p.IsActive == false && p.DeactivationReason == null)
                .Include(p => p.Seller)
                .ToListAsync();
            return View(pending);
        }

        /// <summary>
        /// Displays a list of all verified (live) properties with filtering options.
        /// </summary>
        /// <param name="sellerId">Optional: Filter properties by a specific seller.</param>
        /// <param name="region">Optional: Filter properties by a specific region.</param>
        public async Task<IActionResult> VerifiedProperties(int? sellerId, string? region)
        {
            var query = _context.Properties
                .Include(p => p.Seller)
                .Where(p => p.IsActive == true)
                .AsQueryable();

            if (sellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == sellerId.Value);
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(p => p.Region.Contains(region));
            }

            ViewBag.Sellers = new SelectList(await _context.Sellers.ToListAsync(), "SellerId", "UserName");
            ViewData["CurrentSellerId"] = sellerId;
            ViewData["CurrentRegion"] = region;

            var properties = await query.ToListAsync();
            return View(properties);
        }

        /// <summary>
        /// Approves a pending property, making it live on the site.
        /// </summary>
        /// <param name="id">The ID of the property to approve.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                property.IsActive = true;
                property.DeactivationReason = null; // Clear any previous rejection reason
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Property '{property.PropertyName}' has been approved.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "An error occurred while approving the property.";
                }
            }
            return RedirectToAction(nameof(PendingProperties));
        }

        /// <summary>
        /// Displays the form to enter a reason for deactivating a property.
        /// </summary>
        /// <param name="id">The ID of the property to deactivate.</param>
        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }

        /// <summary>
        /// Deactivates a live property, removing it from public view.
        /// </summary>
        /// <param name="propertyId">The ID of the property to deactivate.</param>
        /// <param name="reason">The reason for deactivation.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int propertyId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                ModelState.AddModelError("Reason", "A reason for deactivation is required.");
                var propertyForView = await _context.Properties.FindAsync(propertyId);
                return View(propertyForView);
            }

            var property = await _context.Properties.FindAsync(propertyId);
            if (property != null)
            {
                property.IsActive = false;
                property.DeactivationReason = reason; // Save the reason
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Property '{property.PropertyName}' has been deactivated.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "An error occurred while deactivating the property.";
                }
            }
            return RedirectToAction(nameof(VerifiedProperties));
        }
        /// <summary>
        /// Displays the full details of a single property for the administrator.
        /// </summary>
        /// <param name="id">The ID of the property to display.</param>
        public async Task<IActionResult> PropertyDetails(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Seller) // Include the seller's information
                .Include(p => p.Images) // Include all property images
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }
    }
}
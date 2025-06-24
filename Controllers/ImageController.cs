using EasyHousingSolution.Models;

using Microsoft.AspNetCore.Mvc;

using System.Linq;

namespace EasyHousingSolution.Controllers

{

    public class ImageController : Controller

    {

        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)

        {

            _context = context;

        }

        // This action fetches an image by its ID and returns it as a file.

        // e.g., /Image/Get/123

        public IActionResult Get(int id)

        {

            var image = _context.Images.FirstOrDefault(i => i.ImageId == id);

            if (image != null && image.ImageData != null)

            {

                // Return the byte array as a jpeg image file.

                // The browser will know how to display this.

                return File(image.ImageData, "image/jpeg");

            }

            // Return a "Not Found" result if no image exists.

            return NotFound();

        }

    }

}

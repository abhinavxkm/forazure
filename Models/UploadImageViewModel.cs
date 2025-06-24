using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class UploadImageViewModel
    {
        [Required]
        public int PropertyId { get; set; }

        // This is not submitted, but used to display the name on the page
        public string PropertyName { get; set; }

        // The validation message is improved for clarity
        [Required(ErrorMessage = "Please select at least one image file.")]
        [Display(Name = "Select Images (Max 6)")]
        public List<IFormFile> Images { get; set; }
    }
}
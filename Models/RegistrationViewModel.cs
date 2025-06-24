using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class RegistrationViewModel
    {
        [Required, MaxLength(25)]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string UserType { get; set; }

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string? LastName { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required, Phone]
        public string PhoneNo { get; set; }

        [Required, EmailAddress]
        public string EmailId { get; set; }

        // Address fields are only required for Sellers
        public string? Address { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }

        // --- NEW PROPERTIES TO HOLD DROPDOWN LISTS ---
        // This removes our dependency on the failing ViewBag
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
    }
}
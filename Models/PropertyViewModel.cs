using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class PropertyViewModel
    {
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Property Name is required.")]
        [MaxLength(50)]
        public string PropertyName { get; set; }

        [Required(ErrorMessage = "Property Type is required.")]
        [MaxLength(15)]
        public string PropertyType { get; set; }

        [Required(ErrorMessage = "Please select an option (Sell or Rent).")]
        [MaxLength(10)]
        public string PropertyOption { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(250)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Region is required.")]
        [MaxLength(50)]
        public string Region { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal PriceRange { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Initial Deposit cannot be negative.")]
        public decimal? InitialDeposit { get; set; }

        [Required(ErrorMessage = "Landmark is required.")]
        [MaxLength(25)]
        public string Landmark { get; set; }

        // This will hold the NEW files the user wants to upload.
        public List<IFormFile>? NewImages { get; set; }

    }
}
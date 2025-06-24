using System;
namespace EasyHousingSolution.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string PropertyOption { get; set; }
        public decimal PriceRange { get; set; }
        public int PropertyId { get; set; }
        public int? FirstImageId { get; set; }
    }
}
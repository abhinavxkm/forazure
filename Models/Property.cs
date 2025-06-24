using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace EasyHousingSolution.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }
        [Required, MaxLength(50)]
        public string PropertyName { get; set; }
        [Required, MaxLength(50)]
        public string PropertyType { get; set; }
        [Required, MaxLength(10)]
        public string PropertyOption { get; set; } // Sell or Rent
        [MaxLength(250)]
        public string? Description { get; set; }
        [Required, MaxLength(250)]
        public string Address { get; set; }
        [Required, MaxLength(50)]
        public string Region { get; set; }
        [Required]
        [Precision(18,2)]
        public decimal PriceRange { get; set; }
        [MaxLength(500)]
        public string? DeactivationReason { get; set; }
        [Precision(18,2)]
        public decimal? InitialDeposit { get; set; }
        [Required, MaxLength(25)]
        public string Landmark { get; set; }
        public bool IsActive { get; set; } = false;
        [ForeignKey("Seller")]
        public int SellerId { get; set; }
        public Seller? Seller { get; set; }

       public virtual ICollection<Image> Images { get; set; } = new List<Image>();
    }
}
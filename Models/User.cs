using System.ComponentModel.DataAnnotations;
namespace EasyHousingSolution.Models
{
    public class User
    {
        [Key]
        [MaxLength(25)]
        public string UserName { get; set; }
        [Required, MaxLength(100)]
        public string Password { get; set; }
        [Required, MaxLength(15)]
        public string UserType { get; set; } // Admin, Buyer, Seller
    }
}
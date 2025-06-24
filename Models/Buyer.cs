using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyHousingSolution.Models
{
    public class Buyer
    {
        [Key]
        public int BuyerId { get; set; }

        // It is the required link to the Users table for login credentials.
        [Required, MaxLength(25)]
        public string UserName { get; set; }
        [ForeignKey("UserName")]
        public virtual User User { get; set; }
        // -----------------------------------------------------------

        [Required, MaxLength(25)]
        public string FirstName { get; set; }

        [MaxLength(25)]
        public string? LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public string PhoneNo { get; set; }

        [Required, MaxLength(50)]
        public string EmailId { get; set; }
    }
}
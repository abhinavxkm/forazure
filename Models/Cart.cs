using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyHousingSolution.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        // CORRECTED: This now correctly links to the Buyer table's integer ID.
        public int BuyerId { get; set; }
        [ForeignKey("BuyerId")]
        public virtual Buyer Buyer { get; set; }

        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }
    }
}
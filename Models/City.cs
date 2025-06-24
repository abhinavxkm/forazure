using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EasyHousingSolution.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }
        [Required, MaxLength(50)]
        public string CityName { get; set; }
        [ForeignKey("State")]
        public int StateId { get; set; }
        public State? State { get; set; }
    }
}
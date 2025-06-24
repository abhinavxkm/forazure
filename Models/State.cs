using System.ComponentModel.DataAnnotations;

namespace EasyHousingSolution.Models
{
    public class State
    {
        [Key]
        public int StateId { get; set; }

        [Required, MaxLength(30)]
        public string StateName { get; set; }
    }
}
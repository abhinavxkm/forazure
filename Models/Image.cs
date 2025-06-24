using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EasyHousingSolution.Models
{
    public class Image
    {
        [Key]
        public int ImageId { get; set; }
        [ForeignKey("Property")]
        public int PropertyId { get; set; }
        public byte[] ImageData { get; set; }
        public Property? Property { get; set; }
    }
}
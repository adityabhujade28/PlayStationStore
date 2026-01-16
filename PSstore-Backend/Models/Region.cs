using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RegionId { get; set; }

        [Required]
        [MaxLength(10)]
        public string RegionCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RegionName { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Country> Countries { get; set; } = new List<Country>();
    }
}

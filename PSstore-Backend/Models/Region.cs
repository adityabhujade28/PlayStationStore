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

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? RegionTimezone { get; set; }

        // Navigation properties
        public ICollection<UsersRegion> UsersRegions { get; set; } = new List<UsersRegion>();
        public ICollection<SubscriptionPlanRegion> SubscriptionPlanRegions { get; set; } = new List<SubscriptionPlanRegion>();
    }
}

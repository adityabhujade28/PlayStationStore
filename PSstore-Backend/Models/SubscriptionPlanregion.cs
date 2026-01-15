using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class SubscriptionPlanRegion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanRegionId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        public int RegionId { get; set; }

        [Required]
        public int DurationMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey(nameof(SubscriptionId))]
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        [ForeignKey(nameof(RegionId))]
        public Region Region { get; set; } = null!;

        public ICollection<UserSubscriptionPlan> UserSubscriptionPlans { get; set; } = new List<UserSubscriptionPlan>();
    }
}

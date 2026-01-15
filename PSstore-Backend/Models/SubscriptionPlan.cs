using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class SubscriptionPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubscriptionType { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<GameSubscription> GameSubscriptions { get; set; } = new List<GameSubscription>();
        public ICollection<SubscriptionPlanRegion> SubscriptionPlanRegions { get; set; } = new List<SubscriptionPlanRegion>();
    }
}

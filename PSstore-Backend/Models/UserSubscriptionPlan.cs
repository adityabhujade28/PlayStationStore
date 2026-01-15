using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class UserSubscriptionPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSubscriptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SubscriptionPlanRegionId { get; set; }

        public DateTime PlanStartDate { get; set; } = DateTime.UtcNow;

        public DateTime PlanEndDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(SubscriptionPlanRegionId))]
        public SubscriptionPlanRegion SubscriptionPlanRegion { get; set; } = null!;
    }
}

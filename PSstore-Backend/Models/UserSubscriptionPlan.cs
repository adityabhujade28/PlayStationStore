using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class UserSubscriptionPlan
    {
        [Key]
        public Guid UserSubscriptionId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid SubscriptionPlanCountryId { get; set; }

        public DateTime PlanStartDate { get; set; } = DateTime.UtcNow;

        public DateTime PlanEndDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(SubscriptionPlanCountryId))]
        public SubscriptionPlanCountry SubscriptionPlanCountry { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public Guid SubscriptionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubscriptionType { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<GameSubscription> GameSubscriptions { get; set; } = new List<GameSubscription>();
        public ICollection<SubscriptionPlanCountry> SubscriptionPlanCountries { get; set; } = new List<SubscriptionPlanCountry>();
    }
}

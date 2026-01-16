using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class SubscriptionPlanCountry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPlanCountryId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int DurationMonths { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey(nameof(SubscriptionId))]
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        [ForeignKey(nameof(CountryId))]
        public Country Country { get; set; } = null!;

        public ICollection<UserSubscriptionPlan> UserSubscriptionPlans { get; set; } = new List<UserSubscriptionPlan>();
    }
}

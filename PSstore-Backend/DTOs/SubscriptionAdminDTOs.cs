using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Create new plan type (e.g. "Ultimate")
    public class CreatePlanDTO
    {
        [Required]
        [MaxLength(50)]
        public string SubscriptionType { get; set; } = string.Empty;
    }

    // Update plan name
    public class UpdatePlanDTO
    {
        [Required]
        [MaxLength(50)]
        public string SubscriptionType { get; set; } = string.Empty;
    }

    // Create pricing option (e.g. India - 1 Month - 499)
    public class CreatePlanPricingDTO
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        public Guid? CountryId { get; set; }
        public string? CountryName { get; set; }

        [Range(1, 120)]
        public int DurationMonths { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }

    public class SubscriptionPricingDTO
    {
        public Guid PlanCountryId { get; set; }
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
    }
}
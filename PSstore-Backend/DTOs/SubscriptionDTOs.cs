using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Subscription plan details
    public class SubscriptionPlanDTO
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public List<SubscriptionPlanCountryDTO> CountryPricing { get; set; } = new List<SubscriptionPlanCountryDTO>();
        public List<string> IncludedGames { get; set; } = new List<string>();
    }

    // Subscription plan pricing per country
    public class SubscriptionPlanCountryDTO
    {
        public int SubscriptionPlanCountryId { get; set; }
        public int SubscriptionId { get; set; }
        public int CountryId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    // Create subscription (subscribe to a plan)
    public class CreateSubscriptionDTO
    {
        [Required]
        public int SubscriptionPlanCountryId { get; set; }
    }

    // Subscribe to a plan
    public class SubscribeDTO
    {
        [Required]
        public int SubscriptionPlanCountryId { get; set; }
    }

    // Subscription response
    public class SubscriptionResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UserSubscriptionId { get; set; }
        public int UserId { get; set; }
        public string SubscriptionType { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
    }

    // User's active/past subscriptions
    public class UserSubscriptionDTO
    {
        public int UserSubscriptionId { get; set; }
        public int UserId { get; set; }
        public int SubscriptionPlanCountryId { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

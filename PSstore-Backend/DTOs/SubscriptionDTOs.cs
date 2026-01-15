using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Subscription plan details
    public class SubscriptionPlanDTO
    {
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public List<SubscriptionPlanRegionDTO> RegionPricing { get; set; } = new List<SubscriptionPlanRegionDTO>();
        public List<string> IncludedGames { get; set; } = new List<string>();
    }

    // Subscription plan pricing per region
    public class SubscriptionPlanRegionDTO
    {
        public int SubscriptionPlanRegionId { get; set; }
        public int SubscriptionId { get; set; }
        public int RegionId { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public int DurationMonths { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    // Create subscription (subscribe to a plan)
    public class CreateSubscriptionDTO
    {
        [Required]
        public int SubscriptionPlanRegionId { get; set; }
    }

    // Subscribe to a plan
    public class SubscribeDTO
    {
        [Required]
        public int SubscriptionPlanRegionId { get; set; }
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
        public int SubscriptionPlanRegionId { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

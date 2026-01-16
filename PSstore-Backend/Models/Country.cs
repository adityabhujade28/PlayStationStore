using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CountryId { get; set; }

        [Required]
        [MaxLength(10)]
        public string CountryCode { get; set; } = string.Empty; // e.g., "US", "UK", "DE", "JP", "IN"

        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; } = string.Empty; // e.g., "United States", "United Kingdom"

        [Required]
        public int RegionId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = string.Empty; // e.g., "USD", "GBP", "EUR", "JPY", "INR"

        [MaxLength(100)]
        public string? Timezone { get; set; } // e.g., "America/New_York", "Europe/London"

        [Column(TypeName = "decimal(5,4)")]
        public decimal? TaxRate { get; set; } // Optional: e.g., 0.20 for 20% VAT

        // Navigation properties
        [ForeignKey("RegionId")]
        public Region Region { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<SubscriptionPlanCountry> SubscriptionPlanCountries { get; set; } = new List<SubscriptionPlanCountry>();
        public ICollection<GameCountry> GameCountries { get; set; } = new List<GameCountry>();
    }
}

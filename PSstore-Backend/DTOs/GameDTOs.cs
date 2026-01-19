using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Response for game details
    public class GameDTO
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? PublishedBy { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public bool FreeToPlay { get; set; }
        public decimal Price { get; set; }
        public bool IsMultiplayer { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> AvailableInPlans { get; set; } = new List<string>();
    }

    // Create new game (Admin)
    public class CreateGameDTO
    {
        [Required]
        [MaxLength(200)]
        public string GameName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PublishedBy { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public bool FreeToPlay { get; set; } = false;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsMultiplayer { get; set; } = false;

        public List<int> CategoryIds { get; set; } = new List<int>();
    }

    // Update game (Admin)
    public class UpdateGameDTO
    {
        [MaxLength(200)]
        public string? GameName { get; set; }

        [MaxLength(200)]
        public string? PublishedBy { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public bool? FreeToPlay { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public bool? IsMultiplayer { get; set; }
    }

    // Game with access info for user
    public class GameWithAccessDTO : GameDTO
    {
        public bool CanAccess { get; set; }
        public string AccessType { get; set; } = string.Empty; // "FREE", "PURCHASED", "SUBSCRIPTION", "NO_ACCESS"
        public DateTime? AccessExpiresOn { get; set; }
        public DateTime? PurchasedOn { get; set; }
    }

    public class CreateGamePricingDTO
    {
        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }

    public class GamePricingDTO
    {
        public Guid GameCountryId { get; set; }
        public Guid GameId { get; set; }
        public Guid CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

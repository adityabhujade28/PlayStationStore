using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class Game
    {
        [Key]
        public Guid GameId { get; set; }

        [Required]
        [MaxLength(200)]
        public string GameName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PublishedBy { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public bool FreeToPlay { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? BasePrice { get; set; } // Deprecated: Use GameCountry for pricing

        public bool IsMultiplayer { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public ICollection<GameCountry> GameCountries { get; set; } = new List<GameCountry>();
        public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();
        public ICollection<GameSubscription> GameSubscriptions { get; set; } = new List<GameSubscription>();
        public ICollection<UserPurchaseGame> UserPurchases { get; set; } = new List<UserPurchaseGame>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}

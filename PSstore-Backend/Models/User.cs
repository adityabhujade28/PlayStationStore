using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string UserPassword { get; set; } = string.Empty;

        [Range(0, 150)]
        public int? Age { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SubscriptionStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public int? CountryId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CountryId))]
        public Country? Country { get; set; }

        public ICollection<UserPurchaseGame> UserPurchasedGames { get; set; } = new List<UserPurchaseGame>();
        public ICollection<UserSubscriptionPlan> UserSubscriptionPlans { get; set; } = new List<UserSubscriptionPlan>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}

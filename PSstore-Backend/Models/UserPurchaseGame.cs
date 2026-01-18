using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class UserPurchaseGame
    {
        [Key]
        public Guid PurchaseId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PurchasePrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;
    }
}

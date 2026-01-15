using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class GameSubscription
    {
        [Required]
        public int GameId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;

        [ForeignKey(nameof(SubscriptionId))]
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    }
}

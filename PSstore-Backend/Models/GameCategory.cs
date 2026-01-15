using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class GameCategory
    {
        [Required]
        public int GameId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class GameCountry
    {
        [Key]
        public Guid GameCountryId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; } = null!;

        [ForeignKey(nameof(CountryId))]
        public Country Country { get; set; } = null!;
    }
}

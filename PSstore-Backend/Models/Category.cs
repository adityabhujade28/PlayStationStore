using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string CategoryName { get; set; } 

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public ICollection<GameCategory> GameCategories { get; set; } = new List<GameCategory>();
    }
}

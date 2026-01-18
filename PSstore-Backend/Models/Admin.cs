using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSstore.Models
{
    public class Admin
    {
        [Key]
        public Guid AdminId { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string AdminPassword { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}

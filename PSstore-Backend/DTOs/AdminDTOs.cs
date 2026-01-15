using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Admin login
    public class AdminLoginDTO
    {
        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        public string AdminPassword { get; set; } = string.Empty;
    }

    // Admin response
    public class AdminResponseDTO
    {
        public int AdminId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Create admin
    public class CreateAdminDTO
    {
        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string AdminPassword { get; set; } = string.Empty;
    }
}

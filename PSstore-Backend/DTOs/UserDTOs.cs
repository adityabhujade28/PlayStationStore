using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Create new user
    public class CreateUserDTO
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string UserPassword { get; set; } = string.Empty;

        [Range(0, 150)]
        public int? Age { get; set; }

        public int? CountryId { get; set; }
    }

    // Update user details
    public class UpdateUserDTO
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [EmailAddress]
        public string? UserEmail { get; set; }

        [Range(0, 150)]
        public int? Age { get; set; }
    }

    // Response for user details
    public class UserResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? SubscriptionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public RegionDTO? CurrentRegion { get; set; }
    }

    // Simple user DTO
    public class UserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int? Age { get; set; }
        public bool SubscriptionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // User login
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string UserPassword { get; set; } = string.Empty;
    }

    // Login response with JWT token
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

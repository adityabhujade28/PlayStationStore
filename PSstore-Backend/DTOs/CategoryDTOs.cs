using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Category response
    public class CategoryDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    // Create category (Admin)
    public class CreateCategoryDTO
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    // Update category (Admin)
    public class UpdateCategoryDTO
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }
}

using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            return category != null ? MapToCategoryDTO(category) : null;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(bool includeDeleted = false)
        {
            // Note: includeDeleted parameter not implemented in repository yet
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(MapToCategoryDTO);
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO createCategoryDTO)
        {
            var category = new Category
            {
                CategoryName = createCategoryDTO.CategoryName,
                IsDeleted = false
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToCategoryDTO(category);
        }

        public async Task<CategoryDTO?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDTO updateCategoryDTO)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null) return null;

            category.CategoryName = updateCategoryDTO.CategoryName;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToCategoryDTO(category);
        }

        public async Task<bool> SoftDeleteCategoryAsync(Guid categoryId)
        {
            await _categoryRepository.SoftDeleteAsync(categoryId);
            return true;
        }

        public async Task<bool> RestoreCategory(Guid categoryId)
        {
            return await _categoryRepository.RestoreAsync(categoryId);
        }

        private static CategoryDTO MapToCategoryDTO(Category category)
        {
            return new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }
    }
}

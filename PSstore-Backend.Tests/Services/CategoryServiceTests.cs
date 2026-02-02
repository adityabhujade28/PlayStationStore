using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class CategoryServiceTests : IntegrationTestBase
    {
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryService = new CategoryService(CategoryRepository);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Action", IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "Adventure", IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), CategoryName = "RPG", IsDeleted = false }
            };

            await Context.Categories.AddRangeAsync(categories);
            await Context.SaveChangesAsync();

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(c => c.CategoryName).Should().Contain(new[] { "Action", "Adventure", "RPG" });
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Action",
                IsDeleted = false
            };

            await Context.Categories.AddAsync(category);
            await Context.SaveChangesAsync();

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryId.Should().Be(categoryId);
            result.CategoryName.Should().Be("Action");
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateCategory()
        {
            // Arrange
            var createDTO = new CreateCategoryDTO { CategoryName = "Puzzle" };

            // Act
            var result = await _categoryService.CreateCategoryAsync(createDTO);

            // Assert
            result.Should().NotBeNull();
            result.CategoryName.Should().Be("Puzzle");

            var saved = await Context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Puzzle");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdate_WhenExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Old Name",
                IsDeleted = false
            };

            await Context.Categories.AddAsync(category);
            await Context.SaveChangesAsync();

            var updateDTO = new UpdateCategoryDTO { CategoryName = "New Name" };

            // Act
            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDTO);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().Be("New Name");

            var updated = await Context.Categories.FindAsync(categoryId);
            updated!.CategoryName.Should().Be("New Name");
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateDTO = new UpdateCategoryDTO { CategoryName = "New Name" };

            // Act
            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteCategoryAsync_ShouldMarkAsDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Action",
                IsDeleted = false
            };

            await Context.Categories.AddAsync(category);
            await Context.SaveChangesAsync();

            // Act
            var result = await _categoryService.SoftDeleteCategoryAsync(categoryId);

            // Assert
            result.Should().BeTrue();

            var deleted = await Context.Categories.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreCategoryAsync_ShouldRestoreDeletedCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = "Action",
                IsDeleted = true
            };

            await Context.Categories.AddAsync(category);
            await Context.SaveChangesAsync();

            // Act
            var result = await _categoryService.RestoreCategory(categoryId);

            // Assert
            result.Should().BeTrue();

            var restored = await Context.Categories.FindAsync(categoryId);
            restored.Should().NotBeNull();
            restored!.IsDeleted.Should().BeFalse();
        }
    }
}

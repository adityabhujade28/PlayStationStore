using Bogus;
using FluentAssertions;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryService _categoryService;
        private readonly Faker _faker;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object);
            _faker = new Faker();
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new Faker<Category>()
                .RuleFor(c => c.CategoryId, f => Guid.NewGuid())
                .RuleFor(c => c.CategoryName, f => f.Commerce.Categories(1)[0])
                .Generate(3);

            _mockCategoryRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.First().CategoryName.Should().Be(categories.First().CategoryName);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Faker<Category>()
                .RuleFor(c => c.CategoryId, categoryId)
                .RuleFor(c => c.CategoryName, f => f.Commerce.Categories(1)[0])
                .Generate();

            _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryId.Should().Be(categoryId);
            result.CategoryName.Should().Be(category.CategoryName);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateAndReturnCategory()
        {
            // Arrange
            var createDTO = new Faker<CreateCategoryDTO>()
                .RuleFor(d => d.CategoryName, f => f.Commerce.Categories(1)[0])
                .Generate();

            // Act
            var result = await _categoryService.CreateCategoryAsync(createDTO);

            // Assert
            result.Should().NotBeNull();
            result.CategoryName.Should().Be(createDTO.CategoryName);

            _mockCategoryRepository.Verify(r => r.AddAsync(It.Is<Category>(c => 
                c.CategoryName == createDTO.CategoryName && 
                !c.IsDeleted
            )), Times.Once);
            _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldUpdateAndReturnCategory_WhenExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateDTO = new Faker<UpdateCategoryDTO>()
                .RuleFor(d => d.CategoryName, f => "Updated " + f.Commerce.Categories(1)[0])
                .Generate();

            var existingCategory = new Faker<Category>()
                .RuleFor(c => c.CategoryId, categoryId)
                .RuleFor(c => c.CategoryName, "Original Name")
                .Generate();

            _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDTO);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().Be(updateDTO.CategoryName);
            
            _mockCategoryRepository.Verify(r => r.Update(It.Is<Category>(c => 
                c.CategoryId == categoryId && 
                c.CategoryName == updateDTO.CategoryName
            )), Times.Once);
            _mockCategoryRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnNull_WhenDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateDTO = new UpdateCategoryDTO { CategoryName = "New Name" };

            _mockCategoryRepository.Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteCategoryAsync_ShouldCallRepository()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockCategoryRepository.Setup(r => r.SoftDeleteAsync(categoryId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.SoftDeleteCategoryAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockCategoryRepository.Verify(r => r.SoftDeleteAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task RestoreCategory_ShouldCallRepository()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockCategoryRepository.Setup(r => r.RestoreAsync(categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _categoryService.RestoreCategory(categoryId);

            // Assert
            result.Should().BeTrue();
            _mockCategoryRepository.Verify(r => r.RestoreAsync(categoryId), Times.Once);
        }
    }
}

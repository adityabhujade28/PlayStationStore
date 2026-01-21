using Bogus;
using FluentAssertions;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepo;
        private readonly Mock<IEntitlementService> _mockEntitlementService;
        private readonly Mock<IGameCountryRepository> _mockGameCountryRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly GameService _gameService;
        private readonly Faker _faker;

        public GameServiceTests()
        {
            _mockGameRepo = new Mock<IGameRepository>();
            _mockEntitlementService = new Mock<IEntitlementService>();
            _mockGameCountryRepo = new Mock<IGameCountryRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _gameService = new GameService(
                _mockGameRepo.Object,
                _mockEntitlementService.Object,
                _mockGameCountryRepo.Object,
                _mockUserRepo.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldReturnGame_WhenExists()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Faker<Game>()
                .RuleFor(g => g.GameId, gameId)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .RuleFor(g => g.BasePrice, 50m)
                .Generate();

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _gameService.GetGameByIdAsync(gameId);

            // Assert
            result.Should().NotBeNull();
            result!.GameId.Should().Be(gameId);
            result.Price.Should().Be(50m);
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldReturnNull_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetGameByIdAsync(gameId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldApplyRegionalPricing_WhenUserCountryProvided()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var countryId = Guid.Parse("20000000-0000-0000-0000-000000000001");
            
            var game = new Game { GameId = gameId, BasePrice = 50m, FreeToPlay = false };
            var user = new User { UserId = userId, CountryId = countryId };
            var gameCountry = new GameCountry { GameId = gameId, CountryId = countryId, Price = 25m }; // Regional price

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameCountryRepo.Setup(r => r.GetGamePricingAsync(gameId, countryId)).ReturnsAsync(gameCountry);

            // Act
            var result = await _gameService.GetGameByIdAsync(gameId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.Price.Should().Be(25m); // Should be regional price, not base price
        }

        [Fact]
        public async Task GetGameWithAccessAsync_ShouldReturnDetailsWithAccessInfo()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { GameId = gameId, GameName = "Test Game", BasePrice = 60m };
            
            var accessResult = new GameAccessResultDTO { CanAccess = true, AccessType = "PURCHASED" };

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockEntitlementService.Setup(s => s.CanUserAccessGameAsync(userId, gameId)).ReturnsAsync(accessResult);
            
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _gameService.GetGameWithAccessAsync(gameId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.GameName.Should().Be("Test Game");
            result.CanAccess.Should().BeTrue();
            result.AccessType.Should().Be("PURCHASED");
        }

        [Fact]
        public async Task GetGameWithAccessAsync_ShouldReturnNull_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetGameWithAccessAsync(gameId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllGamesAsync_ShouldReturnMappedGames()
        {
            // Arrange
            var games = new Faker<Game>()
                .RuleFor(g => g.GameId, Guid.NewGuid)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .Generate(3);

            _mockGameRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

            // Act
            var result = await _gameService.GetAllGamesAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateGame()
        {
            // Arrange
            var createDTO = new Faker<CreateGameDTO>()
                .RuleFor(d => d.GameName, f => f.Commerce.ProductName())
                .RuleFor(d => d.Price, 40m)
                .Generate();

            // Act
            var result = await _gameService.CreateGameAsync(createDTO);

            // Assert
            result.Should().NotBeNull();
            result.GameName.Should().Be(createDTO.GameName);

            _mockGameRepo.Verify(r => r.AddAsync(It.Is<Game>(g => 
                g.GameName == createDTO.GameName && 
                g.BasePrice == createDTO.Price
            )), Times.Once);
            _mockGameRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldUpdate_WhenGameExists()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var updateDTO = new UpdateGameDTO { GameName = "Updated Title" };
            var existingGame = new Game { GameId = gameId, GameName = "Old Title" };

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(existingGame);

            // Act
            var result = await _gameService.UpdateGameAsync(gameId, updateDTO);

            // Assert
            result.Should().NotBeNull();
            result!.GameName.Should().Be("Updated Title");
            
            _mockGameRepo.Verify(r => r.Update(existingGame), Times.Once);
            _mockGameRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldReturnNull_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var updateDTO = new UpdateGameDTO();
            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.UpdateGameAsync(gameId, updateDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteGameAsync_ShouldCallRepository()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            
            // Act
            var result = await _gameService.SoftDeleteGameAsync(gameId);

            // Assert
            result.Should().BeTrue();
            _mockGameRepo.Verify(r => r.SoftDeleteAsync(gameId), Times.Once);
        }

        [Fact]
        public async Task RestoreGameAsync_ShouldCallRepository()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _mockGameRepo.Setup(r => r.RestoreAsync(gameId)).ReturnsAsync(true);

            // Act
            var result = await _gameService.RestoreGameAsync(gameId);

            // Assert
            result.Should().BeTrue();
            _mockGameRepo.Verify(r => r.RestoreAsync(gameId), Times.Once);
        }
        [Fact]
        public async Task SearchGamesAsync_ShouldReturnMatchingGames()
        {
            // Arrange
            var searchTerm = "Action";
            var games = new Faker<Game>()
                .RuleFor(g => g.GameId, Guid.NewGuid)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .Generate(2);

            _mockGameRepo.Setup(r => r.SearchGamesAsync(searchTerm)).ReturnsAsync(games);

            // Act
            var result = await _gameService.SearchGamesAsync(searchTerm);

            // Assert
            result.Should().HaveCount(2);
            _mockGameRepo.Verify(r => r.SearchGamesAsync(searchTerm), Times.Once);
        }

        [Fact]
        public async Task GetGamesByCategoryAsync_ShouldReturnGamesForCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var games = new Faker<Game>()
                .RuleFor(g => g.GameId, Guid.NewGuid)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .Generate(3);

            _mockGameRepo.Setup(r => r.GetGamesByCategoryAsync(categoryId)).ReturnsAsync(games);

            // Act
            var result = await _gameService.GetGamesByCategoryAsync(categoryId);

            // Assert
            result.Should().HaveCount(3);
            _mockGameRepo.Verify(r => r.GetGamesByCategoryAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetFreeToPlayGamesAsync_ShouldReturnFreeGames()
        {
            // Arrange
            var games = new Faker<Game>()
                .RuleFor(g => g.GameId, Guid.NewGuid)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .RuleFor(g => g.FreeToPlay, true)
                .Generate(2);

            _mockGameRepo.Setup(r => r.GetFreeGamesAsync()).ReturnsAsync(games);

            // Act
            var result = await _gameService.GetFreeToPlayGamesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(g => g.FreeToPlay).Should().BeTrue();
            _mockGameRepo.Verify(r => r.GetFreeGamesAsync(), Times.Once);
        }
    }
}

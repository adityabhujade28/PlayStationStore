using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class GameServiceTests : IntegrationTestBase
    {
        private readonly GameService _gameService;
        private readonly EntitlementService _entitlementService;

        public GameServiceTests()
        {
            // Create EntitlementService first (needed by GameService)
            _entitlementService = new EntitlementService(
                Context,
                GameRepository,
                UserPurchaseGameRepository,
                UserSubscriptionPlanRepository,
                SubscriptionPlanRepository
            );

            _gameService = new GameService(
                GameRepository,
                _entitlementService,
                GameCountryRepository,
                UserRepository,
                CountryRepository
            );
        }

        [Fact]
        public async Task GetGameByIdAsync_ShouldReturnGame_WhenExists()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                BasePrice = 50m,
                FreeToPlay = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

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
            var countryId = Guid.NewGuid();

            var country = new Country
            {
                CountryId = countryId,
                CountryName = "Test Country",
                Currency = "USD",
                RegionId = Guid.NewGuid()
            };

            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                BasePrice = 50m,
                FreeToPlay = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = countryId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var gameCountry = new GameCountry
            {
                GameCountryId = Guid.NewGuid(),
                GameId = gameId,
                CountryId = countryId,
                Price = 25m // Regional price
            };

            await Context.Countries.AddAsync(country);
            await Context.Games.AddAsync(game);
            await Context.Users.AddAsync(user);
            await Context.GameCountries.AddAsync(gameCountry);
            await Context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetGameByIdAsync(gameId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.Price.Should().Be(25m); // Should be regional price, not base price
        }

        [Fact]
        public async Task CreateGameAsync_ShouldCreateGame()
        {
            // Arrange
            var createDTO = new CreateGameDTO
            {
                GameName = Faker.Commerce.ProductName(),
                Price = 40m,
                ReleaseDate = DateTime.UtcNow.AddMonths(-1),
                FreeToPlay = false
            };

            // Act
            var result = await _gameService.CreateGameAsync(createDTO);

            // Assert
            result.Should().NotBeNull();
            result.GameName.Should().Be(createDTO.GameName);

            var savedGame = await Context.Games.FirstOrDefaultAsync(g => g.GameName == createDTO.GameName);
            savedGame.Should().NotBeNull();
            savedGame!.BasePrice.Should().Be(createDTO.Price);
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldUpdate_WhenGameExists()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var existingGame = new Game
            {
                GameId = gameId,
                GameName = "Old Title",
                BasePrice = 50m,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Games.AddAsync(existingGame);
            await Context.SaveChangesAsync();

            var updateDTO = new UpdateGameDTO { GameName = "Updated Title" };

            // Act
            var result = await _gameService.UpdateGameAsync(gameId, updateDTO);

            // Assert
            result.Should().NotBeNull();
            result!.GameName.Should().Be("Updated Title");

            var updated = await Context.Games.FindAsync(gameId);
            updated!.GameName.Should().Be("Updated Title");
        }

        [Fact]
        public async Task UpdateGameAsync_ShouldReturnNull_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var updateDTO = new UpdateGameDTO { GameName = "New Title" };

            // Act
            var result = await _gameService.UpdateGameAsync(gameId, updateDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteGameAsync_ShouldMarkAsDeleted()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                BasePrice = 50m,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

            // Act
            var result = await _gameService.SoftDeleteGameAsync(gameId);

            // Assert
            result.Should().BeTrue();

            // Need to query with IgnoreQueryFilters to see soft-deleted entities
            var deleted = await Context.Games.IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.GameId == gameId);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task SearchGamesAsync_ShouldReturnMatchingGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { GameId = Guid.NewGuid(), GameName = "Action Adventure", BasePrice = 50m, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Game { GameId = Guid.NewGuid(), GameName = "Action RPG", BasePrice = 60m, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Game { GameId = Guid.NewGuid(), GameName = "Puzzle Game", BasePrice = 20m, CreatedAt = DateTime.UtcNow, IsDeleted = false }
            };

            await Context.Games.AddRangeAsync(games);
            await Context.SaveChangesAsync();

            // Act
            var result = await _gameService.SearchGamesAsync("Action");

            // Assert
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.All(g => g.GameName.Contains("Action")).Should().BeTrue();
        }

        [Fact]
        public async Task GetFreeToPlayGamesAsync_ShouldReturnFreeGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { GameId = Guid.NewGuid(), GameName = "Free Game 1", FreeToPlay = true, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Game { GameId = Guid.NewGuid(), GameName = "Free Game 2", FreeToPlay = true, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Game { GameId = Guid.NewGuid(), GameName = "Paid Game", BasePrice = 50m, FreeToPlay = false, CreatedAt = DateTime.UtcNow, IsDeleted = false }
            };

            await Context.Games.AddRangeAsync(games);
            await Context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetFreeToPlayGamesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(g => g.FreeToPlay).Should().BeTrue();
        }
    }
}

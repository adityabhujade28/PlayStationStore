using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class PurchaseServiceTests : IntegrationTestBase
    {
        private readonly PurchaseService _purchaseService;
        private readonly EntitlementService _entitlementService;

        public PurchaseServiceTests()
        {
            _entitlementService = new EntitlementService(
                Context,
                GameRepository,
                UserPurchaseGameRepository,
                UserSubscriptionPlanRepository,
                SubscriptionPlanRepository
            );

            _purchaseService = new PurchaseService(
                UserPurchaseGameRepository,
                GameRepository,
                UserRepository,
                _entitlementService
            );
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldCreatePurchase_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                BasePrice = 59.99m,
                FreeToPlay = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Should().NotBeNull();
            result.GameId.Should().Be(gameId);
            result.PurchasePrice.Should().Be(59.99m);

            var savedPurchase = await Context.UserPurchaseGames
                .FirstOrDefaultAsync(p => p.UserId == userId && p.GameId == gameId);
            savedPurchase.Should().NotBeNull();
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnError_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var purchaseDTO = new CreatePurchaseDTO { GameId = Guid.NewGuid() };

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnError_WhenGameNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            var purchaseDTO = new CreatePurchaseDTO { GameId = Guid.NewGuid() };

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Game not found.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnError_WhenGameIsFreeToPlay()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                FreeToPlay = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("free to play");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnError_WhenAlreadyPurchased()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var game = new Game
            {
                GameId = gameId,
                GameName = Faker.Commerce.ProductName(),
                BasePrice = 59.99m,
                FreeToPlay = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var existingPurchase = new UserPurchaseGame
            {
                PurchaseId = Guid.NewGuid(),
                UserId = userId,
                GameId = gameId,
                PurchasePrice = 59.99m,
                PurchaseDate = DateTime.UtcNow.AddDays(-10)
            };

            await Context.Users.AddAsync(user);
            await Context.Games.AddAsync(game);
            await Context.UserPurchaseGames.AddAsync(existingPurchase);
            await Context.SaveChangesAsync();

            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already own");
        }

        [Fact]
        public async Task GetUserPurchaseHistoryAsync_ShouldReturnPurchases()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();

            var game1 = new Game { GameId = gameId1, GameName = "Game 1", BasePrice = 50m, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var game2 = new Game { GameId = gameId2, GameName = "Game 2", BasePrice = 60m, CreatedAt = DateTime.UtcNow, IsDeleted = false };

            var purchases = new List<UserPurchaseGame>
            {
                new UserPurchaseGame { PurchaseId = Guid.NewGuid(), UserId = userId, GameId = gameId1, PurchasePrice = 50m, PurchaseDate = DateTime.UtcNow.AddDays(-5) },
                new UserPurchaseGame { PurchaseId = Guid.NewGuid(), UserId = userId, GameId = gameId2, PurchasePrice = 60m, PurchaseDate = DateTime.UtcNow.AddDays(-2) }
            };

            await Context.Games.AddRangeAsync(new[] { game1, game2 });
            await Context.UserPurchaseGames.AddRangeAsync(purchases);
            await Context.SaveChangesAsync();

            // Act
            var result = await _purchaseService.GetUserPurchaseHistoryAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.GameName == "Game 1");
            result.Should().Contain(p => p.GameName == "Game 2");
        }

        [Fact]
        public async Task GetPurchaseDetailsAsync_ShouldReturnDetails_WhenExists()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var game = new Game { GameId = gameId, GameName = "Test Game", BasePrice = 50m, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var purchase = new UserPurchaseGame
            {
                PurchaseId = purchaseId,
                UserId = userId,
                GameId = gameId,
                PurchasePrice = 50m,
                PurchaseDate = DateTime.UtcNow
            };

            await Context.Users.AddAsync(user);
            await Context.Games.AddAsync(game);
            await Context.UserPurchaseGames.AddAsync(purchase);
            await Context.SaveChangesAsync();

            // Act
            var result = await _purchaseService.GetPurchaseDetailsAsync(purchaseId);

            // Assert
            result.Should().NotBeNull();
            result!.PurchaseId.Should().Be(purchaseId);
            result.GameName.Should().Be("Test Game");
        }

        [Fact]
        public async Task GetPurchaseDetailsAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();

            // Act
            var result = await _purchaseService.GetPurchaseDetailsAsync(purchaseId);

            // Assert
            result.Should().BeNull();
        }
    }
}

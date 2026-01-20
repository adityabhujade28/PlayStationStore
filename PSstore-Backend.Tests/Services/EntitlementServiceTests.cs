using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PSstore.Data;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class EntitlementServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepo;
        private readonly Mock<IUserPurchaseGameRepository> _mockPurchaseRepo;
        private readonly Mock<IUserSubscriptionPlanRepository> _mockUserSubRepo;
        private readonly Mock<ISubscriptionPlanRepository> _mockPlanRepo;
        private readonly AppDbContext _context;
        private readonly EntitlementService _entitlementService;
        private readonly Faker _faker;

        public EntitlementServiceTests()
        {
            _mockGameRepo = new Mock<IGameRepository>();
            _mockPurchaseRepo = new Mock<IUserPurchaseGameRepository>();
            _mockUserSubRepo = new Mock<IUserSubscriptionPlanRepository>();
            _mockPlanRepo = new Mock<ISubscriptionPlanRepository>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _entitlementService = new EntitlementService(
                _context,
                _mockGameRepo.Object,
                _mockPurchaseRepo.Object,
                _mockUserSubRepo.Object,
                _mockPlanRepo.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnNoAccess_WhenGameNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            // Assert
            result.CanAccess.Should().BeFalse();
            result.AccessType.Should().Be("NO_ACCESS");
            result.Message.Should().Be("Game not found");
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnFree_WhenGameIsFree()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, GameName = "Free Game", FreeToPlay = true };

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            // Assert
            result.CanAccess.Should().BeTrue();
            result.AccessType.Should().Be("FREE");
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnPurchased_WhenUserOwnsGame()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, GameName = "Paid Game", FreeToPlay = false };
            var purchaseDate = DateTime.UtcNow.AddDays(-10);

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepo.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(true);

            // Add purchase to InMemory DB for direct context access
            _context.UserPurchaseGames.Add(new UserPurchaseGame 
            { 
                PurchaseId = Guid.NewGuid(), 
                UserId = userId, 
                GameId = gameId, 
                PurchaseDate = purchaseDate 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            // Assert
            result.CanAccess.Should().BeTrue();
            result.AccessType.Should().Be("PURCHASED");
            result.PurchasedOn.Should().BeCloseTo(purchaseDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnSubscription_WhenIncludedInPlan()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var game = new Game { GameId = gameId, GameName = "Sub Game", FreeToPlay = false };

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepo.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(false);

            // Create correctly structured UserSubscriptionPlan Model (Not DTO)
            var activeSub = new UserSubscriptionPlan 
            { 
                UserId = userId,
                PlanEndDate = DateTime.UtcNow.AddDays(30),
                SubscriptionPlanCountry = new SubscriptionPlanCountry 
                { 
                    SubscriptionId = subscriptionId,
                    SubscriptionPlan = new SubscriptionPlan { SubscriptionId = subscriptionId, SubscriptionType = "Premium" }
                }
            };
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);

            // Add game to subscription plan in InMemory DB
            _context.GameSubscriptions.Add(new GameSubscription 
            { 
                GameId = gameId, 
                SubscriptionId = subscriptionId 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            // Assert
            result.CanAccess.Should().BeTrue();
            result.AccessType.Should().Be("SUBSCRIPTION");
            result.SubscriptionPlan.Should().Be("Premium");
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnNoAccess_WhenNotFreeNotPurchasedNotSubscribed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, GameName = "Paid Game", FreeToPlay = false };

            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepo.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(false);
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((UserSubscriptionPlan?)null);

            // Act
            var result = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            // Assert
            result.CanAccess.Should().BeFalse();
            result.AccessType.Should().Be("NO_ACCESS");
        }

        [Fact]
        public async Task HasAnyEntitlementsAsync_ShouldReturnTrue_WhenPurchasesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.UserPurchaseGames.Add(new UserPurchaseGame { UserId = userId, GameId = Guid.NewGuid() });
            await _context.SaveChangesAsync();

            // Act
            var result = await _entitlementService.HasAnyEntitlementsAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasAnyEntitlementsAsync_ShouldReturnTrue_WhenActiveSubscriptionExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserSubRepo.Setup(r => r.HasActiveSubscriptionAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _entitlementService.HasAnyEntitlementsAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasAnyEntitlementsAsync_ShouldReturnFalse_WhenNoEntitlements()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserSubRepo.Setup(r => r.HasActiveSubscriptionAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _entitlementService.HasAnyEntitlementsAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserLibraryAsync_ShouldConsolidateAllAccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var freeGameId = Guid.NewGuid();
            var purchasedGameId = Guid.NewGuid();
            var subGameId = Guid.NewGuid();

            // Mock Data
            var purchasedGame = new Game { GameId = purchasedGameId, GameName = "Purchased Game" };
            var freeGame = new Game { GameId = freeGameId, GameName = "Free Game" };
            var subGame = new Game { GameId = subGameId, GameName = "Sub Game" };

            // 1. Mock Purchased Ids & Dates
            var purchases = new List<UserPurchaseGame> 
            { 
                new UserPurchaseGame { GameId = purchasedGameId, PurchaseDate = DateTime.UtcNow } 
            };
            _mockPurchaseRepo.Setup(r => r.GetPurchasedGameIdsAsync(userId)).ReturnsAsync(new List<Guid> { purchasedGameId });
            _mockPurchaseRepo.Setup(r => r.GetUserPurchasesAsync(userId)).ReturnsAsync(purchases);

            // 2. Mock Free Ids
            _mockGameRepo.Setup(r => r.GetFreeGameIdsAsync()).ReturnsAsync(new List<Guid> { freeGameId });

            // 3. Mock Subscription Ids
            var subscriptionId = Guid.NewGuid();
            
            // Use correct Model here
            var activeSub = new UserSubscriptionPlan 
            { 
                SubscriptionPlanCountry = new SubscriptionPlanCountry 
                { 
                    SubscriptionId = subscriptionId,
                    SubscriptionPlan = new SubscriptionPlan { SubscriptionType = "Extra" }
                }
            };
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);
            
            var planWithGames = new SubscriptionPlan 
            { 
                GameSubscriptions = new List<GameSubscription> 
                { 
                    new GameSubscription { GameId = subGameId } 
                } 
            };
            _mockPlanRepo.Setup(r => r.GetPlanWithGamesAsync(subscriptionId)).ReturnsAsync(planWithGames);

            // 4. Mock Game Details retrieval
            _mockGameRepo.Setup(r => r.GetGamesByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<Game> { purchasedGame, freeGame, subGame });

            // Act
            var result = await _entitlementService.GetUserLibraryAsync(userId);

            // Assert
            result.AccessibleGames.Should().Contain(g => g.GameId == purchasedGameId && g.AccessType == "PURCHASED");
            result.AccessibleGames.Should().Contain(g => g.GameId == freeGameId && g.AccessType == "FREE");
            result.AccessibleGames.Should().Contain(g => g.GameId == subGameId && g.AccessType == "SUBSCRIPTION");
            result.TotalPurchasedGames.Should().Be(1);
            result.TotalFreeGames.Should().Be(1);
            result.TotalSubscriptionGames.Should().Be(1);
        }

        [Fact]
        public async Task GetSubscriptionGamesAsync_ShouldReturnGamesForPlan()
        {
            // Arrange
            var subId = Guid.NewGuid();
            var game = new Game 
            { 
                GameId = Guid.NewGuid(), 
                GameName = "Included Game", 
                GameCategories = new List<GameCategory> 
                { 
                    new GameCategory { Category = new Category { CategoryName = "Action" } } 
                } 
            };

            _context.GameSubscriptions.Add(new GameSubscription 
            { 
                SubscriptionId = subId, 
                Game = game 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _entitlementService.GetSubscriptionGamesAsync(subId);

            // Assert
            result.Should().HaveCount(1);
            result.First().GameName.Should().Be("Included Game");
            result.First().Categories.Should().Contain("Action");
        }
    }
}

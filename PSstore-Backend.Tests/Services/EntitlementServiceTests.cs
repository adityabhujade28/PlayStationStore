using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class EntitlementServiceTests : IntegrationTestBase
    {
        private readonly EntitlementService _entitlementService;

        public EntitlementServiceTests()
        {
            _entitlementService = new EntitlementService(
                Context,
                GameRepository,
                UserPurchaseGameRepository,
                UserSubscriptionPlanRepository,
                SubscriptionPlanRepository
            );
        }

        [Fact]
        public async Task CanUserAccessGameAsync_ShouldReturnNoAccess_WhenGameNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

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
            var game = new Game { GameId = gameId, GameName = "Free Game", FreeToPlay = true, CreatedAt = DateTime.UtcNow, IsDeleted = false };

            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

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
            var game = new Game { GameId = gameId, GameName = "Paid Game", FreeToPlay = false, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var purchaseDate = DateTime.UtcNow.AddDays(-10);

            var purchase = new UserPurchaseGame 
            { 
                PurchaseId = Guid.NewGuid(), 
                UserId = userId, 
                GameId = gameId, 
                PurchaseDate = purchaseDate,
                PurchasePrice = 50m
            };

            await Context.Games.AddAsync(game);
            await Context.UserPurchaseGames.AddAsync(purchase);
            await Context.SaveChangesAsync();

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
            var planCountryId = Guid.NewGuid();

            var game = new Game { GameId = gameId, GameName = "Sub Game", FreeToPlay = false, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            
            var subscriptionPlan = new SubscriptionPlan 
            { 
                SubscriptionId = subscriptionId, 
                SubscriptionType = "Premium"
            };

            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subscriptionId,
                CountryId = Guid.NewGuid(),
                DurationMonths = 12,
                Price = 60m
            };

            var activeSub = new UserSubscriptionPlan 
            { 
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                PlanStartDate = DateTime.UtcNow.AddDays(-10),
                PlanEndDate = DateTime.UtcNow.AddDays(30),
                SubscriptionPlanCountryId = planCountryId
            };

            var gameSubscription = new GameSubscription 
            { 
                GameId = gameId, 
                SubscriptionId = subscriptionId 
            };

            await Context.Games.AddAsync(game);
            await Context.SubscriptionPlans.AddAsync(subscriptionPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.GameSubscriptions.AddAsync(gameSubscription);
            await Context.SaveChangesAsync();

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
            var game = new Game { GameId = gameId, GameName = "Paid Game", FreeToPlay = false, CreatedAt = DateTime.UtcNow, IsDeleted = false };

            await Context.Games.AddAsync(game);
            await Context.SaveChangesAsync();

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
            var purchase = new UserPurchaseGame 
            { 
                PurchaseId = Guid.NewGuid(),
                UserId = userId, 
                GameId = Guid.NewGuid(),
                PurchasePrice = 50m,
                PurchaseDate = DateTime.UtcNow
            };

            await Context.UserPurchaseGames.AddAsync(purchase);
            await Context.SaveChangesAsync();

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
            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanCountryId = Guid.NewGuid(),
                PlanStartDate = DateTime.UtcNow.AddDays(-10),
                PlanEndDate = DateTime.UtcNow.AddDays(30)
            };

            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.SaveChangesAsync();

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
            var subscriptionId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();

            var purchasedGame = new Game { GameId = purchasedGameId, GameName = "Purchased Game", BasePrice = 50m, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var freeGame = new Game { GameId = freeGameId, GameName = "Free Game", FreeToPlay = true, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var subGame = new Game { GameId = subGameId, GameName = "Sub Game", BasePrice = 60m, CreatedAt = DateTime.UtcNow, IsDeleted = false };

            var purchase = new UserPurchaseGame 
            { 
                PurchaseId = Guid.NewGuid(),
                GameId = purchasedGameId, 
                UserId = userId,
                PurchaseDate = DateTime.UtcNow,
                PurchasePrice = 50m
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                SubscriptionId = subscriptionId,
                SubscriptionType = "Extra"
            };

            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subscriptionId,
                CountryId = Guid.NewGuid(),
                DurationMonths = 12,
                Price = 60m
            };

            var activeSub = new UserSubscriptionPlan 
            { 
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanCountryId = planCountryId,
                PlanStartDate = DateTime.UtcNow.AddDays(-10),
                PlanEndDate = DateTime.UtcNow.AddDays(30)
            };

            var gameSubscription = new GameSubscription 
            { 
                GameId = subGameId, 
                SubscriptionId = subscriptionId 
            };

            await Context.Games.AddRangeAsync(new[] { purchasedGame, freeGame, subGame });
            await Context.UserPurchaseGames.AddAsync(purchase);
            await Context.SubscriptionPlans.AddAsync(subscriptionPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.GameSubscriptions.AddAsync(gameSubscription);
            await Context.SaveChangesAsync();

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
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var category = new Category { CategoryId = categoryId, CategoryName = "Action", IsDeleted = false };
            var game = new Game 
            { 
                GameId = gameId, 
                GameName = "Included Game",
                BasePrice = 50m,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var gameCategory = new GameCategory 
            { 
                GameId = gameId,
                CategoryId = categoryId
            };

            var gameSubscription = new GameSubscription 
            { 
                SubscriptionId = subId, 
                GameId = gameId
            };

            await Context.Categories.AddAsync(category);
            await Context.Games.AddAsync(game);
            await Context.GameCategories.AddAsync(gameCategory);
            await Context.GameSubscriptions.AddAsync(gameSubscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _entitlementService.GetSubscriptionGamesAsync(subId);

            // Assert
            result.Should().HaveCount(1);
            result.First().GameName.Should().Be("Included Game");
            result.First().Categories.Should().Contain("Action");
        }
    }
}

using Bogus;
using FluentAssertions;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class PurchaseServiceTests
    {
        private readonly Mock<IUserPurchaseGameRepository> _mockPurchaseRepository;
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEntitlementService> _mockEntitlementService;
        private readonly PurchaseService _purchaseService;
        private readonly Faker _faker;

        public PurchaseServiceTests()
        {
            _mockPurchaseRepository = new Mock<IUserPurchaseGameRepository>();
            _mockGameRepository = new Mock<IGameRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEntitlementService = new Mock<IEntitlementService>();
            
            _purchaseService = new PurchaseService(
                _mockPurchaseRepository.Object,
                _mockGameRepository.Object,
                _mockUserRepository.Object,
                _mockEntitlementService.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnSuccess_WhenPurchaseIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            
            var purchaseDTO = new Faker<CreatePurchaseDTO>()
                .RuleFor(d => d.GameId, gameId)
                .Generate();

            var user = new Faker<User>()
                .RuleFor(u => u.UserId, userId)
                .Generate();

            var game = new Faker<Game>()
                .RuleFor(g => g.GameId, gameId)
                .RuleFor(g => g.GameName, f => f.Commerce.ProductName())
                .RuleFor(g => g.BasePrice, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(g => g.FreeToPlay, false)
                .Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameRepository.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepository.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(false);
            _mockEntitlementService.Setup(s => s.CanUserAccessGameAsync(userId, gameId))
                .ReturnsAsync(new GameAccessResultDTO { CanAccess = false });

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Game purchased successfully!");
            result.GameName.Should().Be(game.GameName);
            result.PurchasePrice.Should().Be(game.BasePrice!.Value);
            
            _mockPurchaseRepository.Verify(r => r.AddAsync(It.IsAny<UserPurchaseGame>()), Times.Once);
            _mockPurchaseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var purchaseDTO = new Faker<CreatePurchaseDTO>().Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnFailure_WhenGameNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var purchaseDTO = new Faker<CreatePurchaseDTO>().Generate();
            var user = new Faker<User>().Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameRepository.Setup(r => r.GetByIdAsync(purchaseDTO.GameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Game not found.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnFailure_WhenGameIsFreeToPlay()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };
            var user = new Faker<User>().Generate();
            var game = new Faker<Game>()
                .RuleFor(g => g.GameId, gameId)
                .RuleFor(g => g.FreeToPlay, true) // Free game
                .Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameRepository.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("This game is free to play. No purchase required.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnFailure_WhenGameAlreadyPurchased()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };
            var user = new Faker<User>().Generate();
            var game = new Faker<Game>()
                .RuleFor(g => g.GameId, gameId)
                .RuleFor(g => g.FreeToPlay, false)
                .Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameRepository.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepository.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(true); // Already purchased

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You already own this game.");
        }

        [Fact]
        public async Task PurchaseGameAsync_ShouldReturnFailure_WhenGameAccessibleViaSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var purchaseDTO = new CreatePurchaseDTO { GameId = gameId };
            var user = new Faker<User>().Generate();
            var game = new Faker<Game>()
                .RuleFor(g => g.GameId, gameId)
                .RuleFor(g => g.FreeToPlay, false)
                .Generate();

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockGameRepository.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseRepository.Setup(r => r.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(false);
            
            // Accessible via subscription
            _mockEntitlementService.Setup(s => s.CanUserAccessGameAsync(userId, gameId))
                .ReturnsAsync(new GameAccessResultDTO { CanAccess = true, AccessType = "SUBSCRIPTION" });

            // Act
            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("This game is already accessible through your subscription. No purchase needed.");
        }

        [Fact]
        public async Task GetUserPurchaseHistoryAsync_ShouldReturnHistory()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            // Generate dynamic list of purchases
            var purchases = new Faker<UserPurchaseGame>()
                .RuleFor(p => p.UserId, userId)
                .RuleFor(p => p.GameId, Guid.NewGuid)
                .RuleFor(p => p.PurchasePrice, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past())
                .RuleFor(p => p.Game, f => new Game { GameName = f.Commerce.ProductName() })
                .Generate(3);

            _mockPurchaseRepository.Setup(r => r.GetUserPurchasesAsync(userId))
                .ReturnsAsync(purchases);

            // Act
            var result = await _purchaseService.GetUserPurchaseHistoryAsync(userId);

            // Assert
            result.Should().HaveCount(3);
            result.First().GameName.Should().Be(purchases.First().Game!.GameName);
        }

        [Fact]
        public async Task HasUserPurchasedGameAsync_ShouldReturnTrue_WhenUserHasPurchasedGame()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            _mockPurchaseRepository
                .Setup(r => r.HasUserPurchasedGameAsync(userId, gameId))
                .ReturnsAsync(true);

            // Act
            var result = await _purchaseService.HasUserPurchasedGameAsync(userId, gameId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetPurchaseDetailsAsync_ShouldReturnNull_WhenPurchaseNotFound()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();

            _mockPurchaseRepository
                .Setup(r => r.GetPurchaseDetailsAsync(purchaseId))
                .ReturnsAsync((UserPurchaseGame?)null);

            // Act
            var result = await _purchaseService.GetPurchaseDetailsAsync(purchaseId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPurchaseDetailsAsync_ShouldReturnPurchaseDetails_WhenPurchaseExists()
        {
            // Arrange
            var purchaseId = Guid.NewGuid();

            var purchase = new Faker<UserPurchaseGame>()
                .RuleFor(p => p.PurchaseId, purchaseId)
                .RuleFor(p => p.PurchasePrice, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(p => p.PurchaseDate, f => f.Date.Recent())
                .RuleFor(p => p.Game, f => new Game { GameName = f.Commerce.ProductName() })
                .Generate();

            _mockPurchaseRepository
                .Setup(r => r.GetPurchaseDetailsAsync(purchaseId))
                .ReturnsAsync(purchase);

            // Act
            var result = await _purchaseService.GetPurchaseDetailsAsync(purchaseId);

            // Assert
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.GameName.Should().Be(purchase.Game!.GameName);
            result.PurchasePrice.Should().Be(purchase.PurchasePrice);
            result.Message.Should().Be("Purchase found.");
        }

    }
}

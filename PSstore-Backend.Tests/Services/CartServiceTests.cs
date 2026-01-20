using Bogus;
using FluentAssertions;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _mockCartRepo;
        private readonly Mock<ICartItemRepository> _mockCartItemRepo;
        private readonly Mock<IGameRepository> _mockGameRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPurchaseService> _mockPurchaseService;
        private readonly Mock<IEntitlementService> _mockEntitlementService;
        private readonly CartService _cartService;
        private readonly Faker _faker;

        public CartServiceTests()
        {
            _mockCartRepo = new Mock<ICartRepository>();
            _mockCartItemRepo = new Mock<ICartItemRepository>();
            _mockGameRepo = new Mock<IGameRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPurchaseService = new Mock<IPurchaseService>();
            _mockEntitlementService = new Mock<IEntitlementService>();

            _cartService = new CartService(
                _mockCartRepo.Object,
                _mockCartItemRepo.Object,
                _mockGameRepo.Object,
                _mockUserRepo.Object,
                _mockPurchaseService.Object,
                _mockEntitlementService.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldAddItem_WhenComputedTotalIsCorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var dto = new CreateCartItemDTO { GameId = gameId, Quantity = 1 };

            var game = new Game { GameId = gameId, GameName = "Test Game", BasePrice = 50m, FreeToPlay = false };
            var cart = new Cart { CartId = cartId, UserId = userId, CartItems = new List<CartItem>() };
            
            // Mock Repos
            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cartId)).ReturnsAsync(cart);
            _mockGameRepo.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _mockPurchaseService.Setup(p => p.HasUserPurchasedGameAsync(userId, gameId)).ReturnsAsync(false);
            _mockEntitlementService.Setup(e => e.CanUserAccessGameAsync(userId, gameId)).ReturnsAsync(new GameAccessResultDTO { CanAccess = false });

            // Prepare validation for CalculateCartTotalAsync logic
            // When CalculateCartTotalAsync is called, it fetches items from repo. 
            // We simulate that the new item is now in the repo (or we mock the get call to return what we expect)
            var expectedItems = new List<CartItem> 
            { 
                new CartItem { GameId = gameId, Quantity = 1, UnitPrice = 50m, TotalPrice = 50m } 
            };
            _mockCartItemRepo.Setup(r => r.GetCartItemsAsync(cartId)).ReturnsAsync(expectedItems);

            // Act
            var result = await _cartService.AddItemToCartAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.TotalPrice.Should().Be(50m);
            
            // Verify TotalAmount on cart was updated via CalculateCartTotalAsync
            cart.TotalAmount.Should().Be(50m); 
            
            _mockCartItemRepo.Verify(r => r.AddAsync(It.IsAny<CartItem>()), Times.Once);
            _mockCartRepo.Verify(r => r.Update(cart), Times.Once);
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldThrow_WhenGameIsFreeToPlay()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CreateCartItemDTO { GameId = Guid.NewGuid(), Quantity = 1 };
            var game = new Game { GameId = dto.GameId, FreeToPlay = true };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(new Cart());
            _mockGameRepo.Setup(r => r.GetByIdAsync(dto.GameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = async () => await _cartService.AddItemToCartAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*free-to-play*");
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldThrow_WhenGameAlreadyPurchased()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CreateCartItemDTO { GameId = Guid.NewGuid(), Quantity = 1 };
            var game = new Game { GameId = dto.GameId, FreeToPlay = false };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(new Cart());
            _mockGameRepo.Setup(r => r.GetByIdAsync(dto.GameId)).ReturnsAsync(game);
            _mockPurchaseService.Setup(s => s.HasUserPurchasedGameAsync(userId, dto.GameId)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _cartService.AddItemToCartAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already own*");
        }

        [Fact]
        public async Task RemoveItemFromCartAsync_ShouldRemoveAndRecalculateTotal()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();
            
            var itemToRemove = new CartItem { CartItemId = cartItemId, TotalPrice = 50m };
            var remainingItem = new CartItem { CartItemId = Guid.NewGuid(), TotalPrice = 30m };
            
            var cart = new Cart 
            { 
                CartId = cartId, 
                UserId = userId, 
                CartItems = new List<CartItem> { itemToRemove, remainingItem },
                TotalAmount = 80m
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cartId)).ReturnsAsync(cart);
            
            // Mock items left after removal for total calculation
            _mockCartItemRepo.Setup(r => r.GetCartItemsAsync(cartId)).ReturnsAsync(new List<CartItem> { remainingItem });

            // Act
            var result = await _cartService.RemoveItemFromCartAsync(userId, cartItemId);

            // Assert
            result.Should().BeTrue();
            _mockCartItemRepo.Verify(r => r.Remove(itemToRemove), Times.Once);
            
            // Verify recalculation
            cart.TotalAmount.Should().Be(30m);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldUpdateAndRecalculate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();
            
            var item = new CartItem { CartItemId = cartItemId, Quantity = 1, UnitPrice = 50m, TotalPrice = 50m };
            var cart = new Cart 
            { 
                CartId = cartId, 
                CartItems = new List<CartItem> { item }
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cartId)).ReturnsAsync(cart);
            
            // Mock items for calc (simulating the update happened in memory or db context)
            _mockCartItemRepo.Setup(r => r.GetCartItemsAsync(cartId)).ReturnsAsync(new List<CartItem> { item });

            // Act
            var result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, 2);

            // Assert
            result.Should().BeTrue();
            item.Quantity.Should().Be(2);
            item.TotalPrice.Should().Be(100m);
            cart.TotalAmount.Should().Be(100m);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldRemoveAllItemsAndResetTotal()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cart = new Cart 
            { 
                CartItems = new List<CartItem> { new CartItem(), new CartItem() },
                TotalAmount = 100m
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cart.CartId)).ReturnsAsync(cart);

            // Act
            var result = await _cartService.ClearCartAsync(userId);

            // Assert
            result.Should().BeTrue();
            cart.TotalAmount.Should().Be(0);
            _mockCartItemRepo.Verify(r => r.Remove(It.IsAny<CartItem>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CheckoutAsync_ShouldProcessPurchasesAndClearCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cart = new Cart 
            { 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { GameId = gameId, TotalPrice = 60m, Game = new Game { GameName = "Elden Ring" } } 
                },
                TotalAmount = 60m
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cart.CartId)).ReturnsAsync(cart);
            
            // Mock successful purchase
            _mockPurchaseService.Setup(p => p.PurchaseGameAsync(userId, It.IsAny<CreatePurchaseDTO>()))
                .ReturnsAsync(new PurchaseResponseDTO { Success = true, GameName = "Elden Ring" });

            // Act
            var result = await _cartService.CheckoutAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.PurchasedGames.Should().Contain("Elden Ring");
            result.TotalAmount.Should().Be(60m);
            
            // Verify cart was cleared (ClearCartAsync logic is reused)
            cart.TotalAmount.Should().Be(0);
        }

        [Fact]
        public async Task CheckoutAsync_ShouldReturnFailures_WhenPurchaseFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cart = new Cart 
            { 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { GameId = gameId, TotalPrice = 60m, Game = new Game { GameName = "Failed Game" } } 
                }
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cart.CartId)).ReturnsAsync(cart);
            
            // Mock failed purchase
            _mockPurchaseService.Setup(p => p.PurchaseGameAsync(userId, It.IsAny<CreatePurchaseDTO>()))
                .ReturnsAsync(new PurchaseResponseDTO { Success = false, Message = "Payment failed" });

            // Act
            var result = await _cartService.CheckoutAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.FailedGames.Should().ContainMatch("*Payment failed*");
        }
        [Fact]
        public async Task GetUserCartAsync_ShouldReturnCart_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cart = new Cart 
            { 
                CartId = Guid.NewGuid(), 
                UserId = userId,
                TotalAmount = 100m,
                CartItems = new List<CartItem> 
                { 
                    new CartItem { GameId = Guid.NewGuid(), Quantity = 2, UnitPrice = 50m, TotalPrice = 100m, Game = new Game { GameName = "Test Game" } } 
                }
            };

            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(r => r.GetCartWithItemsAsync(cart.CartId)).ReturnsAsync(cart);

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(100m);
            result.Items.Should().HaveCount(1);
            result.Items.First().GameName.Should().Be("Test Game");
        }

        [Fact]
        public async Task GetUserCartAsync_ShouldReturnNull_WhenCartDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockCartRepo.Setup(r => r.GetUserCartAsync(userId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().BeNull();
        }
    }
}

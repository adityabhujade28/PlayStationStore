using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class CartServiceTests : IntegrationTestBase
    {
        private readonly CartService _cartService;
        private readonly PurchaseService _purchaseService;
        private readonly EntitlementService _entitlementService;

        public CartServiceTests()
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

            _cartService = new CartService(
                CartRepository,
                CartItemRepository,
                GameRepository,
                UserRepository,
                _purchaseService,
                _entitlementService
            );
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldAddItem_WhenValid()
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

            var addDTO = new CreateCartItemDTO { GameId = gameId, Quantity = 1 };

            // Act
            var result = await _cartService.AddItemToCartAsync(userId, addDTO);

            // Assert
            result.Should().NotBeNull();
            result.GameId.Should().Be(gameId);
            result.Quantity.Should().Be(1);
        }

        [Fact]
        public async Task RemoveItemFromCartAsync_ShouldRemoveItem_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();

            var cart = new Cart { CartId = cartId, UserId = userId };
            var cartItem = new CartItem
            {
                CartItemId = cartItemId,
                CartId = cartId,
                GameId = gameId,
                Quantity = 1,
                UnitPrice = 50m,
                TotalPrice = 50m
            };

            await Context.Carts.AddAsync(cart);
            await Context.CartItems.AddAsync(cartItem);
            await Context.SaveChangesAsync();

            // Act
            var result = await _cartService.RemoveItemFromCartAsync(userId, cartItemId);

            // Assert
            result.Should().BeTrue();

            var removedItem = await Context.CartItems.FindAsync(cartItemId);
            removedItem.Should().BeNull();
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldUpdateQuantity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();

            var cart = new Cart { CartId = cartId, UserId = userId };
            var cartItem = new CartItem
            {
                CartItemId = cartItemId,
                CartId = cartId,
                GameId = gameId,
                Quantity = 1,
                UnitPrice = 50m,
                TotalPrice = 50m
            };

            await Context.Carts.AddAsync(cart);
            await Context.CartItems.AddAsync(cartItem);
            await Context.SaveChangesAsync();

            // Act
            var result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, 3);

            // Assert
            result.Should().BeTrue();

            var updated = await Context.CartItems.FindAsync(cartItemId);
            updated!.Quantity.Should().Be(3);
            updated.TotalPrice.Should().Be(150m);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldRemoveAllItems()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cartId = Guid.NewGuid();

            var cart = new Cart { CartId = cartId, UserId = userId };
            var items = new List<CartItem>
            {
                new CartItem { CartItemId = Guid.NewGuid(), CartId = cartId, GameId = Guid.NewGuid(), Quantity = 1, UnitPrice = 50m, TotalPrice = 50m },
                new CartItem { CartItemId = Guid.NewGuid(), CartId = cartId, GameId = Guid.NewGuid(), Quantity = 2, UnitPrice = 30m, TotalPrice = 60m }
            };

            await Context.Carts.AddAsync(cart);
            await Context.CartItems.AddRangeAsync(items);
            await Context.SaveChangesAsync();

            // Act
            var result = await _cartService.ClearCartAsync(userId);

            // Assert
            result.Should().BeTrue();

            var remainingItems = await Context.CartItems.Where(ci => ci.CartId == cartId).ToListAsync();
            remainingItems.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserCartAsync_ShouldReturnCart_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var cartId = Guid.NewGuid();

            var game = new Game { GameId = gameId, GameName = "Test Game", BasePrice = 50m, CreatedAt = DateTime.UtcNow, IsDeleted = false };
            var cart = new Cart { CartId = cartId, UserId = userId, TotalAmount = 50m };
            var cartItem = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cartId,
                GameId = gameId,
                Quantity = 1,
                UnitPrice = 50m,
                TotalPrice = 50m
            };

            await Context.Games.AddAsync(game);
            await Context.Carts.AddAsync(cart);
            await Context.CartItems.AddAsync(cartItem);
            await Context.SaveChangesAsync();

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(1);
            result.TotalAmount.Should().Be(50m);
        }
    }
}

using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPurchaseService _purchaseService;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IPurchaseService purchaseService)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _purchaseService = purchaseService;
        }

        public async Task<CartDTO?> GetUserCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null) return null;

            return new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                TotalAmount = cart.TotalAmount,
                Items = cart.CartItems?.Select(item => new CartItemDTO
                {
                    CartItemId = item.CartItemId,
                    GameId = item.GameId,
                    GameName = item.Game?.GameName ?? "Unknown",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList() ?? new List<CartItemDTO>()
            };
        }

        public async Task<CartItemDTO> AddItemToCartAsync(Guid userId, CreateCartItemDTO cartItemDTO)
        {
            // Get or create cart for user
            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    TotalAmount = 0
                };
                await _cartRepository.AddAsync(cart);
                await _cartRepository.SaveChangesAsync();
            }

            // Validate game exists
            var game = await _gameRepository.GetByIdAsync(cartItemDTO.GameId);
            if (game == null)
            {
                throw new InvalidOperationException("Game not found.");
            }

            if (game.FreeToPlay)
            {
                throw new InvalidOperationException("Cannot add free-to-play games to cart.");
            }

            // Check if user already owns the game
            var alreadyPurchased = await _purchaseService.HasUserPurchasedGameAsync(userId, cartItemDTO.GameId);
            if (alreadyPurchased)
            {
                throw new InvalidOperationException("You already own this game.");
            }

            // Check if item already in cart
            var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.GameId == cartItemDTO.GameId);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += cartItemDTO.Quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    GameId = cartItemDTO.GameId,
                    Quantity = cartItemDTO.Quantity,
                    UnitPrice = game.BasePrice ?? 0m,
                    TotalPrice = cartItemDTO.Quantity * (game.BasePrice ?? 0m)
                };
                await _cartItemRepository.AddAsync(cartItem);
                existingItem = cartItem;
            }

            // Update cart total
            cart.TotalAmount = await CalculateCartTotalAsync(cart.CartId);
            _cartRepository.Update(cart);
            await _cartRepository.SaveChangesAsync();

            return new CartItemDTO
            {
                CartItemId = existingItem.CartItemId,
                GameId = existingItem.GameId,
                GameName = game.GameName,
                Quantity = existingItem.Quantity,
                UnitPrice = existingItem.UnitPrice,
                TotalPrice = existingItem.TotalPrice
            };
        }

        public async Task<bool> RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null) return false;

            var item = cart.CartItems?.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item == null) return false;

            _cartItemRepository.Remove(item);
            
            // Update cart total
            cart.TotalAmount = await CalculateCartTotalAsync(cart.CartId);
            _cartRepository.Update(cart);
            
            await _cartItemRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveItemFromCartAsync(userId, cartItemId);
            }

            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null) return false;

            var item = cart.CartItems?.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item == null) return false;

            item.Quantity = quantity;
            item.TotalPrice = item.Quantity * item.UnitPrice;
            _cartItemRepository.Update(item);

            // Update cart total
            cart.TotalAmount = await CalculateCartTotalAsync(cart.CartId);
            _cartRepository.Update(cart);

            await _cartItemRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null) return false;

            if (cart.CartItems != null && cart.CartItems.Any())
            {
                foreach (var item in cart.CartItems.ToList())
                {
                    _cartItemRepository.Remove(item);
                }
            }

            cart.TotalAmount = 0;
            _cartRepository.Update(cart);
            await _cartRepository.SaveChangesAsync();

            return true;
        }

        public async Task<CheckoutResultDTO> CheckoutAsync(Guid userId)
        {
            var cart = await _cartRepository.GetUserCartAsync(userId);
            if (cart != null)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            }
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return new CheckoutResultDTO
                {
                    Success = false,
                    Message = "Cart is empty."
                };
            }

            var purchasedGames = new List<string>();
            var failedGames = new List<string>();

            foreach (var item in cart.CartItems.ToList())
            {
                var purchaseDTO = new CreatePurchaseDTO { GameId = item.GameId };
                var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);

                if (result.Success)
                {
                    purchasedGames.Add(result.GameName ?? "Unknown");
                }
                else
                {
                    failedGames.Add($"{item.Game?.GameName ?? "Unknown"}: {result.Message}");
                }
            }

            // Clear cart after checkout
            await ClearCartAsync(userId);

            return new CheckoutResultDTO
            {
                Success = purchasedGames.Any(),
                Message = $"Purchased {purchasedGames.Count} game(s) successfully.",
                PurchasedGames = purchasedGames,
                FailedGames = failedGames,
                TotalAmount = cart.TotalAmount
            };
        }

        private async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var items = await _cartItemRepository.GetCartItemsAsync(cartId);
            return items.Sum(item => item.TotalPrice);
        }
    }
}

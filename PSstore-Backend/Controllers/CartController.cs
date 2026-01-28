
using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDTO>> GetUserCart(Guid userId)
        {
            _logger.LogInformation("Fetching cart for user ID: {UserId}", userId);
            try
            {
                var cart = await _cartService.GetUserCartAsync(userId);
                if (cart == null)
                {
                    _logger.LogInformation("Cart not found for user ID: {UserId}. Creating empty cart.", userId);
                    return Ok(new CartDTO { UserId = userId, Items = new List<CartItemDTO>(), TotalAmount = 0 });
                }

                _logger.LogInformation("Cart retrieved successfully for user ID: {UserId}", userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user ID: {UserId}", userId);
                throw;
            }
        }

        [HttpPost("user/{userId}/items")]
        public async Task<ActionResult<CartItemDTO>> AddItemToCart(Guid userId, [FromBody] CreateCartItemDTO cartItemDTO)
        {
            _logger.LogInformation("Adding item to cart for user ID: {UserId}, Game ID: {GameId}", userId, cartItemDTO.GameId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = await _cartService.AddItemToCartAsync(userId, cartItemDTO);
                _logger.LogInformation("Item added to cart successfully for user ID: {UserId}, Cart Item ID: {CartItemId}", userId, item.CartItemId);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Failed to add item to cart for user ID: {UserId}. Reason: {Reason}", userId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("user/{userId}/items/{cartItemId}")]
        public async Task<ActionResult> RemoveItemFromCart(Guid userId, Guid cartItemId)
        {
            _logger.LogInformation("Removing item from cart. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
            try
            {
                var result = await _cartService.RemoveItemFromCartAsync(userId, cartItemId);
                if (!result)
                {
                    _logger.LogWarning("Cart item not found for removal. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
                    return NotFound(new { message = "Cart item not found." });
                }

                _logger.LogInformation("Cart item removed successfully. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
                throw;
            }
        }

        [HttpPut("user/{userId}/items/{cartItemId}")]
        public async Task<ActionResult> UpdateCartItemQuantity(Guid userId, Guid cartItemId, [FromBody] int quantity)
        {
            _logger.LogInformation("Updating cart item quantity. User ID: {UserId}, Cart Item ID: {CartItemId}, New Quantity: {Quantity}", userId, cartItemId, quantity);
            if (quantity < 0)
            {
                _logger.LogWarning("Invalid quantity provided: {Quantity}. Quantity cannot be negative.", quantity);
                return BadRequest(new { message = "Quantity cannot be negative." });
            }

            try
            {
                var result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
                if (!result)
                {
                    _logger.LogWarning("Cart item not found for update. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
                    return NotFound(new { message = "Cart item not found." });
                }

                _logger.LogInformation("Cart item quantity updated successfully. User ID: {UserId}, Cart Item ID: {CartItemId}, Quantity: {Quantity}", userId, cartItemId, quantity);
                return Ok(new { message = "Cart item updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item quantity. User ID: {UserId}, Cart Item ID: {CartItemId}", userId, cartItemId);
                throw;
            }
        }

        [HttpDelete("user/{userId}")]
        public async Task<ActionResult> ClearCart(Guid userId)
        {
            _logger.LogInformation("Clearing cart for user ID: {UserId}", userId);
            try
            {
                var result = await _cartService.ClearCartAsync(userId);
                if (!result)
                {
                    _logger.LogWarning("Cart not found for clearing. User ID: {UserId}", userId);
                    return NotFound(new { message = "Cart not found." });
                }

                _logger.LogInformation("Cart cleared successfully for user ID: {UserId}", userId);
                return Ok(new { message = "Cart cleared successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user ID: {UserId}", userId);
                throw;
            }
        }

        [HttpPost("user/{userId}/checkout")]
        public async Task<ActionResult<CheckoutResultDTO>> Checkout(Guid userId)
        {
            _logger.LogInformation("Checkout initiated for user ID: {UserId}", userId);
            try
            {
                var result = await _cartService.CheckoutAsync(userId);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Checkout failed for user ID: {UserId}. Reason: {Reason}", userId, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Checkout completed successfully for user ID: {UserId}", userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user ID: {UserId}", userId);
                throw;
            }
        }
    }
}


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

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDTO>> GetUserCart(Guid userId)
        {
            var cart = await _cartService.GetUserCartAsync(userId);
            if (cart == null)
                return Ok(new CartDTO { UserId = userId, Items = new List<CartItemDTO>(), TotalAmount = 0 });

            return Ok(cart);
        }

        [HttpPost("user/{userId}/items")]
        public async Task<ActionResult<CartItemDTO>> AddItemToCart(Guid userId, [FromBody] CreateCartItemDTO cartItemDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var item = await _cartService.AddItemToCartAsync(userId, cartItemDTO);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("user/{userId}/items/{cartItemId}")]
        public async Task<ActionResult> RemoveItemFromCart(Guid userId, Guid cartItemId)
        {
            var result = await _cartService.RemoveItemFromCartAsync(userId, cartItemId);
            if (!result)
                return NotFound(new { message = "Cart item not found." });

            return NoContent();
        }

        [HttpPut("user/{userId}/items/{cartItemId}")]
        public async Task<ActionResult> UpdateCartItemQuantity(Guid userId, Guid cartItemId, [FromBody] int quantity)
        {
            if (quantity < 0)
                return BadRequest(new { message = "Quantity cannot be negative." });

            var result = await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
            if (!result)
                return NotFound(new { message = "Cart item not found." });

            return Ok(new { message = "Cart item updated successfully." });
        }

        [HttpDelete("user/{userId}")]
        public async Task<ActionResult> ClearCart(Guid userId)
        {
            var result = await _cartService.ClearCartAsync(userId);
            if (!result)
                return NotFound(new { message = "Cart not found." });

            return Ok(new { message = "Cart cleared successfully." });
        }

        [HttpPost("user/{userId}/checkout")]
        public async Task<ActionResult<CheckoutResultDTO>> Checkout(Guid userId)
        {
            var result = await _cartService.CheckoutAsync(userId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}

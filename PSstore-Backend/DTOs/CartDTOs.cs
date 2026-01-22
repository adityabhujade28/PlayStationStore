using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Create cart item (add to cart)
    public class CreateCartItemDTO
    {
        [Required]
        public Guid GameId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    // Cart item response
    public class CartItemDTO
    {
        public Guid CartItemId { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Publisher { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // User's cart
    public class CartDTO
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        public decimal TotalAmount { get; set; }
    }

    // Checkout result
    public class CheckoutResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> PurchasedGames { get; set; } = new List<string>();
        public List<string> FailedGames { get; set; } = new List<string>();
        public decimal TotalAmount { get; set; }
    }

    // Legacy DTOs for backwards compatibility
    public class AddToCartDTO
    {
        [Required]
        public Guid GameId { get; set; }
    }

    public class CheckoutDTO
    {
        public string? PaymentMethod { get; set; }
    }

    public class CheckoutResponseDTO
    {
        public List<PurchaseResponseDTO> Purchases { get; set; } = new List<PurchaseResponseDTO>();
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}

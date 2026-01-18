using System.ComponentModel.DataAnnotations;

namespace PSstore.DTOs
{
    // Purchase a game
    public class CreatePurchaseDTO
    {
        [Required]
        public Guid GameId { get; set; }
    }

    // Purchase response
    public class PurchaseResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid PurchaseId { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }

    // Purchase history entry
    public class PurchaseHistoryDTO
    {
        public Guid PurchaseId { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }

    // List user's purchased games
    public class UserPurchasedGameDTO
    {
        public Guid PurchaseId { get; set; }
        public GameDTO Game { get; set; } = null!;
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}

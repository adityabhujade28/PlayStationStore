namespace PSstore.DTOs
{
    // Check game access result
    public class GameAccessResultDTO
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public bool CanAccess { get; set; }
        public string AccessType { get; set; } = string.Empty; // "FREE", "PURCHASED", "SUBSCRIPTION", "NO_ACCESS"
        public string? Message { get; set; }
        
        // Additional metadata
        public DateTime? PurchasedOn { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionExpiresOn { get; set; }
    }

    // Bulk access check (multiple games)
    public class UserLibraryDTO
    {
        public int UserId { get; set; }
        public List<GameAccessResultDTO> AccessibleGames { get; set; } = new List<GameAccessResultDTO>();
        public int TotalPurchasedGames { get; set; }
        public int TotalSubscriptionGames { get; set; }
        public int TotalFreeGames { get; set; }
    }
}

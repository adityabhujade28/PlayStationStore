namespace PSstore.DTOs
{
    public class AdminDTO
    {
        public Guid AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    public class DashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalGames { get; set; }
        public int ActiveSubscriptions { get; set; }
    }
}

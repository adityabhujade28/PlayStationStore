namespace PSstore.DTOs
{
    // Region details
    public class RegionDTO
    {
        public int RegionId { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? RegionTimezone { get; set; }
    }
}

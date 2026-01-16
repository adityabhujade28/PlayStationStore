namespace PSstore.DTOs
{
    // Region details
    public class RegionDTO
    {
        public Guid RegionId { get; set; }
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? RegionTimezone { get; set; }
    }

    // Country details
    public class CountryDTO
    {
        public Guid CountryId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public Guid RegionId { get; set; }
    }
}

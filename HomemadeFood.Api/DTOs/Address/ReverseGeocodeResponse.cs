namespace HomemadeFood.Api.DTOs.Address
{
    public class ReverseGeocodeResponse
    {
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNo { get; set; } = string.Empty;
        public string FormattedAddress { get; set; } = string.Empty;
        public string Granularity { get; set; } = string.Empty;
    }
}
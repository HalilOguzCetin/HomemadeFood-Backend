namespace HomemadeFood.Api.DTOs.Address
{
    public class AddressResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNo { get; set; } = string.Empty;

        public string? Floor { get; set; }
        public string? ApartmentNo { get; set; }
        public string? AddressNote { get; set; }
    }
}
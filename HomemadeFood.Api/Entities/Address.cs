namespace HomemadeFood.Api.Entities
{
    public class Address
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDefault { get; set; } = false;
        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string Neighborhood { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;

        public string BuildingNo { get; set; } = string.Empty;

        public string? Floor { get; set; }

        public string? ApartmentNo { get; set; }

        public string? AddressNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
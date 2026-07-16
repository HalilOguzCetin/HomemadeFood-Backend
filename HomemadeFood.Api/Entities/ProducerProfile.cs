using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class ProducerProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string BusinessName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public decimal Rating { get; set; } = 0;

        public int DailyCapacity { get; set; }

        public int RemainingCapacity { get; set; }
        public DateOnly? CapacityDate { get; set; }

        public bool IsAvailable { get; set; } = true;

        public bool IsApproved { get; set; } = false;

        public string VerificationStatus { get; set; }
    = ProducerVerificationStatuses.Pending;
        public DateTime? ApprovedAt { get; set; }

        public int? ApprovedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Food> Foods { get; set; } = new List<Food>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Cart> Carts { get; set; }
    = new List<Cart>();
       
    }
}
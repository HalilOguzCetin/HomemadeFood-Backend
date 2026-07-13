namespace HomemadeFood.Api.Entities
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int FoodId { get; set; }

        public Food Food { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
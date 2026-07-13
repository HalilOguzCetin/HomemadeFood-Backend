namespace HomemadeFood.Api.DTOs.Food
{
    public class FoodResponse
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int PreparationTimeMinutes { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
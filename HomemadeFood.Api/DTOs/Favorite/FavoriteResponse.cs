namespace HomemadeFood.Api.DTOs.Favorite
{
    public class FavoriteResponse
    {
        public int FavoriteId { get; set; }

        public int FoodId { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string FoodName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int PreparationTimeMinutes { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
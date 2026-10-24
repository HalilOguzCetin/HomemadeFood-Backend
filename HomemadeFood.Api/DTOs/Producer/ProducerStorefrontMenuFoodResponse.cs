namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerStorefrontMenuFoodResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public decimal Price { get; set; }

        public string UnitType { get; set; } =
            string.Empty;

        public int PreparationTimeMinutes { get; set; }

        public string ImageUrl { get; set; } =
            string.Empty;
    }
}
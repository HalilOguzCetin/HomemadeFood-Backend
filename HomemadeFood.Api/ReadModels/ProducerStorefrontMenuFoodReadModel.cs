namespace HomemadeFood.Api.ReadModels
{
    public class ProducerStorefrontMenuFoodReadModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public decimal Price { get; set; }

        public int PreparationTimeMinutes { get; set; }

        public string ImageUrl { get; set; } =
            string.Empty;
    }
}
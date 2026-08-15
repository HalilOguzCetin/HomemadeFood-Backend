namespace HomemadeFood.Api.ReadModels
{
    public class ProducerStorefrontMenuReadModel
    {
        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string? BusinessImageUrl { get; set; }

        public decimal Rating { get; set; }

        public string City { get; set; } =
            string.Empty;

        public string District { get; set; } =
            string.Empty;

        public List<ProducerStorefrontMenuFoodReadModel>
            Foods
        { get; set; } =
            new();
    }
}
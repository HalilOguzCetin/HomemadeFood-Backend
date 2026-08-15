namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerStorefrontMenuCategoryResponse
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } =
            string.Empty;

        public List<ProducerStorefrontMenuFoodResponse>
            Foods
        { get; set; } =
            new();
    }
}
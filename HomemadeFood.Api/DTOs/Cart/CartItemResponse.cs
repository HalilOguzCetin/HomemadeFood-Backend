namespace HomemadeFood.Api.DTOs.Cart
{
    public class CartItemResponse
    {
        public int CartItemId { get; set; }

        public int FoodId { get; set; }

        public string FoodName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }

        public bool IsAvailable { get; set; }
    }
}
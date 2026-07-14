namespace HomemadeFood.Api.DTOs.Cart
{
    public class CartResponse
    {
        public int? CartId { get; set; }

        public int? ProducerProfileId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public List<CartItemResponse> Items { get; set; }
            = new List<CartItemResponse>();

        public int TotalQuantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
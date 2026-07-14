namespace HomemadeFood.Api.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public Cart Cart { get; set; } = null!;

        public int FoodId { get; set; }

        public Food Food { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
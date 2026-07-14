namespace HomemadeFood.Api.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public int FoodId { get; set; }

        public Food Food { get; set; } = null!;

        // Yemek adı sipariş anında kopyalanır.
        public string FoodName { get; set; }
            = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
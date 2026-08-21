namespace HomemadeFood.Api.ReadModels
{
    /*
     * H8D-1A
     *
     * Keşfet / Yemekler için yalnızca backend'in
     * ihtiyaç duyduğu alanları taşır.
     *
     * Producer koordinatları response'a verilmez.
     * Yalnız service katmanı mesafe hesabında kullanır.
     */
    public class FoodDiscoverCandidateReadModel
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string ProducerCity { get; set; } =
            string.Empty;

        public double ProducerLatitude { get; set; }

        public double ProducerLongitude { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public decimal Price { get; set; }

        public int PreparationTimeMinutes
        {
            get;
            set;
        }

        public string ImageUrl { get; set; } =
            string.Empty;

        public bool IsAvailable { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DeliveredOrderCount30Days
        {
            get;
            set;
        }

        public int SoldQuantity30Days
        {
            get;
            set;
        }

        public int DistinctCustomerCount30Days
        {
            get;
            set;
        }

        public int RepeatCustomerCount30Days
        {
            get;
            set;
        }

        public int FavoriteCount { get; set; }
    }
}
namespace HomemadeFood.Api.ReadModels
{
    /*
     * H5A:
     * Popüler yemek skoru için repository'den service'e
     * taşınan ham metrikler.
     *
     * Skor repository'de değil service katmanında hesaplanır.
     */
    public class FoodPopularityCandidateReadModel
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

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

        /*
         * Favorite entity'sinde CreatedAt olmadığı için
         * bu değer all-time toplamdır.
         */
        public int FavoriteCount { get; set; }
    }
}
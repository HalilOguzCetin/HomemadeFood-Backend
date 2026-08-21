namespace HomemadeFood.Api.ReadModels
{
    /*
     * H8D-1B
     *
     * Keşfet / İşletmeler için repository -> service
     * arasında taşınan aday işletme modeli.
     *
     * Latitude / Longitude yalnız backend mesafe hesabı içindir.
     * API response'una kesinlikle aktarılmaz.
     */
    public class ProducerDiscoverCandidateReadModel
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

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int AvailableFoodCount { get; set; }

        public int AvailableCategoryCount { get; set; }

        public int MatchingFoodCount { get; set; }

        public int? MinimumPreparationTimeMinutes
        {
            get;
            set;
        }

        public int DeliveredOrderCount30Days
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

        public int ReviewCount { get; set; }
    }
}
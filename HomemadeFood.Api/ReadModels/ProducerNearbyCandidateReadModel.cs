namespace HomemadeFood.Api.ReadModels
{
    public class ProducerNearbyCandidateReadModel
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

        public int? MinimumPreparationTimeMinutes
        {
            get;
            set;
        }
    }
}
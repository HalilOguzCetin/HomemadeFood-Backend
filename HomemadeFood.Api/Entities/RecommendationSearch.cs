namespace HomemadeFood.Api.Entities
{
    public class RecommendationSearch
    {
        public int Id { get; set; }

        // Aramayı yapan müşteri.
        public int CustomerUserId { get; set; }

        // Aramada kullanılan adres.
        public int AddressId { get; set; }

        // Adres sonradan değişse bile arama anındaki
        // koordinatların korunması için saklanır.
        public double CustomerLatitude { get; set; }

        public double CustomerLongitude { get; set; }

        public string SearchText { get; set; } =
            string.Empty;

        public int? RequestedQuantity { get; set; }

        // Müşteri önerilerden birini seçtiğinde doldurulacak.
        public int? SelectedFoodId { get; set; }

        public int? SelectedProducerProfileId { get; set; }

        public DateTime? SelectedAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<RecommendationCandidate>
            Candidates
        { get; set; } =
                new List<RecommendationCandidate>();
    }
}
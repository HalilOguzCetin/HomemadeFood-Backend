namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class RecommendationPerformanceResponse
    {
        public int TotalSearches { get; set; }

        public int SearchesWithRecommendations { get; set; }

        public int SearchesWithoutRecommendations { get; set; }

        public int TotalCandidatesShown { get; set; }

        public int SelectedSearches { get; set; }

        public int RecommendationOrders { get; set; }

        public int DeliveredOrders { get; set; }

        public int CancelledOrders { get; set; }

        public int RejectedOrders { get; set; }

        public int ReviewedOrders { get; set; }

        public double SearchToSelectionRate { get; set; }

        public double SelectionToOrderRate { get; set; }

        public double OrderDeliveryRate { get; set; }

        public double ReviewRate { get; set; }

        public decimal AverageSuitabilityScore { get; set; }

        public double AverageCustomerRating { get; set; }

        public double AverageSelectedRank { get; set; }
    }
}
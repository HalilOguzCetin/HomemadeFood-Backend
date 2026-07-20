using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class ProducerRecommendationSelectionRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Öneri arama ID değeri sıfırdan büyük olmalıdır.")]
        public int RecommendationSearchId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Yemek ID değeri sıfırdan büyük olmalıdır.")]
        public int FoodId { get; set; }
    }
}
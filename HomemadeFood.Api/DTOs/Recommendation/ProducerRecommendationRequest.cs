using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class ProducerRecommendationRequest
    {
        [Required(
            ErrorMessage = "Aranacak yemek adı zorunludur.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage =
                "Arama metni 2 ile 100 karakter arasında olmalıdır.")]
        public string SearchText { get; set; } =
            string.Empty;

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Adres ID değeri sıfırdan büyük olmalıdır.")]
        public int AddressId { get; set; }

        [Range(
            1,
            100,
            ErrorMessage =
                "Miktar 1 ile 100 arasında olmalıdır.")]
        public int? Quantity { get; set; }
    }
}
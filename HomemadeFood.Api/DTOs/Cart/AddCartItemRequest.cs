using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Cart
{
    public class AddCartItemRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Geçerli bir yemek seçilmelidir.")]
        public int FoodId { get; set; }

        [Range(
            1,
            50,
            ErrorMessage =
                "Miktar 1 ile 50 arasında olmalıdır.")]
        public int Quantity { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Öneri arama ID değeri sıfırdan büyük olmalıdır.")]
        public int? RecommendationSearchId { get; set; }
    }
}
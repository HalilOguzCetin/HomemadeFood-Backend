using System.ComponentModel.DataAnnotations;
using HomemadeFood.Api.Constants;
using Microsoft.AspNetCore.Http;

namespace HomemadeFood.Api.DTOs.Food
{
    public class UpdateFoodRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Geçerli bir kategori seçilmelidir.")]
        public int CategoryId { get; set; }

        [Required(
            ErrorMessage = "Yemek adı zorunludur.")]
        [MaxLength(
            100,
            ErrorMessage = "Yemek adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Yemek açıklaması zorunludur.")]
        [MaxLength(
            1000,
            ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "İçindekiler bilgisi zorunludur.")]
        [MaxLength(
            2000,
            ErrorMessage = "İçindekiler bilgisi en fazla 2000 karakter olabilir.")]
        public string Ingredients { get; set; } = string.Empty;

        [Range(
            typeof(decimal),
            "0.01",
            "9999999",
            ErrorMessage = "Fiyat sıfırdan büyük olmalıdır.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public decimal Price { get; set; }

        [Required(
            ErrorMessage = "Satış birimi zorunludur.")]
        [MaxLength(
            20,
            ErrorMessage = "Satış birimi en fazla 20 karakter olabilir.")]
        public string UnitType { get; set; } =
            FoodUnitTypes.Portion;

        [Range(
            1,
            10000,
            ErrorMessage = "Günlük kapasite 1 ile 10000 arasında olmalıdır.")]
        public int DailyCapacity { get; set; } = 1;

        [Range(
            1,
            1440,
            ErrorMessage = "Hazırlama süresi 1 ile 1440 dakika arasında olmalıdır.")]
        public int PreparationTimeMinutes { get; set; }

        [Range(
            0,
            10080,
            ErrorMessage = "Minimum sipariş ön süresi 0 ile 10080 dakika arasında olmalıdır.")]
        public int MinimumOrderLeadTimeMinutes { get; set; }

        public List<string> AllergenCodes { get; set; } = new();

        /*
         * Edit sırasında yeni fotoğraf seçmek zorunlu değildir.
         * Image null ise mevcut Food.ImageUrl korunur.
         */
        public IFormFile? Image { get; set; }

        public bool IsAvailable { get; set; }
    }
}

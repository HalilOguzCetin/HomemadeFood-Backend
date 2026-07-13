using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Food
{
    public class UpdateFoodRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçilmelidir.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Yemek adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Yemek adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yemek açıklaması zorunludur.")]
        [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999",
     ErrorMessage = "Fiyat sıfırdan büyük olmalıdır.",
     ParseLimitsInInvariantCulture = true,
     ConvertValueInInvariantCulture = true)]
        public decimal Price { get; set; }

        [Range(1, 1440,
            ErrorMessage = "Hazırlama süresi 1 ile 1440 dakika arasında olmalıdır.")]
        public int PreparationTimeMinutes { get; set; }

        [MaxLength(500, ErrorMessage = "Görsel adresi en fazla 500 karakter olabilir.")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
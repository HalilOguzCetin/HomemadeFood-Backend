using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Producer
{
    public class UpdateProducerProfileRequest
    {
        [Required(ErrorMessage = "İşletme adı zorunludur.")]
        [StringLength(
            150,
            MinimumLength = 2,
            ErrorMessage =
                "İşletme adı 2 ile 150 karakter arasında olmalıdır.")]
        public string BusinessName { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "İşletme açıklaması zorunludur.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage =
                "İşletme açıklaması 10 ile 1000 karakter arasında olmalıdır.")]
        public string Description { get; set; } =
            string.Empty;

        /*
         * Profil güncellemede yeni görsel seçimi opsiyoneldir.
         * Null gelirse mevcut BusinessImageUrl korunur.
         */
        public IFormFile? BusinessImage { get; set; }

        [Required(ErrorMessage = "İşletme adresi zorunludur.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage =
                "Adres 10 ile 500 karakter arasında olmalıdır.")]
        public string Address { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "İl bilgisi zorunludur.")]
        [StringLength(
            100,
            ErrorMessage =
                "İl bilgisi en fazla 100 karakter olabilir.")]
        public string City { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "İlçe bilgisi zorunludur.")]
        [StringLength(
            100,
            ErrorMessage =
                "İlçe bilgisi en fazla 100 karakter olabilir.")]
        public string District { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Mahalle bilgisi zorunludur.")]
        [StringLength(
            120,
            ErrorMessage =
                "Mahalle bilgisi en fazla 120 karakter olabilir.")]
        public string Neighborhood { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Cadde veya sokak bilgisi zorunludur.")]
        [StringLength(
            150,
            ErrorMessage =
                "Cadde veya sokak bilgisi en fazla 150 karakter olabilir.")]
        public string Street { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Bina numarası zorunludur.")]
        [StringLength(
            30,
            ErrorMessage =
                "Bina numarası en fazla 30 karakter olabilir.")]
        public string BuildingNo { get; set; } =
            string.Empty;

        [StringLength(
            20,
            ErrorMessage =
                "Kat bilgisi en fazla 20 karakter olabilir.")]
        public string? Floor { get; set; }

        [StringLength(
            20,
            ErrorMessage =
                "Daire bilgisi en fazla 20 karakter olabilir.")]
        public string? ApartmentNo { get; set; }

        [StringLength(
            300,
            ErrorMessage =
                "Adres tarifi en fazla 300 karakter olabilir.")]
        public string? AddressNote { get; set; }

        /*
         * multipart/form-data kültür farklarından etkilenmemek için
         * koordinatlar formdan string alınır ve servis katmanında
         * InvariantCulture ile double'a çevrilir.
         */
        [Required(ErrorMessage = "Enlem bilgisi zorunludur.")]
        public string Latitude { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Boylam bilgisi zorunludur.")]
        public string Longitude { get; set; } =
            string.Empty;

        [Range(
            1,
            1000,
            ErrorMessage =
                "Günlük kapasite 1 ile 1000 arasında olmalıdır.")]
        public int DailyCapacity { get; set; }

        public bool IsAvailable { get; set; }
    }
}
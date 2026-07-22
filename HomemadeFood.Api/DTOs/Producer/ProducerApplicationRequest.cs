using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerApplicationRequest
    {
        [Required(
            ErrorMessage = "İşletme adı zorunludur.")]
        [StringLength(
            150,
            MinimumLength = 2,
            ErrorMessage =
                "İşletme adı 2 ile 150 karakter arasında olmalıdır.")]
        public string BusinessName { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "İşletme açıklaması zorunludur.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage =
                "İşletme açıklaması 10 ile 1000 karakter arasında olmalıdır.")]
        public string Description { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "İşletme adresi zorunludur.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage =
                "İşletme adresi 10 ile 500 karakter arasında olmalıdır.")]
        public string Address { get; set; }
            = string.Empty;

        [Range(
            typeof(double),
            "-90",
            "90",
            ErrorMessage =
                "Enlem -90 ile 90 arasında olmalıdır.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public double Latitude { get; set; }

        [Range(
            typeof(double),
            "-180",
            "180",
            ErrorMessage =
                "Boylam -180 ile 180 arasında olmalıdır.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public double Longitude { get; set; }

        [Range(
            1,
            1000,
            ErrorMessage =
                "Günlük kapasite 1 ile 1000 arasında olmalıdır.")]
        public int DailyCapacity { get; set; }
    }
}
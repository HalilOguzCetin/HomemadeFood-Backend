using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Address
{
    public class CreateAddressRequest
    {
        [Required(ErrorMessage = "Adres başlığı zorunludur.")]
        [MaxLength(50, ErrorMessage = "Adres başlığı en fazla 50 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açık adres zorunludur.")]
        [MaxLength(500, ErrorMessage = "Açık adres en fazla 500 karakter olabilir.")]
        public string FullAddress { get; set; } = string.Empty;

        [Range(
            typeof(double),
            "-90",
            "90",
            ErrorMessage = "Enlem -90 ile 90 arasında olmalıdır.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public double Latitude { get; set; }

        [Range(
            typeof(double),
            "-180",
            "180",
            ErrorMessage = "Boylam -180 ile 180 arasında olmalıdır.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public double Longitude { get; set; }

        public bool IsDefault { get; set; }
    }
}
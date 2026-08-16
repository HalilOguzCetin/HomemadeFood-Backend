using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class VerifyPhoneRequest
    {
        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [MaxLength(30, ErrorMessage = "Telefon numarası çok uzun.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
        public string Code { get; set; } = string.Empty;
    }
}
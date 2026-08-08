using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class VerifyEmailRequest
    {
        [Required(
            ErrorMessage =
                "E-posta adresi zorunludur.")]
        [EmailAddress(
            ErrorMessage =
                "Geçerli bir e-posta adresi girilmelidir.")]
        [MaxLength(255)]
        public string Email { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage =
                "Doğrulama kodu zorunludur.")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage =
                "Doğrulama kodu 6 haneli olmalıdır.")]
        public string Code { get; set; } =
            string.Empty;
    }
}
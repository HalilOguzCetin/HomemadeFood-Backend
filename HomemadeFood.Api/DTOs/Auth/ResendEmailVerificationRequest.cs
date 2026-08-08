using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class ResendEmailVerificationRequest
    {
        [Required(
            ErrorMessage =
                "E-posta adresi zorunludur.")]
        [EmailAddress(
            ErrorMessage =
                "Geçerli bir e-posta adresi girilmelidir.")]
        [MaxLength(
            255,
            ErrorMessage =
                "E-posta adresi en fazla 255 karakter olabilir.")]
        public string Email { get; set; } =
            string.Empty;
    }
}
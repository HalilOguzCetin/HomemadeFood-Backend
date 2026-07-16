using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girilmelidir.")]
        [MaxLength(
            255,
            ErrorMessage = "E-posta adresi en fazla 255 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MaxLength(
            100,
            ErrorMessage = "Şifre en fazla 100 karakter olabilir.")]
        public string Password { get; set; } = string.Empty;
    }
}
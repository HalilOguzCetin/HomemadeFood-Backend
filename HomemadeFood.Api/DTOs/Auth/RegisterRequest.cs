using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(
            ErrorMessage =
                "Ad soyad zorunludur.")]
        [MinLength(
            2,
            ErrorMessage =
                "Ad soyad en az 2 karakter olmalıdır.")]
        [MaxLength(
            100,
            ErrorMessage =
                "Ad soyad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; } =
            string.Empty;

        [Required(
    ErrorMessage =
        "E-posta adresi zorunludur.")]
        [EmailAddress(
    ErrorMessage =
        "Geçerli bir e-posta adresi girilmelidir.")]
        [RegularExpression(
    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage =
        "Geçerli bir e-posta adresi girilmelidir. Örnek: ad@ornek.com")]
        [MaxLength(
    255,
    ErrorMessage =
        "E-posta adresi en fazla 255 karakter olabilir.")]
        public string Email { get; set; } =
    string.Empty;

        [Required(
    ErrorMessage =
        "Şifre zorunludur.")]
        [MinLength(
    8,
    ErrorMessage =
        "Şifre en az 8 karakter olmalıdır.")]
        [MaxLength(
    100,
    ErrorMessage =
        "Şifre en fazla 100 karakter olabilir.")]
        [RegularExpression(
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$",
    ErrorMessage =
        "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string Password { get; set; } =
    string.Empty;
    }
}
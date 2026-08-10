using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required(
            ErrorMessage =
                "E-posta alanı zorunludur.")]
        [EmailAddress(
            ErrorMessage =
                "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(
            255,
            ErrorMessage =
                "E-posta en fazla 255 karakter olabilir.")]
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

        [Required(
            ErrorMessage =
                "Yeni şifre zorunludur.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Şifre en az 8, en fazla 100 karakter olmalıdır.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$",
            ErrorMessage =
                "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string NewPassword { get; set; } =
            string.Empty;
    }
}
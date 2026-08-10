using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class ForgotPasswordRequest
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
    }
}
using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    public class RequestPhoneVerificationRequest
    {
        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [MaxLength(30, ErrorMessage = "Telefon numarası çok uzun.")]
        public string Phone { get; set; } = string.Empty;
    }
}
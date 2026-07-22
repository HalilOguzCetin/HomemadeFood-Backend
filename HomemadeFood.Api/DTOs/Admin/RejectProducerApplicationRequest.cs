using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Admin
{
    public sealed class RejectProducerApplicationRequest
    {
        [Required(
            ErrorMessage =
                "Reddetme gerekçesi zorunludur.")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage =
                "Reddetme gerekçesi 10 ile 500 karakter arasında olmalıdır.")]
        public string Reason { get; set; }
            = string.Empty;
    }
}
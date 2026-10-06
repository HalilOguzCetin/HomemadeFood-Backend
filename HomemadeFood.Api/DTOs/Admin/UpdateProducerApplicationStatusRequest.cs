using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Admin
{
    public sealed class UpdateProducerApplicationStatusRequest
    {
        [Required(ErrorMessage = "Hedef başvuru durumu zorunludur.")]
        [StringLength(40)]
        public string Status { get; set; } =
            string.Empty;

        [StringLength(
            1000,
            ErrorMessage = "İnceleme notu en fazla 1000 karakter olabilir.")]
        public string? ReviewNote { get; set; }
    }
}

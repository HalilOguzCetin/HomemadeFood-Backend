using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class UpdateProducerComplianceRequest
    {
        [Required(ErrorMessage = "Vergi statüsü zorunludur.")]
        [StringLength(40)]
        public string TaxStatus { get; set; } =
            string.Empty;

        [StringLength(32)]
        public string? TaxNumber { get; set; }

        [StringLength(100)]
        public string? TaxExemptionCertificateNumber { get; set; }

        [StringLength(100)]
        public string? FoodBusinessRegistrationNumber { get; set; }

        [Required(ErrorMessage = "Gıda işletmesi kayıt durumu zorunludur.")]
        [StringLength(40)]
        public string FoodRegistrationStatus { get; set; } =
            string.Empty;
    }
}

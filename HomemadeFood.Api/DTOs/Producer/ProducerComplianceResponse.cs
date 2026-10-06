namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class ProducerComplianceResponse
    {
        public int ProducerProfileId { get; set; }

        public string TaxStatus { get; set; } =
            string.Empty;

        public string? TaxNumber { get; set; }

        public string? TaxExemptionCertificateNumber { get; set; }

        public string? FoodBusinessRegistrationNumber { get; set; }

        public string FoodRegistrationStatus { get; set; } =
            string.Empty;

        public string PaymentAccountStatus { get; set; } =
            string.Empty;

        public string ComplianceStatus { get; set; } =
            string.Empty;

        /*
         * Şimdilik resmi kurum/API doğrulaması bağlı değildir.
         * Bu alan özellikle istemcinin kayıtları "resmi doğrulanmış"
         * gibi göstermesini önlemek için açıkça false döner.
         */
        public bool IsOfficiallyVerified { get; set; }

        public DateTime? LastReviewedAt { get; set; }

        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

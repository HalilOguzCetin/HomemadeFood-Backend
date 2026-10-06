using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Entities
{
    public class ProducerComplianceProfile
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }
        public ProducerProfile ProducerProfile { get; set; } = null!;

        /*
         * Vergi statüsü yalnız beyan/altyapı bilgisidir.
         * Gerçek GİB doğrulaması bağlanana kadar Verified anlamına gelmez.
         */
        public string TaxStatus { get; set; } =
            ProducerTaxStatuses.NotDeclared;

        public string? TaxNumber { get; set; }

        public string? TaxExemptionCertificateNumber { get; set; }

        public string? FoodBusinessRegistrationNumber { get; set; }

        public string FoodRegistrationStatus { get; set; } =
            ProducerFoodRegistrationStatuses.NotDeclared;

        /*
         * Ham IBAN / kart bilgisi tutmak yerine ileride ödeme sağlayıcısının
         * opaque/reference kimliği tutulabilir.
         */
        public string PaymentAccountStatus { get; set; } =
            ProducerPaymentAccountStatuses.NotConfigured;

        public string? PaymentAccountReference { get; set; }

        /*
         * Resmi entegrasyon kurulana kadar InfrastructureOnly / ManualReview
         * gibi durumlar kullanılır. Kullanıcıya resmi doğrulama yapılmış gibi
         * gösterilmez.
         */
        public string ComplianceStatus { get; set; } =
            ProducerComplianceStatuses.InfrastructureOnly;

        public DateTime? LastReviewedAt { get; set; }

        public int? ReviewedByAdminId { get; set; }

        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

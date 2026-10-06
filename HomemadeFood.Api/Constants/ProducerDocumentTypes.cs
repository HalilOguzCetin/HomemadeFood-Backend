namespace HomemadeFood.Api.Constants
{
    public static class ProducerDocumentTypes
    {
        public const string TaxCertificate = "TaxCertificate";
        public const string TradesmanTaxExemptionCertificate =
            "TradesmanTaxExemptionCertificate";
        public const string FoodBusinessRegistration =
            "FoodBusinessRegistration";
        public const string PaymentAccountProof =
            "PaymentAccountProof";
        public const string Other = "Other";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                TaxCertificate,
                TradesmanTaxExemptionCertificate,
                FoodBusinessRegistration,
                PaymentAccountProof,
                Other
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

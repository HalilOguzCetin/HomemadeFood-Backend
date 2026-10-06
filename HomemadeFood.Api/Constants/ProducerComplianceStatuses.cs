namespace HomemadeFood.Api.Constants
{
    public static class ProducerComplianceStatuses
    {
        /*
         * Gerçek kurum entegrasyonları bağlanana kadar yeni kayıtların
         * güvenli varsayılanıdır. Sistem resmi doğrulama yapılmış gibi
         * davranmaz.
         */
        public const string InfrastructureOnly = "InfrastructureOnly";
        public const string ManualReview = "ManualReview";
        public const string Compliant = "Compliant";
        public const string AdditionalActionRequired =
            "AdditionalActionRequired";
        public const string Suspended = "Suspended";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                InfrastructureOnly,
                ManualReview,
                Compliant,
                AdditionalActionRequired,
                Suspended
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

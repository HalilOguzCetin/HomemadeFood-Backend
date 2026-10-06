namespace HomemadeFood.Api.Constants
{
    public static class ProducerPaymentAccountStatuses
    {
        public const string NotConfigured = "NotConfigured";
        public const string ManualReview = "ManualReview";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                NotConfigured,
                ManualReview,
                Verified,
                Rejected
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

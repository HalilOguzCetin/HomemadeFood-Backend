namespace HomemadeFood.Api.Constants
{
    public static class ProducerDocumentVerificationStatuses
    {
        public const string NotVerified = "NotVerified";
        public const string ManualReview = "ManualReview";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
        public const string Expired = "Expired";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                NotVerified,
                ManualReview,
                Verified,
                Rejected,
                Expired
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

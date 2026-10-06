namespace HomemadeFood.Api.Constants
{
    public static class ProducerVerificationStatuses
    {
        public const string Pending = "Pending";
        public const string UnderReview = "UnderReview";
        public const string AdditionalDocumentRequired =
            "AdditionalDocumentRequired";
        public const string Approved = "Approved";
        public const string Suspended = "Suspended";
        public const string Rejected = "Rejected";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                Pending,
                UnderReview,
                AdditionalDocumentRequired,
                Approved,
                Suspended,
                Rejected
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

namespace HomemadeFood.Api.Constants
{
    public static class ProducerFoodRegistrationStatuses
    {
        public const string NotDeclared = "NotDeclared";
        public const string ManualReview = "ManualReview";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
        public const string Expired = "Expired";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                NotDeclared,
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

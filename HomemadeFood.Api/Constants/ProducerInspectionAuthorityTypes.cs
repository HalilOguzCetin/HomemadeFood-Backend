namespace HomemadeFood.Api.Constants
{
    public static class ProducerInspectionAuthorityTypes
    {
        public const string AgricultureAndForestry =
            "AgricultureAndForestry";
        public const string Municipality = "Municipality";
        public const string PlatformInternal = "PlatformInternal";
        public const string Other = "Other";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                AgricultureAndForestry,
                Municipality,
                PlatformInternal,
                Other
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

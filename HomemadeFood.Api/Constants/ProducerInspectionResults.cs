namespace HomemadeFood.Api.Constants
{
    public static class ProducerInspectionResults
    {
        public const string Pending = "Pending";
        public const string Compliant = "Compliant";
        public const string DeficiencyFound = "DeficiencyFound";
        public const string Suspended = "Suspended";
        public const string Closed = "Closed";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                Pending,
                Compliant,
                DeficiencyFound,
                Suspended,
                Closed
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

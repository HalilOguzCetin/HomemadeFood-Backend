namespace HomemadeFood.Api.Constants
{
    public static class ProducerAvailabilityModes
    {
        public const string Scheduled = "Scheduled";
        public const string ForceOpen = "ForceOpen";
        public const string ForceClosed = "ForceClosed";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                Scheduled,
                ForceOpen,
                ForceClosed
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

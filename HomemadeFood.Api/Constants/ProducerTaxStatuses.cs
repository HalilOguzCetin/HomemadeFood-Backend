namespace HomemadeFood.Api.Constants
{
    public static class ProducerTaxStatuses
    {
        public const string NotDeclared = "NotDeclared";
        public const string Taxpayer = "Taxpayer";
        public const string TradesmanExemption = "TradesmanExemption";
        public const string PendingSetup = "PendingSetup";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(StringComparer.Ordinal)
            {
                NotDeclared,
                Taxpayer,
                TradesmanExemption,
                PendingSetup
            };

        public static bool IsValid(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   All.Contains(value);
        }
    }
}

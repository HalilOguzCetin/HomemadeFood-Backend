namespace HomemadeFood.Api.Constants
{
    public static class FoodUnitTypes
    {
        public const string Portion = "Portion";
        public const string Tray = "Tray";
        public const string Piece = "Piece";
        public const string Kilogram = "Kilogram";
        public const string Liter = "Liter";
        public const string Package = "Package";
        public const string Other = "Other";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                Portion,
                Tray,
                Piece,
                Kilogram,
                Liter,
                Package,
                Other
            };
    }
}

namespace HomemadeFood.Api.Constants
{
    /*
     * Yapılandırılmış alerjen kodları.
     *
     * Serbest metin yerine sabit kod tutulur. Böylece Android,
     * sipariş snapshot'ı ve ileride filtreleme/uyarı sistemleri
     * aynı anlamı güvenilir biçimde kullanabilir.
     */
    public static class FoodAllergenCodes
    {
        public const string Gluten = "Gluten";
        public const string Crustaceans = "Crustaceans";
        public const string Eggs = "Eggs";
        public const string Fish = "Fish";
        public const string Peanuts = "Peanuts";
        public const string Soybeans = "Soybeans";
        public const string Milk = "Milk";
        public const string Nuts = "Nuts";
        public const string Celery = "Celery";
        public const string Mustard = "Mustard";
        public const string Sesame = "Sesame";
        public const string Sulphites = "Sulphites";
        public const string Lupin = "Lupin";
        public const string Molluscs = "Molluscs";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                Gluten,
                Crustaceans,
                Eggs,
                Fish,
                Peanuts,
                Soybeans,
                Milk,
                Nuts,
                Celery,
                Mustard,
                Sesame,
                Sulphites,
                Lupin,
                Molluscs
            };
    }
}

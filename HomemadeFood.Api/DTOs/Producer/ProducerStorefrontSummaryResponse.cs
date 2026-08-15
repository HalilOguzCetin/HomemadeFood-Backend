namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerStorefrontSummaryResponse
    {
        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string? BusinessImageUrl { get; set; }

        public decimal Rating { get; set; }

        /*
         * Ana sayfa kartında tam işletme adresi yerine
         * yalnızca şehir/ilçe özet bilgisi kullanılır.
         */
        public string City { get; set; } =
            string.Empty;

        public string District { get; set; } =
            string.Empty;

        /*
         * İşletmenin satışta olan toplam yemek sayısı.
         */
        public int AvailableFoodCount { get; set; }

        /*
         * İşletmenin satışta yemek bulunan farklı kategori sayısı.
         */
        public int AvailableCategoryCount { get; set; }

        /*
         * categoryId verildiyse seçilen kategorideki yemek sayısı,
         * verilmediyse AvailableFoodCount ile aynı değerdir.
         */
        public int MatchingFoodCount { get; set; }

        /*
         * Seçili filtreye uyan yemekler arasındaki en kısa
         * hazırlanma süresi. Uygun yemek yoksa null olur;
         * ancak storefront sorgusu zaten en az bir uygun yemek
         * şartı uyguladığı için normal akışta değer bulunur.
         */
        public int? MinimumPreparationTimeMinutes
        {
            get;
            set;
        }
    }
}
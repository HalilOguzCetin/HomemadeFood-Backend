namespace HomemadeFood.Api.DTOs.Producer
{
    /*
     * Popüler işletmeler endpoint'i için response modeli.
     *
     * Normal storefront alanlarını korur; yalnız popülerliğe
     * özgü açıklayıcı alanları ekler.
     */
    public class PopularProducerStorefrontResponse :
        ProducerStorefrontSummaryResponse
    {
        /*
         * 0-100 aralığında normalize edilmiş final skor.
         */
        public double PopularityScore { get; set; }

        public int DeliveredOrderCount30Days
        {
            get;
            set;
        }

        public int DistinctCustomerCount30Days
        {
            get;
            set;
        }

        public int ReviewCount { get; set; }

        public int FavoriteCount { get; set; }
    }
}
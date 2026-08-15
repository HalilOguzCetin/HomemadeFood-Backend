namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerStorefrontMenuResponse
    {
        public int ProducerProfileId { get; set; }

        public string BusinessName { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public string? BusinessImageUrl { get; set; }

        public decimal Rating { get; set; }

        /*
         * Müşteriye işletmenin tam açık adresini açmıyoruz.
         * Menü ekranı için şehir / ilçe özeti yeterlidir.
         */
        public string City { get; set; } =
            string.Empty;

        public string District { get; set; } =
            string.Empty;

        public int AvailableFoodCount { get; set; }

        public int AvailableCategoryCount { get; set; }

        /*
         * Boş kategoriler response'a eklenmez.
         * Yalnızca aktif ve satışta yemeği bulunan kategoriler gelir.
         */
        public List<ProducerStorefrontMenuCategoryResponse>
            Categories
        { get; set; } =
            new();
    }
}
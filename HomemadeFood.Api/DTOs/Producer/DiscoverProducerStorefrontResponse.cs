namespace HomemadeFood.Api.DTOs.Producer
{
    /*
     * Keşfet ekranına dönen işletme.
     *
     * Producer koordinatları gizli kalır;
     * yalnız müşterinin seçili adresine göre
     * hesaplanan distanceKm gönderilir.
     */
    public class DiscoverProducerStorefrontResponse :
        ProducerStorefrontSummaryResponse
    {
        public double DistanceKm { get; set; }

        public double PopularityScore { get; set; }
    }
}
namespace HomemadeFood.Api.DTOs.Producer
{
    public class NearbyProducerStorefrontResponse :
        ProducerStorefrontSummaryResponse
    {
        public double DistanceKm { get; set; }
    }
}
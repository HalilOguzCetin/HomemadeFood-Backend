namespace HomemadeFood.Api.DTOs.Food
{
    /*
     * Android'e producer'ın gerçek koordinatları
     * kesinlikle gönderilmez.
     *
     * Yalnız müşterinin seçili adresine göre
     * hesaplanan yaklaşık mesafe döner.
     */
    public class DiscoverFoodResponse :
        FoodResponse
    {
        public double DistanceKm { get; set; }

        public double PopularityScore { get; set; }
    }
}
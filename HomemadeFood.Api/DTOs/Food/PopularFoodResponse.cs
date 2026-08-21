namespace HomemadeFood.Api.DTOs.Food
{
    /*
     * H5A:
     * Home Popüler Yemekler carousel'i için response.
     *
     * FoodResponse alanlarını korur ve popülerlik
     * açıklama/test metriklerini ekler.
     */
    public class PopularFoodResponse :
        FoodResponse
    {
        public double PopularityScore { get; set; }

        public int DeliveredOrderCount30Days
        {
            get;
            set;
        }

        public int SoldQuantity30Days
        {
            get;
            set;
        }

        public int DistinctCustomerCount30Days
        {
            get;
            set;
        }

        public int FavoriteCount { get; set; }
    }
}
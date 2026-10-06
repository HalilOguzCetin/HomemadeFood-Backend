namespace HomemadeFood.Api.Entities
{
    public class FoodAllergen
    {
        public int FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public string AllergenCode { get; set; } =
            string.Empty;
    }
}

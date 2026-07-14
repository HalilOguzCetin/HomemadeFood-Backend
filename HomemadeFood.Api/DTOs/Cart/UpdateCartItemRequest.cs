using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Cart
{
    public class UpdateCartItemRequest
    {
        [Range(
            1,
            50,
            ErrorMessage = "Miktar 1 ile 50 arasında olmalıdır.")]
        public int Quantity { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Order
{
    public class CreateOrderRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Geçerli bir teslimat adresi seçilmelidir.")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Ödeme yöntemi zorunludur.")]
        [RegularExpression(
            "^(CashOnDelivery|CardOnDelivery)$",
            ErrorMessage =
                "Ödeme yöntemi CashOnDelivery veya CardOnDelivery olmalıdır.")]
        public string PaymentMethod { get; set; }
            = "CashOnDelivery";

        [MaxLength(
            500,
            ErrorMessage = "Sipariş notu en fazla 500 karakter olabilir.")]
        public string CustomerNote { get; set; }
            = string.Empty;
    }
}
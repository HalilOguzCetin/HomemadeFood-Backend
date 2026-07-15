using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Review
{
    public class CreateReviewRequest
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Geçerli bir sipariş seçilmelidir.")]
        public int OrderId { get; set; }

        [Range(
            1,
            5,
            ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [MaxLength(
            1000,
            ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string Comment { get; set; } = string.Empty;
    }
}
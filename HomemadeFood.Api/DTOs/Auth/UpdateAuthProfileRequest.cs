using System.ComponentModel.DataAnnotations;

namespace HomemadeFood.Api.DTOs.Auth
{
    /*
     * C2 aşamasında yalnızca güvenli biçimde doğrudan
     * değiştirilebilecek temel profil alanını güncelliyoruz.
     *
     * E-posta ve telefon burada değiştirilmez.
     * Bu alanlar doğrulama challenge akışlarıyla
     * ayrı endpoint'lerde ele alınacaktır.
     */
    public class UpdateAuthProfileRequest
    {
        [Required(
            ErrorMessage =
                "Ad soyad zorunludur.")]
        [MinLength(
            2,
            ErrorMessage =
                "Ad soyad en az 2 karakter olmalıdır.")]
        [MaxLength(
            100,
            ErrorMessage =
                "Ad soyad en fazla 100 karakter olabilir.")]
        public string FullName { get; set; } =
            string.Empty;
    }
}
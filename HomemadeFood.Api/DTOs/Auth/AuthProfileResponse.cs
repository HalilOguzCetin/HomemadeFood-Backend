namespace HomemadeFood.Api.DTOs.Auth
{
    public class AuthProfileResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        /*
         * Telefon C3'te OTP ile doğrulanacak/değiştirilecek.
         * C2 profil ekranı mevcut değeri güvenli biçimde
         * görüntüleyebilmek için bu alanı alır.
         */
        public string Phone { get; set; } =
            string.Empty;

        public bool IsPhoneVerified { get; set; }

        public DateTime? PhoneVerifiedAt
        {
            get;
            set;
        }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAt
        {
            get;
            set;
        }

        public string Role { get; set; } =
            string.Empty;

        public bool CanUseProducerMode
        {
            get;
            set;
        }

        public int? ProducerProfileId
        {
            get;
            set;
        }

        public string? ProducerVerificationStatus
        {
            get;
            set;
        }
    }
}
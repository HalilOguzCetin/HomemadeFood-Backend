namespace HomemadeFood.Api.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string PasswordHash { get; set; } =
            string.Empty;

        public string Phone { get; set; } =
            string.Empty;

        /*
         * Telefon karşılaştırmaları ve uniqueness için
         * canonical biçim: +905XXXXXXXXX.
         * Eski / doğrulanmamış hesaplarda null olabilir.
         */
        public string? NormalizedPhone
        {
            get;
            set;
        }

        public bool IsPhoneVerified
        {
            get;
            set;
        } = false;

        public DateTime? PhoneVerifiedAt
        {
            get;
            set;
        }

        public string Role { get; set; } =
            "Customer";

        public bool IsActive { get; set; } =
            true;

        /*
         * Yeni kayıtlar e-posta doğrulanmadan
         * başlar. Mevcut hesapları migration
         * sırasında doğrulanmış kabul edeceğiz.
         */
        public bool IsEmailVerified { get; set; } =
            false;

        /*
         * E-posta doğrulamasının tamamlandığı
         * UTC zaman. Henüz doğrulanmadıysa null.
         */
        public DateTime? EmailVerifiedAt
        {
            get;
            set;
        }
        /*
 * Kullanıcının oturum sürümü.
 *
 * Şifre sıfırlama gibi kritik güvenlik
 * işlemlerinde artırılır. Böylece daha önce
 * oluşturulan JWT'ler geçersiz hale gelir.
 */
        public int TokenVersion { get; set; } = 0;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        /*
         * Art arda başarısız giriş sayısı.
         * Başarılı girişten sonra sıfırlanır.
         */
        public int FailedLoginCount { get; set; } =
            0;

        /*
         * Hesabın geçici kilidinin biteceği UTC zaman.
         * Null ise hesap giriş açısından kilitli değildir.
         */
        public DateTime? LockoutEndAt { get; set; }

        /*
         * Son başarısız giriş denemesinin UTC zamanı.
         * İleride güvenlik kayıtları için kullanılabilir.
         */
        public DateTime? LastFailedLoginAt
        {
            get;
            set;
        }

        /*
         * Son başarılı girişin UTC zamanı.
         */
        public DateTime? LastLoginAt { get; set; }

        public ProducerProfile? ProducerProfile
        {
            get;
            set;
        }

        public ICollection<VerificationChallenge>
            VerificationChallenges
        {
            get;
            set;
        } = new List<VerificationChallenge>();

        public ICollection<Address> Addresses
        {
            get;
            set;
        } = new List<Address>();

        public ICollection<Order> Orders
        {
            get;
            set;
        } = new List<Order>();

        public ICollection<Review> Reviews
        {
            get;
            set;
        } = new List<Review>();

        public ICollection<Favorite> Favorites
        {
            get;
            set;
        } = new List<Favorite>();

        public Cart? Cart { get; set; }
    }
}
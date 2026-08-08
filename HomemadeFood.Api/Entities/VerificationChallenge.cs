namespace HomemadeFood.Api.Entities
{
    public class VerificationChallenge
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        /*
         * Örn:
         * EmailVerification
         * PasswordReset
         * PhoneVerification
         */
        public string Type { get; set; } =
            string.Empty;

        /*
         * Doğrulama hedefini (e-posta/telefon)
         * veritabanında tekrar açık metin olarak
         * saklamamak için hash değeri tutulur.
         */
        public string TargetHash { get; set; } =
            string.Empty;

        /*
         * Kullanıcıya gönderilen kod/token açık
         * metin olarak saklanmaz. Yalnızca güvenli
         * hash değeri veritabanında tutulur.
         */
        public string SecretHash { get; set; } =
            string.Empty;

        public DateTime ExpiresAt { get; set; }

        /*
         * Challenge başarıyla kullanıldığında
         * UTC zaman yazılır. Null ise henüz
         * kullanılmamıştır.
         */
        public DateTime? UsedAt { get; set; }

        /*
         * Hatalı doğrulama denemelerini sınırlamak
         * için kullanılacaktır.
         */
        public int AttemptCount { get; set; } =
            0;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public User User { get; set; } =
            null!;
    }
}
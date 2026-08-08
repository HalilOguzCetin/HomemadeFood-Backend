using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class VerificationChallengeService :
        IVerificationChallengeService
    {
        private static readonly TimeSpan
            EmailVerificationLifetime =
                TimeSpan.FromMinutes(10);

        private const int
            MaxEmailVerificationAttempts =
                5;

        private readonly
            IVerificationChallengeRepository
            _verificationChallengeRepository;

        private readonly
            IVerificationCodeService
            _verificationCodeService;

        private readonly
            IAppClock
            _appClock;

        private readonly
            IUserRepository
            _userRepository;
        private static readonly TimeSpan
    EmailVerificationResendCooldown =
        TimeSpan.FromMinutes(1);

        public VerificationChallengeService(
            IVerificationChallengeRepository
                verificationChallengeRepository,
            IVerificationCodeService
                verificationCodeService,
            IAppClock appClock,
            IUserRepository userRepository)
        {
            _verificationChallengeRepository =
                verificationChallengeRepository;

            _verificationCodeService =
                verificationCodeService;

            _appClock =
                appClock;

            _userRepository =
                userRepository;
        }
        public async Task<string?>
    PrepareEmailVerificationResendAsync(
        string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail =
                email
                    .Trim()
                    .ToLowerInvariant();

            var now =
                _appClock.UtcNow;

            var user =
                await _userRepository
                    .GetByEmailAsync(
                        normalizedEmail);

            /*
             * Hesap yoksa, pasifse veya zaten
             * doğrulanmışsa yeni challenge
             * oluşturulmaz.
             */
            if (
                user == null ||
                !user.IsActive ||
                user.IsEmailVerified
            )
            {
                return null;
            }

            var currentChallenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        VerificationChallengeTypes
                            .EmailVerification,
                        now);

            /*
             * Son kodun oluşturulmasından itibaren
             * en az 60 saniye geçmeden yeni kod
             * oluşturulamaz.
             */
            if (
                currentChallenge != null &&
                now - currentChallenge.CreatedAt <
                    EmailVerificationResendCooldown
            )
            {
                return null;
            }

            /*
             * Varsa eski aktif kod artık
             * kullanılamaz hâle getirilir.
             */
            await _verificationChallengeRepository
                .ExpireActiveAsync(
                    user.Id,
                    VerificationChallengeTypes
                        .EmailVerification,
                    now);

            var verificationCode =
                _verificationCodeService
                    .GenerateSixDigitCode();

            var challenge =
                new VerificationChallenge
                {
                    UserId =
                        user.Id,

                    User =
                        user,

                    Type =
                        VerificationChallengeTypes
                            .EmailVerification,

                    TargetHash =
                        _verificationCodeService
                            .HashTarget(
                                user.Email),

                    SecretHash =
                        _verificationCodeService
                            .HashSecret(
                                verificationCode),

                    ExpiresAt =
                        now.Add(
                            EmailVerificationLifetime),

                    UsedAt =
                        null,

                    AttemptCount =
                        0,

                    CreatedAt =
                        now
                };

            await _verificationChallengeRepository
                .AddAsync(
                    challenge);

            await _verificationChallengeRepository
                .SaveChangesAsync();

            return verificationCode;
        }

        public async Task<string>
            PrepareEmailVerificationAsync(
                User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(
                    nameof(user));
            }

            if (string.IsNullOrWhiteSpace(
                    user.Email))
            {
                throw new InvalidOperationException(
                    "E-posta doğrulama challenge'ı için kullanıcı e-postası gereklidir.");
            }

            var now =
                _appClock.UtcNow;

            var verificationCode =
                _verificationCodeService
                    .GenerateSixDigitCode();

            var challenge =
                new VerificationChallenge
                {
                    /*
                     * User navigation verilerek yeni
                     * kullanıcı ve challenge aynı
                     * SaveChanges işlemi içinde
                     * kaydedilebilir.
                     */
                    User =
                        user,

                    Type =
                        VerificationChallengeTypes
                            .EmailVerification,

                    TargetHash =
                        _verificationCodeService
                            .HashTarget(
                                user.Email),

                    /*
                     * Gerçek doğrulama kodu
                     * veritabanına yazılmaz.
                     * Yalnızca güvenli hash değeri
                     * saklanır.
                     */
                    SecretHash =
                        _verificationCodeService
                            .HashSecret(
                                verificationCode),

                    ExpiresAt =
                        now.Add(
                            EmailVerificationLifetime),

                    UsedAt =
                        null,

                    AttemptCount =
                        0,

                    CreatedAt =
                        now
                };

            await _verificationChallengeRepository
                .AddAsync(
                    challenge);

            /*
             * Düz kod yalnızca e-posta gönderme
             * katmanına aktarılmak üzere döndürülür.
             */
            return verificationCode;
        }

        public async Task<bool>
            VerifyEmailAsync(
                string email,
                string code)
        {
            if (
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code)
            )
            {
                return false;
            }

            var normalizedEmail =
                email
                    .Trim()
                    .ToLowerInvariant();

            var normalizedCode =
                code.Trim();

            var now =
                _appClock.UtcNow;

            var user =
                await _userRepository
                    .GetByEmailAsync(
                        normalizedEmail);

            /*
             * Kullanıcı bulunamadığında veya hesap
             * pasif olduğunda ayrıntılı bilgi
             * dışarı verilmez.
             */
            if (
                user == null ||
                !user.IsActive
            )
            {
                return false;
            }

            /*
             * Daha önce doğrulanmış bir adres için
             * işlem idempotent kabul edilir.
             */
            if (user.IsEmailVerified)
            {
                return true;
            }

            var challenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        VerificationChallengeTypes
                            .EmailVerification,
                        now);

            /*
             * Aktif challenge yoksa kodun süresi
             * dolmuş, kullanılmış veya hiç
             * oluşturulmamış olabilir.
             */
            if (challenge == null)
            {
                return false;
            }

            /*
             * Deneme sınırına daha önce ulaşılmışsa
             * challenge geçersiz hâle getirilir.
             */
            if (
                challenge.AttemptCount >=
                MaxEmailVerificationAttempts
            )
            {
                challenge.ExpiresAt =
                    now;

                await _verificationChallengeRepository
                    .SaveChangesAsync();

                return false;
            }

            var isCodeValid =
                _verificationCodeService
                    .VerifySecret(
                        normalizedCode,
                        challenge.SecretHash);

            if (!isCodeValid)
            {
                challenge.AttemptCount++;

                /*
                 * Beşinci hatalı denemeden sonra
                 * challenge hemen geçersiz olur.
                 */
                if (
                    challenge.AttemptCount >=
                    MaxEmailVerificationAttempts
                )
                {
                    challenge.ExpiresAt =
                        now;
                }

                await _verificationChallengeRepository
                    .SaveChangesAsync();

                return false;
            }

            /*
             * Doğru kod girildi.
             *
             * Kullanıcı doğrulanır ve challenge
             * tek kullanımlık olacak şekilde
             * işaretlenir.
             */
            user.IsEmailVerified =
                true;

            user.EmailVerifiedAt =
                now;

            challenge.UsedAt =
                now;

            await _verificationChallengeRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
using BCrypt.Net;
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

        private static readonly TimeSpan
            PasswordResetLifetime =
                TimeSpan.FromMinutes(10);

        private static readonly TimeSpan
            PhoneVerificationLifetime =
                TimeSpan.FromMinutes(10);

        private static readonly TimeSpan
            EmailVerificationResendCooldown =
                TimeSpan.FromMinutes(1);

        private static readonly TimeSpan
            PasswordResetResendCooldown =
                TimeSpan.FromMinutes(1);

        private static readonly TimeSpan
            PhoneVerificationResendCooldown =
                TimeSpan.FromMinutes(1);

        private const int
            MaxEmailVerificationAttempts =
                5;

        private const int
            MaxPasswordResetAttempts =
                5;

        private const int
            MaxPhoneVerificationAttempts =
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

            if (
                currentChallenge != null &&
                now - currentChallenge.CreatedAt <
                    EmailVerificationResendCooldown
            )
            {
                return null;
            }

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

            if (
                user == null ||
                !user.IsActive
            )
            {
                return false;
            }

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

            if (challenge == null)
            {
                return false;
            }

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

        public async Task<string?>
            PreparePhoneVerificationAsync(
                User user,
                string normalizedPhone)
        {
            if (
                user == null ||
                !user.IsActive ||
                string.IsNullOrWhiteSpace(
                    normalizedPhone)
            )
            {
                return null;
            }

            if (
                user.IsPhoneVerified &&
                string.Equals(
                    user.NormalizedPhone,
                    normalizedPhone,
                    StringComparison.Ordinal)
            )
            {
                return null;
            }

            var phoneOwner =
                await _userRepository
                    .GetByNormalizedPhoneAsync(
                        normalizedPhone);

            if (
                phoneOwner != null &&
                phoneOwner.Id != user.Id
            )
            {
                return null;
            }

            var now =
                _appClock.UtcNow;

            var challengeType =
                ResolvePhoneChallengeType(
                    user,
                    normalizedPhone);

            var currentChallenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        challengeType,
                        now);

            if (
                currentChallenge != null &&
                now - currentChallenge.CreatedAt <
                    PhoneVerificationResendCooldown
            )
            {
                return null;
            }

            await _verificationChallengeRepository
                .ExpireActiveAsync(
                    user.Id,
                    VerificationChallengeTypes
                        .PhoneVerification,
                    now);

            await _verificationChallengeRepository
                .ExpireActiveAsync(
                    user.Id,
                    VerificationChallengeTypes
                        .PhoneChange,
                    now);

            var verificationCode =
                _verificationCodeService
                    .GenerateSixDigitCode();

            var challenge =
                new VerificationChallenge
                {
                    UserId = user.Id,
                    User = user,
                    Type = challengeType,

                    TargetHash =
                        _verificationCodeService
                            .HashTarget(
                                normalizedPhone),

                    SecretHash =
                        _verificationCodeService
                            .HashSecret(
                                verificationCode),

                    ExpiresAt =
                        now.Add(
                            PhoneVerificationLifetime),

                    UsedAt = null,
                    AttemptCount = 0,
                    CreatedAt = now
                };

            await _verificationChallengeRepository
                .AddAsync(
                    challenge);

            await _verificationChallengeRepository
                .SaveChangesAsync();

            return verificationCode;
        }

        public async Task<bool>
            VerifyPhoneAsync(
                User user,
                string normalizedPhone,
                string code)
        {
            if (
                user == null ||
                !user.IsActive ||
                string.IsNullOrWhiteSpace(
                    normalizedPhone) ||
                string.IsNullOrWhiteSpace(code)
            )
            {
                return false;
            }

            var normalizedCode =
                code.Trim();

            var now =
                _appClock.UtcNow;

            var challengeType =
                ResolvePhoneChallengeType(
                    user,
                    normalizedPhone);

            var challenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        challengeType,
                        now);

            if (challenge == null)
            {
                return false;
            }

            if (
                challenge.AttemptCount >=
                    MaxPhoneVerificationAttempts
            )
            {
                challenge.ExpiresAt =
                    now;

                await _verificationChallengeRepository
                    .SaveChangesAsync();

                return false;
            }

            var isTargetValid =
                _verificationCodeService
                    .VerifyTarget(
                        normalizedPhone,
                        challenge.TargetHash);

            var isCodeValid =
                _verificationCodeService
                    .VerifySecret(
                        normalizedCode,
                        challenge.SecretHash);

            if (
                !isTargetValid ||
                !isCodeValid
            )
            {
                challenge.AttemptCount++;

                if (
                    challenge.AttemptCount >=
                        MaxPhoneVerificationAttempts
                )
                {
                    challenge.ExpiresAt =
                        now;
                }

                await _verificationChallengeRepository
                    .SaveChangesAsync();

                return false;
            }

            var phoneOwner =
                await _userRepository
                    .GetByNormalizedPhoneAsync(
                        normalizedPhone);

            if (
                phoneOwner != null &&
                phoneOwner.Id != user.Id
            )
            {
                challenge.ExpiresAt =
                    now;

                await _verificationChallengeRepository
                    .SaveChangesAsync();

                return false;
            }

            user.Phone =
                normalizedPhone;

            user.NormalizedPhone =
                normalizedPhone;

            user.IsPhoneVerified =
                true;

            user.PhoneVerifiedAt =
                now;

            challenge.UsedAt =
                now;

            await _verificationChallengeRepository
                .SaveChangesAsync();

            return true;
        }

        private static string
            ResolvePhoneChallengeType(
                User user,
                string normalizedPhone)
        {
            if (
                user.IsPhoneVerified &&
                !string.IsNullOrWhiteSpace(
                    user.NormalizedPhone) &&
                !string.Equals(
                    user.NormalizedPhone,
                    normalizedPhone,
                    StringComparison.Ordinal)
            )
            {
                return VerificationChallengeTypes
                    .PhoneChange;
            }

            return VerificationChallengeTypes
                .PhoneVerification;
        }

        public async Task<string?>
            PreparePasswordResetAsync(
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
             * Hesabın varlığı, aktiflik durumu veya
             * e-posta doğrulama durumu dışarıya
             * açıklanmaz.
             *
             * Şifre sıfırlama yalnızca doğrulanmış
             * e-posta adresine sahip aktif hesaplar
             * için oluşturulur.
             */
            if (
                user == null ||
                !user.IsActive ||
                !user.IsEmailVerified
            )
            {
                return null;
            }

            var currentChallenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        VerificationChallengeTypes
                            .PasswordReset,
                        now);

            /*
             * 60 saniyelik yeniden gönderme
             * cooldown'u.
             */
            if (
                currentChallenge != null &&
                now - currentChallenge.CreatedAt <
                    PasswordResetResendCooldown
            )
            {
                return null;
            }

            /*
             * Yeni kod oluşturulmadan önce eski
             * aktif PasswordReset challenge'ları
             * geçersizleştirilir.
             */
            await _verificationChallengeRepository
                .ExpireActiveAsync(
                    user.Id,
                    VerificationChallengeTypes
                        .PasswordReset,
                    now);

            var resetCode =
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
                            .PasswordReset,

                    TargetHash =
                        _verificationCodeService
                            .HashTarget(
                                user.Email),

                    SecretHash =
                        _verificationCodeService
                            .HashSecret(
                                resetCode),

                    ExpiresAt =
                        now.Add(
                            PasswordResetLifetime),

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

            return resetCode;
        }

        public async Task<bool>
            ResetPasswordAsync(
                string email,
                string code,
                string newPassword)
        {
            if (
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(newPassword)
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

            if (
                user == null ||
                !user.IsActive ||
                !user.IsEmailVerified
            )
            {
                return false;
            }

            var challenge =
                await _verificationChallengeRepository
                    .GetLatestActiveAsync(
                        user.Id,
                        VerificationChallengeTypes
                            .PasswordReset,
                        now);

            if (challenge == null)
            {
                return false;
            }

            if (
                challenge.AttemptCount >=
                MaxPasswordResetAttempts
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

                if (
                    challenge.AttemptCount >=
                    MaxPasswordResetAttempts
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
             * Kod doğruysa şifre yalnızca BCrypt
             * hash'i olarak değiştirilir.
             */
            user.PasswordHash =
                BCrypt.Net.BCrypt
                    .HashPassword(
                        newPassword);
            /*
 * Şifre sıfırlandığında mevcut bütün
 * JWT oturumları geçersiz hale gelir.
 */
            user.TokenVersion++;

            /*
             * Kullanıcı doğru sıfırlama kodunu
             * kanıtladığı için eski login kilidi
             * temizlenir.
             */
            user.FailedLoginCount =
                0;

            user.LockoutEndAt =
                null;

            challenge.UsedAt =
                now;

            await _verificationChallengeRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
using BCrypt.Net;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Auth;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository
            _userRepository;

        private readonly IJwtTokenGenerator
            _jwtTokenGenerator;

        private readonly IAppClock
            _appClock;

        private readonly
            IVerificationChallengeService
            _verificationChallengeService;

        private readonly
            IEmailSender
            _emailSender;

        private readonly
            IPhoneVerificationSender
            _phoneVerificationSender;

        private readonly
            IPhoneNumberNormalizer
            _phoneNumberNormalizer;

        private const int MaxFailedLoginAttempts =
            5;

        private static readonly TimeSpan
            LoginLockoutDuration =
                TimeSpan.FromMinutes(15);

        private static readonly string
            DummyPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "HomemadeFood-Dummy-Login-Password",
                    12);

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IAppClock appClock,
            IVerificationChallengeService
                verificationChallengeService,
            IEmailSender emailSender,
            IPhoneVerificationSender
                phoneVerificationSender,
            IPhoneNumberNormalizer
                phoneNumberNormalizer)
        {
            _userRepository =
                userRepository;

            _jwtTokenGenerator =
                jwtTokenGenerator;

            _appClock =
                appClock;

            _verificationChallengeService =
                verificationChallengeService;

            _emailSender =
                emailSender;

            _phoneVerificationSender =
                phoneVerificationSender;

            _phoneNumberNormalizer =
                phoneNumberNormalizer;
        }

        public async Task
            ResendEmailVerificationAsync(
                ResendEmailVerificationRequest request)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var verificationCode =
                await _verificationChallengeService
                    .PrepareEmailVerificationResendAsync(
                        normalizedEmail);

            if (verificationCode == null)
            {
                return;
            }

            await _emailSender
                .SendEmailVerificationCodeAsync(
                    normalizedEmail,
                    verificationCode);
        }
        public async Task ForgotPasswordAsync(
    ForgotPasswordRequest request)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var resetCode =
                await _verificationChallengeService
                    .PreparePasswordResetAsync(
                        normalizedEmail);

            /*
             * null olması:
             * - kullanıcı yok,
             * - hesap pasif,
             * - e-posta doğrulanmamış,
             * - cooldown devam ediyor
             *
             * olabilir.
             *
             * Bu ayrım dışarıya verilmez.
             */
            if (resetCode == null)
            {
                return;
            }

            await _emailSender
                .SendPasswordResetCodeAsync(
                    normalizedEmail,
                    resetCode);
        }

        public async Task<bool> ResetPasswordAsync(
            ResetPasswordRequest request)
        {
            return await
                _verificationChallengeService
                    .ResetPasswordAsync(
                        request.Email,
                        request.Code,
                        request.NewPassword);
        }

        public async Task<bool> RegisterAsync(
            RegisterRequest request)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var existingUser =
                await _userRepository
                    .GetByEmailAsync(
                        normalizedEmail);

            if (existingUser != null)
            {
                return false;
            }

            var user =
                new User
                {
                    FullName =
                        request.FullName.Trim(),

                    Email =
                        normalizedEmail,

                    PasswordHash =
                        BCrypt.Net.BCrypt
                            .HashPassword(
                                request.Password),

                    Phone =
                        string.Empty,

                    Role =
                        UserRoles.Customer,

                    IsActive =
                        true,

                    IsEmailVerified =
                        false,

                    EmailVerifiedAt =
                        null,

                    CreatedAt =
                        _appClock.UtcNow
                };

            await _userRepository
                .AddAsync(user);

            var verificationCode =
                await _verificationChallengeService
                    .PrepareEmailVerificationAsync(
                        user);

            await _userRepository
                .SaveChangesAsync();

            await _emailSender
                .SendEmailVerificationCodeAsync(
                    normalizedEmail,
                    verificationCode);

            return true;
        }

        public async Task<bool>
            VerifyEmailAsync(
                VerifyEmailRequest request)
        {
            return await
                _verificationChallengeService
                    .VerifyEmailAsync(
                        request.Email,
                        request.Code);
        }

        public async Task<LoginServiceResult>
            LoginAsync(
                LoginRequest request)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var user =
                await _userRepository
                    .GetByEmailAsync(
                        normalizedEmail);

            var passwordHashToVerify =
                user?.PasswordHash ??
                DummyPasswordHash;

            var isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    passwordHashToVerify);

            var now =
                _appClock.UtcNow;

            if (user == null)
            {
                return LoginServiceResult
                    .InvalidCredentials();
            }

            if (!user.IsActive)
            {
                return LoginServiceResult
                    .InvalidCredentials();
            }

            if (
                user.LockoutEndAt.HasValue &&
                user.LockoutEndAt.Value > now
            )
            {
                return LoginServiceResult
                    .InvalidCredentials();
            }

            if (
                user.LockoutEndAt.HasValue &&
                user.LockoutEndAt.Value <= now
            )
            {
                user.LockoutEndAt =
                    null;

                user.FailedLoginCount =
                    0;
            }

            if (!isPasswordValid)
            {
                user.FailedLoginCount++;

                user.LastFailedLoginAt =
                    now;

                if (
                    user.FailedLoginCount >=
                    MaxFailedLoginAttempts
                )
                {
                    user.LockoutEndAt =
                        now.Add(
                            LoginLockoutDuration);
                }

                await _userRepository
                    .SaveChangesAsync();

                return LoginServiceResult
                    .InvalidCredentials();
            }

            /*
             * E-posta doğrulanmadı bilgisi yalnızca
             * şifre doğruysa döndürülür. Bu nedenle
             * hesap enumeration riski azaltılır.
             */
            if (!user.IsEmailVerified)
            {
                user.FailedLoginCount =
                    0;

                user.LockoutEndAt =
                    null;

                await _userRepository
                    .SaveChangesAsync();

                return LoginServiceResult
                    .EmailNotVerified(
                        user.Email);
            }

            user.FailedLoginCount =
                0;

            user.LockoutEndAt =
                null;

            user.LastLoginAt =
                now;

            await _userRepository
                .SaveChangesAsync();

            var token =
                _jwtTokenGenerator
                    .GenerateToken(user);

            var response =
                new LoginResponse
                {
                    UserId =
                        user.Id,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    Role =
                        user.Role,

                    CanUseProducerMode =
                        CanUseProducerMode(
                            user),

                    ProducerProfileId =
                        user.ProducerProfile?.Id,

                    ProducerVerificationStatus =
                        user.ProducerProfile?
                            .VerificationStatus,

                    Token =
                        token
                };

            return LoginServiceResult
                .Success(
                    response);
        }

        public async Task<bool>
            RequestPhoneVerificationAsync(
                int userId,
                RequestPhoneVerificationRequest request)
        {
            if (
                userId <= 0 ||
                request == null ||
                !_phoneNumberNormalizer
                    .TryNormalizeTurkishMobile(
                        request.Phone,
                        out var normalizedPhone)
            )
            {
                return false;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(
                        userId);

            if (
                user == null ||
                !user.IsActive ||
                !string.Equals(
                    user.Role,
                    UserRoles.Customer,
                    StringComparison.Ordinal)
            )
            {
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
                return false;
            }

            var verificationCode =
                await _verificationChallengeService
                    .PreparePhoneVerificationAsync(
                        user,
                        normalizedPhone);

            if (verificationCode == null)
            {
                return false;
            }

            await _phoneVerificationSender
                .SendPhoneVerificationCodeAsync(
                    normalizedPhone,
                    verificationCode);

            return true;
        }

        public async Task<AuthProfileResponse?>
            VerifyPhoneAsync(
                int userId,
                VerifyPhoneRequest request)
        {
            if (
                userId <= 0 ||
                request == null ||
                !_phoneNumberNormalizer
                    .TryNormalizeTurkishMobile(
                        request.Phone,
                        out var normalizedPhone)
            )
            {
                return null;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(
                        userId);

            if (
                user == null ||
                !user.IsActive ||
                !string.Equals(
                    user.Role,
                    UserRoles.Customer,
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

            var verified =
                await _verificationChallengeService
                    .VerifyPhoneAsync(
                        user,
                        normalizedPhone,
                        request.Code);

            if (!verified)
            {
                return null;
            }

            return MapToProfileResponse(
                user);
        }

        public async Task<AuthProfileResponse?>
            GetProfileAsync(
                int userId)
        {
            if (userId <= 0)
            {
                return null;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(
                        userId);

            if (
                user == null ||
                !user.IsActive
            )
            {
                return null;
            }

            return MapToProfileResponse(
                user);
        }

        public async Task<AuthProfileResponse?>
            UpdateProfileAsync(
                int userId,
                UpdateAuthProfileRequest request)
        {
            if (
                userId <= 0 ||
                request == null
            )
            {
                return null;
            }

            var normalizedFullName =
                request.FullName
                    .Trim();

            /*
             * DataAnnotations normal istekleri korur.
             * Whitespace-only değerinin validasyondan
             * kaçmasını ayrıca engelliyoruz.
             */
            if (
                string.IsNullOrWhiteSpace(
                    normalizedFullName) ||
                normalizedFullName.Length < 2 ||
                normalizedFullName.Length > 100
            )
            {
                return null;
            }

            var user =
                await _userRepository
                    .GetByIdAsync(
                        userId);

            if (
                user == null ||
                !user.IsActive ||
                !string.Equals(
                    user.Role,
                    UserRoles.Customer,
                    StringComparison.Ordinal)
            )
            {
                return null;
            }

            user.FullName =
                normalizedFullName;

            await _userRepository
                .SaveChangesAsync();

            return MapToProfileResponse(
                user);
        }

        private static AuthProfileResponse
            MapToProfileResponse(
                User user)
        {
            return new AuthProfileResponse
            {
                UserId =
                    user.Id,

                FullName =
                    user.FullName,

                Email =
                    user.Email,

                Phone =
                    user.Phone,

                IsPhoneVerified =
                    user.IsPhoneVerified,

                PhoneVerifiedAt =
                    user.PhoneVerifiedAt,

                IsEmailVerified =
                    user.IsEmailVerified,

                EmailVerifiedAt =
                    user.EmailVerifiedAt,

                Role =
                    user.Role,

                CanUseProducerMode =
                    CanUseProducerMode(
                        user),

                ProducerProfileId =
                    user.ProducerProfile?.Id,

                ProducerVerificationStatus =
                    user.ProducerProfile?
                        .VerificationStatus
            };
        }

        private static bool CanUseProducerMode(
            User user)
        {
            return
                user.IsActive &&

                string.Equals(
                    user.Role,
                    UserRoles.Customer,
                    StringComparison.Ordinal) &&

                user.ProducerProfile != null &&

                user.ProducerProfile.IsApproved &&

                string.Equals(
                    user.ProducerProfile
                        .VerificationStatus,
                    ProducerVerificationStatuses
                        .Approved,
                    StringComparison.Ordinal);
        }
    }
}
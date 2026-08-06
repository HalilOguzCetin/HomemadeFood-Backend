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

        public AuthService(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IAppClock appClock)
        {
            _userRepository =
                userRepository;

            _jwtTokenGenerator =
                jwtTokenGenerator;

            _appClock =
                appClock;
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
                        request.Phone.Trim(),

                    /*
                     * Normal kayıt işlemi hiçbir
                     * zaman Producer veya Admin
                     * rolü oluşturamaz.
                     */
                    Role =
                        UserRoles.Customer,

                    IsActive =
                        true,

                    CreatedAt =
    _appClock.UtcNow
                };

            await _userRepository
                .AddAsync(user);

            await _userRepository
                .SaveChangesAsync();

            return true;
        }
        private readonly IAppClock _appClock;

        private const int MaxFailedLoginAttempts =
            5;

        private static readonly TimeSpan
            LoginLockoutDuration =
                TimeSpan.FromMinutes(15);

        /*
         * Sistemde bulunmayan e-postalarda da BCrypt
         * çalıştırarak cevap süresi farkını azaltır.
         *
         * Bu gerçek bir kullanıcı şifresi değildir.
         */
        private static readonly string
            DummyPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "HomemadeFood-Dummy-Login-Password",
                    12);

        public async Task<LoginResponse?> LoginAsync(
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

            /*
             * Kullanıcı bulunmasa bile BCrypt çalıştırılır.
             * Böylece "hesap yok" ve "şifre yanlış"
             * durumlarının işlem süreleri birbirine
             * daha yakın tutulur.
             */
            var passwordHashToVerify =
                user?.PasswordHash ??
                DummyPasswordHash;

            var isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    passwordHashToVerify);

            var now =
                _appClock.UtcNow;

            /*
             * Kullanıcı yok, hesap pasif veya hesap
             * geçici olarak kilitliyse aynı genel
             * başarısızlık sonucu döndürülür.
             */
            if (user == null)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            if (
                user.LockoutEndAt.HasValue &&
                user.LockoutEndAt.Value > now
            )
            {
                return null;
            }

            /*
             * Önceki kilit süresi tamamlandıysa
             * sayaç temizlenir ve yeni denemeler
             * sıfırdan başlatılır.
             */
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

                return null;
            }

            /*
             * Başarılı girişte başarısız deneme
             * sayacı ve geçici kilit temizlenir.
             */
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

            return new LoginResponse
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
                    CanUseProducerMode(user),

                ProducerProfileId =
                    user.ProducerProfile?.Id,

                ProducerVerificationStatus =
                    user.ProducerProfile?
                        .VerificationStatus,

                Token =
                    token
            };
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
                    .GetByIdAsync(userId);

            if (user == null ||
                !user.IsActive)
            {
                return null;
            }

            return new AuthProfileResponse
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
                    CanUseProducerMode(user),

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
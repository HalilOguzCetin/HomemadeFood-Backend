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
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository =
                userRepository;

            _jwtTokenGenerator =
                jwtTokenGenerator;
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
                        DateTime.UtcNow
                };

            await _userRepository
                .AddAsync(user);

            await _userRepository
                .SaveChangesAsync();

            return true;
        }

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

            if (user == null ||
                !user.IsActive)
            {
                return null;
            }

            var isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!isPasswordValid)
            {
                return null;
            }

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
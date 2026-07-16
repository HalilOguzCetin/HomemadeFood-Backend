using BCrypt.Net;
using HomemadeFood.Api.DTOs.Auth;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.Constants;


namespace HomemadeFood.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> RegisterAsync(
     RegisterRequest request)
        {
            var normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            var existingUser =
                await _userRepository
                    .GetByEmailAsync(normalizedEmail);

            if (existingUser != null)
            {
                return false;
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                Phone = request.Phone.Trim(),
                Role = UserRoles.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResponse?> LoginAsync(
     LoginRequest request)
        {
            var normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            var user =
                await _userRepository
                    .GetByEmailAsync(normalizedEmail);

            if (user == null || !user.IsActive)
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
                _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }
    }
}
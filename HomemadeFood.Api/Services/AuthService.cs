using BCrypt.Net;
using HomemadeFood.Api.DTOs.Auth;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // Email daha önce kayıtlı mı?
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return false;
            }

            // Yeni kullanıcı oluştur
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Veritabanına ekle
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            return await Task.FromResult<LoginResponse?>(null);
        }
    }
}
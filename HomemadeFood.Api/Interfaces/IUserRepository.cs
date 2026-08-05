using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(
            string email);

        Task<User?> GetByIdAsync(
            int userId);

        Task<List<User>> GetUsersAsync(
            string? role,
            bool? isActive,
            string? search);

        Task<User?> GetByIdWithAdminDetailsAsync(
            int userId);

        Task AddAsync(
            User user);

        Task SaveChangesAsync();
    }
}
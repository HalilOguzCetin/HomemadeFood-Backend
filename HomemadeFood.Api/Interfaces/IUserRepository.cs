using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);

        Task SaveChangesAsync();
    }
}
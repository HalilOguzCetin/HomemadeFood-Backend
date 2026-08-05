using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(
            string email)
        {
            var normalizedEmail =
                email
                    .Trim()
                    .ToLowerInvariant();

            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == normalizedEmail);
        }

        public async Task<User?> GetByIdAsync(
     int userId)
        {
            return await _context.Users
                .Include(x => x.ProducerProfile)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId);
        }

        public async Task<List<User>> GetUsersAsync(
            string? role,
            bool? isActive,
            string? search)
        {
            var query =
                _context.Users
                    .AsNoTracking()
                    .Include(x =>
                        x.ProducerProfile)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole =
                    role.Trim();

                query = query.Where(x =>
                    x.Role == normalizedRole);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x =>
                    x.IsActive ==
                    isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch =
                    search
                        .Trim()
                        .ToLowerInvariant();

                query = query.Where(x =>
                    x.FullName
                        .ToLower()
                        .Contains(
                            normalizedSearch) ||

                    x.Email
                        .Contains(
                            normalizedSearch) ||

                    x.Phone
                        .Contains(
                            normalizedSearch));
            }

            return await query
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }

        public async Task<User?>
            GetByIdWithAdminDetailsAsync(
                int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.Addresses)
                .Include(x =>
                    x.Orders)
                .Include(x =>
                    x.Reviews)
                .Include(x =>
                    x.Favorites)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId);
        }

        public async Task AddAsync(
            User user)
        {
            await _context.Users
                .AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
        }
    }
}
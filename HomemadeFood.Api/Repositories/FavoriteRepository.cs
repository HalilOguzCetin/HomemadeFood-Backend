using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Favorite favorite)
        {
            await _context.Favorites.AddAsync(favorite);
        }

        public async Task<Favorite?>
            GetByUserIdAndFoodIdAsync(
                int userId,
                int foodId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.FoodId == foodId);
        }

        public async Task<List<Favorite>>
            GetByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Include(x => x.Food)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Food)
                    .ThenInclude(x => x.ProducerProfile)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public void Remove(Favorite favorite)
        {
            _context.Favorites.Remove(favorite);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
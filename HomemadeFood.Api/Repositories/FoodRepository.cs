using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class FoodRepository : IFoodRepository
    {
        private readonly AppDbContext _context;

        public FoodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Food food)
        {
            await _context.Foods.AddAsync(food);
        }
        public async Task<List<Food>> GetAvailableFoodsAsync()
        {
            return await _context.Foods
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.ProducerProfile)
                .Where(x =>
                    x.IsAvailable &&
                    x.Category.IsActive &&
                    x.ProducerProfile.IsApproved &&
                    x.ProducerProfile.IsAvailable &&
                    x.ProducerProfile.VerificationStatus == "Approved")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Food>>
            GetByProducerProfileIdAsync(int producerProfileId)
        {
            return await _context.Foods
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.ProducerProfile)
                .Where(x =>
                    x.ProducerProfileId == producerProfileId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Food?>
            GetByIdAndProducerProfileIdAsync(
                int foodId,
                int producerProfileId)
        {
            return await _context.Foods
                .Include(x => x.Category)
                .Include(x => x.ProducerProfile)
                .FirstOrDefaultAsync(x =>
                    x.Id == foodId &&
                    x.ProducerProfileId == producerProfileId);
        }

        public async Task<Category?>
            GetActiveCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == categoryId &&
                    x.IsActive);
        }

       

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
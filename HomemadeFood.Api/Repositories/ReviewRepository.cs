using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Review review)
        {
            await _context.Reviews
                .AddAsync(review);
        }

        public async Task<Review?> GetByOrderIdAsync(
            int orderId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.OrderId == orderId);
        }

        public async Task<List<Review>>
            GetByCustomerIdAsync(
                int customerId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.ProducerProfile)
                .Where(x =>
                    x.CustomerId == customerId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>>
            GetByProducerProfileIdAsync(
                int producerProfileId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.ProducerProfile)
                .Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?>
            GetTrackedByIdAndCustomerIdAsync(
                int reviewId,
                int customerId)
        {
            return await _context.Reviews
                .Include(x => x.Customer)
                .Include(x => x.ProducerProfile)
                .FirstOrDefaultAsync(x =>
                    x.Id == reviewId &&
                    x.CustomerId == customerId);
        }

        public async Task<
            (decimal TotalRating, int ReviewCount)>
            GetRatingSummaryAsync(
                int producerProfileId)
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId);

            var reviewCount =
                await query.CountAsync();

            if (reviewCount == 0)
            {
                return (0m, 0);
            }

            var totalRating =
                await query.SumAsync(x =>
                    (decimal)x.Rating);

            return (
                totalRating,
                reviewCount);
        }

        public void Remove(
            Review review)
        {
            _context.Reviews.Remove(review);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public sealed class ProducerAvailabilityRepository
        : IProducerAvailabilityRepository
    {
        private readonly AppDbContext _context;

        public ProducerAvailabilityRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProducerProfile?>
            GetApprovedByUserIdWithBusinessHoursAsync(
                int userId)
        {
            return await _context
                .ProducerProfiles
                .Include(x => x.BusinessHours)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.IsApproved &&
                    x.VerificationStatus ==
                        ProducerVerificationStatuses
                            .Approved);
        }

        public async Task<List<ProducerProfile>>
            GetByIdsWithBusinessHoursAsync(
                IEnumerable<int>
                    producerProfileIds)
        {
            var ids =
                producerProfileIds
                    .Where(id => id > 0)
                    .Distinct()
                    .ToArray();

            if (ids.Length == 0)
            {
                return new List<
                    ProducerProfile>();
            }

            return await _context
                .ProducerProfiles
                .AsNoTracking()
                .Include(x =>
                    x.BusinessHours)
                .Where(x =>
                    ids.Contains(x.Id))
                .ToListAsync();
        }

        public void RemoveBusinessHours(
            IEnumerable<ProducerBusinessHour>
                businessHours)
        {
            _context.ProducerBusinessHours
                .RemoveRange(businessHours);
        }

        public async Task AddBusinessHoursAsync(
            IEnumerable<ProducerBusinessHour>
                businessHours)
        {
            await _context.ProducerBusinessHours
                .AddRangeAsync(businessHours);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

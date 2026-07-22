using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Repositories
{
    public class ProducerRepository : IProducerRepository
    {
        private readonly AppDbContext _context;

        public ProducerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProducerProfile producerProfile)
        {
            await _context.ProducerProfiles.AddAsync(producerProfile);
        }

        public async Task<bool> HasApplicationAsync(int userId)
        {
            return await _context.ProducerProfiles
                .AnyAsync(x => x.UserId == userId);
        }
        public async Task<ProducerProfile?>
    GetByUserIdAsync(
        int userId)
        {
            return await _context.ProducerProfiles
                .FirstOrDefaultAsync(
                    x => x.UserId == userId);
        }
        public async Task<List<ProducerProfile>> GetPendingApplicationsAsync()
        {
            return await _context.ProducerProfiles
                .Include(x => x.User)
                .Where(x => x.VerificationStatus ==
    ProducerVerificationStatuses.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProducerProfile?> GetByIdWithUserAsync(int producerProfileId)
        {
            return await _context.ProducerProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == producerProfileId);
        }
        public async Task<ProducerProfile?>
    GetApprovedByUserIdAsync(int userId)
        {
            return await _context.ProducerProfiles
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.IsApproved &&
                    x.VerificationStatus ==
    ProducerVerificationStatuses.Approved);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
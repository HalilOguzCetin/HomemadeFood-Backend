using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class VerificationChallengeRepository :
        IVerificationChallengeRepository
    {
        private readonly AppDbContext _context;

        public VerificationChallengeRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            VerificationChallenge challenge)
        {
            await _context
                .VerificationChallenges
                .AddAsync(challenge);
        }

        public async Task<VerificationChallenge?>
            GetLatestActiveAsync(
                int userId,
                string type,
                DateTime now)
        {
            return await _context
                .VerificationChallenges
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.ExpiresAt > now)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ThenByDescending(x =>
                    x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task ExpireActiveAsync(
            int userId,
            string type,
            DateTime now)
        {
            var activeChallenges =
                await _context
                    .VerificationChallenges
                    .Where(x =>
                        x.UserId == userId &&
                        x.Type == type &&
                        x.UsedAt == null &&
                        x.ExpiresAt > now)
                    .ToListAsync();

            foreach (
                var challenge in activeChallenges)
            {
                challenge.ExpiresAt = now;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
        }
    }
}
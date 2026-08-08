using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IVerificationChallengeRepository
    {
        Task AddAsync(
            VerificationChallenge challenge);

        Task<VerificationChallenge?>
            GetLatestActiveAsync(
                int userId,
                string type,
                DateTime now);

        Task ExpireActiveAsync(
            int userId,
            string type,
            DateTime now);

        Task SaveChangesAsync();
    }
}
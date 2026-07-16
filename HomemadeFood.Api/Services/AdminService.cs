using HomemadeFood.Api.DTOs.Admin;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly IProducerRepository _producerRepository;

        public AdminService(IProducerRepository producerRepository)
        {
            _producerRepository = producerRepository;
        }

        public async Task<List<PendingProducerResponse>>
            GetPendingProducerApplicationsAsync()
        {
            var applications =
                await _producerRepository.GetPendingApplicationsAsync();

            return applications.Select(x => new PendingProducerResponse
            {
                ProducerProfileId = x.Id,
                UserId = x.UserId,
                FullName = x.User.FullName,
                Email = x.User.Email,
                BusinessName = x.BusinessName,
                Description = x.Description,
                Address = x.Address,
                DailyCapacity = x.DailyCapacity,
                VerificationStatus = x.VerificationStatus,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<bool> ApproveProducerAsync(
            int producerProfileId,
            int adminUserId)
        {
            var producerProfile =
                await _producerRepository.GetByIdWithUserAsync(
                    producerProfileId);

            if (producerProfile == null)
            {
                return false;
            }

            if (producerProfile.VerificationStatus !=
    ProducerVerificationStatuses.Pending)
            {
                return false;
            }

            producerProfile.IsApproved = true;
            producerProfile.VerificationStatus =
    ProducerVerificationStatuses.Approved;
            producerProfile.ApprovedAt = DateTime.UtcNow;
            producerProfile.ApprovedByAdminId = adminUserId;

            producerProfile.User.Role = UserRoles.Producer;

            await _producerRepository.SaveChangesAsync();

            return true;
        }
    }
}
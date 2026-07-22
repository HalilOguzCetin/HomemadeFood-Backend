using HomemadeFood.Api.DTOs.Admin;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAdminService
    {
        Task<List<PendingProducerResponse>>
            GetPendingProducerApplicationsAsync();

        Task<bool> ApproveProducerAsync(
            int producerProfileId,
            int adminUserId);

        Task<bool> RejectProducerAsync(
            int producerProfileId,
            int adminUserId,
            string rejectionReason);
    }
}
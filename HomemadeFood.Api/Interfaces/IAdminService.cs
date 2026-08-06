using HomemadeFood.Api.DTOs.Admin;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAdminService
    {
        Task<List<AdminProducerApplicationResponse>>
            GetProducerApplicationsAsync(
                string verificationStatus);

        Task<bool> ApproveProducerAsync(
            int producerProfileId,
            int adminUserId);

        Task<bool> RejectProducerAsync(
            int producerProfileId,
            int adminUserId,
            string rejectionReason);

        Task<List<AdminUserListItemResponse>>
            GetUsersAsync(
                string? role,
                bool? isActive,
                string? search);

        Task<AdminUserDetailResponse?>
            GetUserByIdAsync(
                int userId);

        Task<bool> UpdateUserStatusAsync(
            int userId,
            bool isActive);

        Task<List<AdminOrderListItemResponse>>
            GetOrdersAsync(
                string? status,
                int? customerId,
                int? producerProfileId,
                string? search,
                DateOnly? dateFrom,
                DateOnly? dateTo);

        Task<AdminOrderDetailResponse?>
            GetOrderByIdAsync(
                int orderId);
    }
}
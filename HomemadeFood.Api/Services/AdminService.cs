using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Admin;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly IProducerRepository
            _producerRepository;

        private readonly IAppClock
            _appClock;

        public AdminService(
            IProducerRepository producerRepository,
            IAppClock appClock)
        {
            _producerRepository =
                producerRepository;

            _appClock =
                appClock;
        }

        public async Task<List<PendingProducerResponse>>
            GetPendingProducerApplicationsAsync()
        {
            var applications =
                await _producerRepository
                    .GetPendingApplicationsAsync();

            return applications
                .Select(application =>
                    new PendingProducerResponse
                    {
                        ProducerProfileId =
                            application.Id,

                        UserId =
                            application.UserId,

                        FullName =
                            application.User.FullName,

                        Email =
                            application.User.Email,

                        BusinessName =
                            application.BusinessName,

                        Description =
                            application.Description,

                        Address =
                            application.Address,

                        DailyCapacity =
                            application.DailyCapacity,

                        VerificationStatus =
                            application.VerificationStatus,

                        CreatedAt =
                            application.CreatedAt
                    })
                .ToList();
        }

        public async Task<bool> ApproveProducerAsync(
            int producerProfileId,
            int adminUserId)
        {
            var producerProfile =
                await _producerRepository
                    .GetByIdWithUserAsync(
                        producerProfileId);

            if (producerProfile == null)
            {
                return false;
            }

            if (!string.Equals(
                    producerProfile.VerificationStatus,
                    ProducerVerificationStatuses.Pending,
                    StringComparison.Ordinal))
            {
                return false;
            }

            producerProfile.IsApproved =
                true;

            producerProfile.IsAvailable =
                true;

            producerProfile.VerificationStatus =
                ProducerVerificationStatuses.Approved;

            producerProfile.ApprovedAt =
                _appClock.UtcNow;

            producerProfile.ApprovedByAdminId =
                adminUserId;

            /*
             * Güvenlik amacıyla daha önce bulunabilecek
             * reddetme bilgileri temizlenir.
             */
            producerProfile.RejectedAt =
                null;

            producerProfile.RejectedByAdminId =
                null;

            producerProfile.RejectionReason =
                null;

            producerProfile.User.Role =
                UserRoles.Producer;

            await _producerRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectProducerAsync(
            int producerProfileId,
            int adminUserId,
            string rejectionReason)
        {
            var producerProfile =
                await _producerRepository
                    .GetByIdWithUserAsync(
                        producerProfileId);

            if (producerProfile == null)
            {
                return false;
            }

            /*
             * Yalnızca Pending durumundaki
             * başvurular reddedilebilir.
             */
            if (!string.Equals(
                    producerProfile.VerificationStatus,
                    ProducerVerificationStatuses.Pending,
                    StringComparison.Ordinal))
            {
                return false;
            }

            var normalizedReason =
                rejectionReason.Trim();

            /*
             * DTO doğrulamasına ek olarak servis
             * katmanında da savunmacı kontrol yapılır.
             */
            if (normalizedReason.Length < 10 ||
                normalizedReason.Length > 500)
            {
                return false;
            }

            producerProfile.IsApproved =
                false;

            producerProfile.IsAvailable =
                false;

            producerProfile.VerificationStatus =
                ProducerVerificationStatuses.Rejected;

            producerProfile.RejectedAt =
                _appClock.UtcNow;

            producerProfile.RejectedByAdminId =
                adminUserId;

            producerProfile.RejectionReason =
                normalizedReason;

            producerProfile.ApprovedAt =
                null;

            producerProfile.ApprovedByAdminId =
                null;

            /*
             * Başvuru reddedildiğinde kullanıcının
             * rolü Customer olarak kalır.
             */
            producerProfile.User.Role =
                UserRoles.Customer;

            await _producerRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
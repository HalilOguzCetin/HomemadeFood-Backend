using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository
            _producerRepository;

        private readonly IAppClock
            _appClock;

        public ProducerService(
            IProducerRepository producerRepository,
            IAppClock appClock)
        {
            _producerRepository =
                producerRepository;

            _appClock =
                appClock;
        }

        public async Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request)
        {
            var businessName =
                request.BusinessName.Trim();

            var description =
                request.Description.Trim();

            var address =
                request.Address.Trim();

            /*
             * DTO doğrulamasına ek olarak servis
             * katmanında savunmacı kontroller yapılır.
             */
            if (string.IsNullOrWhiteSpace(
                    businessName) ||
                businessName.Length < 2 ||
                businessName.Length > 150)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    description) ||
                description.Length < 10 ||
                description.Length > 1000)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    address) ||
                address.Length < 10 ||
                address.Length > 500)
            {
                return false;
            }

            if (request.Latitude < -90 ||
                request.Latitude > 90)
            {
                return false;
            }

            if (request.Longitude < -180 ||
                request.Longitude > 180)
            {
                return false;
            }

            if (request.DailyCapacity < 1 ||
                request.DailyCapacity > 1000)
            {
                return false;
            }

            var existingApplication =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            /*
             * Kullanıcının daha önce hiç başvurusu
             * bulunmuyorsa yeni profil oluşturulur.
             */
            if (existingApplication == null)
            {
                var producerProfile =
                    new ProducerProfile
                    {
                        UserId =
                            userId,

                        BusinessName =
                            businessName,

                        Description =
                            description,

                        Address =
                            address,

                        Latitude =
                            request.Latitude,

                        Longitude =
                            request.Longitude,

                        DailyCapacity =
                            request.DailyCapacity,

                        RemainingCapacity =
                            request.DailyCapacity,

                        CapacityDate =
                            _appClock.TurkeyToday,

                        Rating =
                            0,

                        /*
                         * Başvuru henüz onaylanmadığı için
                         * üretici aktif kabul edilmez.
                         */
                        IsAvailable =
                            false,

                        IsApproved =
                            false,

                        VerificationStatus =
                            ProducerVerificationStatuses
                                .Pending,

                        CreatedAt =
                            _appClock.UtcNow
                    };

                await _producerRepository
                    .AddAsync(producerProfile);

                await _producerRepository
                    .SaveChangesAsync();

                return true;
            }

            /*
             * Pending veya Approved durumundaki bir
             * kullanıcı tekrar başvuru yapamaz.
             */
            if (!string.Equals(
                    existingApplication
                        .VerificationStatus,
                    ProducerVerificationStatuses
                        .Rejected,
                    StringComparison.Ordinal))
            {
                return false;
            }

            /*
             * Reddedilmiş başvuru yeni bilgilerle
             * güncellenerek tekrar Pending yapılır.
             */
            existingApplication.BusinessName =
                businessName;

            existingApplication.Description =
                description;

            existingApplication.Address =
                address;

            existingApplication.Latitude =
                request.Latitude;

            existingApplication.Longitude =
                request.Longitude;

            existingApplication.DailyCapacity =
                request.DailyCapacity;

            existingApplication.RemainingCapacity =
                request.DailyCapacity;

            existingApplication.CapacityDate =
                _appClock.TurkeyToday;

            existingApplication.IsAvailable =
                false;

            existingApplication.IsApproved =
                false;

            existingApplication.VerificationStatus =
                ProducerVerificationStatuses.Pending;

            /*
             * Eski onay ve reddetme bilgileri temizlenir.
             */
            existingApplication.ApprovedAt =
                null;

            existingApplication.ApprovedByAdminId =
                null;

            existingApplication.RejectedAt =
                null;

            existingApplication.RejectedByAdminId =
                null;

            existingApplication.RejectionReason =
                null;

            /*
             * CreatedAt bu yapıda en son başvuru
             * tarihini temsil edecek şekilde yenilenir.
             */
            existingApplication.CreatedAt =
                _appClock.UtcNow;

            /*
             * ProducerProfile üzerinde concurrency
             * token bulunduğu için sürüm artırılır.
             */
            existingApplication.CapacityVersion++;

            try
            {
                await _producerRepository
                    .SaveChangesAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                /*
                 * Başvuru aynı anda başka bir işlemle
                 * değiştirilmişse güvenli biçimde
                 * başarısız sonuç döndürülür.
                 */
                return false;
            }
        }

        public async Task<
            ProducerApplicationStatusResponse?>
            GetMyApplicationAsync(
                int userId)
        {
            var producerProfile =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            return new ProducerApplicationStatusResponse
            {
                ProducerProfileId =
                    producerProfile.Id,

                BusinessName =
                    producerProfile.BusinessName,

                Description =
                    producerProfile.Description,

                Address =
                    producerProfile.Address,

                Latitude =
                    producerProfile.Latitude,

                Longitude =
                    producerProfile.Longitude,

                DailyCapacity =
                    producerProfile.DailyCapacity,

                RemainingCapacity =
                    producerProfile.RemainingCapacity,

                IsAvailable =
                    producerProfile.IsAvailable,

                IsApproved =
                    producerProfile.IsApproved,

                VerificationStatus =
                    producerProfile
                        .VerificationStatus,

                CreatedAt =
                    producerProfile.CreatedAt,

                ApprovedAt =
                    producerProfile.ApprovedAt,

                RejectedAt =
                    producerProfile.RejectedAt,

                RejectionReason =
                    producerProfile.RejectionReason
            };
        }
    }
}
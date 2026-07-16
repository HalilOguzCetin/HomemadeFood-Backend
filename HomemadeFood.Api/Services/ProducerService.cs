using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository _producerRepository;
        private readonly IAppClock _appClock;

        public ProducerService(
            IProducerRepository producerRepository,
            IAppClock appClock)
        {
            _producerRepository = producerRepository;
            _appClock = appClock;
        }

        public async Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request)
        {
            var hasApplication =
                await _producerRepository
                    .HasApplicationAsync(userId);

            if (hasApplication)
            {
                return false;
            }

            if (request.DailyCapacity <= 0)
            {
                return false;
            }

            var producerProfile = new ProducerProfile
            {
                UserId = userId,

                BusinessName =
                    request.BusinessName.Trim(),

                Description =
                    request.Description.Trim(),

                Address =
                    request.Address.Trim(),

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

                Rating = 0,

                IsAvailable = true,

                IsApproved = false,

                VerificationStatus =
                    ProducerVerificationStatuses.Pending,

                CreatedAt =
                    _appClock.UtcNow
            };

            await _producerRepository
                .AddAsync(producerProfile);

            await _producerRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
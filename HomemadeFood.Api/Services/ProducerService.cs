using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository _producerRepository;

        public ProducerService(IProducerRepository producerRepository)
        {
            _producerRepository = producerRepository;
        }

        public async Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request)
        {
            if (await _producerRepository.HasApplicationAsync(userId))
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
                BusinessName = request.BusinessName,
                Description = request.Description,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                DailyCapacity = request.DailyCapacity,
                RemainingCapacity = request.DailyCapacity,
                Rating = 0,
                IsAvailable = true,
                IsApproved = false,
                VerificationStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _producerRepository.AddAsync(producerProfile);
            await _producerRepository.SaveChangesAsync();

            return true;
        }
    }
}
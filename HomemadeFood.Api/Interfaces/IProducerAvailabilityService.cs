using HomemadeFood.Api.DTOs.Producer;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerAvailabilityService
    {
        Task<ProducerAvailabilityResponse?>
            GetMyAvailabilityAsync(
                int userId);

        Task<ProducerAvailabilityResponse?>
            UpdateMyBusinessHoursAsync(
                int userId,
                UpdateProducerBusinessHoursRequest request);

        Task<ProducerAvailabilityResponse?>
            UpdateMyAvailabilityModeAsync(
                int userId,
                UpdateProducerAvailabilityModeRequest request);


        Task<bool>
            IsProducerCurrentlyOpenAsync(
                int producerProfileId);

        Task<IReadOnlyDictionary<int, bool>>
            GetCurrentOpenStatesAsync(
                IEnumerable<int>
                    producerProfileIds);
    }
}

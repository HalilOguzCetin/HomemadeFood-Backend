using HomemadeFood.Api.DTOs.Producer;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerService
    {
        Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request);

        Task<ProducerApplicationStatusResponse?>
            GetMyApplicationAsync(
                int userId);
    }
}
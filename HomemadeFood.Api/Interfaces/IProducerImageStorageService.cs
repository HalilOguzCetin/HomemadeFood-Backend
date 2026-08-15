using Microsoft.AspNetCore.Http;

namespace HomemadeFood.Api.Interfaces
{
    public interface IProducerImageStorageService
    {
        Task<string> SaveAsync(
            IFormFile image,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string? imageUrl,
            CancellationToken cancellationToken = default);
    }
}
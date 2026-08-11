using Microsoft.AspNetCore.Http;

namespace HomemadeFood.Api.Interfaces
{
    public interface IFoodImageStorageService
    {
        Task<string> SaveAsync(
            IFormFile image,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string? imageUrl,
            CancellationToken cancellationToken = default);
    }
}
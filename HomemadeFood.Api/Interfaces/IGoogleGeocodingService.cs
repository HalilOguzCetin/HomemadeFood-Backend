using HomemadeFood.Api.DTOs.Address;

namespace HomemadeFood.Api.Interfaces
{
    public interface IGoogleGeocodingService
    {
        Task<ReverseGeocodeResponse?> ReverseGeocodeAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken = default);
    }
}
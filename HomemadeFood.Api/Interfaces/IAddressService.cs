using HomemadeFood.Api.DTOs.Address;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAddressService
    {
        Task<AddressResponse> CreateAddressAsync(
            int userId,
            CreateAddressRequest request);

        Task<List<AddressResponse>> GetMyAddressesAsync(
            int userId);

        Task<AddressResponse?> UpdateAddressAsync(
            int userId,
            int addressId,
            UpdateAddressRequest request);

        Task<bool> DeleteAddressAsync(
            int userId,
            int addressId);
    }
}
using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface IAddressRepository
    {
        Task AddAsync(Address address);

        Task<List<Address>> GetByUserIdAsync(int userId);

        Task<Address?> GetByIdAndUserIdAsync(
            int addressId,
            int userId);

        Task<bool> HasAnyAddressAsync(int userId);

        Task ClearDefaultAddressesAsync(
            int userId,
            int? exceptAddressId = null);

        void Remove(Address address);

        Task SaveChangesAsync();
        Task<Address?> GetReplacementAddressAsync(
    int userId,
    int excludedAddressId);
    }
}
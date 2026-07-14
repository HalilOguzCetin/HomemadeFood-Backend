using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }

        public async Task<List<Address>>
            GetByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Address?>
            GetByIdAndUserIdAsync(
                int addressId,
                int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.UserId == userId);
        }
        public async Task<Address?>
    GetReplacementAddressAsync(
        int userId,
        int excludedAddressId)
        {
            return await _context.Addresses
                .Where(x =>
                    x.UserId == userId &&
                    x.Id != excludedAddressId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool>
            HasAnyAddressAsync(int userId)
        {
            return await _context.Addresses
                .AnyAsync(x => x.UserId == userId);
        }

        public async Task ClearDefaultAddressesAsync(
            int userId,
            int? exceptAddressId = null)
        {
            var query = _context.Addresses
                .Where(x =>
                    x.UserId == userId &&
                    x.IsDefault);

            if (exceptAddressId.HasValue)
            {
                query = query.Where(x =>
                    x.Id != exceptAddressId.Value);
            }

            var defaultAddresses =
                await query.ToListAsync();

            foreach (var address in defaultAddresses)
            {
                address.IsDefault = false;
            }
        }

        public void Remove(Address address)
        {
            _context.Addresses.Remove(address);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
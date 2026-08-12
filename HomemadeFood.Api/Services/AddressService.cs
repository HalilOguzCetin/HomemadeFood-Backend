using HomemadeFood.Api.DTOs.Address;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(
            IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<AddressResponse> CreateAddressAsync(
            int userId,
            CreateAddressRequest request)
        {
            var hasAnyAddress =
                await _addressRepository
                    .HasAnyAddressAsync(userId);

            // Kullanıcının ilk adresi otomatik olarak
            // varsayılan adres yapılır.
            var shouldBeDefault =
                request.IsDefault || !hasAnyAddress;

            if (shouldBeDefault)
            {
                await _addressRepository
                    .ClearDefaultAddressesAsync(userId);
            }

            var address = new Address
            {
                UserId = userId,

                Title = request.Title.Trim(),

                FullAddress =
                    request.FullAddress.Trim(),

                City =
                    request.City.Trim(),

                District =
                    request.District.Trim(),

                Neighborhood =
                    request.Neighborhood.Trim(),

                Street =
                    request.Street.Trim(),

                BuildingNo =
                    request.BuildingNo.Trim(),

                Floor =
                    string.IsNullOrWhiteSpace(
                        request.Floor)
                        ? null
                        : request.Floor.Trim(),

                ApartmentNo =
                    string.IsNullOrWhiteSpace(
                        request.ApartmentNo)
                        ? null
                        : request.ApartmentNo.Trim(),

                AddressNote =
                    string.IsNullOrWhiteSpace(
                        request.AddressNote)
                        ? null
                        : request.AddressNote.Trim(),

                Latitude =
                    request.Latitude,

                Longitude =
                    request.Longitude,

                IsDefault =
                    shouldBeDefault,

                CreatedAt =
                    DateTime.UtcNow
            };

            await _addressRepository
                .AddAsync(address);

            await _addressRepository
                .SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task<List<AddressResponse>>
            GetMyAddressesAsync(
                int userId)
        {
            var addresses =
                await _addressRepository
                    .GetByUserIdAsync(userId);

            return addresses
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<AddressResponse?>
            UpdateAddressAsync(
                int userId,
                int addressId,
                UpdateAddressRequest request)
        {
            var address =
                await _addressRepository
                    .GetByIdAndUserIdAsync(
                        addressId,
                        userId);

            if (address == null)
            {
                return null;
            }

            if (request.IsDefault)
            {
                await _addressRepository
                    .ClearDefaultAddressesAsync(
                        userId,
                        address.Id);
            }

            address.Title =
                request.Title.Trim();

            address.FullAddress =
                request.FullAddress.Trim();

            address.City =
                request.City.Trim();

            address.District =
                request.District.Trim();

            address.Neighborhood =
                request.Neighborhood.Trim();

            address.Street =
                request.Street.Trim();

            address.BuildingNo =
                request.BuildingNo.Trim();

            address.Floor =
                string.IsNullOrWhiteSpace(
                    request.Floor)
                    ? null
                    : request.Floor.Trim();

            address.ApartmentNo =
                string.IsNullOrWhiteSpace(
                    request.ApartmentNo)
                    ? null
                    : request.ApartmentNo.Trim();

            address.AddressNote =
                string.IsNullOrWhiteSpace(
                    request.AddressNote)
                    ? null
                    : request.AddressNote.Trim();

            address.Latitude =
                request.Latitude;

            address.Longitude =
                request.Longitude;

            if (
                address.IsDefault &&
                !request.IsDefault)
            {
                // Mevcut varsayılan adres tek başına
                // pasif yapılamaz.
                // Varsayılanlık başka bir adres seçilerek
                // değiştirilmelidir.
                address.IsDefault = true;
            }
            else
            {
                address.IsDefault =
                    request.IsDefault;
            }

            await _addressRepository
                .SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task<bool>
            DeleteAddressAsync(
                int userId,
                int addressId)
        {
            var address =
                await _addressRepository
                    .GetByIdAndUserIdAsync(
                        addressId,
                        userId);

            if (address == null)
            {
                return false;
            }

            if (address.IsDefault)
            {
                var replacementAddress =
                    await _addressRepository
                        .GetReplacementAddressAsync(
                            userId,
                            address.Id);

                if (replacementAddress != null)
                {
                    replacementAddress.IsDefault =
                        true;
                }
            }

            _addressRepository
                .Remove(address);

            await _addressRepository
                .SaveChangesAsync();

            return true;
        }

        private static AddressResponse
            MapToResponse(
                Address address)
        {
            return new AddressResponse
            {
                Id =
                    address.Id,

                Title =
                    address.Title,

                FullAddress =
                    address.FullAddress,

                City =
                    address.City,

                District =
                    address.District,

                Neighborhood =
                    address.Neighborhood,

                Street =
                    address.Street,

                BuildingNo =
                    address.BuildingNo,

                Floor =
                    address.Floor,

                ApartmentNo =
                    address.ApartmentNo,

                AddressNote =
                    address.AddressNote,

                Latitude =
                    address.Latitude,

                Longitude =
                    address.Longitude,

                IsDefault =
                    address.IsDefault,

                CreatedAt =
                    address.CreatedAt
            };
        }
    }
}
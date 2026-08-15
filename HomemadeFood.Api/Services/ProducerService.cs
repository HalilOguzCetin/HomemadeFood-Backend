using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Services
{
    public class ProducerService : IProducerService
    {
        private readonly IProducerRepository _producerRepository;
        private readonly IAppClock _appClock;

        public ProducerService(
            IProducerRepository producerRepository,
            IAppClock appClock)
        {
            _producerRepository = producerRepository;
            _appClock = appClock;
        }

        public async Task<bool> ApplyAsync(
            int userId,
            ProducerApplicationRequest request)
        {
            var businessName =
                request.BusinessName?.Trim() ?? string.Empty;

            var description =
                request.Description?.Trim() ?? string.Empty;

            var address =
                request.Address?.Trim() ?? string.Empty;

            var city =
                request.City?.Trim() ?? string.Empty;

            var district =
                request.District?.Trim() ?? string.Empty;

            var neighborhood =
                request.Neighborhood?.Trim() ?? string.Empty;

            var street =
                request.Street?.Trim() ?? string.Empty;

            var buildingNo =
                request.BuildingNo?.Trim() ?? string.Empty;

            var floor =
                NormalizeOptional(request.Floor);

            var apartmentNo =
                NormalizeOptional(request.ApartmentNo);

            var addressNote =
                NormalizeOptional(request.AddressNote);

            if (!IsRequiredTextValid(
                    businessName,
                    2,
                    150) ||
                !IsRequiredTextValid(
                    description,
                    10,
                    1000) ||
                !IsRequiredTextValid(
                    address,
                    10,
                    500) ||
                !IsRequiredTextValid(
                    city,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    district,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    neighborhood,
                    1,
                    120) ||
                !IsRequiredTextValid(
                    street,
                    1,
                    150) ||
                !IsRequiredTextValid(
                    buildingNo,
                    1,
                    30))
            {
                return false;
            }

            if (!IsOptionalTextValid(
                    floor,
                    20) ||
                !IsOptionalTextValid(
                    apartmentNo,
                    20) ||
                !IsOptionalTextValid(
                    addressNote,
                    300))
            {
                return false;
            }

            if (request.Latitude < -90 ||
                request.Latitude > 90)
            {
                return false;
            }

            if (request.Longitude < -180 ||
                request.Longitude > 180)
            {
                return false;
            }

            if (request.DailyCapacity < 1 ||
                request.DailyCapacity > 1000)
            {
                return false;
            }

            var existingApplication =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (existingApplication == null)
            {
                var producerProfile =
                    new ProducerProfile
                    {
                        UserId = userId,
                        BusinessName = businessName,
                        Description = description,
                        Address = address,

                        City = city,
                        District = district,
                        Neighborhood = neighborhood,
                        Street = street,
                        BuildingNo = buildingNo,
                        Floor = floor,
                        ApartmentNo = apartmentNo,
                        AddressNote = addressNote,

                        Latitude = request.Latitude,
                        Longitude = request.Longitude,

                        DailyCapacity =
                            request.DailyCapacity,

                        RemainingCapacity =
                            request.DailyCapacity,

                        CapacityDate =
                            _appClock.TurkeyToday,

                        Rating = 0,

                        IsAvailable = false,
                        IsApproved = false,

                        VerificationStatus =
                            ProducerVerificationStatuses
                                .Pending,

                        CreatedAt =
                            _appClock.UtcNow
                    };

                await _producerRepository
                    .AddAsync(producerProfile);

                await _producerRepository
                    .SaveChangesAsync();

                return true;
            }

            if (!string.Equals(
                    existingApplication
                        .VerificationStatus,
                    ProducerVerificationStatuses
                        .Rejected,
                    StringComparison.Ordinal))
            {
                return false;
            }

            existingApplication.BusinessName =
                businessName;

            existingApplication.Description =
                description;

            existingApplication.Address =
                address;

            ApplyStructuredAddress(
                existingApplication,
                city,
                district,
                neighborhood,
                street,
                buildingNo,
                floor,
                apartmentNo,
                addressNote,
                request.Latitude,
                request.Longitude);

            existingApplication.DailyCapacity =
                request.DailyCapacity;

            existingApplication.RemainingCapacity =
                request.DailyCapacity;

            existingApplication.CapacityDate =
                _appClock.TurkeyToday;

            existingApplication.IsAvailable =
                false;

            existingApplication.IsApproved =
                false;

            existingApplication.VerificationStatus =
                ProducerVerificationStatuses.Pending;

            existingApplication.ApprovedAt = null;
            existingApplication.ApprovedByAdminId = null;

            existingApplication.RejectedAt = null;
            existingApplication.RejectedByAdminId = null;

            existingApplication.RejectionReason = null;

            existingApplication.CreatedAt =
                _appClock.UtcNow;

            existingApplication.CapacityVersion++;

            try
            {
                await _producerRepository
                    .SaveChangesAsync();

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<
            ProducerApplicationStatusResponse?>
            GetMyApplicationAsync(
                int userId)
        {
            var producerProfile =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            return MapToResponse(
                producerProfile);
        }

        public async Task<
            ProducerApplicationStatusResponse?>
            UpdateMyProfileAsync(
                int userId,
                UpdateProducerProfileRequest request)
        {
            var businessName =
                request.BusinessName?.Trim() ?? string.Empty;

            var description =
                request.Description?.Trim() ?? string.Empty;

            var address =
                request.Address?.Trim() ?? string.Empty;

            var city =
                request.City?.Trim() ?? string.Empty;

            var district =
                request.District?.Trim() ?? string.Empty;

            var neighborhood =
                request.Neighborhood?.Trim() ?? string.Empty;

            var street =
                request.Street?.Trim() ?? string.Empty;

            var buildingNo =
                request.BuildingNo?.Trim() ?? string.Empty;

            var floor =
                NormalizeOptional(request.Floor);

            var apartmentNo =
                NormalizeOptional(request.ApartmentNo);

            var addressNote =
                NormalizeOptional(request.AddressNote);

            if (!IsRequiredTextValid(
                    businessName,
                    2,
                    150) ||
                !IsRequiredTextValid(
                    description,
                    10,
                    1000) ||
                !IsRequiredTextValid(
                    address,
                    10,
                    500) ||
                !IsRequiredTextValid(
                    city,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    district,
                    1,
                    100) ||
                !IsRequiredTextValid(
                    neighborhood,
                    1,
                    120) ||
                !IsRequiredTextValid(
                    street,
                    1,
                    150) ||
                !IsRequiredTextValid(
                    buildingNo,
                    1,
                    30))
            {
                return null;
            }

            if (!IsOptionalTextValid(
                    floor,
                    20) ||
                !IsOptionalTextValid(
                    apartmentNo,
                    20) ||
                !IsOptionalTextValid(
                    addressNote,
                    300))
            {
                return null;
            }

            if (request.Latitude < -90 ||
                request.Latitude > 90)
            {
                return null;
            }

            if (request.Longitude < -180 ||
                request.Longitude > 180)
            {
                return null;
            }

            if (request.DailyCapacity < 1 ||
                request.DailyCapacity > 1000)
            {
                return null;
            }

            var producerProfile =
                await _producerRepository
                    .GetByUserIdAsync(userId);

            if (producerProfile == null)
            {
                return null;
            }

            if (!producerProfile.IsApproved ||
                !string.Equals(
                    producerProfile.VerificationStatus,
                    ProducerVerificationStatuses
                        .Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var today =
                _appClock.TurkeyToday;

            int usedCapacity;

            if (producerProfile.CapacityDate != today)
            {
                usedCapacity = 0;
            }
            else
            {
                usedCapacity =
                    Math.Max(
                        0,
                        producerProfile.DailyCapacity -
                        producerProfile.RemainingCapacity);
            }

            var newRemainingCapacity =
                Math.Max(
                    0,
                    request.DailyCapacity -
                    usedCapacity);

            producerProfile.BusinessName =
                businessName;

            producerProfile.Description =
                description;

            producerProfile.Address =
                address;

            ApplyStructuredAddress(
                producerProfile,
                city,
                district,
                neighborhood,
                street,
                buildingNo,
                floor,
                apartmentNo,
                addressNote,
                request.Latitude,
                request.Longitude);

            producerProfile.DailyCapacity =
                request.DailyCapacity;

            producerProfile.RemainingCapacity =
                newRemainingCapacity;

            producerProfile.CapacityDate =
                today;

            producerProfile.IsAvailable =
                request.IsAvailable;

            producerProfile.CapacityVersion++;

            try
            {
                await _producerRepository
                    .SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return null;
            }

            return MapToResponse(
                producerProfile);
        }

        private static void ApplyStructuredAddress(
            ProducerProfile producerProfile,
            string city,
            string district,
            string neighborhood,
            string street,
            string buildingNo,
            string? floor,
            string? apartmentNo,
            string? addressNote,
            double latitude,
            double longitude)
        {
            producerProfile.City = city;
            producerProfile.District = district;
            producerProfile.Neighborhood = neighborhood;
            producerProfile.Street = street;
            producerProfile.BuildingNo = buildingNo;
            producerProfile.Floor = floor;
            producerProfile.ApartmentNo = apartmentNo;
            producerProfile.AddressNote = addressNote;
            producerProfile.Latitude = latitude;
            producerProfile.Longitude = longitude;
        }

        private static ProducerApplicationStatusResponse
            MapToResponse(
                ProducerProfile producerProfile)
        {
            return new ProducerApplicationStatusResponse
            {
                ProducerProfileId =
                    producerProfile.Id,

                BusinessName =
                    producerProfile.BusinessName,

                Description =
                    producerProfile.Description,

                Address =
                    producerProfile.Address,

                City =
                    producerProfile.City,

                District =
                    producerProfile.District,

                Neighborhood =
                    producerProfile.Neighborhood,

                Street =
                    producerProfile.Street,

                BuildingNo =
                    producerProfile.BuildingNo,

                Floor =
                    producerProfile.Floor,

                ApartmentNo =
                    producerProfile.ApartmentNo,

                AddressNote =
                    producerProfile.AddressNote,

                Latitude =
                    producerProfile.Latitude,

                Longitude =
                    producerProfile.Longitude,

                DailyCapacity =
                    producerProfile.DailyCapacity,

                RemainingCapacity =
                    producerProfile.RemainingCapacity,

                IsAvailable =
                    producerProfile.IsAvailable,

                IsApproved =
                    producerProfile.IsApproved,

                VerificationStatus =
                    producerProfile.VerificationStatus,

                CreatedAt =
                    producerProfile.CreatedAt,

                ApprovedAt =
                    producerProfile.ApprovedAt,

                RejectedAt =
                    producerProfile.RejectedAt,

                RejectionReason =
                    producerProfile.RejectionReason
            };
        }

        private static bool IsRequiredTextValid(
            string value,
            int minimumLength,
            int maximumLength)
        {
            return !string.IsNullOrWhiteSpace(
                       value) &&
                   value.Length >= minimumLength &&
                   value.Length <= maximumLength;
        }

        private static string? NormalizeOptional(
            string? value)
        {
            var trimmed =
                value?.Trim();

            return string.IsNullOrWhiteSpace(
                trimmed)
                ? null
                : trimmed;
        }

        private static bool IsOptionalTextValid(
            string? value,
            int maximumLength)
        {
            return value == null ||
                   value.Length <= maximumLength;
        }
    }
}
using System.Globalization;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Helpers;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public sealed class ProducerAvailabilityService
        : IProducerAvailabilityService
    {
        private static readonly string[]
            TurkishDayNames =
            {
                string.Empty,
                "Pazartesi",
                "Salı",
                "Çarşamba",
                "Perşembe",
                "Cuma",
                "Cumartesi",
                "Pazar"
            };

        private readonly
            IProducerAvailabilityRepository
            _repository;

        private readonly IAppClock _appClock;

        public ProducerAvailabilityService(
            IProducerAvailabilityRepository
                repository,
            IAppClock appClock)
        {
            _repository = repository;
            _appClock = appClock;
        }

        public async Task<
            ProducerAvailabilityResponse?>
            GetMyAvailabilityAsync(
                int userId)
        {
            var producerProfile =
                await _repository
                    .GetApprovedByUserIdWithBusinessHoursAsync(
                        userId);

            if (producerProfile == null)
            {
                return null;
            }

            return MapResponse(
                producerProfile);
        }

        public async Task<
            ProducerAvailabilityResponse?>
            UpdateMyBusinessHoursAsync(
                int userId,
                UpdateProducerBusinessHoursRequest
                    request)
        {
            var producerProfile =
                await _repository
                    .GetApprovedByUserIdWithBusinessHoursAsync(
                        userId);

            if (producerProfile == null)
            {
                return null;
            }

            var normalizedHours =
                ValidateAndNormalize(
                    request);

            _repository.RemoveBusinessHours(
                producerProfile.BusinessHours);

            var newBusinessHours =
                normalizedHours
                    .Select(x =>
                        new ProducerBusinessHour
                        {
                            ProducerProfileId =
                                producerProfile.Id,

                            DayOfWeek =
                                x.DayOfWeek,

                            IsClosed =
                                x.IsClosed,

                            OpenTime =
                                x.OpenTime,

                            CloseTime =
                                x.CloseTime
                        })
                    .ToList();

            await _repository
                .AddBusinessHoursAsync(
                    newBusinessHours);

            await _repository
                .SaveChangesAsync();

            /*
             * Response için navigation koleksiyonunu
             * yeni değerlerle güncelliyoruz.
             */
            producerProfile.BusinessHours =
                newBusinessHours;

            return MapResponse(
                producerProfile);
        }

        public async Task<
            ProducerAvailabilityResponse?>
            UpdateMyAvailabilityModeAsync(
                int userId,
                UpdateProducerAvailabilityModeRequest
                    request)
        {
            var producerProfile =
                await _repository
                    .GetApprovedByUserIdWithBusinessHoursAsync(
                        userId);

            if (producerProfile == null)
            {
                return null;
            }

            var availabilityMode =
                request.AvailabilityMode
                    ?.Trim()
                ?? string.Empty;

            if (
                !ProducerAvailabilityModes
                    .IsValid(
                        availabilityMode)
            )
            {
                throw new ArgumentException(
                    "Geçersiz çalışma modu. " +
                    "Scheduled, ForceOpen veya ForceClosed kullanılmalıdır.");
            }

            if (
                availabilityMode ==
                    ProducerAvailabilityModes
                        .Scheduled &&
                producerProfile.BusinessHours
                    .Count != 7
            )
            {
                throw new ArgumentException(
                    "Otomatik çalışma moduna geçmeden önce haftanın 7 günü için çalışma saatlerini kaydedin.");
            }

            producerProfile.AvailabilityMode =
                availabilityMode;

            await _repository
                .SaveChangesAsync();

            return MapResponse(
                producerProfile);
        }

        private ProducerAvailabilityResponse
            MapResponse(
                ProducerProfile producerProfile)
        {
            return new ProducerAvailabilityResponse
            {
                AvailabilityMode =
                    producerProfile
                        .AvailabilityMode,

                IsCurrentlyOpen =
                    ProducerAvailabilityEvaluator
                        .IsCurrentlyOpen(
                            producerProfile,
                            _appClock.TurkeyNow),

                HasSchedule =
                    producerProfile.BusinessHours
                        .Count == 7,

                BusinessHours =
                    producerProfile.BusinessHours
                        .OrderBy(x =>
                            x.DayOfWeek)
                        .Select(x =>
                            new ProducerBusinessHourResponse
                            {
                                DayOfWeek =
                                    x.DayOfWeek,

                                DayName =
                                    GetDayName(
                                        x.DayOfWeek),

                                IsClosed =
                                    x.IsClosed,

                                OpenTime =
                                    FormatTime(
                                        x.OpenTime),

                                CloseTime =
                                    FormatTime(
                                        x.CloseTime)
                            })
                        .ToList()
            };
        }

        private static List<
            NormalizedBusinessHour>
            ValidateAndNormalize(
                UpdateProducerBusinessHoursRequest
                    request)
        {
            if (
                request.BusinessHours == null ||
                request.BusinessHours.Count != 7
            )
            {
                throw new ArgumentException(
                    "Haftanın 7 günü için çalışma saati bilgisi gönderilmelidir.");
            }

            var duplicateDayExists =
                request.BusinessHours
                    .GroupBy(x =>
                        x.DayOfWeek)
                    .Any(group =>
                        group.Count() > 1);

            if (duplicateDayExists)
            {
                throw new ArgumentException(
                    "Aynı gün için birden fazla çalışma saati gönderilemez.");
            }

            var dayNumbers =
                request.BusinessHours
                    .Select(x =>
                        x.DayOfWeek)
                    .OrderBy(x => x)
                    .ToArray();

            if (
                !dayNumbers.SequenceEqual(
                    new[]
                    {
                        1, 2, 3, 4, 5, 6, 7
                    })
            )
            {
                throw new ArgumentException(
                    "DayOfWeek değerleri 1 (Pazartesi) ile 7 (Pazar) arasındaki tüm günleri içermelidir.");
            }

            var normalized =
                new List<
                    NormalizedBusinessHour>(
                        capacity: 7);

            foreach (
                var businessHour
                in request.BusinessHours
                    .OrderBy(x =>
                        x.DayOfWeek)
            )
            {
                if (businessHour.IsClosed)
                {
                    normalized.Add(
                        new NormalizedBusinessHour(
                            businessHour.DayOfWeek,
                            true,
                            null,
                            null));

                    continue;
                }

                var openTime =
                    ParseTime(
                        businessHour.OpenTime,
                        businessHour.DayOfWeek,
                        "açılış");

                var closeTime =
                    ParseTime(
                        businessHour.CloseTime,
                        businessHour.DayOfWeek,
                        "kapanış");

                if (openTime == closeTime)
                {
                    throw new ArgumentException(
                        $"{GetDayName(businessHour.DayOfWeek)} için açılış ve kapanış saati aynı olamaz.");
                }

                normalized.Add(
                    new NormalizedBusinessHour(
                        businessHour.DayOfWeek,
                        false,
                        openTime,
                        closeTime));
            }

            return normalized;
        }

        private static TimeOnly ParseTime(
            string? value,
            int dayOfWeek,
            string fieldName)
        {
            if (
                string.IsNullOrWhiteSpace(
                    value)
            )
            {
                throw new ArgumentException(
                    $"{GetDayName(dayOfWeek)} için {fieldName} saati zorunludur.");
            }

            if (
                !TimeOnly.TryParseExact(
                    value.Trim(),
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed)
            )
            {
                throw new ArgumentException(
                    $"{GetDayName(dayOfWeek)} için {fieldName} saati HH:mm formatında olmalıdır.");
            }

            return parsed;
        }

        private static string? FormatTime(
            TimeOnly? value)
        {
            return value?.ToString(
                "HH:mm",
                CultureInfo.InvariantCulture);
        }

        private static string GetDayName(
            int dayOfWeek)
        {
            if (
                dayOfWeek < 1 ||
                dayOfWeek > 7
            )
            {
                return "Bilinmeyen";
            }

            return TurkishDayNames[
                dayOfWeek];
        }

        private sealed record
            NormalizedBusinessHour(
                int DayOfWeek,
                bool IsClosed,
                TimeOnly? OpenTime,
                TimeOnly? CloseTime);
    }
}
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Helpers
{
    public static class ProducerAvailabilityEvaluator
    {
        public static bool IsCurrentlyOpen(
            ProducerProfile producerProfile,
            DateTime turkeyNow)
        {
            /*
             * Bu bayrak platform seviyesindeki uygunluktur.
             * Üretici ForceOpen yapsa bile platform tarafından
             * kullanılamaz durumdaki bir profil açılamaz.
             */
            if (
                !producerProfile.IsApproved ||
                !producerProfile.IsAvailable ||
                !string.Equals(
                    producerProfile.VerificationStatus,
                    ProducerVerificationStatuses.Approved,
                    StringComparison.Ordinal)
            )
            {
                return false;
            }

            if (
                string.Equals(
                    producerProfile.AvailabilityMode,
                    ProducerAvailabilityModes.ForceClosed,
                    StringComparison.Ordinal)
            )
            {
                return false;
            }

            if (
                string.Equals(
                    producerProfile.AvailabilityMode,
                    ProducerAvailabilityModes.ForceOpen,
                    StringComparison.Ordinal)
            )
            {
                return true;
            }

            if (
                !string.Equals(
                    producerProfile.AvailabilityMode,
                    ProducerAvailabilityModes.Scheduled,
                    StringComparison.Ordinal)
            )
            {
                return false;
            }

            if (
                producerProfile.BusinessHours == null ||
                producerProfile.BusinessHours.Count == 0
            )
            {
                return false;
            }

            var currentDay =
                ToIsoDayOfWeek(
                    turkeyNow.DayOfWeek);

            var previousDay =
                currentDay == 1
                    ? 7
                    : currentDay - 1;

            var currentTime =
                TimeOnly.FromDateTime(
                    turkeyNow);

            var today =
                producerProfile.BusinessHours
                    .FirstOrDefault(x =>
                        x.DayOfWeek ==
                            currentDay);

            if (
                IsOpenDuringOwnDay(
                    today,
                    currentTime)
            )
            {
                return true;
            }

            /*
             * Gece yarısını aşan vardiya desteği:
             * Pazartesi 18:00 - 02:00 ise
             * Salı 01:00'da hâlâ açıktır.
             */
            var yesterday =
                producerProfile.BusinessHours
                    .FirstOrDefault(x =>
                        x.DayOfWeek ==
                            previousDay);

            return IsOpenFromPreviousDay(
                yesterday,
                currentTime);
        }

        private static bool IsOpenDuringOwnDay(
            ProducerBusinessHour? businessHour,
            TimeOnly currentTime)
        {
            if (
                businessHour == null ||
                businessHour.IsClosed ||
                !businessHour.OpenTime.HasValue ||
                !businessHour.CloseTime.HasValue
            )
            {
                return false;
            }

            var openTime =
                businessHour.OpenTime.Value;

            var closeTime =
                businessHour.CloseTime.Value;

            if (openTime == closeTime)
            {
                return false;
            }

            if (openTime < closeTime)
            {
                return currentTime >= openTime &&
                       currentTime < closeTime;
            }

            /*
             * Örnek 18:00 - 02:00:
             * aynı günün 18:00-23:59 kısmı.
             */
            return currentTime >= openTime;
        }

        private static bool IsOpenFromPreviousDay(
            ProducerBusinessHour? businessHour,
            TimeOnly currentTime)
        {
            if (
                businessHour == null ||
                businessHour.IsClosed ||
                !businessHour.OpenTime.HasValue ||
                !businessHour.CloseTime.HasValue
            )
            {
                return false;
            }

            var openTime =
                businessHour.OpenTime.Value;

            var closeTime =
                businessHour.CloseTime.Value;

            /*
             * Yalnız gece yarısını aşan çalışma saati
             * ertesi güne taşınabilir.
             */
            if (openTime <= closeTime)
            {
                return false;
            }

            return currentTime < closeTime;
        }

        private static int ToIsoDayOfWeek(
            DayOfWeek dayOfWeek)
        {
            return dayOfWeek ==
                DayOfWeek.Sunday
                    ? 7
                    : (int)dayOfWeek;
        }
    }
}
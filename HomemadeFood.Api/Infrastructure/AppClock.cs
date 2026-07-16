using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class AppClock : IAppClock
    {
        private readonly TimeZoneInfo _turkeyTimeZone;

        public AppClock()
        {
            var timeZoneId = OperatingSystem.IsWindows()
                ? "Turkey Standard Time"
                : "Europe/Istanbul";

            _turkeyTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    timeZoneId);
        }

        public DateTime UtcNow => DateTime.UtcNow;

        public DateOnly TurkeyToday =>
            GetTurkeyDate(UtcNow);

        public DateOnly GetTurkeyDate(
            DateTime utcDateTime)
        {
            var normalizedUtc =
                utcDateTime.Kind == DateTimeKind.Utc
                    ? utcDateTime
                    : DateTime.SpecifyKind(
                        utcDateTime,
                        DateTimeKind.Utc);

            var turkeyDateTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    normalizedUtc,
                    _turkeyTimeZone);

            return DateOnly.FromDateTime(
                turkeyDateTime);
        }
    }
}
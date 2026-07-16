namespace HomemadeFood.Api.Interfaces
{
    public interface IAppClock
    {
        DateTime UtcNow { get; }

        DateOnly TurkeyToday { get; }

        DateOnly GetTurkeyDate(DateTime utcDateTime);
    }
}
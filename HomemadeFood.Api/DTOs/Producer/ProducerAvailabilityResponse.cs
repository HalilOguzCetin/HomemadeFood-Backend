namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class ProducerAvailabilityResponse
    {
        public string AvailabilityMode { get; set; } =
            string.Empty;

        public bool IsCurrentlyOpen { get; set; }

        public bool HasSchedule { get; set; }

        public List<ProducerBusinessHourResponse>
            BusinessHours
        { get; set; } = new();
    }
}

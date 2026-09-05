namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class ProducerBusinessHourResponse
    {
        public int DayOfWeek { get; set; }

        public string DayName { get; set; } =
            string.Empty;

        public bool IsClosed { get; set; }

        public string? OpenTime { get; set; }

        public string? CloseTime { get; set; }
    }
}

namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class UpdateProducerBusinessHourRequest
    {
        /*
         * ISO-8601:
         * 1 = Pazartesi ... 7 = Pazar
         */
        public int DayOfWeek { get; set; }

        public bool IsClosed { get; set; }

        /*
         * Açık günlerde HH:mm formatı beklenir.
         * Örnek: 09:00 / 22:30
         */
        public string? OpenTime { get; set; }

        public string? CloseTime { get; set; }
    }
}

namespace HomemadeFood.Api.Entities
{
    public class ProducerBusinessHour
    {
        public int Id { get; set; }

        public int ProducerProfileId { get; set; }
        public ProducerProfile ProducerProfile { get; set; } = null!;

        /*
         * ISO-8601 haftanın günü:
         * 1 = Pazartesi ... 7 = Pazar
         */
        public int DayOfWeek { get; set; }

        /*
         * Gün tamamen kapalıysa OpenTime ve CloseTime null tutulur.
         */
        public bool IsClosed { get; set; }

        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }
    }
}
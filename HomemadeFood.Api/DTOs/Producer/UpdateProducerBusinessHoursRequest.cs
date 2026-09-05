namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class UpdateProducerBusinessHoursRequest
    {
        /*
         * Haftanın 7 günü de gönderilmelidir.
         */
        public List<UpdateProducerBusinessHourRequest>
            BusinessHours
        { get; set; } = new();
    }
}

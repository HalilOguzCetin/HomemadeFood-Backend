namespace HomemadeFood.Api.DTOs.Producer
{
    public sealed class UpdateProducerAvailabilityModeRequest
    {
        /*
         * Scheduled
         * ForceOpen
         * ForceClosed
         */
        public string AvailabilityMode { get; set; } =
            string.Empty;
    }
}

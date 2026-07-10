namespace HomemadeFood.Api.DTOs.Producer
{
    public class ProducerApplicationRequest
    {
        public string BusinessName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int DailyCapacity { get; set; }
    }
}
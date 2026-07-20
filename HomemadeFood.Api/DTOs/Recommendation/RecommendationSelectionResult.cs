namespace HomemadeFood.Api.DTOs.Recommendation
{
    public sealed class RecommendationSelectionResult
    {
        public bool IsSuccess { get; init; }

        public string Code { get; init; } =
            string.Empty;

        public string Message { get; init; } =
            string.Empty;

        public ProducerRecommendationSelectionResponse?
            Data
        { get; init; }

        public static RecommendationSelectionResult Succeed(
            ProducerRecommendationSelectionResponse data,
            string message)
        {
            return new RecommendationSelectionResult
            {
                IsSuccess = true,
                Code = string.Empty,
                Message = message,
                Data = data
            };
        }

        public static RecommendationSelectionResult Fail(
            string code,
            string message)
        {
            return new RecommendationSelectionResult
            {
                IsSuccess = false,
                Code = code,
                Message = message,
                Data = null
            };
        }
    }
}
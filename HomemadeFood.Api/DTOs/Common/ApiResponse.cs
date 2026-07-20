using HomemadeFood.Api.Constants;

namespace HomemadeFood.Api.DTOs.Common
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Code { get; init; }
            = string.Empty;

        public string Message { get; init; }
            = string.Empty;

        public T? Data { get; init; }

        public static ApiResponse<T> Succeed(
            T data,
            string message = "İşlem başarılı.",
            string code = ApiResponseCodes.Success)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = code,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(
            string code,
            string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = default
            };
        }
    }
}
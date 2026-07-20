using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler>
            _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Beklenmeyen hata oluştu. TraceId: {TraceId}",
                httpContext.TraceIdentifier);

            if (httpContext.Response.HasStarted)
            {
                return false;
            }

            httpContext.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            httpContext.Response.ContentType =
                "application/json; charset=utf-8";

            var response =
                new ApiResponse<object>
                {
                    Success = false,

                    Code =
                        ApiResponseCodes
                            .InternalServerError,

                    Message =
                        "Beklenmeyen bir sunucu hatası oluştu.",

                    Data = new
                    {
                        traceId =
                            httpContext.TraceIdentifier
                    }
                };

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}
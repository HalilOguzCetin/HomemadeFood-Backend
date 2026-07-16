using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Infrastructure
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

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

            var problemDetails = new ProblemDetails
            {
                Status =
                    StatusCodes.Status500InternalServerError,

                Title =
                    "Beklenmeyen bir sunucu hatası oluştu.",

                Detail =
                    "İşlem şu anda tamamlanamadı. Lütfen daha sonra tekrar deneyin.",

                Instance =
                    httpContext.Request.Path
            };

            problemDetails.Extensions["code"] =
                "INTERNAL_SERVER_ERROR";

            problemDetails.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            httpContext.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.DTOs.Recommendation;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public sealed class RecommendationAnalyticsController
        : ControllerBase
    {
        private readonly IRecommendationAnalyticsService
            _analyticsService;

        public RecommendationAnalyticsController(
            IRecommendationAnalyticsService analyticsService)
        {
            _analyticsService =
                analyticsService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult>
            GetPerformanceSummary()
        {
            var result =
                await _analyticsService
                    .GetPerformanceSummaryAsync();

            return Ok(
                ApiResponse<RecommendationPerformanceResponse>
                    .Succeed(
                        result,
                        "Öneri sistemi performans özeti başarıyla getirildi."));
        }
    }
}
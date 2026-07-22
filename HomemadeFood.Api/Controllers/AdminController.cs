using System.Security.Claims;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Admin;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService
            _adminService;

        public AdminController(
            IAdminService adminService)
        {
            _adminService =
                adminService;
        }

        [HttpGet("producer-applications")]
        public async Task<IActionResult>
            GetPendingApplications()
        {
            var applications =
                await _adminService
                    .GetPendingProducerApplicationsAsync();

            return Ok(
                ApiResponse<object>.Succeed(
                    applications,
                    "Bekleyen üretici başvuruları başarıyla getirildi."));
        }

        [HttpPost(
            "producer-applications/{id:int}/approve")]
        public async Task<IActionResult>
            ApproveProducer(
                int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici başvuru ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetAdminId(
                    out var adminId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Admin bilgisi alınamadı."));
            }

            var result =
                await _adminService
                    .ApproveProducerAsync(
                        id,
                        adminId);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerApprovalFailed,
                        "Başvuru bulunamadı veya daha önce işleme alınmış olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        producerApplicationId =
                            id,

                        approved =
                            true
                    },
                    "Üretici başvurusu başarıyla onaylandı."));
        }

        [HttpPost(
            "producer-applications/{id:int}/reject")]
        public async Task<IActionResult>
            RejectProducer(
                int id,
                [FromBody]
                RejectProducerApplicationRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.BadRequest,
                        "Üretici başvuru ID değeri sıfırdan büyük olmalıdır."));
            }

            if (!TryGetAdminId(
                    out var adminId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes.Unauthorized,
                        "Admin bilgisi alınamadı."));
            }

            var rejectionReason =
                request.Reason.Trim();

            var result =
                await _adminService
                    .RejectProducerAsync(
                        id,
                        adminId,
                        rejectionReason);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ProducerRejectionFailed,
                        "Başvuru bulunamadı veya daha önce işleme alınmış olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        producerApplicationId =
                            id,

                        rejected =
                            true,

                        reason =
                            rejectionReason
                    },
                    "Üretici başvurusu başarıyla reddedildi."));
        }

        private bool TryGetAdminId(
            out int adminId)
        {
            var adminIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                adminIdValue,
                out adminId);
        }
    }
}
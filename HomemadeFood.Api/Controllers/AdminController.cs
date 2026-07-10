using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("producer-applications")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var applications =
                await _adminService.GetPendingProducerApplicationsAsync();

            return Ok(applications);
        }

        [HttpPost("producer-applications/{id}/approve")]
        public async Task<IActionResult> ApproveProducer(int id)
        {
            var adminIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(adminIdValue, out var adminId))
            {
                return Unauthorized("Admin bilgisi alınamadı.");
            }

            var result =
                await _adminService.ApproveProducerAsync(id, adminId);

            if (!result)
            {
                return BadRequest(
                    "Başvuru bulunamadı veya daha önce işleme alınmış.");
            }

            return Ok("Üretici başvurusu onaylandı.");
        }
    }
}
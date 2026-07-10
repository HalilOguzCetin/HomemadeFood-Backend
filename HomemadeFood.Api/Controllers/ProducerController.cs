using HomemadeFood.Api.DTOs.Producer;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducerController : ControllerBase
    {
        private readonly IProducerService _producerService;

        public ProducerController(IProducerService producerService)
        {
            _producerService = producerService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply(ProducerApplicationRequest request)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı.");
            }

            var result = await _producerService.ApplyAsync(userId, request);

            if (!result)
            {
                return BadRequest(
                    "Başvuru oluşturulamadı. Daha önce başvuru yapmış olabilirsiniz veya kapasite bilgisi geçersizdir.");
            }

            return Ok("Üretici başvurusu başarıyla oluşturuldu. Admin onayı bekleniyor.");
        }
    }
}
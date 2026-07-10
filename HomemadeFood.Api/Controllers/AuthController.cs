using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HomemadeFood.Api.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Auth API çalışıyor.");
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result)
            {
                return BadRequest("Kayıt başarısız.");
            }

            return Ok("Kullanıcı başarıyla kaydedildi.");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized("Email veya şifre hatalı.");
            }


            return Ok(result);
        }
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("Bu endpoint'e sadece giriş yapan kullanıcı erişebilir.");
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("customer-area")]
        public IActionResult CustomerArea()
        {
            return Ok("Customer yetkisi doğrulandı.");
        }

        [Authorize(Roles = "Producer")]
        [HttpGet("producer-area")]
        public IActionResult ProducerArea()
        {
            return Ok("Producer yetkisi doğrulandı.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-area")]
        public IActionResult AdminArea()
        {
            return Ok("Admin yetkisi doğrulandı.");
        }
    }
}
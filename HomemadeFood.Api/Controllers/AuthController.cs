using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HomemadeFood.Api.DTOs.Auth;

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
    }
}
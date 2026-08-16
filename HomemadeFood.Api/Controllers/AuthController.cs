using HomemadeFood.Api.Constants;
using HomemadeFood.Api.DTOs.Auth;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace HomemadeFood.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService
            _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService =
                authService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        apiStatus =
                            "Running"
                    },
                    "Auth API çalışıyor."));
        }

        [HttpPost("register")]
        public async Task<IActionResult>
            Register(
                [FromBody]
                RegisterRequest request)
        {
            var result =
                await _authService
                    .RegisterAsync(
                        request);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .RegistrationFailed,

                        "Kayıt işlemi başarısız oldu. E-posta adresi daha önce kullanılmış olabilir."));
            }

            return StatusCode(
                StatusCodes
                    .Status201Created,

                ApiResponse<object>.Succeed(
                    new
                    {
                        email =
                            request.Email
                                .Trim()
                                .ToLowerInvariant()
                    },

                    "Kullanıcı başarıyla kaydedildi.",

                    ApiResponseCodes
                        .Created));
        }

        [EnableRateLimiting(
            RateLimitPolicies
                .EmailVerificationResend)]
        [HttpPost(
            "resend-email-verification")]
        public async Task<IActionResult>
            ResendEmailVerification(
                [FromBody]
                ResendEmailVerificationRequest request)
        {
            await _authService
                .ResendEmailVerificationAsync(
                    request);

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        email =
                            request.Email
                                .Trim()
                                .ToLowerInvariant()
                    },

                    "E-posta adresi uygunsa yeni bir doğrulama kodu gönderildi.",

                    ApiResponseCodes
                        .EmailVerificationCodeRequested));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult>
            VerifyEmail(
                [FromBody]
                VerifyEmailRequest request)
        {
            var result =
                await _authService
                    .VerifyEmailAsync(
                        request);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .EmailVerificationFailed,

                        "Doğrulama kodu geçersiz, süresi dolmuş veya kullanılamıyor."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        email =
                            request.Email
                                .Trim()
                                .ToLowerInvariant()
                    },

                    "E-posta adresi başarıyla doğrulandı.",

                    ApiResponseCodes
                        .EmailVerified));
        }

        [EnableRateLimiting(
            RateLimitPolicies
                .PasswordResetRequest)]
        [HttpPost("forgot-password")]
        public async Task<IActionResult>
            ForgotPassword(
                [FromBody]
                ForgotPasswordRequest request)
        {
            await _authService
                .ForgotPasswordAsync(
                    request);

            /*
             * Hesabın var olup olmadığı,
             * aktif olup olmadığı veya e-posta
             * doğrulama durumu açıklanmaz.
             */
            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        email =
                            request.Email
                                .Trim()
                                .ToLowerInvariant()
                    },

                    "E-posta adresi uygunsa şifre sıfırlama kodu gönderildi.",

                    ApiResponseCodes
                        .PasswordResetCodeRequested));
        }

        [EnableRateLimiting(
            RateLimitPolicies
                .PasswordResetConfirm)]
        [HttpPost("reset-password")]
        public async Task<IActionResult>
            ResetPassword(
                [FromBody]
                ResetPasswordRequest request)
        {
            var result =
                await _authService
                    .ResetPasswordAsync(
                        request);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .PasswordResetFailed,

                        "Şifre sıfırlama kodu geçersiz, süresi dolmuş veya kullanılamıyor."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        email =
                            request.Email
                                .Trim()
                                .ToLowerInvariant()
                    },

                    "Şifreniz başarıyla güncellendi.",

                    ApiResponseCodes
                        .PasswordResetSuccess));
        }

        [EnableRateLimiting(
            RateLimitPolicies.Login)]
        [HttpPost("login")]
        public async Task<IActionResult>
            Login(
                [FromBody]
                LoginRequest request)
        {
            var result =
                await _authService
                    .LoginAsync(
                        request);

            if (
                result.Status ==
                LoginResultStatus
                    .InvalidCredentials
            )
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .LoginFailed,

                        "E-posta veya şifre hatalı."));
            }

            if (
                result.Status ==
                LoginResultStatus
                    .EmailNotVerified
            )
            {
                return StatusCode(
                    StatusCodes
                        .Status403Forbidden,

                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .EmailNotVerified,

                        "E-posta adresinizi doğrulamanız gerekiyor."));
            }

            if (
                result.Status !=
                    LoginResultStatus.Success ||
                result.Response == null
            )
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .LoginFailed,

                        "E-posta veya şifre hatalı."));
            }

            return Ok(
                ApiResponse<LoginResponse>.Succeed(
                    result.Response,
                    "Giriş başarılı."));
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult>
            Profile()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes
                        .NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,

                        "Kullanıcı bilgisi alınamadı."));
            }

            var profile =
                await _authService
                    .GetProfileAsync(
                        userId);

            if (profile == null)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,

                        "Kullanıcı bulunamadı veya hesap pasif."));
            }

            return Ok(
                ApiResponse<AuthProfileResponse>
                    .Succeed(
                        profile,

                        "Kullanıcı profili başarıyla getirildi."));
        }

        [Authorize(
            Roles =
                UserRoles.Customer)]
        [EnableRateLimiting(
            RateLimitPolicies
                .PhoneVerificationRequest)]
        [HttpPost(
            "phone/request-code")]
        public async Task<IActionResult>
            RequestPhoneVerification(
                [FromBody]
                RequestPhoneVerificationRequest request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes
                        .NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,

                        "Kullanıcı bilgisi alınamadı."));
            }

            var result =
                await _authService
                    .RequestPhoneVerificationAsync(
                        userId,
                        request);

            if (!result)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .PhoneVerificationCodeRequestFailed,

                        "Telefon doğrulama kodu gönderilemedi. Numara geçersiz, kullanımda veya kısa süre önce kod gönderilmiş olabilir."));
            }

            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        phone =
                            request.Phone.Trim()
                    },

                    "Telefon doğrulama kodu gönderildi.",

                    ApiResponseCodes
                        .PhoneVerificationCodeRequested));
        }

        [Authorize(
            Roles =
                UserRoles.Customer)]
        [EnableRateLimiting(
            RateLimitPolicies
                .PhoneVerificationConfirm)]
        [HttpPost("phone/verify")]
        public async Task<IActionResult>
            VerifyPhone(
                [FromBody]
                VerifyPhoneRequest request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes
                        .NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,

                        "Kullanıcı bilgisi alınamadı."));
            }

            var profile =
                await _authService
                    .VerifyPhoneAsync(
                        userId,
                        request);

            if (profile == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .PhoneVerificationFailed,

                        "Telefon doğrulama kodu geçersiz, süresi dolmuş veya numara kullanılamıyor."));
            }

            return Ok(
                ApiResponse<AuthProfileResponse>
                    .Succeed(
                        profile,

                        "Telefon numarası başarıyla doğrulandı.",

                        ApiResponseCodes
                            .PhoneVerified));
        }

        /*
         * C2 profil düzenleme.
         *
         * E-posta ve telefon burada doğrudan
         * değiştirilemez. Bunlar doğrulama
         * challenge akışlarıyla ayrı tutulur.
         */
        [Authorize(
            Roles =
                UserRoles.Customer)]
        [HttpPut("profile")]
        public async Task<IActionResult>
            UpdateProfile(
                [FromBody]
                UpdateAuthProfileRequest request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes
                        .NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .Unauthorized,

                        "Kullanıcı bilgisi alınamadı."));
            }

            var profile =
                await _authService
                    .UpdateProfileAsync(
                        userId,
                        request);

            if (profile == null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        ApiResponseCodes
                            .ValidationError,

                        "Profil bilgileri güncellenemedi."));
            }

            return Ok(
                ApiResponse<AuthProfileResponse>
                    .Succeed(
                        profile,

                        "Profil bilgileri başarıyla güncellendi."));
        }

        [Authorize(
            Roles =
                UserRoles.Customer)]
        [HttpGet("customer-area")]
        public IActionResult
            CustomerArea()
        {
            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        authorizedRole =
                            UserRoles.Customer
                    },

                    "Customer yetkisi doğrulandı."));
        }

        [Authorize(
            Policy =
                AuthorizationPolicies
                    .ApprovedProducer)]
        [HttpGet("producer-area")]
        public IActionResult
            ProducerArea()
        {
            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        producerModeAuthorized =
                            true
                    },

                    "Onaylı üretici yetkisi doğrulandı."));
        }

        [Authorize(
            Roles =
                UserRoles.Admin)]
        [HttpGet("admin-area")]
        public IActionResult
            AdminArea()
        {
            return Ok(
                ApiResponse<object>.Succeed(
                    new
                    {
                        authorizedRole =
                            UserRoles.Admin
                    },

                    "Admin yetkisi doğrulandı."));
        }
    }
}
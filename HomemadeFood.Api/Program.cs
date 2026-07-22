using System.Security.Claims;
using System.Text;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using HomemadeFood.Api.DTOs.Common;
using HomemadeFood.Api.Helpers;
using HomemadeFood.Api.Infrastructure;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.Repositories;
using HomemadeFood.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// ZORUNLU YAPILANDIRMA KONTROLLERÝ
// ---------------------------------------------------------

var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection deðeri bulunamadý. " +
        "Baðlantý bilgisini User Secrets veya uygulama ayarlarýna ekleyin.");
}

var jwtKey =
    builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key deðeri bulunamadý. " +
        "JWT anahtarýný User Secrets içine ekleyin.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "Jwt:Issuer deðeri bulunamadý.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Jwt:Audience deðeri bulunamadý.");
}

var jwtSigningKey =
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey));

// ---------------------------------------------------------
// CONTROLLER VE API DAVRANIÞI
// ---------------------------------------------------------

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var errors =
                    context.ModelState
                        .Where(x =>
                            x.Value != null &&
                            x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(error =>
                                    string.IsNullOrWhiteSpace(
                                        error.ErrorMessage)
                                        ? "Geçersiz deðer gönderildi."
                                        : error.ErrorMessage)
                                .ToArray());

                var response =
                    new ApiResponse<object>
                    {
                        Success = false,

                        Code =
                            ApiResponseCodes
                                .ValidationError,

                        Message =
                            "Gönderilen bilgiler doðrulanamadý.",

                        Data = new
                        {
                            errors,

                            traceId =
                                context.HttpContext
                                    .TraceIdentifier
                        }
                    };

                return new BadRequestObjectResult(
                    response);
            };
    });

// ---------------------------------------------------------
// VERÝTABANI
// ---------------------------------------------------------

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(
                connectionString)));

// ---------------------------------------------------------
// SERVÝSLER VE REPOSITORY KAYITLARI
// ---------------------------------------------------------

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IJwtTokenGenerator,
    JwtTokenGenerator>();

builder.Services.AddScoped<
    IProducerRepository,
    ProducerRepository>();

builder.Services.AddScoped<
    IProducerService,
    ProducerService>();

builder.Services.AddScoped<
    IAdminService,
    AdminService>();

builder.Services.AddScoped<
    IFoodRepository,
    FoodRepository>();

builder.Services.AddScoped<
    IFoodService,
    FoodService>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IFavoriteRepository,
    FavoriteRepository>();

builder.Services.AddScoped<
    IFavoriteService,
    FavoriteService>();

builder.Services.AddScoped<
    IAddressRepository,
    AddressRepository>();

builder.Services.AddScoped<
    IAddressService,
    AddressService>();

builder.Services.AddScoped<
    ICartRepository,
    CartRepository>();

builder.Services.AddScoped<
    ICartService,
    CartService>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddScoped<
    IProducerOrderService,
    ProducerOrderService>();

builder.Services.AddScoped<
    IReviewRepository,
    ReviewRepository>();

builder.Services.AddScoped<
    IReviewService,
    ReviewService>();

builder.Services.AddScoped<
    IProducerCapacityService,
    ProducerCapacityService>();

builder.Services.AddScoped<
    IProducerRecommendationService,
    ProducerRecommendationService>();

builder.Services.AddScoped<
    IRecommendationAnalyticsService,
    RecommendationAnalyticsService>();

builder.Services.AddSingleton<
    IAppClock,
    AppClock>();

// ---------------------------------------------------------
// GLOBAL HATA YÖNETÝMÝ
// ---------------------------------------------------------

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

// ---------------------------------------------------------
// JWT KÝMLÝK DOÐRULAMA
// ---------------------------------------------------------

builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer =
                        jwtIssuer,

                    ValidAudience =
                        jwtAudience,

                    IssuerSigningKey =
                        jwtSigningKey,

                    /*
                     * Token süresi dolduðunda varsayýlan
                     * beþ dakikalýk toleransý kaldýrýr.
                     */
                    ClockSkew =
                        TimeSpan.Zero
                };

            options.Events =
                new JwtBearerEvents
                {
                    /*
                     * Token imzasý geçerli olsa bile
                     * kullanýcýnýn güncel durumu
                     * veritabanýndan kontrol edilir.
                     */
                    OnTokenValidated =
                        async context =>
                        {
                            var userIdValue =
                                context.Principal?
                                    .FindFirstValue(
                                        ClaimTypes
                                            .NameIdentifier);

                            if (!int.TryParse(
                                    userIdValue,
                                    out var userId))
                            {
                                context.Fail(
                                    "Token içindeki kullanýcý bilgisi geçersiz.");

                                return;
                            }

                            var dbContext =
                                context.HttpContext
                                    .RequestServices
                                    .GetRequiredService<
                                        AppDbContext>();

                            var user =
                                await dbContext.Users
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(
                                        x => x.Id == userId,
                                        context.HttpContext
                                            .RequestAborted);

                            if (user == null ||
                                !user.IsActive)
                            {
                                context.Fail(
                                    "Kullanýcý bulunamadý veya hesap pasif.");

                                return;
                            }

                            var tokenRole =
                                context.Principal?
                                    .FindFirstValue(
                                        ClaimTypes.Role);

                            if (!string.Equals(
                                    tokenRole,
                                    user.Role,
                                    StringComparison
                                        .Ordinal))
                            {
                                context.Fail(
                                    "Kullanýcýnýn rolü deðiþti. Yeniden giriþ yapýlmalýdýr.");

                                return;
                            }
                        },

                    /*
                     * Giriþ yapýlmamýþ veya token
                     * geçersiz olduðunda standart 401 cevabý.
                     */
                    OnChallenge =
                        async context =>
                        {
                            context.HandleResponse();

                            if (context.Response
                                .HasStarted)
                            {
                                return;
                            }

                            context.Response.StatusCode =
                                StatusCodes
                                    .Status401Unauthorized;

                            context.Response.ContentType =
                                "application/json; charset=utf-8";

                            var response =
                                ApiResponse<object>.Fail(
                                    ApiResponseCodes
                                        .Unauthorized,

                                    "Bu iþlem için giriþ yapmanýz gerekiyor veya oturumunuz geçersiz.");

                            await context.Response
                                .WriteAsJsonAsync(
                                    response);
                        },

                    /*
                     * Kullanýcý giriþ yapmýþ olsa bile
                     * rolü yetmiyorsa standart 403 cevabý.
                     */
                    OnForbidden =
                        async context =>
                        {
                            if (context.Response
                                .HasStarted)
                            {
                                return;
                            }

                            context.Response.StatusCode =
                                StatusCodes
                                    .Status403Forbidden;

                            context.Response.ContentType =
                                "application/json; charset=utf-8";

                            var response =
                                ApiResponse<object>.Fail(
                                    ApiResponseCodes
                                        .Forbidden,

                                    "Bu iþlem için gerekli yetkiye sahip deðilsiniz.");

                            await context.Response
                                .WriteAsJsonAsync(
                                    response);
                        }
                };
        });

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// SWAGGER
// ---------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    options =>
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name =
                    "Authorization",

                Type =
                    SecuritySchemeType.Http,

                Scheme =
                    "bearer",

                BearerFormat =
                    "JWT",

                In =
                    ParameterLocation.Header,

                Description =
                    "JWT token deðerini giriniz."
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType
                                        .SecurityScheme,

                                Id =
                                    "Bearer"
                            }
                    },

                    Array.Empty<string>()
                }
            });
    });

// ---------------------------------------------------------
// UYGULAMA
// ---------------------------------------------------------

var app =
    builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
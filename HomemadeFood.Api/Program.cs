using HomemadeFood.Api.Data;
using Microsoft.EntityFrameworkCore;
using HomemadeFood.Api.Interfaces;
using HomemadeFood.Api.Services;
using HomemadeFood.Api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HomemadeFood.Api.Helpers;
using System.Security.Claims;
using HomemadeFood.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProducerOrderService,ProducerOrderService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddSingleton<IAppClock, AppClock>();
builder.Services.AddScoped<
    IProducerCapacityService,
    ProducerCapacityService>();
builder.Services.AddScoped<
    IReviewService,
    ReviewService>();
builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var details =
                    new ValidationProblemDetails(
                        context.ModelState)
                    {
                        Status =
                            StatusCodes.Status400BadRequest,

                        Title =
                            "Gönderilen bilgiler doðrulanamadý.",

                        Instance =
                            context.HttpContext.Request.Path
                    };

                details.Extensions["code"] =
                    "VALIDATION_ERROR";

                details.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(details);
            };
    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key)
        };

    // Token geçerli göründükten sonra kullanýcýnýn
    // güncel durumunu veritabanýndan kontrol eder.
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdValue =
                context.Principal?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                context.Fail(
                    "Token içindeki kullanýcý bilgisi geçersiz.");

                return;
            }

            var dbContext =
                context.HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>();

            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || !user.IsActive)
            {
                context.Fail(
                    "Kullanýcý bulunamadý veya hesap pasif.");

                return;
            }

            var tokenRole =
                context.Principal?.FindFirstValue(
                    ClaimTypes.Role);

            if (!string.Equals(
                    tokenRole,
                    user.Role,
                    StringComparison.Ordinal))
            {
                context.Fail(
                    "Kullanýcýnýn rolü deðiþti. Yeniden giriþ yapýlmalýdýr.");

                return;
            }
        }
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token giriniz."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
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
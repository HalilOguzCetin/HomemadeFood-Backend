using System.Security.Claims;
using HomemadeFood.Api.Constants;
using HomemadeFood.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Authorization
{
    public sealed class ApprovedProducerAuthorizationHandler
        : AuthorizationHandler<ApprovedProducerRequirement>
    {
        private readonly AppDbContext _context;

        public ApprovedProducerAuthorizationHandler(
            AppDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApprovedProducerRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userIdValue = context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return;
            }

            var canUseProducerMode =
                await _context.ProducerProfiles
                    .AsNoTracking()
                    .AnyAsync(producerProfile =>
                        producerProfile.UserId == userId &&
                        producerProfile.IsApproved &&
                        producerProfile.VerificationStatus ==
                            ProducerVerificationStatuses.Approved &&
                        producerProfile.User.IsActive &&
                        producerProfile.User.Role ==
                            UserRoles.Customer);

            if (canUseProducerMode)
            {
                context.Succeed(requirement);
            }
        }
    }
}
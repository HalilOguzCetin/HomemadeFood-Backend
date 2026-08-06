using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Order order)
        {
            await _context.Orders
                .AddAsync(order);
        }

        public async Task<List<Order>>
            GetByCustomerIdAsync(
                int customerId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .Where(x =>
                    x.CustomerId == customerId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?>
            GetByIdAndCustomerIdAsync(
                int orderId,
                int customerId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.CustomerId == customerId);
        }

        public async Task<Order?>
            GetTrackedByIdAndCustomerIdAsync(
                int orderId,
                int customerId)
        {
            return await _context.Orders
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.CustomerId == customerId);
        }

        public async Task<List<Order>>
            GetByProducerProfileIdAsync(
                int producerProfileId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?>
            GetTrackedByIdAndProducerProfileIdAsync(
                int orderId,
                int producerProfileId)
        {
            return await _context.Orders
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.ProducerProfileId ==
                    producerProfileId);
        }

        public async Task<List<Order>>
            GetForAdminAsync(
                string? status,
                int? customerId,
                int? producerProfileId,
                string? search,
                DateTime? dateFrom,
                DateTime? dateToExclusive)
        {
            var query =
                _context.Orders
                    .AsNoTracking()
                    .Include(x =>
                        x.Customer)
                    .Include(x =>
                        x.ProducerProfile)
                    .Include(x =>
                        x.OrderItems)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x =>
                    x.Status == status);
            }

            if (customerId.HasValue)
            {
                query = query.Where(x =>
                    x.CustomerId ==
                    customerId.Value);
            }

            if (producerProfileId.HasValue)
            {
                query = query.Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchPattern =
                    $"%{search.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.Like(
                        x.Customer.FullName,
                        searchPattern) ||

                    EF.Functions.Like(
                        x.Customer.Email,
                        searchPattern) ||

                    EF.Functions.Like(
                        x.ProducerProfile.BusinessName,
                        searchPattern) ||

                    x.OrderItems.Any(item =>
                        EF.Functions.Like(
                            item.FoodName,
                            searchPattern)));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt >=
                    dateFrom.Value);
            }

            if (dateToExclusive.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedAt <
                    dateToExclusive.Value);
            }

            return await query
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }

        public async Task<Order?>
            GetByIdForAdminAsync(
                int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.ProducerProfile)
                .Include(x =>
                    x.OrderItems)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId);
        }

        public async Task SaveChangesAsync()
        {
            await _context
                .SaveChangesAsync();
        }
    }
}
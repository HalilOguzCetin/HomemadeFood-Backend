using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<List<Order>>
            GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x => x.ProducerProfile)
                .Include(x => x.OrderItems)
                .Where(x =>
                    x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?>
            GetByIdAndCustomerIdAsync(
                int orderId,
                int customerId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(x => x.ProducerProfile)
                .Include(x => x.OrderItems)
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
                .Include(x => x.ProducerProfile)
                .Include(x => x.OrderItems)
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
                .Include(x => x.Customer)
                .Include(x => x.ProducerProfile)
                .Include(x => x.OrderItems)
                .Where(x =>
                    x.ProducerProfileId ==
                    producerProfileId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?>
            GetTrackedByIdAndProducerProfileIdAsync(
                int orderId,
                int producerProfileId)
        {
            return await _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.ProducerProfile)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.ProducerProfileId ==
                    producerProfileId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
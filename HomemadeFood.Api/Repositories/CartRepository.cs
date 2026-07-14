using HomemadeFood.Api.Data;
using HomemadeFood.Api.Entities;
using HomemadeFood.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomemadeFood.Api.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?>
    GetByUserIdWithDetailsAsync(int userId)
        {
            return await _context.Carts
                .AsNoTracking()
                .Include(x => x.ProducerProfile)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Food)
                        .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId);
        }

        public async Task<Cart?>
            GetTrackedByUserIdWithItemsAsync(int userId)
        {
            return await _context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId);
        }

        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public void Remove(Cart cart)
        {
            _context.Carts.Remove(cart);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public void RemoveItem(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
        }
        public async Task<Cart?>
    GetForOrderCreationAsync(int userId)
        {
            return await _context.Carts
                .Include(x => x.ProducerProfile)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Food)
                        .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId);
        }
    }
}
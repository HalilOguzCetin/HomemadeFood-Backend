using HomemadeFood.Api.DTOs.Cart;
using HomemadeFood.Api.Interfaces;
using CartEntity = HomemadeFood.Api.Entities.Cart;
using CartItemEntity = HomemadeFood.Api.Entities.CartItem;

namespace HomemadeFood.Api.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IFoodRepository _foodRepository;

        public CartService(
            ICartRepository cartRepository,
            IFoodRepository foodRepository)
        {
            _cartRepository = cartRepository;
            _foodRepository = foodRepository;
        }

        public async Task<CartResponse> GetCartAsync(
            int userId)
        {
            var cart =
                await _cartRepository
                    .GetByUserIdWithDetailsAsync(userId);

            return MapToResponse(cart);
        }

        public async Task<CartResponse?> AddItemAsync(
            int userId,
            AddCartItemRequest request)
        {
            var food =
                await _foodRepository
                    .GetAvailableFoodByIdAsync(
                        request.FoodId);

            // Bulunmayan veya satışta olmayan yemek
            // sepete eklenemez.
            if (food == null)
            {
                return null;
            }

            var cart =
                await _cartRepository
                    .GetTrackedByUserIdWithItemsAsync(
                        userId);

            if (cart == null)
            {
                cart = new CartEntity
                {
                    UserId = userId,

                    ProducerProfileId =
                        food.ProducerProfileId,

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                cart.Items.Add(new CartItemEntity
                {
                    FoodId = food.Id,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow
                });

                await _cartRepository.AddAsync(cart);
            }
            else
            {
                // Mevcut sepette yalnızca aynı üreticinin
                // yemekleri bulunabilir.
                if (cart.ProducerProfileId !=
                    food.ProducerProfileId)
                {
                    return null;
                }

                var existingItem =
                    cart.Items.FirstOrDefault(x =>
                        x.FoodId == food.Id);

                if (existingItem != null)
                {
                    var newQuantity =
                        existingItem.Quantity +
                        request.Quantity;

                    if (newQuantity > 50)
                    {
                        return null;
                    }

                    existingItem.Quantity =
                        newQuantity;
                }
                else
                {
                    cart.Items.Add(new CartItemEntity
                    {
                        FoodId = food.Id,
                        Quantity = request.Quantity,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                cart.UpdatedAt = DateTime.UtcNow;
            }

            await _cartRepository.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<CartResponse?> UpdateItemAsync(
            int userId,
            int cartItemId,
            UpdateCartItemRequest request)
        {
            var cart =
                await _cartRepository
                    .GetTrackedByUserIdWithItemsAsync(
                        userId);

            if (cart == null)
            {
                return null;
            }

            var cartItem =
                cart.Items.FirstOrDefault(x =>
                    x.Id == cartItemId);

            if (cartItem == null)
            {
                return null;
            }

            cartItem.Quantity = request.Quantity;
            cart.UpdatedAt = DateTime.UtcNow;

            await _cartRepository.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<CartResponse?> RemoveItemAsync(
            int userId,
            int cartItemId)
        {
            var cart =
                await _cartRepository
                    .GetTrackedByUserIdWithItemsAsync(
                        userId);

            if (cart == null)
            {
                return null;
            }

            var cartItem =
                cart.Items.FirstOrDefault(x =>
                    x.Id == cartItemId);

            if (cartItem == null)
            {
                return null;
            }

            // Sepette yalnızca bu ürün kaldıysa,
            // boş sepet bırakmak yerine sepeti sil.
            if (cart.Items.Count == 1)
            {
                _cartRepository.Remove(cart);
            }
            else
            {
                _cartRepository.RemoveItem(cartItem);
                cart.UpdatedAt = DateTime.UtcNow;
            }

            await _cartRepository.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<bool> ClearCartAsync(
            int userId)
        {
            var cart =
                await _cartRepository
                    .GetTrackedByUserIdWithItemsAsync(
                        userId);

            if (cart == null)
            {
                return false;
            }

            _cartRepository.Remove(cart);

            await _cartRepository.SaveChangesAsync();

            return true;
        }

        private static CartResponse MapToResponse(
            CartEntity? cart)
        {
            if (cart == null)
            {
                return new CartResponse
                {
                    CartId = null,
                    ProducerProfileId = null,
                    BusinessName = string.Empty,
                    Items = new List<CartItemResponse>(),
                    TotalQuantity = 0,
                    TotalPrice = 0
                };
            }

            var items = cart.Items
                .OrderByDescending(x => x.CreatedAt)
                .Select(x =>
                {
                    var isAvailable =
                        x.Food.IsAvailable &&
                        x.Food.Category.IsActive &&
                        cart.ProducerProfile.IsApproved &&
                        cart.ProducerProfile.IsAvailable &&
                        cart.ProducerProfile
                            .VerificationStatus == "Approved";

                    return new CartItemResponse
                    {
                        CartItemId = x.Id,
                        FoodId = x.FoodId,
                        FoodName = x.Food.Name,
                        ImageUrl = x.Food.ImageUrl,
                        UnitPrice = x.Food.Price,
                        Quantity = x.Quantity,

                        LineTotal =
                            x.Food.Price * x.Quantity,

                        IsAvailable = isAvailable
                    };
                })
                .ToList();

            return new CartResponse
            {
                CartId = cart.Id,

                ProducerProfileId =
                    cart.ProducerProfileId,

                BusinessName =
                    cart.ProducerProfile.BusinessName,

                Items = items,

                TotalQuantity =
                    items.Sum(x => x.Quantity),

                TotalPrice =
                    items.Sum(x => x.LineTotal)
            };
        }
    }
}
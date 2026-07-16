using HomemadeFood.Api.Entities;

namespace HomemadeFood.Api.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetActiveCategoriesAsync();
    }
}
using HomemadeFood.Api.DTOs.Category;

namespace HomemadeFood.Api.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetActiveCategoriesAsync();
    }
}
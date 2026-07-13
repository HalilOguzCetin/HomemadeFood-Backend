using HomemadeFood.Api.DTOs.Category;
using HomemadeFood.Api.Interfaces;

namespace HomemadeFood.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(
            ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryResponse>>
            GetActiveCategoriesAsync()
        {
            var categories =
                await _categoryRepository
                    .GetActiveCategoriesAsync();

            return categories
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToList();
        }
    }
}
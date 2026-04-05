using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface IProductRecipe
    {
        Task SaveRecipeAsync(CreateProductRecipeDto dto);
        Task<List<ProductRecipeListVM>> GetAllProductRecipes();
    }
}

using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class ProductRecipeService : IProductRecipe
    {
        private readonly AppDbContext _context;

        public ProductRecipeService(AppDbContext context)
        {
            _context = context;
        }
        /*public async Task SaveRecipeAsync(CreateProductRecipeDto dto)
        {
            // Optional: prevent duplicate recipes
            var existingRecipe = await _context.ProductRecipes
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.CategoryId == dto.CategoryId);

            if (existingRecipe != null)
            {
                throw new Exception("Recipe already exists for this product and category.");
            }

            var recipe = new ProductRecipe
            {
                CategoryId = dto.CategoryId,
                ProductId = dto.ProductId,
                Details = dto.Ingredients.Select(i => new ProductRecipeDetail
                {
                    ProductIngredientId = i.ProductIngredientId,
                    UnitName = i.UnitName,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.ProductRecipes.Add(recipe);
            await _context.SaveChangesAsync();
        }*/
        public async Task SaveRecipeAsync(CreateProductRecipeDto dto)
        {
            var existingRecipe = await _context.ProductRecipes
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.CategoryId == dto.CategoryId);

            if (existingRecipe != null)
            {
                _context.ProductRecipeDetails.RemoveRange(existingRecipe.Details);

                existingRecipe.Details = dto.Ingredients.Select(i => new ProductRecipeDetail
                {
                    ProductIngredientId = i.ProductIngredientId,
                    UnitName = i.UnitName,
                    Quantity = i.Quantity
                }).ToList();

                existingRecipe.CategoryId = dto.CategoryId;
                existingRecipe.ProductId = dto.ProductId;

                _context.ProductRecipes.Update(existingRecipe);
            }
            else
            {
                var recipe = new ProductRecipe
                {
                    CategoryId = dto.CategoryId,
                    ProductId = dto.ProductId,
                    Details = dto.Ingredients.Select(i => new ProductRecipeDetail
                    {
                        ProductIngredientId = i.ProductIngredientId,
                        UnitName = i.UnitName,
                        Quantity = i.Quantity
                    }).ToList()
                };

                _context.ProductRecipes.Add(recipe);
            }

            await _context.SaveChangesAsync();
        }
    }
}

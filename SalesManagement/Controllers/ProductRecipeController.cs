using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Implementation;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [ApiController]
    [Route("api/product-recipes")]
    public class ProductRecipeController : ControllerBase
    {
        private readonly IProductRecipe _service;

        public ProductRecipeController(IProductRecipe service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] CreateProductRecipeDto dto)
        {
            if (dto == null || dto.Ingredients == null || !dto.Ingredients.Any())
            {
                return BadRequest("Invalid recipe data.");
            }

            try
            {
                await _service.SaveRecipeAsync(dto);
                return Ok(new { message = "Recipe saved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<ProductRecipeListVM>>> GetAllProductRecipes()
        {
            try
            {
                var recipes = await _service.GetAllProductRecipes();
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductIngredientController : ControllerBase
    {
        private readonly IProductIngredient _service;
        private readonly ILogger<ProductIngredientController> _logger;

        public ProductIngredientController(
            IProductIngredient service,
            ILogger<ProductIngredientController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadIngredients(IFormFile file)
        {
            try
            {
                var result = await _service.UploadExcelAsync(file);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading ingredients");
                return StatusCode(500, new { Message = "An error occurred while processing the file" });
            }
        }
        [HttpGet("Dropdown")]
        public async Task<IActionResult> GetProductIngredientDropdown()
        {
            var result = await _service.GetProductIngredientDropdownAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductIngredientDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                if (result == null)
                {
                    return BadRequest(new { message = "Product Ingredient already exists" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
    }
}

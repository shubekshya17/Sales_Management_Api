using Microsoft.AspNetCore.Mvc;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("Dropdown")]
        public async Task<IActionResult> GetProductDropdown(int categoryId)
        {
            var result = await _service.GetProductDropdownAsync(categoryId);
            return Ok(result);
        }
    }
}

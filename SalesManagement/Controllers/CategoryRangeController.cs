using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Implementation;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryRangeController : ControllerBase
    {
        private readonly ICategoryRange _service;

        public CategoryRangeController(ICategoryRange service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRangeDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
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

        [HttpGet("Dropdown")]
        public async Task<IActionResult> GetCategoryDropdown()
        {
            var result = await _service.GetCategoryDropdownAsync();
            return Ok(result);
        }
    }
}

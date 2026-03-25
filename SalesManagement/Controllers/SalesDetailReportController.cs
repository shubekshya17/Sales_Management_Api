using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Implementation;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesDetailReportController : ControllerBase
    {
        private readonly ISalesDetailReport _service;
        public SalesDetailReportController(ISalesDetailReport service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> GetSalesDetailReport([FromBody] SalesDetailReportRequest request)
        {
            var report = await _service.GetSalesDetailReport(request);
            return Ok(report);
        }
        [HttpPost("category-detail")]
        public async Task<IActionResult> GetCategoryWiseDetail([FromBody] SalesDetailCategoryWise request)
        {
            var result = await _service.GetSalesDetailCategoryWise(request);
            return Ok(result);
        }
    }
}

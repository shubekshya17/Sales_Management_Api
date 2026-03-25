using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesCollectionReportController : ControllerBase
    {
        private readonly ISalesCollectionReport _salesCollectionReport;

        public SalesCollectionReportController(ISalesCollectionReport salesCollectionReport)
        {
            _salesCollectionReport = salesCollectionReport;
        }

        [HttpPost]
        public async Task<IActionResult> GetSalesCollectionReport([FromBody] SalesCollectionReportRequest request)
        {
            if (request.FromDate > request.ToDate)
            {
                return BadRequest("FromDate cannot be greater than ToDate");
            }

            var result = await _salesCollectionReport.GetSalesCollectionReport(request);
            return Ok(result);
        }
    }
}

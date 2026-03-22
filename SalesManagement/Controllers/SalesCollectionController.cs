using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesCollectionController : ControllerBase
    {
        private readonly ISalesCollection _service;
        public SalesCollectionController(ISalesCollection service)
        {
            _service = service;
        }

        /// <summary>
        /// Upload a SalesCollection CSV file.
        /// New rows are inserted; rows already in the DB (same Date + Invoice) are skipped.
        /// </summary>
       
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto request)
        {
            var result = await _service.UploadExcelAsync(request.File, "Sales Collection");
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}

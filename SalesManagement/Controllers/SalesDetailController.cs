using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SalesDetailController : ControllerBase
    {
        private readonly ISalesDetail _service;

        public SalesDetailController(ISalesDetail service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto request)
        {
            var result = await _service.UploadExcelAsync(request.File);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KOTController : ControllerBase
    {
        private readonly IKOTService _service;

        public KOTController(IKOTService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto request)
        {
            var result = await _service.UploadExcelAsync(request.File, "KOT");
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}

using SalesManagement.Dtos;
using SalesManagement.Models;

namespace SalesManagement.Services.Interface
{
    public interface ISalesCollection
    {
        Task<UploadResultDto> UploadExcelAsync(IFormFile file, string expectedType);
    }
}

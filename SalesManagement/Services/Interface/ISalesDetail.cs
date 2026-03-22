using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface ISalesDetail
    {
        Task<UploadResultDto> UploadExcelAsync(IFormFile file, string expectedType);
    }
}

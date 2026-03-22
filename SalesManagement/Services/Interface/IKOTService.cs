using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface IKOTService
    {
        Task<UploadResultDto> UploadExcelAsync(IFormFile file, string expectedType);
    }
}

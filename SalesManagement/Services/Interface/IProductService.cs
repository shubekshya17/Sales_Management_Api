using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<List<ProductDropdownDto>> GetProductDropdownAsync(int categoryId);
    }
}

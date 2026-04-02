using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface IProductIngredient
    {
        Task<UploadResultDto> UploadExcelAsync(IFormFile file);
        Task<List<ProductIngredientDropdownDto>> GetProductIngredientDropdownAsync();
        Task<ProductIngredientCreateResponseDto> CreateAsync(CreateProductIngredientDto dto);
        Task<List<ProductIngredientCreateResponseDto>> GetAllAsync();
    }
}

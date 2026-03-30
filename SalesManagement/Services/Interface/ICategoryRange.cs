using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface ICategoryRange
    {
        Task<CategoryRangeDto> CreateAsync(CreateCategoryRangeDto dto);
        Task<List<CategoryRangeDto>> GetAllAsync();
        Task<List<CategoryDropdownDto>> GetCategoryDropdownAsync();
    }
}

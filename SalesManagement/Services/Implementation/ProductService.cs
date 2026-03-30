using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        public ProductService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<ProductDto>> GetAllAsync()
        {
            return await _db.Products
                .OrderBy(x => x.Name)
                .Select(x => new ProductDto
                {
                    Name = x.Name,
                    ItemCode = x.ItemCode,
                })
                .ToListAsync();
        }

        public async Task<List<ProductDropdownDto>> GetProductDropdownAsync(int categoryId)
        {
            // Step 1: Get range for the category
            var range = await _db.CategoryRanges
                .Where(x => x.Id == categoryId)
                .Select(x => new { x.MinValue, x.MaxValue })
                .FirstOrDefaultAsync();

            if (range == null)
                return new List<ProductDropdownDto>();

            // Step 2: Filter products based on range
            var products = await _db.Products
     .OrderBy(p => p.Name)
     .ToListAsync();

            return products
                .Where(p => decimal.TryParse(p.ItemCode, out var code) &&
                            code >= range.MinValue &&
                            code < range.MaxValue)
                .Select(p => new ProductDropdownDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToList();
        }
    }
}

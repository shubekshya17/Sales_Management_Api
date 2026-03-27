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
    }
}

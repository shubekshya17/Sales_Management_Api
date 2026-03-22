using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class CategoryRangeService : ICategoryRange
    {
        private readonly AppDbContext _db;

        public CategoryRangeService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<CategoryRangeDto> CreateAsync(CreateCategoryRangeDto dto)
        {
            var isOverlapping = await _db.CategoryRanges.AnyAsync(x =>
                dto.MinValue < x.MaxValue && dto.MaxValue > x.MinValue);

            if (isOverlapping)
                throw new Exception("Range overlaps with existing category");

            var entity = new CategoryRange
            {
                MinValue = dto.MinValue,
                MaxValue = dto.MaxValue,
                CategoryName = dto.CategoryName
            };

            _db.CategoryRanges.Add(entity);
            await _db.SaveChangesAsync();

            return new CategoryRangeDto
            {
                Id = entity.Id,
                MinValue = entity.MinValue,
                MaxValue = entity.MaxValue,
                CategoryName = entity.CategoryName
            };
        }

        public async Task<List<CategoryRangeDto>> GetAllAsync()
        {
            return await _db.CategoryRanges
                .OrderBy(x => x.MinValue)
                .Select(x => new CategoryRangeDto
                {
                    Id = x.Id,
                    MinValue = x.MinValue,
                    MaxValue = x.MaxValue,
                    CategoryName = x.CategoryName
                })
                .ToListAsync();
        }
    }
}

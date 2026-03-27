using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Models;

namespace SalesManagement.Services.Implementation
{
    public class ProductSyncService
    {
        private readonly AppDbContext _context;

        public ProductSyncService(AppDbContext context)
        {
            _context = context;
        }
        public async Task SyncProductsAsync()
        {
            var salesDetails = await _context.SalesDetail
                .Where(x => !string.IsNullOrEmpty(x.ItemCode))
                .GroupBy(x => x.ItemCode)
                .Select(g => new
                {
                    ItemCode = g.Key,
                    Name = g.First().Desca
                })
                .ToListAsync();

            var existingProducts = await _context.Products.ToListAsync();

            var productDict = existingProducts
                .ToDictionary(p => p.ItemCode, p => p);

            var newProducts = new List<Product>();

            foreach (var item in salesDetails)
            {
                if (!productDict.ContainsKey(item.ItemCode))
                {
                    newProducts.Add(new Product
                    {
                        ItemCode = item.ItemCode,
                        Name = item.Name,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            if (newProducts.Any())
            {
                await _context.Products.AddRangeAsync(newProducts);
                await _context.SaveChangesAsync();
            }
        }
    }
}

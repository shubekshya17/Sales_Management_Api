using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class SalesDetailReportService : ISalesDetailReport
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SalesDetailReportService> _logger;
        public SalesDetailReportService(AppDbContext db, ILogger<SalesDetailReportService> logger)
            => (_context, _logger) = (db, logger);
        public async Task<SalesDetailReportResponse> GetSalesDetailReport(SalesDetailReportRequest request)
        {
            var sales = await _context.SalesDetail
                .Where(s => s.TRNDATE >= request.FromDate && s.TRNDATE <= request.ToDate)
                .ToListAsync();

            var categories = await _context.CategoryRanges.ToListAsync();

            // Map sales to category
            var salesWithCategory = sales.Select(s =>
            {
                if (!decimal.TryParse(s.ItemCode, out var itemValue))
                    itemValue = 0;

                var category = categories.FirstOrDefault(c => itemValue >= c.MinValue && itemValue <= c.MaxValue);
                return new
                {
                    CategoryName = category?.CategoryName ?? "Uncategorized",
                    s.Amount,
                    s.Discount
                };
            });

            // Group by category for CategoryReports
            var categoryReports = salesWithCategory
                .GroupBy(x => x.CategoryName)
                .Select(g => new CategoryWiseAmount
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToList();

            // Calculate overall totals
            var totalAmount = salesWithCategory.Sum(x => x.Amount);
            var totalDiscount = salesWithCategory.Sum(x => x.Discount);
            var vat = (totalAmount - totalDiscount) * 0.13m;
            var total = (totalAmount - totalDiscount) + vat;

            return new SalesDetailReportResponse
            {
                CategoryWiseAmount = categoryReports,
                NetTotal = totalAmount,
                Discount = totalDiscount,
                Vat = vat,
                Total = total
            };
        }
    }
}

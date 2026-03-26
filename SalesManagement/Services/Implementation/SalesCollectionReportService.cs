using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class SalesCollectionReportService : ISalesCollectionReport
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SalesCollectionReportService> _logger;
        public SalesCollectionReportService(AppDbContext db, ILogger<SalesCollectionReportService> logger)
             => (_context, _logger) = (db, logger);
        public async Task<SalesCollectionReportResponse> GetSalesCollectionReport(SalesCollectionReportRequest request)
        {
            var data = await _context.SalesCollections
                .Where(x => x.Date >= request.FromDate && x.Date <= request.ToDate)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalCash = g.Sum(x => x.Cash),
                    TotalCreditCard = g.Sum(x => x.CreditCard),
                    TotalOnline = g.Sum(x => x.Online),
                    TotalCredit = g.Sum(x => x.Credit),
                    TotalDiscount = g.Sum(x => x.Discount),
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return new SalesCollectionReportResponse();

            // Step 1: Payment totals
            var amountCalc = new AmountCalculation
            {
                TotalCash = data.TotalCash,
                TotalCreditCard = data.TotalCreditCard,
                TotalOnline = data.TotalOnline,
                TotalCredit = data.TotalCredit
            };

            // Step 2: Net Total
            var netTotal = amountCalc.TotalCash
                         + amountCalc.TotalCreditCard
                         + amountCalc.TotalOnline
                         + amountCalc.TotalCredit;

            // Step 3: Discount
            var discount = data.TotalDiscount;

            // Step 4: VAT (13%)
            var vat = (netTotal - discount) * 0.13m;

            // Step 5: Final Total
            var total = netTotal - discount + vat;

            return new SalesCollectionReportResponse
            {
                AmountCalculation = amountCalc,
                NetTotal = netTotal,
                Discount = discount,
                Vat = vat,
                Total = total
            };
        }
        public Task<List<SalesCollectionDetail>> GetPaymentDetail(PaymentDetailRequest request)
        {
            var query = _context.SalesCollections
                .Where(x => x.Date >= request.FromDate && x.Date <= request.ToDate);

            // Normalize input (avoid case issues)
            var method = request.PaymentMethod.ToLower().Trim();

            query = method switch
            {
                "cash" => query.Where(x => x.Cash > 0),
                "credit card" => query.Where(x => x.CreditCard > 0),
                "online" => query.Where(x => x.Online > 0),
                "credit" => query.Where(x => x.Credit > 0),

                _ => throw new Exception("Invalid payment method")
            };

            return query
                .OrderByDescending(x => x.Date)
                .Select(x => new SalesCollectionDetail
                {
                    Date = x.Date,
                    Invoice = x.Invoice,
                    Party = x.Party ?? string.Empty,
                    Gross = x.Gross,
                    Discount = x.Discount,
                    NetSale = x.NetSale,
                    Vat = x.Vat,
                    Total = x.Total,
                    TRNUser = x.TRNUser ?? string.Empty,
                    TRNTime = x.TRNTime ?? string.Empty,
                    STax = x.STax,
                    Pax = x.Pax ?? 0,
                    BillToPan = x.BillToPan ?? string.Empty,
                    BillToMob = x.BillToMob ?? string.Empty,
                    Cash = x.Cash,
                    CreditCard = x.CreditCard,
                    Credit = x.Credit,
                    Online = x.Online,
                    GVoucher = x.GVoucher,
                    SalesReturnVoucher = x.SalesReturnVoucher,
                    Complimentary = x.Complimentary,
                    TransactionId = x.TransactionId ?? string.Empty,
                    OrderMode = x.OrderMode ?? string.Empty
                })
                .ToListAsync();
        }
    }
}

using ClosedXML.Excel;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;
using System.Globalization;
using System.Linq;

namespace SalesManagement.Services.Implementation
{
    public class SalesCollectionService : ISalesCollection
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SalesCollectionService> _logger;
        public SalesCollectionService(AppDbContext db, ILogger<SalesCollectionService> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<UploadResultDto> UploadExcelAsync(IFormFile file, string expectedType)
        {
            var result = new UploadResultDto();

            if (file == null || file.Length == 0)
                return new UploadResultDto { Message = "File missing" };

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return new UploadResultDto { Message = "Only .xlsx files accepted" };

            var columnError = ValidateColumns(file, expectedType);
            if (columnError != null)
                return new UploadResultDto { Message = columnError, Success = false };

            List<SalesCollectionExcelRow> rows;
            try { rows = ParseExcel(file); }
            catch (Exception ex) { return new UploadResultDto { Message = $"Parse error: {ex.Message}" }; }

            var existingKeys = (await _db.SalesCollections.Select(x => new { x.Date, x.Invoice }).ToListAsync())
                .Select(x => (x.Date.Date, x.Invoice))
                .ToHashSet();

            var toInsert = new List<SalesCollection>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (!DateTime.TryParse(r.Date, out var date))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {i + 2}: Invalid TRNDATE = {r.Date}");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(r.Invoice))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {i + 2}: Missing VCHRNO = {r.Invoice}");
                    continue;
                }

                var key = (date.Date, r.Invoice);
                if (existingKeys.Contains(key)) { result.Updated++; continue; }

                var entity = new SalesCollection
                {
                    Date = date,
                    Invoice = r.Invoice,
                    Party = r.Party,
                    Gross = ParseDecimal(r.Gross),
                    Discount = ParseDecimal(r.Discount),
                    NetSale = ParseDecimal(r.NetSale),
                    Vat = ParseDecimal(r.Vat),
                    Total = ParseDecimal(r.Total),
                    STax = ParseDecimal(r.STax),
                    Pax = ParseNullableInt(r.Pax),
                    TRNUser = r.TRNUser,
                    TRNTime = r.TRNTime,
                    BillToPan = r.BillToPan,
                    BillToMob = r.BillToMob,
                    Cash = ParseDecimal(r.Cash),
                    CreditCard = ParseDecimal(r.CreditCard),
                    Credit = ParseDecimal(r.Credit),
                    Online = ParseDecimal(r.Online),
                    GVoucher = ParseDecimal(r.GVoucher),
                    SalesReturnVoucher = ParseDecimal(r.SalesReturnVoucher),
                    Complimentary = ParseDecimal(r.Complimentary),
                    TransactionId = r.TransactionId,
                    OrderMode = r.OrderMode,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                toInsert.Add(entity);
                existingKeys.Add(key);
            }

            if (toInsert.Any())
            {
                await _db.SalesCollections.AddRangeAsync(toInsert);
                await _db.SaveChangesAsync();
            }

            result.Inserted = toInsert.Count;
            result.Success = true;
            return result;
        }
        private static List<SalesCollectionExcelRow> ParseExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

            // Find header row dynamically
            int headerRowIndex = 1;
            for (int row = 1; row <= lastRow; row++)
            {
                var rowValues = Enumerable.Range(1, lastCol)
                    .Select(c => ws.Cell(row, c).GetValue<string>().Trim().ToUpperInvariant())
                    .ToArray();

                var matchCount = rowValues.Count(v =>
                    ExpectedColumns.Any(e => e.ToUpperInvariant() == v));

                if (matchCount >= 3)
                {
                    headerRowIndex = row;
                    break;
                }
            }

            var rows = new List<SalesCollectionExcelRow>();

            // Start reading data from the row AFTER the header
            for (int i = headerRowIndex + 1; i <= lastRow; i++)
            {
                var r = ws.Row(i);
                rows.Add(new SalesCollectionExcelRow
                {
                    Date = r.Cell(1).GetValue<string>(),
                    Invoice = r.Cell(2).GetValue<string>(),
                    Party = r.Cell(3).GetValue<string>(),
                    Gross = r.Cell(4).GetValue<string>(),
                    Discount = r.Cell(5).GetValue<string>(),
                    NetSale = r.Cell(6).GetValue<string>(),
                    Vat = r.Cell(7).GetValue<string>(),
                    Total = r.Cell(8).GetValue<string>(),
                    TRNUser = r.Cell(9).GetValue<string>(),
                    TRNTime = r.Cell(10).GetValue<string>(),
                    STax = r.Cell(11).GetValue<string>(),
                    Pax = r.Cell(12).GetValue<string>(),
                    BillToPan = r.Cell(13).GetValue<string>(),
                    BillToMob = r.Cell(14).GetValue<string>(),
                    Cash = r.Cell(15).GetValue<string>(),
                    CreditCard = r.Cell(16).GetValue<string>(),
                    Credit = r.Cell(17).GetValue<string>(),
                    Online = r.Cell(18).GetValue<string>(),
                    GVoucher = r.Cell(19).GetValue<string>(),
                    SalesReturnVoucher = r.Cell(20).GetValue<string>(),
                    Complimentary = r.Cell(21).GetValue<string>(),
                    TransactionId = r.Cell(22).GetValue<string>(),
                    OrderMode = r.Cell(23).GetValue<string>()
                });
            }

            return rows;
        }

        private static decimal ParseDecimal(string? val) => decimal.TryParse(val, out var d) ? d : 0;
        private static int? ParseNullableInt(string? val) => int.TryParse(val, out var i) ? i : null;

        private static readonly string[] SalesDetailSignature = new[] { "ITEMCODE", "BILLQTY", "BILLRATE", "BASEUNIT" };
        private static readonly string[] KotSignature = new[] { "KOTNO", "TABLENO", "WAITER" }; 

        private static string DetectFileType(string[] actualHeaders)
        {
            if (SalesDetailSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "Sales Detail";

            if (KotSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "KOT";

            return "unknown";
        }

        private static readonly string[] ExpectedColumns = new[]
        {
            "Date","Invoice", "Party","Gross", "Discount", "Net Sale", "Vat", "Total",
            "TRNUser", "TRNTime", "STax", "Pax", "BillToPan", "BillToMob",
            "Cash", "CreditCard", "Credit", "Online", "GVoucher",
            "SalesReturnVoucher", "Complimentary", "TransactionId", "OrderMode"
        };
        private static string? ValidateColumns(IFormFile file, string expectedType)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

            string[]? actualHeaders = null;

            // Scan each row until we find the header row
            for (int row = 1; row <= lastRow; row++)
            {
                var rowValues = Enumerable.Range(1, lastCol)
                    .Select(c => ws.Cell(row, c).GetValue<string>().Trim().ToUpperInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToArray();

                // Check if this row contains at least some of our expected columns
                var matchCount = rowValues.Count(v =>
                    ExpectedColumns.Any(e => e.ToUpperInvariant() == v));

                if (matchCount >= 3) // ← if 3+ columns match, this is our header row
                {
                    actualHeaders = rowValues;
                    break;
                }
            }

            // No header row found at all
            if (actualHeaders == null)
                return "Wrong file uploaded. Your selected file doesn't match. Please make sure you are uploading the correct Excel file.";

            var missing = ExpectedColumns
                .Where(e => !actualHeaders.Contains(e.ToUpperInvariant()))
                .ToList();

            var extra = actualHeaders
                .Where(a => !ExpectedColumns.Any(e => e.ToUpperInvariant() == a))
                .ToList();

            if (missing.Any() || extra.Any())
            {
                var detectedType = DetectFileType(actualHeaders);

                var message = detectedType == "unknown"
                    ? "Wrong file uploaded. Your selected file doesn't match. Please make sure you are uploading the correct Excel file."
                    : $"Wrong file uploaded. You uploaded a {detectedType} file instead of {expectedType}. Please make sure you are uploading the correct Excel file.";

                return message;
            }

            return null; // valid
        }
    }
}
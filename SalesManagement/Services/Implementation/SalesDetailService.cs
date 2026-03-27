
using ClosedXML.Excel;
using Hangfire;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;

namespace SalesManagement.Services.Implementation
{
    public class SalesDetailService : ISalesDetail
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SalesDetailService> _logger;
        private readonly IBackgroundJobClient _jobClient;
        public SalesDetailService(AppDbContext db, ILogger<SalesDetailService> logger, IBackgroundJobClient jobClient)
            => (_db, _logger, _jobClient) = (db, logger, jobClient);
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

            List<SalesDetailExcelRow> rows;
            int headerRowIndex;
            try
            {
                var parsed = ParseExcel(file);
                rows = parsed.Rows;
                headerRowIndex = parsed.HeaderRowIndex;
            }
            catch (Exception ex) { return new UploadResultDto { Message = $"Parse error: {ex.Message}" }; }

            var existingKeys = (await _db.SalesDetail.Select(x => new { x.TRNDATE, x.VCHRNO, x.ItemCode }).ToListAsync())
                .Select(x => (x.TRNDATE.Date, x.VCHRNO, x.ItemCode))
                .ToHashSet();

            var toInsert = new List<SalesDetail>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var excelRowNumber = headerRowIndex + 1 + i;
                if (!DateTime.TryParse(r.TRNDATE, out var date))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Invalid TRNDATE = {r.TRNDATE}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.VCHRNO))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing VCHRNO = {r.VCHRNO}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.ItemCode))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing ItemCode = {r.ItemCode}");
                    continue;
                }

                var key = (date.Date, r.VCHRNO, r.ItemCode);
                if (existingKeys.Contains(key)) { result.Updated++; continue; }

                var entity = new SalesDetail
                {
                    TRNDATE = date,
                    VCHRNO = r.VCHRNO,
                    BSDate = r.BSDate,
                    REFNO = r.REFNO,
                    ItemCode = r.ItemCode,
                    Desca = r.Desca,
                    BillTo = r.BillTo,
                    Barcode = r.Barcode,
                    BillUnit = r.BillUnit,
                    BILLQTY = ParseDecimal(r.BillQty),
                    BillRate = ParseDecimal(r.BillRate),
                    BaseUnit = r.BaseUnit,
                    BaseQty = ParseDecimal(r.BaseQty),
                    BaseRate = ParseDecimal(r.BaseRate),
                    Amount = ParseDecimal(r.Amount),
                    Discount = ParseDecimal(r.Discount),
                    SCharge = ParseDecimal(r.SCharge),
                    NetSale = ParseDecimal(r.NetSale),
                    Taxable = ParseDecimal(r.Taxable),
                    NonTaxable = ParseDecimal(r.NonTaxable),
                    Vat = ParseDecimal(r.Vat),
                    NetAmnt = ParseDecimal(r.NetAmnt),
                    TRNUser = r.TRNUser,
                    TRNTime = r.TRNTime,
                    Division = r.Division,
                    Salesman = r.Salesman,
                    MobileNo = r.MobileNo,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Terminal = r.Terminal,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                toInsert.Add(entity);
                existingKeys.Add(key);
            }

            if (toInsert.Any())
            {
                await _db.SalesDetail.AddRangeAsync(toInsert);
                await _db.SaveChangesAsync();
                _jobClient.Enqueue<ProductSyncService>(x => x.SyncProductsAsync());
            }

            result.Inserted = toInsert.Count;
            result.TotalRowsInFile = rows.Count;
            result.Success = true;
            return result;
        }
        private static (List<SalesDetailExcelRow> Rows, int HeaderRowIndex) ParseExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

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

            var rows = new List<SalesDetailExcelRow>();
            for (int i = headerRowIndex + 1; i <= lastRow; i++)
            {
                var r = ws.Row(i);
                var dateVal = r.Cell(1).GetValue<string>().Trim();
                var vchrno = r.Cell(3).GetValue<string>().Trim();
                var itemcode = r.Cell(5).GetValue<string>().Trim();

                if (string.IsNullOrWhiteSpace(dateVal) && string.IsNullOrWhiteSpace(vchrno) && string.IsNullOrWhiteSpace(itemcode))
                    continue;
                rows.Add(new SalesDetailExcelRow
                {
                    TRNDATE = dateVal,
                    BSDate = r.Cell(2).GetValue<string>(),
                    VCHRNO = vchrno,
                    REFNO = r.Cell(4).GetValue<string>(),
                    ItemCode = itemcode,
                    Desca = r.Cell(6).GetValue<string>(),
                    BillTo = r.Cell(7).GetValue<string>(),
                    Barcode = r.Cell(8).GetValue<string>(),
                    BillUnit = r.Cell(9).GetValue<string>(),
                    BillQty = r.Cell(10).GetValue<string>(),
                    BillRate = r.Cell(11).GetValue<string>(),
                    BaseUnit = r.Cell(12).GetValue<string>(),
                    BaseQty = r.Cell(13).GetValue<string>(),
                    BaseRate = r.Cell(14).GetValue<string>(),
                    Amount = r.Cell(15).GetValue<string>(),
                    Discount = r.Cell(16).GetValue<string>(),
                    SCharge = r.Cell(17).GetValue<string>(),
                    NetSale = r.Cell(18).GetValue<string>(),
                    Taxable = r.Cell(19).GetValue<string>(),
                    NonTaxable = r.Cell(20).GetValue<string>(),
                    Vat = r.Cell(21).GetValue<string>(),
                    NetAmnt = r.Cell(22).GetValue<string>(),
                    TRNUser = r.Cell(23).GetValue<string>(),
                    TRNTime = r.Cell(24).GetValue<string>(),
                    Division = r.Cell(25).GetValue<string>(),
                    Salesman = r.Cell(26).GetValue<string>(),
                    MobileNo = r.Cell(27).GetValue<string>(),
                    StartTime = r.Cell(28).GetValue<string>(),
                    EndTime = r.Cell(29).GetValue<string>(),
                    Terminal = r.Cell(30).GetValue<string>()
                });
            }
            return (rows, headerRowIndex);
        }
        private static decimal ParseDecimal(string? val) => decimal.TryParse(val, out var d) ? d : 0;

        private static readonly string[] SalesCollectionSignature = new[] { "DATE", "INVOICE", "PARTY", "GROSS" };
        private static readonly string[] KotSignature = new[] { "KOTNO", "TABLENO", "WAITER" };
        private static string DetectFileType(string[] actualHeaders)
        {
            if (SalesCollectionSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "Sales Collection";

            if (KotSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "KOT";

            return "unknown";
        }

        private static readonly string[] ExpectedColumns = new[]
        {
            "TRNDATE","BSDate", "VCHRNO","REFNO","ItemCode","Desca","BillTo", "Barcode", "BillUnit", "BillQty",
            "BillRate", "BaseUnit", "BaseQty", "BaseRate", "Amount", "Discount",
            "SCharge", "NetSale", "Taxable", "NonTaxable", "Vat",
            "NetAmnt", "TRNUser", "TRNTime", "Division",
            "Salesman", "MobileNo", "StartTime", "EndTime", "Terminal"
        };
        private static readonly string[] DetectColumns = new[]
        {
            "DATE", "INVOICE", "PARTY", "GROSS",
            "KOTNO", "TABLENO", "WAITER",
        };
        private static string? ValidateColumns(IFormFile file, string expectedType)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();
            string[]? actualHeaders = null;

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
                if (actualHeaders == null)
                {
                    var detectCount = rowValues.Count(v =>
                    DetectColumns.Any(e => e.ToUpperInvariant() == v));
                    if (detectCount >= 3)
                    {
                        actualHeaders = rowValues;
                        break;
                    }
                }
            }

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

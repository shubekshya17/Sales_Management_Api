
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
    public class SalesDetailService : ISalesDetail
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SalesDetailService> _logger;
        public SalesDetailService(AppDbContext db, ILogger<SalesDetailService> logger)
            => (_db, _logger) = (db, logger);

        public async Task<UploadResultDto> UploadExcelAsync(IFormFile file)
        {
            var result = new UploadResultDto();
            if (file == null || file.Length == 0)
                return new UploadResultDto { Message = "File missing" };
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return new UploadResultDto { Message = "Only .xlsx files accepted" };
            var columnError = ValidateColumns(file);
            if (columnError != null)
                return new UploadResultDto { Message = columnError, Success = false };

            List<SalesDetailExcelRow> rows;
            try { rows = ParseExcel(file); }
            catch (Exception ex) { return new UploadResultDto { Message = $"Parse error: {ex.Message}" }; }

            var existingKeys = (await _db.SalesDetail.Select(x => new { x.TRNDATE, x.VCHRNO, x.ItemCode }).ToListAsync())
                .Select(x => (x.TRNDATE.Date, x.VCHRNO, x.ItemCode))
                .ToHashSet();

            var toInsert = new List<SalesDetail>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (!DateTime.TryParse(r.TRNDATE, out var date)) { result.Failed++;
                    result.Errors.Add($"Row {i + 2}: Invalid TRNDATE = {r.TRNDATE}");
                    continue; }
                if (string.IsNullOrWhiteSpace(r.VCHRNO)) { result.Failed++;
                    result.Errors.Add($"Row {i + 2}: Missing VCHRNO = {r.VCHRNO}");
                    continue; }
                if (string.IsNullOrWhiteSpace(r.ItemCode)) { result.Failed++;
                    result.Errors.Add($"Row {i + 2}: Missing ItemCode = {r.ItemCode}");
                    continue; }

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
            }

            result.Inserted = toInsert.Count;
            result.Success = true;
            return result;
        }

        private static List<SalesDetailExcelRow> ParseExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();
            var rows = new List<SalesDetailExcelRow>();
            for (int i = 2; i <= ws.LastRowUsed().RowNumber(); i++)
            {
                var r = ws.Row(i);
                rows.Add(new SalesDetailExcelRow
                {
                    TRNDATE = r.Cell(1).GetValue<string>(),
                    BSDate = r.Cell(2).GetValue<string>(),
                    VCHRNO = r.Cell(3).GetValue<string>(),
                    REFNO = r.Cell(4).GetValue<string>(),
                    ItemCode = r.Cell(5).GetValue<string>(),
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
            return rows;
        }
        private static decimal ParseDecimal(string? val) => decimal.TryParse(val, out var d) ? d : 0;

        private static readonly string[] ExpectedColumns = new[]
        {
            "TRNDATE","BSDate", "VCHRNO","REFNO","ItemCode","Desca","BillTo", "Barcode", "BillUnit", "BillQty",
            "BillRate", "BaseUnit", "BaseQty", "BaseRate", "Amount", "Discount",
            "SCharge", "NetSale", "Taxable", "NonTaxable", "Vat",
            "NetAmnt", "TRNUser", "TRNTime", "Division",
            "Salesman", "MobileNo", "StartTime", "EndTime", "Terminal"
        };
        private static string? ValidateColumns(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastCol = ws.LastColumnUsed().ColumnNumber();
            var actualHeaders = Enumerable.Range(1, lastCol)
                .Select(c => ws.Cell(1, c).GetValue<string>().Trim().ToUpperInvariant())
                .ToArray();

            var missing = ExpectedColumns
                .Where(e => !actualHeaders.Contains(e.ToUpperInvariant()))
                .ToList();

            var extra = actualHeaders
                .Where(a => !ExpectedColumns.Any(e => e.ToUpperInvariant() == a) && !string.IsNullOrWhiteSpace(a))
                .ToList();

            if (missing.Any() || extra.Any())
            {
                var msg = $"Wrong file uploaded. Your selected file doesn't match. Please make sure you are uploading the correct Excel file.";
                return msg;
                /*var msg = "Invalid columns.";
                if (missing.Any()) msg +=$" Missing: {string.Join(", ", missing)}.";
                if (extra.Any()) msg += $" Unexpected: {string.Join(", ", extra)}.";
                return msg;*/
            }
            return null; // valid
        }

    }
}

using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace SalesManagement.Services.Implementation
{
    public class KOTService : IKOTService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<KOTService> _logger;
        public KOTService(AppDbContext db, ILogger<KOTService> logger)
            => (_db, _logger) = (db, logger);

        // ─── Upload Entry Point ────────────────────────────────────────────────
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

            List<KOTExcelRow> rows;
            int headerRowIndex;

            try
            {
                var parsed = ParseExcel(file);
                rows = parsed.Rows;
                headerRowIndex = parsed.HeaderRowIndex;
            }
            catch (Exception ex)
            {
                return new UploadResultDto { Message = $"Parse error: {ex.Message}" };
            }

            // ✅ Load all existing row hashes from DB into a HashSet for O(1) lookup
            // Two rows are duplicates only if every single column matches exactly
            var existingHashes = (await _db.KOT
                    .Select(x => x.RowHash)
                    .ToListAsync())
                .Where(h => h != null)
                .Select(h => h!)
                .ToHashSet();

            var toInsert = new List<KOT>();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var excelRowNumber = headerRowIndex + 1 + i;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(r.TableNo))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing TABLENO");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.KotNo))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing KOTNO");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.ItemCode))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing ITEMCODE");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(r.KotId))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {excelRowNumber}: Missing KOTID");
                    continue;
                }

                // ✅ Compute SHA256 hash of all meaningful columns for this row
                var hash = ComputeRowHash(r);

                // If this exact combination of values already exists in DB — skip it
                if (existingHashes.Contains(hash)) { result.Updated++; continue; }

                var entity = new KOT
                {
                    KOTNO = r.KotNo,
                    KOTTIME = r.KotTime,
                    TABLENO = r.TableNo,
                    WAITER = r.Waiter,
                    WAITERNAME = r.WaiterName,
                    ITEMCODE = r.ItemCode,
                    ITEMDESCRIPTION = r.ItemDescription,
                    QUANTITY = ParseDecimal(r.Quantity),
                    UNIT = r.Unit,
                    REMARKS = r.Remarks,
                    BILLED = r.Billed,
                    BILLNO = r.BillNo,
                    TRANSFERKOT = r.TransferKot,
                    MERGEKOT = r.MergeKot,
                    SPLITKOT = r.SplitKot,
                    CANCELBY = r.CancelBy,
                    CANCELREMARKS = r.CancelRemarks,
                    FLG = r.Flg,
                    ISBARITEM = r.IsBarItem,
                    KOTID = r.KotId,
                    RowHash = hash,  // ✅ store the hash
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                toInsert.Add(entity);
                existingHashes.Add(hash); // ✅ also prevents duplicates within the same file
            }

            if (toInsert.Any())
            {
                await _db.KOT.AddRangeAsync(toInsert);
                await _db.SaveChangesAsync();
            }

            result.Inserted = toInsert.Count;
            result.TotalRowsInFile = rows.Count;
            result.Success = true;
            return result;
        }

        // ─── Row Hash ─────────────────────────────────────────────────────────
        // Computes a SHA256 fingerprint from all meaningful columns.
        // If every column matches an existing DB row → it's a duplicate.
        // Pipe-separated so "AB|C" and "A|BC" produce different hashes.
        private static string ComputeRowHash(KOTExcelRow r)
        {
            var raw = $"{r.Waiter}|{r.TableNo}|{r.KotNo}|{r.KotTime}|{r.ItemCode}|" +
                      $"{r.ItemDescription}|{r.Quantity}|{r.Unit}|{r.Remarks}|" +
                      $"{r.Billed}|{r.BillNo}|{r.TransferKot}|{r.MergeKot}|" +
                      $"{r.SplitKot}|{r.CancelBy}|{r.CancelRemarks}|{r.WaiterName}|" +
                      $"{r.Flg}|{r.IsBarItem}|{r.KotId}";

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes); // e.g. "3A9F2C8D..."
        }

        // ─── Parse Excel ───────────────────────────────────────────────────────
        // Columns in Excel order:
        // 1=WAITER  2=TABLENO  3=KOTNO   4=KOTTIME        5=ITEMCODE
        // 6=ITEMDESCRIPTION    7=QUANTITY 8=UNIT           9=REMARKS
        // 10=BILLED 11=BILLNO  12=TRANSFERKOT 13=MERGEKOT 14=SPLITKOT
        // 15=CANCELBY          16=CANCELREMARKS            17=WAITERNAME
        // 18=FLG    19=ISBARITEM 20=KOTID
        private static (List<KOTExcelRow> Rows, int HeaderRowIndex) ParseExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

            // Dynamically find header row
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

            var rows = new List<KOTExcelRow>();

            for (int i = headerRowIndex + 1; i <= lastRow; i++)
            {
                var r = ws.Row(i);
                var waiter = r.Cell(1).GetValue<string>().Trim();
                var tableNo = r.Cell(2).GetValue<string>().Trim();
                var kotNo = r.Cell(3).GetValue<string>().Trim();
                var itemCode = r.Cell(5).GetValue<string>().Trim();
                var kotId = r.Cell(20).GetValue<string>().Trim();

                // Skip completely empty rows (trailing blank rows at bottom)
                if (string.IsNullOrWhiteSpace(waiter) &&
                    string.IsNullOrWhiteSpace(kotNo) &&
                    string.IsNullOrWhiteSpace(itemCode) &&
                    string.IsNullOrWhiteSpace(kotId))
                    continue;

                // ✅ Skip waiter name group header rows
                // The WAITER column contains two types of values:
                //   - A date string like "03-03-26" → real data row    (KEEP)
                //   - A name string like "AAVASH"   → group header row (SKIP)
                // If the WAITER cell has a value but does NOT parse as a date,
                // it is a waiter name header — skip it entirely.
                if (!string.IsNullOrWhiteSpace(waiter) && !DateTime.TryParse(waiter, out _))
                    continue;

                rows.Add(new KOTExcelRow
                {
                    Waiter = waiter,
                    TableNo = tableNo,
                    KotNo = kotNo,
                    KotTime = r.Cell(4).GetValue<string>().Trim(),
                    ItemCode = itemCode,
                    ItemDescription = r.Cell(6).GetValue<string>(),
                    Quantity = r.Cell(7).GetValue<string>(),
                    Unit = r.Cell(8).GetValue<string>(),
                    Remarks = r.Cell(9).GetValue<string>(),
                    Billed = r.Cell(10).GetValue<string>(),
                    BillNo = r.Cell(11).GetValue<string>(),
                    TransferKot = r.Cell(12).GetValue<string>(),
                    MergeKot = r.Cell(13).GetValue<string>(),
                    SplitKot = r.Cell(14).GetValue<string>(),
                    CancelBy = r.Cell(15).GetValue<string>(),
                    CancelRemarks = r.Cell(16).GetValue<string>(),
                    WaiterName = r.Cell(17).GetValue<string>(),
                    Flg = r.Cell(18).GetValue<string>(),
                    IsBarItem = r.Cell(19).GetValue<string>(),
                    KotId = kotId,
                });
            }

            return (rows, headerRowIndex);
        }

        // ─── Helpers ───────────────────────────────────────────────────────────
        private static decimal ParseDecimal(string? val) =>
            decimal.TryParse(val, out var d) ? d : 0;

        // ─── File type detection ───────────────────────────────────────────────
        private static readonly string[] SalesCollectionSignature = new[] { "DATE", "INVOICE", "PARTY", "GROSS" };

        private static readonly string[] SalesDetailSignature =
            new[] { "ITEMCODE", "BILLQTY", "BILLRATE", "BASEUNIT" };

        private static string DetectFileType(string[] actualHeaders)
        {
            if (SalesCollectionSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "Sales Collection";

            if (SalesDetailSignature.All(col => actualHeaders.Contains(col.ToUpperInvariant())))
                return "Sales Detail";

            return "unknown";
        }

        // ─── Expected columns ──────────────────────────────────────────────────
        private static readonly string[] ExpectedColumns = new[]
        {
            "WAITER", "TABLENO", "KOTNO", "KOTTIME", "ITEMCODE",
            "ITEMDESCRIPTION", "QUANTITY", "UNIT", "REMARKS",
            "BILLED", "BILLNO", "TRANSFERKOT", "MERGEKOT", "SPLITKOT",
            "CANCELBY", "CANCELREMARKS", "WAITERNAME",
            "FLG", "ISBARITEM", "KOTID"
        };
        private static readonly string[] DetectColumns = new[]
        {
            "DATE", "INVOICE", "PARTY", "GROSS",
            "ITEMCODE", "BILLQTY", "BILLRATE", "BASEUNIT",
        };

        // ─── Column Validation ─────────────────────────────────────────────────
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

                var matchCount = rowValues.Count(v =>
                    ExpectedColumns.Any(e => e.ToUpperInvariant() == v));

                if (matchCount >= 3)
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
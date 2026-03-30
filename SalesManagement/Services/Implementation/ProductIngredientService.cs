using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Data;
using SalesManagement.Dtos;
using SalesManagement.Models;
using SalesManagement.Services.Interface;

namespace SalesManagement.Services.Implementation
{
    public class ProductIngredientService : IProductIngredient
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ProductIngredientService> _logger;

        public ProductIngredientService(AppDbContext db, ILogger<ProductIngredientService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<UploadResultDto> UploadExcelAsync(IFormFile file)
        {
            var result = new UploadResultDto();

            if (file == null || file.Length == 0)
                return new UploadResultDto { Message = "File missing", Success = false };

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return new UploadResultDto { Message = "Only .xlsx files accepted", Success = false };

            // Validate file type before parsing
            var validationError = ValidateFileType(file);
            if (validationError != null)
                return new UploadResultDto { Message = validationError, Success = false };

            List<ProductIngredientExcelRow> rows;

            try
            {
                rows = ParseExcel(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Excel file");
                return new UploadResultDto { Message = $"Parse error: {ex.Message}", Success = false };
            }

            if (!rows.Any())
            {
                return new UploadResultDto
                {
                    Message = "No valid data found in the Excel file",
                    Success = false
                };
            }

            // Get all existing ingredients (case-insensitive lookup)
            var existingIngredients = await _db.ProductIngredients
                .ToDictionaryAsync(x => x.Ingredient.ToLower(), x => x);

            var toInsert = new List<ProductIngredient>();
            var toUpdate = new List<ProductIngredient>();

            foreach (var r in rows)
            {
                var ingredientKey = r.Ingredient.Trim().ToLower();

                // Check if ingredient already exists
                if (existingIngredients.ContainsKey(ingredientKey))
                {
                    var existing = existingIngredients[ingredientKey];

                    // Update if unit has changed
                    if (existing.Unit != r.Unit.Trim())
                    {
                        existing.Unit = r.Unit.Trim();
                        existing.UpdatedAt = DateTime.UtcNow;
                        toUpdate.Add(existing);
                        result.Updated++;
                    }
                    else
                    {
                        result.Updated++;
                    }
                }
                else
                {
                    // Insert new ingredient
                    var newIngredient = new ProductIngredient
                    {
                        Ingredient = r.Ingredient.Trim(),
                        Unit = r.Unit.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    toInsert.Add(newIngredient);
                    existingIngredients.Add(ingredientKey, newIngredient);
                    result.Inserted++;
                }
            }

            // Save changes
            if (toInsert.Any())
            {
                await _db.ProductIngredients.AddRangeAsync(toInsert);
            }

            if (toUpdate.Any())
            {
                _db.ProductIngredients.UpdateRange(toUpdate);
            }

            if (toInsert.Any() || toUpdate.Any())
            {
                await _db.SaveChangesAsync();
            }

            result.TotalRowsInFile = rows.Count;
            result.Success = true;
            result.Message = $"Successfully processed: {result.Inserted} inserted, {result.Updated} updated";

            return result;
        }

        private static readonly string[] SalesCollectionSignature = new[] { "DATE", "INVOICE", "PARTY", "GROSS" };
        private static readonly string[] SalesDetailSignature = new[] { "ITEMCODE", "BILLQTY", "BILLRATE", "BASEUNIT" };
        private static readonly string[] KotSignature = new[] { "KOTNO", "TABLENO", "WAITER" };
        private static readonly string[] ProductIngredientSignature = new[] { "PARTICULARS", "UNIT RATE" };

        private static string? ValidateFileType(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

            string[]? actualHeaders = null;

            // Search for headers in first 30 rows
            for (int row = 1; row <= Math.Min(30, lastRow); row++)
            {
                var rowValues = Enumerable.Range(1, lastCol)
                    .Select(c => ws.Cell(row, c).GetValue<string>().Trim().ToUpperInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToArray();

                if (rowValues.Length < 2) continue;

                // Check if this row contains "PARTICULARS"
                if (rowValues.Any(v => v == "PARTICULARS"))
                {
                    actualHeaders = rowValues;
                    break;
                }

                // Check if this row contains other known signatures
                var matchesSales = SalesCollectionSignature.Count(col => rowValues.Contains(col));
                var matchesDetail = SalesDetailSignature.Count(col => rowValues.Contains(col));
                var matchesKot = KotSignature.Count(col => rowValues.Contains(col));

                if (matchesSales >= 3 || matchesDetail >= 3 || matchesKot >= 3)
                {
                    actualHeaders = rowValues;
                    break;
                }
            }

            if (actualHeaders == null)
                return "Wrong file uploaded. Your selected file doesn't match. Please make sure you are uploading the correct Excel file.";

            // Check if it's a Product Ingredient file
            var hasParticulars = actualHeaders.Contains("PARTICULARS");
            var hasUnit = actualHeaders.Contains("UNIT RATE") || actualHeaders.Contains("UNIT");

            if (hasParticulars && hasUnit)
            {
                return null; // Valid Product Ingredient file
            }

            // Detect what type of file it actually is
            var detectedType = DetectFileType(actualHeaders);

            if (detectedType == "unknown")
            {
                return "Wrong file uploaded. Your selected file doesn't match. Please make sure you are uploading the correct Excel file.";
            }

            return $"Wrong file uploaded. You uploaded a {detectedType} file instead of Product Ingredients. Please make sure you are uploading the correct Excel file.";
        }

        private static string DetectFileType(string[] actualHeaders)
        {
            if (SalesCollectionSignature.All(col => actualHeaders.Contains(col)))
                return "Sales Collection";

            if (SalesDetailSignature.All(col => actualHeaders.Contains(col)))
                return "Sales Detail";

            if (KotSignature.All(col => actualHeaders.Contains(col)))
                return "KOT";

            return "unknown";
        }

        private static List<ProductIngredientExcelRow> ParseExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var lastRow = ws.LastRowUsed().RowNumber();
            var lastCol = ws.LastColumnUsed().ColumnNumber();

            // Step 1: Find the header row and column indices
            int headerRowIndex = -1;
            int particularsColIndex = -1;
            int unitColIndex = -1;

            // Search for header in first 30 rows
            for (int row = 1; row <= Math.Min(30, lastRow); row++)
            {
                for (int col = 1; col <= lastCol; col++)
                {
                    var cellValue = ws.Cell(row, col).GetValue<string>().Trim();

                    // Look for "Particulars" column (case-insensitive)
                    if (cellValue.Equals("Particulars", StringComparison.OrdinalIgnoreCase))
                    {
                        headerRowIndex = row;
                        particularsColIndex = col;
                    }

                    // Look for "Unit Rate" or "Unit" column (case-insensitive)
                    if (cellValue.Equals("Unit Rate", StringComparison.OrdinalIgnoreCase) ||
                        cellValue.Equals("Unit", StringComparison.OrdinalIgnoreCase))
                    {
                        unitColIndex = col;
                    }
                }

                // Break when both columns are found in the same row
                if (headerRowIndex > 0 && particularsColIndex > 0 && unitColIndex > 0)
                    break;
            }

            // Validate that we found the header
            if (headerRowIndex == -1 || particularsColIndex == -1 || unitColIndex == -1)
            {
                throw new Exception("Could not find required header columns: 'Particulars' and 'Unit' (or 'Unit Rate'). Please ensure the Excel file has the correct format.");
            }

            var rows = new List<ProductIngredientExcelRow>();

            // Step 2: Start reading from the row immediately after the header
            for (int i = headerRowIndex + 1; i <= lastRow; i++)
            {
                var particularsValue = ws.Cell(i, particularsColIndex).GetValue<string>().Trim();
                var unitValue = ws.Cell(i, unitColIndex).GetValue<string>().Trim();

                // Only process rows that have BOTH Particulars and Unit values
                if (string.IsNullOrWhiteSpace(particularsValue) || string.IsNullOrWhiteSpace(unitValue))
                    continue;

                // Skip rows that are clearly section totals or headers
                if (IsSectionHeader(particularsValue))
                    continue;

                // Add valid row
                rows.Add(new ProductIngredientExcelRow
                {
                    Ingredient = particularsValue,
                    Unit = unitValue
                });
            }

            return rows;
        }

        private static bool IsSectionHeader(string value)
        {
            // List of keywords that indicate a section header rather than an actual ingredient
            var sectionKeywords = new[]
            {
            "Total",
            "Consumption Total",
            "Nepal Tea Collective",
            "Nepal Tea",
            "BAKEREY Product",
            "BAKERY Product",
            "Confectionery Item",
            "Merchandise Item",
            "Guest supply",
            "Cleaning Items"
        };

            return sectionKeywords.Any(keyword =>
                value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<List<ProductIngredientDropdownDto>> GetProductIngredientDropdownAsync()
        {
            return await _db.ProductIngredients
                .OrderBy(x => x.Id)
                .Select(x => new ProductIngredientDropdownDto
                {
                    Id = x.Id,
                    Ingredient = x.Ingredient,
                    Unit = x.Unit
                })
                .ToListAsync();
        }
    }
}

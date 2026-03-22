namespace SalesManagement.Dtos
{
    public class UploadResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRowsInFile { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class SalesCollectionFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

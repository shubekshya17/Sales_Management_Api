namespace SalesManagement.Dtos
{
    public class SalesDetailFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

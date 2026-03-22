namespace SalesManagement.Dtos
{
    public class CategoryWiseAmount
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
    public class SalesDetailReportResponse
    {
        public List<CategoryWiseAmount> CategoryWiseAmount { get; set; } = new();
        public decimal NetTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
    }
    public class SalesDetailReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}

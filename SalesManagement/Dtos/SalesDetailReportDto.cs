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
    public class SalesDetailCategoryWise
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
    public class SalesDetailItem
    {
        public DateTime TrnDate { get; set; }
        public string BsDate { get; set; } = string.Empty;
        public string VchrNo { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string Desca { get; set; } = string.Empty;
        public string BillTo { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string BillUnit { get; set; } = string.Empty;
        public decimal BillQty { get; set; }
        public decimal BillRate { get; set; }
        public string BaseUnit { get; set; } = string.Empty;
        public decimal BaseQty { get; set; }
        public decimal BaseRate { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal SCharge { get; set; }
        public decimal NetSale { get; set; }
        public decimal Taxable { get; set; }
        public decimal NonTaxable { get; set; }
        public decimal Vat { get; set; }
        public decimal NetAmnt { get; set; }
        public string TrnUser { get; set; } = string.Empty;
        public string TrnTime { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string Salesman { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Terminal { get; set; } = string.Empty;
    }
}

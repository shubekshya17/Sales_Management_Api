namespace SalesManagement.Models
{
    public class SalesDetail
    {
        public int Id { get; set; }
        public DateTime TRNDATE { get; set; }
        public string? BSDate { get; set; }
        public string? VCHRNO { get; set; }
        public string? REFNO { get; set; }
        public string? ItemCode { get; set; }
        public string? Desca { get; set; }
        public string? BillTo { get; set; }
        public string? Barcode { get; set; }
        public string? BillUnit { get; set; }

        public decimal BILLQTY { get; set; }
        public decimal BillRate { get; set; }

        public string? BaseUnit { get; set; }
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

        public string? TRNUser { get; set; }
        public string? TRNTime { get; set; }
        public string? Division { get; set; }
        public string? Salesman { get; set; }
        public string? MobileNo { get; set; }

        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Terminal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
